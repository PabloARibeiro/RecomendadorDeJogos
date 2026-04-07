using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace RecomendadorDeJogos
{
    public class Jogo
    {
        // RESOLVENDO O AVISO CS8618: 
        // Já deixamos o Nome como texto vazio e as listas como novas listas vazias desde o nascimento da classe.
        // Assim, eles nunca serão 'null'.
        public string Nome { get; set; } = string.Empty;
        public int Ano { get; set; }
        public List<string> Generos { get; set; } = new List<string>();
        public List<string> Temas { get; set; } = new List<string>();
        public List<string> EstilosVisuais { get; set; } = new List<string>();
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== INICIANDO SISTEMA ===");
            List<Jogo> bancoDeJogos = new List<Jogo>();

            try
            {
                Console.WriteLine("Carregando banco de dados...");
                string jsonString = File.ReadAllText("gamelist.json");
                
                // RESOLVENDO O AVISO DO JSON (CS8600):
                // O '?? new List<Jogo>()' significa: "Se a desserialização retornar null, crie uma lista vazia no lugar".
                bancoDeJogos = JsonSerializer.Deserialize<List<Jogo>>(jsonString) ?? new List<Jogo>();
                
                Console.WriteLine($"Sucesso! {bancoDeJogos.Count} jogos carregados na memória.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro fatal ao carregar o banco de dados: {ex.Message}");
                return; 
            }

            Console.WriteLine("=== BEM-VINDO AO RECOMENDADOR DE JOGOS V6 (Blindado) ===");
            List<Jogo> jogosEscolhidos = new List<Jogo>();

            while (jogosEscolhidos.Count < 5)
            {
                Console.Write($"Jogo {jogosEscolhidos.Count + 1}/5 (ou Enter para pular): ");
                
                // RESOLVENDO O AVISO DO READLINE (CS8600):
                // O '?? string.Empty' garante que, se der algum tilt e vier null, vira um texto vazio comum.
                string inputUsuario = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(inputUsuario))
                {
                    if (jogosEscolhidos.Count >= 1) break;
                    Console.WriteLine("Veterano avisa: Digite pelo menos 1 jogo!");
                    continue;
                }

                // Colocamos um '?' no tipo 'Jogo' para avisar o C# que sabemos que pode não achar o jogo (retornar nulo).
                Jogo? jogoEncontrado = bancoDeJogos.FirstOrDefault(j => j.Nome.Equals(inputUsuario, StringComparison.OrdinalIgnoreCase));

                if (jogoEncontrado != null)
                {
                    if (!jogosEscolhidos.Contains(jogoEncontrado))
                    {
                        jogosEscolhidos.Add(jogoEncontrado);
                        Console.WriteLine($"[+] '{jogoEncontrado.Nome}' ({jogoEncontrado.Ano}) adicionado!");
                    }
                    else Console.WriteLine("Você já adicionou esse jogo!");
                }
                else Console.WriteLine("[-] Jogo não encontrado no JSON.");
            }

            Console.WriteLine("\n=== FILTROS ===");
            Console.WriteLine("1. Tem algum gênero que você NÃO quer ver? (Deixe em branco para pular)");
            Console.Write("Gênero a excluir: ");
            string generoExcluido = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("\n2. A partir de que ano você quer buscar? (Deixe em branco para ignorar)");
            Console.Write("Ano Mínimo: ");
            string inputAnoMin = Console.ReadLine() ?? string.Empty;
            
            Console.WriteLine("\n3. Até que ano você quer buscar? (Deixe em branco para ignorar)");
            Console.Write("Ano Máximo: ");
            string inputAnoMax = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("\n=== CALCULANDO RECOMENDAÇÕES ===");

            // Como as listas da classe Jogo já iniciam vazias lá em cima, não precisamos mais do '?? new List<string>()' aqui embaixo!
            List<string> generosInteresse = jogosEscolhidos.SelectMany(j => j.Generos).Distinct().ToList();
            List<string> temasInteresse = jogosEscolhidos.SelectMany(j => j.Temas).Distinct().ToList();
            List<string> estilosInteresse = jogosEscolhidos.SelectMany(j => j.EstilosVisuais).Distinct().ToList();
            List<string> nomesEscolhidos = jogosEscolhidos.Select(j => j.Nome).ToList();

            var candidatos = bancoDeJogos.Where(j => !nomesEscolhidos.Contains(j.Nome));

            if (!string.IsNullOrWhiteSpace(generoExcluido))
            {
                candidatos = candidatos.Where(j => !j.Generos.Any(g => g.Equals(generoExcluido, StringComparison.OrdinalIgnoreCase)));
            }

            if (int.TryParse(inputAnoMin, out int anoMin))
            {
                candidatos = candidatos.Where(j => j.Ano >= anoMin);
            }

            if (int.TryParse(inputAnoMax, out int anoMax))
            {
                candidatos = candidatos.Where(j => j.Ano <= anoMax);
            }

            var recomendacoes = candidatos
                .Select(candidato => 
                {
                    var matchGeneros = candidato.Generos.Where(g => generosInteresse.Contains(g)).ToList();
                    var matchTemas = candidato.Temas.Where(t => temasInteresse.Contains(t)).ToList();
                    var matchEstilos = candidato.EstilosVisuais.Where(e => estilosInteresse.Contains(e)).ToList();

                    int scoreTotal = (matchGeneros.Count * 3) + (matchTemas.Count * 2) + (matchEstilos.Count * 1);

                    return new
                    {
                        Item = candidato,
                        Pontos = scoreTotal,
                        MatchesGen = matchGeneros,
                        MatchesTema = matchTemas,
                        MatchesEst = matchEstilos
                    };
                })
                .Where(x => x.Pontos > 0) 
                .OrderByDescending(x => x.Pontos) 
                .ToList();

            if (recomendacoes.Count == 0)
            {
                Console.WriteLine("Com esses filtros, não sobrou nenhum jogo no banco de dados :(");
            }
            else
            {
                foreach (var rec in recomendacoes)
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