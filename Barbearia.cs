using System.Diagnostics;
using BarbeiroDorminhoco.Infra;

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
                Thread.Sleep(Random.Shared.Next(0, Config.ChegadaMaxMs)); // chega em instante aleatório
                RotinaCliente($"Cliente {id:00}", cts.Token);
            }, TaskCreationOptions.LongRunning))
            .ToArray();

        Task.WaitAll(clientes);
        cts.Cancel(); // todos os clientes já foram embora: o barbeiro pode fechar
        barbeiro.Wait();
        relogio.Stop();

        Relatorio.Imprimir(Nome, Stats, relogio.Elapsed);
    }

    protected static int TempoDeCorte() => Random.Shared.Next(Config.CorteMinMs, Config.CorteMaxMs + 1);
}
