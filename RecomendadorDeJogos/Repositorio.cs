using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RecomendadorDeJogos
{
    public class RepositorioJogos
    {
        private static readonly HttpClient carteiro = new HttpClient();
        // COLOQUE SUAS CHAVES AQUI
        private readonly string clientId = string.Empty;
        private readonly string clientSecret = string.Empty;
        private string tokenAtual = string.Empty;

        // NOVO: O Construtor lê o arquivo json localmente e preenche as variáveis
        public RepositorioJogos()
        {
            try
            {
                string jsonString = System.IO.File.ReadAllText("keys.json");
                var chaves = JsonSerializer.Deserialize<ChavesDto>(jsonString);
                
                if (chaves != null)
                {
                    clientId = chaves.ClientId;
                    clientSecret = chaves.ClientSecret;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[Aviso do Sistema] Arquivo 'keys.json' não encontrado. A API pode falhar.");
            }
        }

        private async Task GarantirToken()
        {
            if (!string.IsNullOrEmpty(tokenAtual)) return;

            Console.WriteLine("[Sistema] Negociando acesso com a Twitch...");
            var request = new HttpRequestMessage(HttpMethod.Post, 
                $"https://id.twitch.tv/oauth2/token?client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials");

            var resposta = await carteiro.SendAsync(request);
            
            // TRUQUE DE VETERANO: Lemos o erro exato da Twitch se algo falhar!
            if (!resposta.IsSuccessStatusCode)
            {
                string erroTwitch = await resposta.Content.ReadAsStringAsync();
                throw new Exception($"Erro na Twitch ({resposta.StatusCode}): {erroTwitch}");
            }

            string jsonString = await resposta.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<TwitchTokenDto>(jsonString);
            tokenAtual = tokenData?.access_token ?? string.Empty;
        }

        public async Task<List<Jogo>> BuscarJogoPorNome(string nomePesquisa)
        {
            try
            {
                await GarantirToken();

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.igdb.com/v4/games");
                request.Headers.Add("Client-ID", clientId);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenAtual);

                string query = $"search \"{nomePesquisa}\"; fields name, first_release_date, genres.name, themes.name; limit 5;";
                request.Content = new StringContent(query, Encoding.UTF8, "text/plain");

                var resposta = await carteiro.SendAsync(request);

                // TRUQUE DE VETERANO: Lemos o erro exato da IGDB se algo falhar!
                if (!resposta.IsSuccessStatusCode)
                {
                    string erroIgdb = await resposta.Content.ReadAsStringAsync();
                    throw new Exception($"Erro na IGDB ({resposta.StatusCode}): {erroIgdb}\nA Query enviada foi: {query}");
                }

                string jsonString = await resposta.Content.ReadAsStringAsync();

                var opcoesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dadosApi = JsonSerializer.Deserialize<List<IgdbGameDto>>(jsonString, opcoesJson) ?? new List<IgdbGameDto>();

                List<Jogo> resultadosLimpos = new List<Jogo>();

                foreach (var jogoSujo in dadosApi)
                {
                    int anoTraduzido = 0;
                    if (jogoSujo.first_release_date.HasValue)
                    {
                        DateTimeOffset dataDateTime = DateTimeOffset.FromUnixTimeSeconds(jogoSujo.first_release_date.Value);
                        anoTraduzido = dataDateTime.Year;
                    }

                    var generosSujos = jogoSujo.genres ?? new List<IgdbItemDto>();
                    var temasSujos = jogoSujo.themes ?? new List<IgdbItemDto>();

                    resultadosLimpos.Add(new Jogo
                    {
                        Nome = jogoSujo.name,
                        Ano = anoTraduzido,
                        Generos = generosSujos.Select(g => g.name).ToList(),
                        Temas = temasSujos.Select(t => t.name).ToList(),
                        EstilosVisuais = new List<string>()
                    });
                }

                return resultadosLimpos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Detalhes do Erro] {ex.Message}");
                return new List<Jogo>();
            }
        }

        public async Task<List<Jogo>> BuscarCatalogoParaRecomendacao(int? anoMin, int? anoMax)
        {
            try
            {
                await GarantirToken();

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.igdb.com/v4/games");
                request.Headers.Add("Client-ID", clientId);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenAtual);

                // Vamos montar uma query inteligente. 
                // Pedimos jogos com mais de 50 avaliações para garantir que são jogos relevantes.
                string queryAnos = "";
                
                // Se o usuário já pediu um filtro de anos, a gente avisa a IGDB para não gastar internet baixando jogos velhos
                if (anoMin.HasValue) 
                {
                    long minUnix = new DateTimeOffset(new DateTime(anoMin.Value, 1, 1)).ToUnixTimeSeconds();
                    queryAnos += $" & first_release_date >= {minUnix}";
                }
                if (anoMax.HasValue) 
                {
                    long maxUnix = new DateTimeOffset(new DateTime(anoMax.Value, 12, 31)).ToUnixTimeSeconds();
                    queryAnos += $" & first_release_date <= {maxUnix}";
                }

                // Trazemos os 100 jogos mais bem avaliados que se encaixam no filtro de tempo!
                string query = $"fields name, first_release_date, genres.name, themes.name; where rating_count > 50 {queryAnos}; sort rating desc; limit 100;";
                
                request.Content = new StringContent(query, Encoding.UTF8, "text/plain");

                var resposta = await carteiro.SendAsync(request);
                resposta.EnsureSuccessStatusCode();

                string jsonString = await resposta.Content.ReadAsStringAsync();

                var opcoesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dadosApi = JsonSerializer.Deserialize<List<IgdbGameDto>>(jsonString, opcoesJson) ?? new List<IgdbGameDto>();

                // Reutilizamos a nossa exata lógica de tradução
                List<Jogo> catalogoLimpo = new List<Jogo>();
                foreach (var jogoSujo in dadosApi)
                {
                    int anoTraduzido = 0;
                    if (jogoSujo.first_release_date.HasValue)
                    {
                        anoTraduzido = DateTimeOffset.FromUnixTimeSeconds(jogoSujo.first_release_date.Value).Year;
                    }

                    var generosSujos = jogoSujo.genres ?? new List<IgdbItemDto>();
                    var temasSujos = jogoSujo.themes ?? new List<IgdbItemDto>();

                    catalogoLimpo.Add(new Jogo
                    {
                        Nome = jogoSujo.name,
                        Ano = anoTraduzido,
                        Generos = generosSujos.Select(g => g.name).ToList(),
                        Temas = temasSujos.Select(t => t.name).ToList(),
                        EstilosVisuais = new List<string>()
                    });
                }

                return catalogoLimpo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Erro ao buscar catálogo] {ex.Message}");
                return new List<Jogo>();
            }
        }
    }
}
