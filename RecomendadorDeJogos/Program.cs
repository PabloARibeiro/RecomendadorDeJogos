namespace RecomendadorDeJogos
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== BEM-VINDO AO RECOMENDADOR DE JOGOS V8 (Busca Dinâmica IGDB) ===");
            
            RepositorioJogos repo = new RepositorioJogos();
            List<Jogo> jogosEscolhidos = new List<Jogo>();

            while (jogosEscolhidos.Count < 5)
            {
                Console.Write($"\nDigite o nome do Jogo {jogosEscolhidos.Count + 1}/5 (ou Enter para pular): ");
                string inputUsuario = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(inputUsuario))
                {
                    if (jogosEscolhidos.Count >= 1) break;
                    Console.WriteLine("Veterano avisa: Digite pelo menos 1 jogo!");
                    continue;
                }

                Console.WriteLine("Pesquisando na Twitch/IGDB...");
                
                // Agora o programa faz uma viagem à internet para buscar EXATAMENTE o que o usuário digitou
                List<Jogo> resultadosBusca = await repo.BuscarJogoPorNome(inputUsuario);

                if (resultadosBusca.Count == 0)
                {
                    Console.WriteLine("[-] Nenhum jogo encontrado com esse nome. Tente novamente.");
                    continue;
                }

                // Se achou, mostramos as opções para o usuário confirmar
                Console.WriteLine("Encontramos estes resultados. Qual deles é o seu?");
                for (int i = 0; i < resultadosBusca.Count; i++)
                {
                    Console.WriteLine($"[{i + 1}] {resultadosBusca[i].Nome} ({resultadosBusca[i].Ano})");
                }
                Console.WriteLine("[0] Nenhum desses (Cancelar)");

                Console.Write("Escolha um número: ");
                string escolhaStr = Console.ReadLine() ?? string.Empty;

                if (int.TryParse(escolhaStr, out int escolhaIndex) && escolhaIndex > 0 && escolhaIndex <= resultadosBusca.Count)
                {
                    Jogo jogoSelecionado = resultadosBusca[escolhaIndex - 1];

                    // Verifica se já não tinha adicionado
                    if (!jogosEscolhidos.Any(j => j.Nome == jogoSelecionado.Nome))
                    {
                        jogosEscolhidos.Add(jogoSelecionado);
                        Console.WriteLine($"[+] '{jogoSelecionado.Nome}' adicionado à sua lista!");
                    }
                    else
                    {
                        Console.WriteLine("[-] Você já adicionou esse jogo à lista!");
                    }
                }
                else
                {
                    Console.WriteLine("Busca cancelada. Vamos tentar outro.");
                }
            }

            Console.WriteLine("\n=== FILTROS DE RECOMENDAÇÃO ===");
            Console.Write("Gênero a excluir (Enter para pular): ");
            string generoExcluido = Console.ReadLine() ?? string.Empty;

            Console.Write("Ano Mínimo (Enter para ignorar): ");
            string inputAnoMin = Console.ReadLine() ?? string.Empty;
            int? anoMin = int.TryParse(inputAnoMin, out int min) ? min : (int?)null;
            
            Console.Write("Ano Máximo (Enter para ignorar): ");
            string inputAnoMax = Console.ReadLine() ?? string.Empty;
            int? anoMax = int.TryParse(inputAnoMax, out int max) ? max : (int?)null;

            Console.WriteLine("\n=== CALCULANDO RECOMENDAÇÕES ===");

            /* * ATENÇÃO AQUI: Como não baixamos o banco inteiro, precisamos buscar 
             * na API jogos semelhantes. Por hora, como já estouramos o limite de
             * arquitetura, o seu Motor.cs exigiria que a gente fizesse uma NOVA consulta
             * na API pedindo recomendações, e não cruzando dados locais.
             */
            
            Console.WriteLine("\n=== BUSCANDO CATÁLOGO DE CANDIDATOS NA NUVEM ===");
            // 1. Pedimos pro Repositório trazer os 100 melhores jogos da API
            List<Jogo> bancoCandidatos = await repo.BuscarCatalogoParaRecomendacao(anoMin, anoMax);

            if (bancoCandidatos.Count == 0)
            {
                Console.WriteLine("Não conseguimos buscar o catálogo na internet. Tente novamente mais tarde.");
                return;
            }

            Console.WriteLine($"[+] {bancoCandidatos.Count} jogos de alto nível recebidos da IGDB.");
            Console.WriteLine("\n=== CALCULANDO RECOMENDAÇÕES ===");

            // 2. A Mágica da Arquitetura: O nosso velho motor processa os dados novos!
            MotorRecomendacao motor = new MotorRecomendacao();
            List<Recomendacao> resultados = motor.Gerar(bancoCandidatos, jogosEscolhidos, generoExcluido, anoMin, anoMax);

            // Exibimos os top 5 resultados para o usuário não ser inundado de texto
            var topResultados = resultados.Take(5).ToList();

            if (topResultados.Count == 0)
            {
                Console.WriteLine("Com os seus jogos e filtros, nosso motor não encontrou nada compatível neste catálogo :(");
            }
            else
            {
                foreach (var rec in topResultados)
                {
                    Console.WriteLine($"\n> {rec.Item.Nome} ({rec.Item.Ano}) (Match: {rec.Pontos} pontos)");
                    if(rec.MatchesGen.Any()) Console.WriteLine($"  [Gêneros em comum]: {string.Join(", ", rec.MatchesGen)}");
                    if(rec.MatchesTema.Any()) Console.WriteLine($"  [Temas em comum]: {string.Join(", ", rec.MatchesTema)}");
                }
            }
            
            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
        }
    }
}