using System;
using System.Collections.Generic;
using System.Linq;

namespace RecomendadorDeJogos
{
    // 1. A classe Jogo evoluiu. Adeus 'Tags' genéricas, olá Categorias!
    public class Jogo
    {
        public string Nome { get; set; }
        public List<string> Generos { get; set; }
        public List<string> Temas { get; set; }
        public List<string> EstilosVisuais { get; set; }

        public Jogo(string nome, List<string> generos, List<string> temas, List<string> estilosVisuais)
        {
            Nome = nome;
            Generos = generos;
            Temas = temas;
            EstilosVisuais = estilosVisuais;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 2. Nosso Banco de Dados agora é muito mais detalhado
            List<Jogo> bancoDeJogos = new List<Jogo>
            {
                new Jogo("The Witcher 3", 
                    new List<string> { "RPG", "Ação" }, 
                    new List<string> { "Mundo Aberto", "Fantasia", "História" }, 
                    new List<string> { "3D", "Realista" }),
                    
                new Jogo("Skyrim", 
                    new List<string> { "RPG", "Ação" }, 
                    new List<string> { "Mundo Aberto", "Fantasia", "Mods" }, 
                    new List<string> { "3D", "Realista" }),
                    
                new Jogo("Hollow Knight", 
                    new List<string> { "Metroidvania", "Plataforma" }, 
                    new List<string> { "Exploração", "Difícil" }, 
                    new List<string> { "2D", "Desenhado à Mão" }),
                    
                new Jogo("Dark Souls", 
                    new List<string> { "RPG", "Ação" }, 
                    new List<string> { "Fantasia Escura", "Difícil" }, 
                    new List<string> { "3D", "Realista" }),
                    
                new Jogo("Stardew Valley", 
                    new List<string> { "Simulação", "RPG" }, 
                    new List<string> { "Fazenda", "Relaxante" }, 
                    new List<string> { "2D", "Pixel Art" }),
                    
                new Jogo("Celeste", 
                    new List<string> { "Plataforma" }, 
                    new List<string> { "História", "Difícil" }, 
                    new List<string> { "2D", "Pixel Art" })
            };

            Console.WriteLine("=== BEM-VINDO AO RECOMENDADOR DE JOGOS V2 ===");
            Console.WriteLine("Digite de 1 a 5 jogos para basearmos sua recomendação.");
            Console.WriteLine("Deixe em branco e aperte Enter para finalizar.\n");

            List<Jogo> jogosEscolhidos = new List<Jogo>();

            while (jogosEscolhidos.Count < 5)
            {
                Console.Write($"Jogo {jogosEscolhidos.Count + 1}/5: ");
                string inputUsuario = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(inputUsuario))
                {
                    if (jogosEscolhidos.Count >= 1) break;
                    Console.WriteLine("Veterano avisa: Digite pelo menos 1 jogo!");
                    continue;
                }

                Jogo jogoEncontrado = bancoDeJogos.FirstOrDefault(j => j.Nome.Equals(inputUsuario, StringComparison.OrdinalIgnoreCase));

                if (jogoEncontrado != null)
                {
                    if (!jogosEscolhidos.Contains(jogoEncontrado))
                    {
                        jogosEscolhidos.Add(jogoEncontrado);
                        Console.WriteLine($"[+] '{jogoEncontrado.Nome}' adicionado!");
                    }
                    else Console.WriteLine("Você já adicionou esse jogo!");
                }
                else Console.WriteLine("[-] Jogo não encontrado. Tente: The Witcher 3, Skyrim, Hollow Knight, Dark Souls, Stardew Valley, Celeste.");
            }

            Console.WriteLine("\n=== CALCULANDO RECOMENDAÇÕES ===");

            // 3. Extraindo os interesses separadamente por categoria
            List<string> generosInteresse = jogosEscolhidos.SelectMany(j => j.Generos).Distinct().ToList();
            List<string> temasInteresse = jogosEscolhidos.SelectMany(j => j.Temas).Distinct().ToList();
            List<string> estilosInteresse = jogosEscolhidos.SelectMany(j => j.EstilosVisuais).Distinct().ToList();

            List<string> nomesEscolhidos = jogosEscolhidos.Select(j => j.Nome).ToList();

            // 4. O Novo Motor com Sistema de Pesos (Weights)
            var recomendacoes = bancoDeJogos
                .Where(j => !nomesEscolhidos.Contains(j.Nome)) 
                .Select(candidato => 
                {
                    // Encontrando os matches de cada categoria
                    var matchGeneros = candidato.Generos.Where(g => generosInteresse.Contains(g)).ToList();
                    var matchTemas = candidato.Temas.Where(t => temasInteresse.Contains(t)).ToList();
                    var matchEstilos = candidato.EstilosVisuais.Where(e => estilosInteresse.Contains(e)).ToList();

                    // Aplicando a Matemática dos Pesos: Gênero vale 3x, Tema 2x, Estilo 1x
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

            // 5. Exibição Detalhada (A Explicabilidade)
            if (recomendacoes.Count == 0)
            {
                Console.WriteLine("Não achamos nada compatível :(");
            }
            else
            {
                foreach (var rec in recomendacoes)
                {
                    Console.WriteLine($"\n> {rec.Item.Nome} (Match: {rec.Pontos} pontos)");
                    
                    if(rec.MatchesGen.Any()) 
                        Console.WriteLine($"  [Gêneros em comum]: {string.Join(", ", rec.MatchesGen)}");
                    if(rec.MatchesTema.Any()) 
                        Console.WriteLine($"  [Temas em comum]: {string.Join(", ", rec.MatchesTema)}");
                    if(rec.MatchesEst.Any()) 
                        Console.WriteLine($"  [Estilo visual em comum]: {string.Join(", ", rec.MatchesEst)}");
                }
            }
            
            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
        }
    }
}