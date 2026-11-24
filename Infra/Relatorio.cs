namespace BarbeiroDorminhoco.Infra;

/// <summary>
/// Requisito 6: relatório final com contadores e 3 verificações automáticas ([OK] / [ERRO]).
/// </summary>
public static class Relatorio
{
    public static void Imprimir(string solucao, Estatisticas s, TimeSpan tempo)
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

        Verificar(s.Atendidos + s.Desistentes == Config.NumClientes,
            $"atendidos + desistentes = {Config.NumClientes} ({s.Atendidos} + {s.Desistentes} = {s.Atendidos + s.Desistentes})");
        Verificar(s.MaxNaEspera <= Config.CadeirasEspera,
            $"nunca mais de {Config.CadeirasEspera} clientes esperando (máximo: {s.MaxNaEspera})");
        Verificar(s.MaxNaCadeiraBarbeiro <= 1,
            $"um corte por vez (máximo na cadeira do barbeiro: {s.MaxNaCadeiraBarbeiro})");

        Console.WriteLine($"Tempo total: {tempo.TotalSeconds:0.0} s");
    }

    private static void Verificar(bool ok, string descricao)
    {
        Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"{(ok ? "[OK]" : "[ERRO]")} {descricao}");
        Console.ResetColor();
    }
}
