namespace RecomendadorDeJogos
{
    public class MotorRecomendacao
    {
        // Esta função faz todo o trabalho pesado!
        public List<Recomendacao> Gerar(
            List<Jogo> bancoDeJogos, 
            List<Jogo> jogosEscolhidos, 
            string generoExcluido, 
            int? anoMin, // O 'int?' significa que o ano pode ser nulo (se o usuário não preencheu)
            int? anoMax)
        {
            List<string> generosInteresse = jogosEscolhidos.SelectMany(j => j.Generos).Distinct().ToList();
            List<string> temasInteresse = jogosEscolhidos.SelectMany(j => j.Temas).Distinct().ToList();
            List<string> estilosInteresse = jogosEscolhidos.SelectMany(j => j.EstilosVisuais).Distinct().ToList();
            List<string> nomesEscolhidos = jogosEscolhidos.Select(j => j.Nome).ToList();

            var candidatos = bancoDeJogos.Where(j => !nomesEscolhidos.Contains(j.Nome));

            // Aplicando Filtros
            if (!string.IsNullOrWhiteSpace(generoExcluido))
            {
                candidatos = candidatos.Where(j => !j.Generos.Any(g => g.Equals(generoExcluido, StringComparison.OrdinalIgnoreCase)));
            }

            if (anoMin.HasValue) candidatos = candidatos.Where(j => j.Ano >= anoMin.Value);
            if (anoMax.HasValue) candidatos = candidatos.Where(j => j.Ano <= anoMax.Value);

            // Calculando Pontos e criando os objetos 'Recomendacao'
            return candidatos
                .Select(candidato => 
                {
                    var matchGeneros = candidato.Generos.Where(g => generosInteresse.Contains(g)).ToList();
                    var matchTemas = candidato.Temas.Where(t => temasInteresse.Contains(t)).ToList();
                    var matchEstilos = candidato.EstilosVisuais.Where(e => estilosInteresse.Contains(e)).ToList();

                    int scoreTotal = (matchGeneros.Count * 3) + (matchTemas.Count * 2) + (matchEstilos.Count * 1);

                    return new Recomendacao
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
        }
    }
}