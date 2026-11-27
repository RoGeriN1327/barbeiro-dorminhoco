namespace BarbeiroDorminhoco;

/// <summary>
/// Requisito 1: todo o cenário (quantidade de threads, capacidades e tempos) fica neste único lugar.
/// </summary>
public static class Config
{
    // Cenário mínimo do enunciado: 1 barbeiro, 3 cadeiras de espera e 20 clientes.
    public const int NumClientes = 20;
    public const int CadeirasEspera = 3;

    // A barbearia abre e o barbeiro fica um tempo sem clientes (mostra como cada solução "espera").
    public const int AberturaMs = 1000;

    // Depois da abertura, cada cliente chega em um instante aleatório entre 0 e este valor (Random.Shared).
    public const int ChegadaMaxMs = 800;

    // Duração de um corte de cabelo (aleatória entre mínimo e máximo).
    public const int CorteMinMs = 150;
    public const int CorteMaxMs = 300;

    // Intervalo de "olhar de novo" nas soluções ingênuas que ficam conferindo variáveis.
    public const int IntervaloVerificacaoMs = 5;

    // Modo inseguro: tempo entre OLHAR as cadeiras e SENTAR. Alarga a janela da condição de corrida
    // para que o problema apareça em toda execução (seção 6 do enunciado).
    public const int JanelaCorridaMs = 15;

    // Prazo máximo de uma execução ingênua (ela pode nunca terminar sozinha, ex.: Ingênua 3).
    public const int PrazoIngenuaMs = 8_000;

    // Ingênua 2: a cada quantas perguntas sem cliente o barbeiro escreve no log (senão o log explode).
    public const long VoltasPorLog = 10_000_000;
}
