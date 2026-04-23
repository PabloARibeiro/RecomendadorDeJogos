namespace RecomendadorDeJogos
{   
    public class IgdbGameDto
    {
        public string name { get; set; } = string.Empty;
        public long? first_release_date { get; set; } 
        public List<IgdbItemDto> genres { get; set; } = new List<IgdbItemDto>();
        public List<IgdbItemDto> themes { get; set; } = new List<IgdbItemDto>();
        public IgdbCoverDto cover { get; set; } 
    }

    public class IgdbCoverDto
    {
        public string url { get; set; } = string.Empty;
    }

    public class IgdbItemDto
    {
        public string name { get; set; } = string.Empty;
    }


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