using System.Text;
using BarbeiroDorminhoco;
using BarbeiroDorminhoco.Solucoes;

Console.OutputEncoding = Encoding.UTF8;

// Requisito 8: menu com uma opção por solução.
// Também aceita a opção como argumento (ex.: dotnet run -- 2) para rodar direto, sem menu.
if (args.Length > 0)
{
    Executar(args[0]);
    return;
}

while (true)
{
    Console.WriteLine();
    Console.WriteLine("===== O BARBEIRO DORMINHOCO =====");
    Console.WriteLine($"1 · Ingênua 1 · sem controle de concorrência    -> race condition");
    Console.WriteLine($"2 · Ingênua 2 · lock + laço infinito             -> espera ocupada (CPU)");
    Console.WriteLine($"3 · Ingênua 3 · campainha sem memória (Pulse)    -> lost wakeup (impasse)");
    Console.WriteLine($"4 · Solução ideal (SemaphoreSlim + lock)         -> em breve (Entrega 3)");
    Console.WriteLine($"5 · Comparar desempenho                          -> em breve (Entrega 3)");
    Console.WriteLine("0 · Sair");
    Console.Write("Opção: ");

    string? opcao = Console.ReadLine()?.Trim();
    if (opcao is null or "0") break;
    Executar(opcao);
}

static void Executar(string opcao)
{
    Barbearia? solucao = opcao switch
    {
        "1" => new Ingenua1SemControle(),
        "2" => new Ingenua2EsperaOcupada(),
        "3" => new Ingenua3SinalPerdido(),
        _ => null,
    };

    if (solucao is null)
    {
        Console.WriteLine(opcao is "4" or "5" ? "Disponível na Entrega 3." : "Opção inválida.");
        return;
    }

    Console.WriteLine();
    solucao.Executar(Config.PrazoIngenuaMs);
}
