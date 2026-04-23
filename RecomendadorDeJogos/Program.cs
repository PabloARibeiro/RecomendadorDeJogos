using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RecomendadorDeJogos;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// Permite que o nosso futuro site HTML consiga conversar com esta API sem bloqueios de segurança
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTudo", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("PermitirTudo");

// Iniciamos nossas ferramentas
RepositorioJogos repo = new RepositorioJogos();
MotorRecomendacao motor = new MotorRecomendacao();

// --- NOSSOS "ENDPOINTS" (As portas que o site vai chamar) ---

// 1. Porta de Busca (O site chama: /api/buscar?nome=skyrim)
app.MapGet("/api/buscar", async (string nome) =>
{
    if (string.IsNullOrWhiteSpace(nome)) return Results.BadRequest("Nome não pode ser vazio.");
    
    var resultados = await repo.BuscarJogoPorNome(nome);
    return Results.Ok(resultados);
});

// 2. Porta de Recomendação
// (Aqui a gente usa o método POST, que recebe do HTML a lista dos jogos que o usuário escolheu)
app.MapPost("/api/recomendar", async (RequisicaoRecomendacao req) =>
{
    var catalogo = await repo.BuscarCatalogoParaRecomendacao(req.AnoMin, req.AnoMax);
    var recomendacoes = motor.Gerar(catalogo, req.JogosEscolhidos, req.GeneroExcluido, req.AnoMin, req.AnoMax);
    
    // Devolvemos apenas os top 5 para o Front-end
    return Results.Ok(recomendacoes.Take(5));
});

Console.WriteLine("=== API DO RECOMENDADOR RODANDO! ===");
app.Run();

// Classe auxiliar para receber os dados do site
public class RequisicaoRecomendacao
{
    public List<Jogo> JogosEscolhidos { get; set; } = new();
    public string? GeneroExcluido { get; set; }
    public int? AnoMin { get; set; }
    public int? AnoMax { get; set; }
}