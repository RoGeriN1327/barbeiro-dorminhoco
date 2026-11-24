using System.Text;
using BarbeiroDorminhoco;
using BarbeiroDorminhoco.Solucoes;

Console.OutputEncoding = Encoding.UTF8;

// Entrega 1: roda direto a Solução Ingênua 1. O menu com todas as soluções chega na Entrega 2.
new Ingenua1SemControle().Executar(Config.PrazoIngenuaMs);
