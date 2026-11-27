namespace BarbeiroDorminhoco.Infra;

/// <summary>
/// Instrumentação: conta o que REALMENTE aconteceu, independente das variáveis (possivelmente erradas)
/// de cada solução. Usa Interlocked para que a medição em si nunca sofra condição de corrida.
/// Não faz parte da solução do problema: é o "fiscal" que alimenta o relatório.
/// </summary>
public sealed class Estatisticas
{
    private int _atendidos;
    private int _desistentes;
    private int _naoAtendidos;
    private int _cortes;

    private int _naEspera;
    private int _maxNaEspera;
    private int _naCadeiraBarbeiro;
    private int _maxNaCadeiraBarbeiro;

    public int Atendidos => Volatile.Read(ref _atendidos);
    public int Desistentes => Volatile.Read(ref _desistentes);
    public int NaoAtendidos => Volatile.Read(ref _naoAtendidos);
    public int Cortes => Volatile.Read(ref _cortes);
    public int MaxNaEspera => Volatile.Read(ref _maxNaEspera);
    public int MaxNaCadeiraBarbeiro => Volatile.Read(ref _maxNaCadeiraBarbeiro);

    public void Atendido() => Interlocked.Increment(ref _atendidos);
    public void Desistiu() => Interlocked.Increment(ref _desistentes);
    public void NaoAtendido() => Interlocked.Increment(ref _naoAtendidos);
    public void CorteRealizado() => Interlocked.Increment(ref _cortes);

    // Contadores de sinalização: mostram as regras 3 (repouso real) e 4 (despertar exato).
    private long _voltasOciosas;
    private int _sinaisPerdidos;

    /// <summary>Vezes que o barbeiro olhou a fila, não achou ninguém e perguntou de novo sem dormir.</summary>
    public long VoltasOciosas => Interlocked.Read(ref _voltasOciosas);
    /// <summary>Avisos de chegada que ninguém recebeu (o barbeiro não estava esperando o sinal).</summary>
    public int SinaisPerdidos => Volatile.Read(ref _sinaisPerdidos);

    public void RegistrarVoltasOciosas(long voltas) => Interlocked.Add(ref _voltasOciosas, voltas);
    public void SinalPerdido() => Interlocked.Increment(ref _sinaisPerdidos);

    // Uso de CPU do processo durante a execução (preenchido pela Barbearia).
    public TimeSpan TempoCpu { get; set; }
    public TimeSpan TempoTotal { get; set; }

    /// <summary>Percentual de UM núcleo usado em média: 100% = um núcleo ocupado o tempo todo.</summary>
    public double PercentualNucleo =>
        TempoTotal.TotalMilliseconds > 0 ? 100.0 * TempoCpu.TotalMilliseconds / TempoTotal.TotalMilliseconds : 0;

    /// <returns>Quantos clientes estão de fato na sala de espera depois desta entrada.</returns>
    public int EntrouNaEspera()
    {
        int atual = Interlocked.Increment(ref _naEspera);
        AtualizarMaximo(ref _maxNaEspera, atual);
        return atual;
    }

    public void SaiuDaEspera() => Interlocked.Decrement(ref _naEspera);

    /// <returns>Quantos clientes estão de fato na cadeira do barbeiro depois desta entrada.</returns>
    public int SentouNaCadeiraDoBarbeiro()
    {
        int atual = Interlocked.Increment(ref _naCadeiraBarbeiro);
        AtualizarMaximo(ref _maxNaCadeiraBarbeiro, atual);
        return atual;
    }

    public void SaiuDaCadeiraDoBarbeiro() => Interlocked.Decrement(ref _naCadeiraBarbeiro);

    private static void AtualizarMaximo(ref int maximo, int valor)
    {
        int anterior;
        do
        {
            anterior = Volatile.Read(ref maximo);
            if (valor <= anterior) return;
        } while (Interlocked.CompareExchange(ref maximo, valor, anterior) != anterior);
    }
}
