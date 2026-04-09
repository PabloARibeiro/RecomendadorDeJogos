using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecomendadorDeJogos
{
    public class RepositorioJogos
    {
        private static readonly HttpClient carteiro = new HttpClient();

        // COLOQUE SUAS CHAVES AQUI
        private readonly string clientId = "";
        private readonly string clientSecret = "";

        // Função 1: Pegar o Crachá (Token) na Twitch
        private async Task<string> PegarTokenTwitch()
        {
            Console.WriteLine("Pedindo autorização para a Twitch...");
            
            // Requisição POST para a Twitch
            var request = new HttpRequestMessage(HttpMethod.Post, 
                $"https://id.twitch.tv/oauth2/token?client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials");

            var resposta = await carteiro.SendAsync(request);
            resposta.EnsureSuccessStatusCode();

            string jsonString = await resposta.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<TwitchTokenDto>(jsonString);

            return tokenData?.access_token ?? string.Empty;
        }

        // Função 2: A nossa função principal que o Program.cs vai chamar
        public async Task<List<Jogo>> CarregarJogosApi()
        {
            try
            {
                // 1. Pegamos o token
                string token = await PegarTokenTwitch();

                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("Não foi possível gerar o Token da Twitch.");
                }

                Console.WriteLine("Conectando aos servidores da IGDB...");

                // 2. Preparamos a requisição POST para a IGDB
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.igdb.com/v4/games");
                
                // Colocamos o Client-ID e o Token (Bearer) no cabeçalho (Header) como a IGDB exige
                request.Headers.Add("Client-ID", clientId);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 3. O "Corpo" do texto: A linguagem da IGDB! 
                // Pedimos o nome, a data, e que ele 'expanda' os IDs de gêneros e temas para nos dar os nomes.
                string query = "fields name, first_release_date, genres.name, themes.name; limit 100;";
                request.Content = new StringContent(query, Encoding.UTF8, "text/plain");

                // 4. Enviamos e lemos a resposta
                var resposta = await carteiro.SendAsync(request);
                resposta.EnsureSuccessStatusCode();

                string jsonString = await resposta.Content.ReadAsStringAsync();

                // 5. A FASE DE TRADUÇÃO DTO -> JOGO
                var opcoesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dadosApi = JsonSerializer.Deserialize<List<IgdbGameDto>>(jsonString, opcoesJson) ?? new List<IgdbGameDto>();

                List<Jogo> bancoDeJogosLimpo = new List<Jogo>();

                foreach (var jogoSujo in dadosApi)
                {
                    // Convertendo a data maluca da IGDB (Unix Timestamp) para um Ano normal
                    int anoTraduzido = 0;
                    if (jogoSujo.first_release_date.HasValue)
                    {
                        // Magia de veterano: Converte os segundos desde 1970 para um objeto DateTime do C#
                        DateTimeOffset dataDateTime = DateTimeOffset.FromUnixTimeSeconds(jogoSujo.first_release_date.Value);
                        anoTraduzido = dataDateTime.Year;
                    }

                    // Prevenindo nulls se o jogo não tiver gênero ou tema cadastrado
                    var generosSujos = jogoSujo.genres ?? new List<IgdbItemDto>();
                    var temasSujos = jogoSujo.themes ?? new List<IgdbItemDto>();

                    Jogo jogoLimpo = new Jogo
                    {
                        Nome = jogoSujo.name,
                        Ano = anoTraduzido,
                        Generos = generosSujos.Select(g => g.name).ToList(),
                        // O Estilo Visual não tem uma categoria clara na IGDB (é misturado em Temas), 
                        // então deixamos vazio ou podemos pegar os Temas também. Vamos jogar tudo em Temas por enquanto.
                        Temas = temasSujos.Select(t => t.name).ToList(),
                        EstilosVisuais = new List<string>() 
                    };

                    bancoDeJogosLimpo.Add(jogoLimpo);
                }

                return bancoDeJogosLimpo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na API: {ex.Message}");
                Console.WriteLine("Por favor, verifique suas chaves da Twitch e sua conexão.");
                return new List<Jogo>();
            }
        }
    }
}
