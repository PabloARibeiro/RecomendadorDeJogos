namespace RecomendadorDeJogos
{
    // A IGDB não manda aquele "results:" em volta. Ela manda a lista de jogos direto!
    
    public class IgdbGameDto
    {
        public string name { get; set; } = string.Empty;
        
        // A data vem em formato numérico (Unix Timestamp)
        public long? first_release_date { get; set; } 
        
        public List<IgdbItemDto> genres { get; set; } = new List<IgdbItemDto>();
        public List<IgdbItemDto> themes { get; set; } = new List<IgdbItemDto>();
    }

    // A IGDB usa o mesmo formato para Gêneros e Temas
    public class IgdbItemDto
    {
        public string name { get; set; } = string.Empty;
    }

    // Classe para receber o Token da Twitch
    public class TwitchTokenDto
    {
        public string access_token { get; set; } = string.Empty;
        public int expires_in { get; set; }
    }

    public class ChavesDto
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}