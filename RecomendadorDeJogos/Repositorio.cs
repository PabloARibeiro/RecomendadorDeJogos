using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecomendadorDeJogos
{
    public class RepositorioJogos
    {
        private static readonly HttpClient carteiro = new HttpClient();

        public async Task<List<Jogo>> CarregarJogosApi()
        {
            string jsonString = string.Empty;

            try
            {
                Console.WriteLine("Conectando aos servidores da RAWG API...");
                
                // Para a API real funcionar no futuro, você precisará criar uma conta lá e trocar essa chave
                string urlRawg = "https://api.rawg.io/api/games?key=SUA_CHAVE_AQUI&page_size=50";
                
                HttpResponseMessage resposta = await carteiro.GetAsync(urlRawg);
                resposta.EnsureSuccessStatusCode(); 
                
                jsonString = await resposta.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                Console.WriteLine("Aviso: Chave da API ausente ou internet caiu. Usando dados DTO simulados da RAWG...");
                
                // Nosso Plano B: Um JSON exatamente no formato da RAWG para testarmos nosso tradutor!
                jsonString = @"{
                  ""results"": [
                    {
                      ""name"": ""The Witcher 3: Wild Hunt"",
                      ""released"": ""2015-05-18"",
                      ""genres"": [{ ""name"": ""Action"" }, { ""name"": ""RPG"" }],
                      ""tags"": [{ ""name"": ""Singleplayer"" }, { ""name"": ""Story Rich"" }, { ""name"": ""Open World"" }]
                    },
                    {
                      ""name"": ""Hollow Knight"",
                      ""released"": ""2017-02-24"",
                      ""genres"": [{ ""name"": ""Action"" }, { ""name"": ""Indie"" }, { ""name"": ""Platformer"" }],
                      ""tags"": [{ ""name"": ""Metroidvania"" }, { ""name"": ""Difficult"" }, { ""name"": ""2D"" }]
                    }
                  ]
                }";
            }

            // 1. Configuramos para ignorar se a API manda minúsculo (name) e a gente usa Maiúsculo (Name)
            var opcoesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // 2. Deserializamos a string bagunçada para o nosso DTO!
            RawgResponseDto dadosApi = JsonSerializer.Deserialize<RawgResponseDto>(jsonString, opcoesJson) ?? new RawgResponseDto();

            // 3. A FASE DE TRADUÇÃO (Mapeamento)
            List<Jogo> bancoDeJogosLimpo = new List<Jogo>();

            foreach (var jogoSujo in dadosApi.Results)
            {
                // A) Limpando a data (De "2015-05-18" para 2015)
                int anoTraduzido = 0;
                if (!string.IsNullOrEmpty(jogoSujo.Released) && jogoSujo.Released.Length >= 4)
                {
                    int.TryParse(jogoSujo.Released.Substring(0, 4), out anoTraduzido);
                }

                // B) Extraindo os textos das listas de objetos
                List<string> generosLimpos = jogoSujo.Genres.Select(g => g.Name).ToList();
                List<string> tagsLimpas = jogoSujo.Tags.Select(t => t.Name).ToList();

                // C) Criando a nossa classe final imaculada
                Jogo jogoLimpo = new Jogo
                {
                    Nome = jogoSujo.Name,
                    Ano = anoTraduzido,
                    Generos = generosLimpos,
                    // Vamos usar as Tags da API para popular nossos Temas e Estilos (já que a RAWG mistura tudo)
                    Temas = tagsLimpas.Take(3).ToList(), // Pega as 3 primeiras tags
                    EstilosVisuais = tagsLimpas.Skip(3).Take(2).ToList() // Pega a 4ª e 5ª tag
                };

                bancoDeJogosLimpo.Add(jogoLimpo);
            }

            return bancoDeJogosLimpo;
        }
    }
}