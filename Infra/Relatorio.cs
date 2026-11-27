namespace BarbeiroDorminhoco.Infra;

/// <summary>
/// Requisito 6: relatório final com contadores e verificações automáticas ([OK] / [ERRO]).
/// As 3 verificações obrigatórias são as do enunciado; a 4ª (CPU) prova a regra 3 (repouso real).
/// </summary>
public static class Relatorio
{
    /// <summary>Acima disso consideramos que alguém ficou girando em vez de dormir.</summary>
    public const double LimiteCpuPercentual = 20;

    public static void Imprimir(string solucao, Estatisticas s)
    {
        Console.WriteLine();
        Console.WriteLine("===== RELATÓRIO · Barbeiro Dorminhoco =====");
        Console.WriteLine($"Solução: {solucao} | Clientes: {Config.NumClientes} | Cadeiras de espera: {Config.CadeirasEspera}");
        Console.WriteLine($"Clientes atendidos ............... {s.Atendidos}");
        Console.WriteLine($"Clientes desistentes ............. {s.Desistentes}");
        Console.WriteLine($"Clientes nunca atendidos ......... {s.NaoAtendidos}");
        Console.WriteLine($"Cortes feitos pelo barbeiro ...... {s.Cortes}");
        Console.WriteLine($"Maior ocupação da sala de espera . {s.MaxNaEspera}");
        Console.WriteLine($"Maior nº na cadeira do barbeiro .. {s.MaxNaCadeiraBarbeiro}");
        Console.WriteLine($"Voltas ociosas do barbeiro ....... {s.VoltasOciosas:N0}");
        Console.WriteLine($"Avisos de chegada perdidos ....... {s.SinaisPerdidos}");
        Console.WriteLine($"CPU usada ........................ {s.TempoCpu.TotalSeconds:0.00} s ({s.PercentualNucleo:0}% de um núcleo)");

        Verificar(s.Atendidos + s.Desistentes == Config.NumClientes,
            $"atendidos + desistentes = {Config.NumClientes} ({s.Atendidos} + {s.Desistentes} = {s.Atendidos + s.Desistentes})");
        Verificar(s.MaxNaEspera <= Config.CadeirasEspera,
            $"nunca mais de {Config.CadeirasEspera} clientes esperando (máximo: {s.MaxNaEspera})");
        Verificar(s.MaxNaCadeiraBarbeiro <= 1,
            $"um corte por vez (máximo na cadeira do barbeiro: {s.MaxNaCadeiraBarbeiro})");
        Verificar(s.PercentualNucleo <= LimiteCpuPercentual,
            $"repouso real: CPU até {LimiteCpuPercentual:0}% de um núcleo (usou {s.PercentualNucleo:0}%)");

        Console.WriteLine($"Tempo total: {s.TempoTotal.TotalSeconds:0.0} s");
    }

    private static void Verificar(bool ok, string descricao)
    {
        Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"{(ok ? "[OK]" : "[ERRO]")} {descricao}");
        Console.ResetColor();
    }
}
