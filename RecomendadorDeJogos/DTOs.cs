using System.Collections.Generic;

namespace RecomendadorDeJogos
{
    // 1. A API devolve um objeto principal que contém uma lista chamada "results"
    public class RawgResponseDto
    {
        public List<RawgGameDto> Results { get; set; } = new List<RawgGameDto>();
    }

    // 2. Cada jogo dentro de "results" vem nesse formato
    public class RawgGameDto
    {
        public string Name { get; set; } = string.Empty;
        public string Released { get; set; } = string.Empty; // O ano vem misturado: "2015-05-18"
        public List<RawgGenreDto> Genres { get; set; } = new List<RawgGenreDto>();
        public List<RawgTagDto> Tags { get; set; } = new List<RawgTagDto>();
    }

    // 3. Os gêneros não são textos simples, são objetos com "id" e "name"
    public class RawgGenreDto
    {
        public string Name { get; set; } = string.Empty;
    }

    // 4. As tags também são objetos
    public class RawgTagDto
    {
        public string Name { get; set; } = string.Empty;
    }
}