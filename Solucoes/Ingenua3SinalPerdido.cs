using BarbeiroDorminhoco.Infra;

namespace BarbeiroDorminhoco.Solucoes;

/// <summary>
/// SOLUÇÃO INGÊNUA 3 · dormir e acordar com uma campainha SEM MEMÓRIA.
///
/// O que melhorou em relação à Ingênua 2: o barbeiro agora dorme de verdade (Monitor.Wait),
/// então não gasta CPU. A fila continua protegida por lock (regras 1, 2 e 3 cumpridas).
///
/// Ideia ingênua: "cada cliente toca a campainha uma vez, então eu acordo uma vez por cliente".
/// O cliente toca com Monitor.Pulse e o barbeiro dorme em Monitor.Wait, atendendo um cliente por toque.
///
/// Problema esperado: LOST WAKEUP (sinal perdido).
/// Monitor.Pulse só acorda quem JÁ está em Monitor.Wait; ele não fica guardado.
/// Se o cliente toca enquanto o barbeiro está cortando (ou entre ele olhar a fila e deitar),
/// o toque some. O barbeiro volta a dormir com clientes na sala, e eles esperam para sempre:
/// impasse. Viola a regra 4 (despertar exato). A Solução Ideal troca a campainha por um
/// SemaphoreSlim, cujo Release() fica contado mesmo se ninguém estiver esperando.
/// </summary>
public sealed class Ingenua3SinalPerdido : Barbearia
{
    public override string Nome => "INGÊNUA 3 (campainha sem memória · lost wakeup)";

    private readonly object _trava = new();          // protege a fila
    private readonly object _campainha = new();      // Monitor.Wait / Monitor.Pulse
    private readonly Queue<Ficha> _fila = new();
    private bool _dormindo;                          // só lido/escrito dentro de lock (_campainha)

    protected override void RotinaBarbeiro(CancellationToken ct)
    {
        const string eu = "Barbeiro";

        while (!ct.IsCancellationRequested)
        {
            // 1. Dorme até a campainha tocar
            bool tocou = false;
            lock (_campainha)
            {
                _dormindo = true;
                Logger.Log(eu, "dorme esperando a campainha");
                // Wait com tempo só para poder fechar a barbearia; retorna true apenas se houve Pulse.
                while (!tocou && !ct.IsCancellationRequested)
                    tocou = Monitor.Wait(_campainha, 100);
                _dormindo = false;
            }
            if (!tocou) break;

            // 2. Um toque = um cliente
            Ficha? proximo;
            lock (_trava)
            {
                proximo = _fila.Count > 0 ? _fila.Dequeue() : null;
            }
            if (proximo is null) continue;

            Logger.Log(eu, $"acordou com a campainha e chama {proximo.Cliente}");
            CortarCabelo(proximo);
            // 3. volta a dormir SEM olhar a fila: acha que, se tiver alguém, a campainha vai tocar
        }

        int esquecidos;
        lock (_trava) esquecidos = _fila.Count;
        if (esquecidos > 0)
            Logger.Alerta(eu, $"IMPASSE: dormiu o tempo todo com {esquecidos} cliente(s) esperando (LOST WAKEUP)");
        Logger.Log(eu, "fecha a barbearia");
    }

    protected override void RotinaCliente(string eu, CancellationToken ct)
    {
        var ficha = new Ficha(eu);

        lock (_trava)
        {
            if (_fila.Count >= Config.CadeirasEspera)
            {
                Stats.Desistiu();
                Logger.Log(eu, $"chegou, sala cheia ({_fila.Count}/{Config.CadeirasEspera}): desistiu e foi embora");
                return;
            }
            _fila.Enqueue(ficha);
            int ocupadas = Stats.EntrouNaEspera();
            Logger.Log(eu, $"chegou e sentou na espera ({ocupadas}/{Config.CadeirasEspera})");
        }

        // Toca a campainha. Se o barbeiro não estiver em Monitor.Wait, o Pulse se perde.
        lock (_campainha)
        {
            Monitor.Pulse(_campainha);
            if (_dormindo)
                Logger.Log(eu, "tocou a campainha e acordou o barbeiro");
            else
            {
                Stats.SinalPerdido();
                Logger.Alerta(eu, "tocou a campainha com o barbeiro acordado: o toque se perdeu (LOST WAKEUP)");
            }
        }

        AguardarAtendimento(ficha, eu, ct);
    }
}
