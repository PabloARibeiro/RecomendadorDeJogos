namespace RecomendadorDeJogos
{
    public class Jogo
    {
        public string Nome { get; set; } = string.Empty;
        public int Ano { get; set; }
        public List<string> Generos { get; set; } = new List<string>();
        public List<string> Temas { get; set; } = new List<string>();
        public List<string> EstilosVisuais { get; set; } = new List<string>();
        public string ImagemUrl { get; set; } = string.Empty;
    }

    // NOVA CLASSE: Para transportar o resultado do Motor para a Tela
    public class Recomendacao
    {
        public Jogo Item { get; set; } = new Jogo();
        public int Pontos { get; set; }
        public List<string> MatchesGen { get; set; } = new List<string>();
        public List<string> MatchesTema { get; set; } = new List<string>();
        public List<string> MatchesEst { get; set; } = new List<string>();
    }
}