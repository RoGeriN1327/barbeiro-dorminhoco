using System.Diagnostics;
using BarbeiroDorminhoco.Infra;
using BarbeiroDorminhoco.Solucoes;

namespace BarbeiroDorminhoco;

/// <summary>
/// Cenário comum a todas as soluções: cria o barbeiro e os clientes como Tasks (requisito 2),
/// espera os clientes terminarem, fecha a barbearia e imprime o relatório.
/// Cada solução (ingênuas e ideal) só implementa a rotina do barbeiro e a do cliente.
/// </summary>
public abstract class Barbearia
{
    protected readonly Estatisticas Stats = new();

    public abstract string Nome { get; }

    protected abstract void RotinaBarbeiro(CancellationToken ct);

    protected abstract void RotinaCliente(string eu, CancellationToken ct);

    public void Executar(int prazoMs)
    {
        Console.WriteLine($"===== {Nome} =====");
        Logger.Iniciar();
        var processo = Process.GetCurrentProcess();
        var cpuAntes = processo.TotalProcessorTime;
        var relogio = Stopwatch.StartNew();

        // Prazo: uma solução ingênua pode nunca terminar sozinha.
        using var cts = new CancellationTokenSource(prazoMs);

        // LongRunning = thread dedicada para cada participante (eles bloqueiam com Sleep/Wait,
        // e no ThreadPool isso atrasaria a chegada dos clientes).
        var barbeiro = Task.Factory.StartNew(() => RotinaBarbeiro(cts.Token),
            TaskCreationOptions.LongRunning);

        var clientes = Enumerable.Range(1, Config.NumClientes)
            .Select(id => Task.Factory.StartNew(() =>
            {
                Thread.Sleep(Config.AberturaMs + Random.Shared.Next(0, Config.ChegadaMaxMs)); // chega em instante aleatório
                RotinaCliente($"Cliente {id:00}", cts.Token);
            }, TaskCreationOptions.LongRunning))
            .ToArray();

        Task.WaitAll(clientes);
        cts.Cancel(); // todos os clientes já foram embora: o barbeiro pode fechar
        barbeiro.Wait();
        relogio.Stop();
        processo.Refresh();

        Stats.TempoTotal = relogio.Elapsed;
        Stats.TempoCpu = processo.TotalProcessorTime - cpuAntes;
        Relatorio.Imprimir(Nome, Stats);
    }

    protected static int TempoDeCorte() => Random.Shared.Next(Config.CorteMinMs, Config.CorteMaxMs + 1);

    /// <summary>
    /// Lado do cliente depois de sentar (Ingênuas 2 e 3): espera ser chamado e espera o corte terminar.
    /// </summary>
    protected void AguardarAtendimento(Ficha ficha, string eu, CancellationToken ct)
    {
        try
        {
            ficha.Chamado.Wait(ct);
        }
        catch (OperationCanceledException)
        {
            Stats.SaiuDaEspera();
            Stats.NaoAtendido();
            Logger.Alerta(eu, "barbearia fechou e ele nunca foi chamado");
            return;
        }

        Stats.SaiuDaEspera();
        int naCadeira = Stats.SentouNaCadeiraDoBarbeiro();
        Logger.Log(eu, naCadeira > 1 ? $"sentou na cadeira do barbeiro ({naCadeira} na cadeira!)" : "sentou na cadeira do barbeiro");
        ficha.CorteTerminado.Wait(CancellationToken.None); // o barbeiro sempre termina o corte que começou
        Stats.SaiuDaCadeiraDoBarbeiro();
        Stats.Atendido();
        Logger.Log(eu, "saiu com o cabelo cortado");
        ficha.Levantou.Set();
    }

    /// <summary>
    /// Lado do barbeiro com um cliente já tirado da fila (Ingênuas 2 e 3): chama, corta,
    /// avisa que terminou e espera a cadeira ficar livre antes de seguir.
    /// </summary>
    protected void CortarCabelo(Ficha ficha)
    {
        const string eu = "Barbeiro";
        ficha.Chamado.Set();
        int tempo = TempoDeCorte();
        Logger.Log(eu, $"cortando cabelo de {ficha.Cliente} ({tempo} ms)");
        Thread.Sleep(tempo);
        Stats.CorteRealizado();
        Logger.Log(eu, $"terminou o corte de {ficha.Cliente}");
        ficha.CorteTerminado.Set();
        ficha.Levantou.Wait();
    }
}
