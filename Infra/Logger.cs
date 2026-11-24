using System.Diagnostics;

namespace BarbeiroDorminhoco.Infra;

/// <summary>
/// Requisito 5: log de cada evento com horário e autor.
/// Ex.: [00:01.532] Cliente 07    · sentou na espera (2/3)
/// O lock aqui protege só a escrita no console (cor + linha); não faz parte da solução do problema.
/// </summary>
public static class Logger
{
    private static readonly object _console = new();
    private static readonly Stopwatch _relogio = new();

    public static void Iniciar() => _relogio.Restart();

    public static void Log(string autor, string mensagem) => Escrever(autor, mensagem, null);

    public static void Alerta(string autor, string mensagem) => Escrever(autor, "!! " + mensagem, ConsoleColor.Red);

    public static void Destaque(string autor, string mensagem) => Escrever(autor, mensagem, ConsoleColor.Cyan);

    private static void Escrever(string autor, string mensagem, ConsoleColor? cor)
    {
        var t = _relogio.Elapsed;
        lock (_console)
        {
            if (cor is not null) Console.ForegroundColor = cor.Value;
            Console.WriteLine($"[{t:mm\\:ss\\.fff}] {autor,-10} · {mensagem}");
            if (cor is not null) Console.ResetColor();
        }
    }
}
