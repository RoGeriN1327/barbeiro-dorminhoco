using BarbeiroDorminhoco.Infra;

namespace BarbeiroDorminhoco.Solucoes;

/// <summary>
/// SOLUÇÃO INGÊNUA 1 · sem nenhum controle de concorrência.
///
/// Todo o estado da barbearia são campos comuns, lidos e escritos por várias threads sem lock:
///  - o cliente OLHA quantas cadeiras estão ocupadas e só depois SENTA (check-then-act);
///  - o barbeiro "chama o próximo" ligando uma flag, e o cliente que vê a flag a desliga.
///
/// Problema esperado: CONDIÇÃO DE CORRIDA (race condition).
///  - Entre olhar e sentar, outro cliente também olha e vê a mesma vaga: 4 ou 5 clientes em 3 cadeiras.
///  - Dois clientes veem o mesmo chamado do barbeiro: dois clientes na cadeira do barbeiro ao mesmo tempo.
///  - esperando++ / esperando-- sem atomicidade perdem atualizações: o barbeiro pode achar que não há
///    ninguém esperando e clientes ficam sem atendimento até o prazo acabar.
/// Viola as regras 1 (corte exclusivo) e 2 (fila limitada).
/// </summary>
public sealed class Ingenua1SemControle : Barbearia
{
    public override string Nome => "INGÊNUA 1 (sem controle de concorrência)";

    // Estado compartilhado SEM proteção nenhuma (de propósito).
    private int _esperando;      // quantos o programa ACHA que estão na sala de espera
    private bool _chamando;      // o barbeiro chamou o próximo cliente
    private int _cortesFeitos;   // quantos cortes o barbeiro já terminou

    protected override void RotinaBarbeiro(CancellationToken ct)
    {
        const string eu = "Barbeiro";
        bool dormindo = false;

        while (!ct.IsCancellationRequested)
        {
            if (_esperando == 0)
            {
                if (!dormindo) Logger.Log(eu, "ninguém esperando: dorme na cadeira");
                dormindo = true;
                Thread.Sleep(Config.IntervaloVerificacaoMs); // "dorme" olhando a sala de tempos em tempos
                continue;
            }

            dormindo = false;
            Logger.Log(eu, $"chama o próximo (variável esperando = {_esperando})");
            _chamando = true;
            while (_chamando && !ct.IsCancellationRequested) Thread.Sleep(1); // aguarda alguém vir
            if (ct.IsCancellationRequested) break;

            int tempo = TempoDeCorte();
            Logger.Log(eu, $"cortando cabelo ({tempo} ms)");
            Thread.Sleep(tempo);
            _cortesFeitos++;
            Stats.CorteRealizado();
            Logger.Log(eu, $"terminou o corte nº {_cortesFeitos}");
        }

        Logger.Log(eu, "fecha a barbearia");
    }

    protected override void RotinaCliente(string eu, CancellationToken ct)
    {
        // 1. OLHA as cadeiras
        int vistos = _esperando;
        Logger.Log(eu, $"chegou e viu {vistos}/{Config.CadeirasEspera} cadeiras ocupadas");
        if (vistos >= Config.CadeirasEspera)
        {
            Stats.Desistiu();
            Logger.Log(eu, "sala de espera cheia: desistiu e foi embora");
            return;
        }

        Thread.Sleep(Config.JanelaCorridaMs); // janela entre olhar e sentar: outro cliente entra aqui

        // 2. SENTA (a vaga que ele viu pode não existir mais)
        _esperando++;
        int ocupadas = Stats.EntrouNaEspera();
        if (ocupadas > Config.CadeirasEspera)
            Logger.Alerta(eu, $"sentou na espera: {ocupadas} clientes para {Config.CadeirasEspera} cadeiras! (RACE CONDITION)");
        else
            Logger.Log(eu, $"sentou na espera ({ocupadas}/{Config.CadeirasEspera})");

        // 3. Espera o chamado do barbeiro
        while (!_chamando)
        {
            if (ct.IsCancellationRequested)
            {
                Stats.SaiuDaEspera();
                Stats.NaoAtendido();
                Logger.Alerta(eu, "barbearia fechou e ele nunca foi chamado");
                return;
            }
            Thread.Sleep(Config.IntervaloVerificacaoMs);
        }

        Thread.Sleep(1);  // janela: outro cliente também viu o mesmo chamado
        _chamando = false;
        _esperando--;
        Stats.SaiuDaEspera();

        // 4. Senta na cadeira do barbeiro e espera o corte terminar
        int corteAntes = _cortesFeitos;
        int naCadeira = Stats.SentouNaCadeiraDoBarbeiro();
        if (naCadeira > 1)
            Logger.Alerta(eu, $"sentou na cadeira do barbeiro: {naCadeira} clientes sendo atendidos juntos! (RACE CONDITION)");
        else
            Logger.Log(eu, "sentou na cadeira do barbeiro");

        while (_cortesFeitos == corteAntes && !ct.IsCancellationRequested)
            Thread.Sleep(Config.IntervaloVerificacaoMs);

        Stats.SaiuDaCadeiraDoBarbeiro();
        if (ct.IsCancellationRequested)
        {
            Stats.NaoAtendido();
            Logger.Alerta(eu, "barbearia fechou no meio do corte");
            return;
        }

        Stats.Atendido();
        Logger.Log(eu, "saiu com o cabelo cortado");
    }
}
