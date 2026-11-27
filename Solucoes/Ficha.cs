namespace BarbeiroDorminhoco.Solucoes;

/// <summary>
/// A "ficha" de um cliente na fila das Ingênuas 2 e 3: o barbeiro tira a ficha da fila,
/// chama o cliente, avisa quando o corte terminou e espera o cliente levantar.
/// O lado do cliente é correto de propósito (eventos que guardam o sinal), para que o problema
/// de cada solução fique só no lado do barbeiro, que é o que a solução está tentando mostrar.
/// (Sem Dispose: barbeiro e cliente usam a ficha em momentos diferentes; o GC a recolhe depois.)
/// </summary>
public sealed class Ficha(string cliente)
{
    public string Cliente { get; } = cliente;
    public ManualResetEventSlim Chamado { get; } = new(false);
    public ManualResetEventSlim CorteTerminado { get; } = new(false);
    public ManualResetEventSlim Levantou { get; } = new(false);
}
