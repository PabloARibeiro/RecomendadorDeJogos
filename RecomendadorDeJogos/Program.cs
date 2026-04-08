using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecomendadorDeJogos
{
    class Program
    {
        // 1. Mudamos de 'void Main' para 'async Task Main'
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== INICIANDO SISTEMA ===");
            
            RepositorioJogos repo = new RepositorioJogos();
            
            // 2. Colocamos o 'await' antes de chamar a função
            List<Jogo> bancoDeJogos = await repo.CarregarJogosApi();

            if (bancoDeJogos.Count == 0)
            {
                Console.WriteLine("Sem dados para trabalhar. Encerrando.");
                return;
            }
            Console.WriteLine($"Sucesso! {bancoDeJogos.Count} jogos carregados.\n");

            Console.WriteLine("=== BEM-VINDO AO RECOMENDADOR DE JOGOS V7 (Arquitetura Limpa) ===");
            List<Jogo> jogosEscolhidos = new List<Jogo>();

            while (jogosEscolhidos.Count < 5)
            {
                Console.Write($"Jogo {jogosEscolhidos.Count + 1}/5 (ou Enter para pular): ");
                string inputUsuario = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(inputUsuario))
                {
                    if (jogosEscolhidos.Count >= 1) break;
                    Console.WriteLine("Veterano avisa: Digite pelo menos 1 jogo!");
                    continue;
                }

                Jogo? jogoEncontrado = bancoDeJogos.FirstOrDefault(j => j.Nome.Equals(inputUsuario, StringComparison.OrdinalIgnoreCase));

                if (jogoEncontrado != null)
                {
                    if (!jogosEscolhidos.Contains(jogoEncontrado))
                    {
                        jogosEscolhidos.Add(jogoEncontrado);
                        Console.WriteLine($"[+] '{jogoEncontrado.Nome}' adicionado!");
                    }
                    else Console.WriteLine("Você já adicionou esse jogo!");
                }
                else Console.WriteLine("[-] Jogo não encontrado no Banco.");
            }

            Console.WriteLine("\n=== FILTROS ===");
            Console.Write("Gênero a excluir (Enter para pular): ");
            string generoExcluido = Console.ReadLine() ?? string.Empty;

            Console.Write("Ano Mínimo (Enter para ignorar): ");
            string inputAnoMin = Console.ReadLine() ?? string.Empty;
            int? anoMin = int.TryParse(inputAnoMin, out int min) ? min : (int?)null;
            
            Console.Write("Ano Máximo (Enter para ignorar): ");
            string inputAnoMax = Console.ReadLine() ?? string.Empty;
            int? anoMax = int.TryParse(inputAnoMax, out int max) ? max : (int?)null;

            Console.WriteLine("\n=== CALCULANDO RECOMENDAÇÕES ===");

            // 2. Instanciamos nosso Motor e passamos a bola para ele!
            MotorRecomendacao motor = new MotorRecomendacao();
            List<Recomendacao> resultados = motor.Gerar(bancoDeJogos, jogosEscolhidos, generoExcluido, anoMin, anoMax);

            // 3. Exibimos o resultado
            if (resultados.Count == 0)
            {
                Console.WriteLine("Com esses filtros, não achamos nada :(");
            }
            else
            {
                foreach (var rec in resultados)
                {
                    Console.WriteLine($"\n> {rec.Item.Nome} ({rec.Item.Ano}) (Match: {rec.Pontos} pontos)");
                    if(rec.MatchesGen.Any()) Console.WriteLine($"  [Gêneros em comum]: {string.Join(", ", rec.MatchesGen)}");
                    if(rec.MatchesTema.Any()) Console.WriteLine($"  [Temas em comum]: {string.Join(", ", rec.MatchesTema)}");
                    if(rec.MatchesEst.Any()) Console.WriteLine($"  [Estilo visual em comum]: {string.Join(", ", rec.MatchesEst)}");
                }
            }
            
            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
        }
    }
}