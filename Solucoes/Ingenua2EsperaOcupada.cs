using BarbeiroDorminhoco.Infra;

namespace BarbeiroDorminhoco.Solucoes;

/// <summary>
/// SOLUÇÃO INGÊNUA 2 · lock + laço infinito ("trancar a recepção e perguntar sem parar").
///
/// O que melhorou em relação à Ingênua 1: a fila (as 3 cadeiras) agora é protegida por lock.
/// Olhar e sentar viram uma operação só, então nunca sentam 4 clientes, e o barbeiro tira
/// um cliente por vez da fila (regras 1 e 2 cumpridas).
///
/// Problema esperado: ESPERA OCUPADA (busy waiting).
/// Sem nenhum mecanismo para dormir, o barbeiro fica em while (true) { lock { if (fila vazia) ... } }
/// perguntando milhões de vezes por segundo. A barbearia fica vazia e um núcleo da CPU vai a ~100%.
/// Ele ainda pega o lock a cada volta, disputando a "porta" com os clientes que querem sentar.
/// Viola a regra 3 (repouso real).
/// </summary>
public sealed class Ingenua2EsperaOcupada : Barbearia
{
    public override string Nome => "INGÊNUA 2 (lock + espera ocupada)";

    private readonly object _trava = new();          // protege a fila
    private readonly Queue<Ficha> _fila = new();     // as cadeiras de espera

    protected override void RotinaBarbeiro(CancellationToken ct)
    {
        const string eu = "Barbeiro";
        long voltasSeguidas = 0;
        long voltasTotal = 0;

        while (!ct.IsCancellationRequested)     // while (true) do slide
        {
            Ficha? proximo = null;
            lock (_trava)
            {
                if (_fila.Count > 0) proximo = _fila.Dequeue();
            }

            if (proximo is null)
            {
                // 0 clientes: pergunta de novo, JÁ! (sem Sleep, sem Wait)
                if (voltasSeguidas == 0) Logger.Log(eu, "fila vazia: vai perguntar de novo sem parar");
                voltasSeguidas++;
                voltasTotal++;
                if (voltasTotal % Config.VoltasPorLog == 0)
                    Logger.Alerta(eu, $"já perguntou {voltasTotal:N0} vezes se tem cliente (CPU ocupada: ESPERA OCUPADA)");
                continue;
            }

            if (voltasSeguidas > 0)
                Logger.Log(eu, $"achou cliente depois de {voltasSeguidas:N0} perguntas inúteis");
            voltasSeguidas = 0;

            Logger.Log(eu, $"chama {proximo.Cliente}");
            CortarCabelo(proximo);
        }

        Stats.RegistrarVoltasOciosas(voltasTotal);
        Logger.Log(eu, $"fecha a barbearia (perguntou {voltasTotal:N0} vezes à toa)");
    }

    protected override void RotinaCliente(string eu, CancellationToken ct)
    {
        var ficha = new Ficha(eu);

        lock (_trava)
        {
            // olhar e sentar dentro do MESMO lock: ninguém entra no meio
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

        AguardarAtendimento(ficha, eu, ct);
    }
}
