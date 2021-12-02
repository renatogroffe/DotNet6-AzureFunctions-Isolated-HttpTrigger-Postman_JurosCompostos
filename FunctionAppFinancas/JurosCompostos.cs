using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using FunctionAppFinancas.Models;

namespace FunctionAppFinancas;

public class JurosCompostos
{
    private readonly ILogger _logger;

    public JurosCompostos(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<JurosCompostos>();
    }

    [Function(nameof(JurosCompostos))]
    [OpenApiOperation(operationId: nameof(JurosCompostos), tags: new[] { "Juros" })]
    [OpenApiParameter(name: "valorEmprestimo", In = ParameterLocation.Query, Required = true, Type = typeof(double), Description = "Valor do Empr�stimo")]
    [OpenApiParameter(name: "numMeses", In = ParameterLocation.Query, Required = true, Type = typeof(double), Description = "N�mero de Meses para pagamento")]
    [OpenApiParameter(name: "percTaxa", In = ParameterLocation.Query, Required = true, Type = typeof(double), Description = "Percentual da Taxa de Juros mensal")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Emprestimo), Description = "Valor do Empr�stimo")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(FalhaCalculo), Description = "Falha nos c�lculos do Empr�stimo")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")]
            HttpRequestData req, double? valorEmprestimo, int? numMeses, double? percTaxa)
    {
        _logger.LogInformation(
            "Recebida nova requisi��o|" +
           $"Valor do empr�stimo: {valorEmprestimo}|" +
           $"N�mero de meses: {numMeses}|" +
           $"% Taxa de Juros: {percTaxa}");

        // FIXME: C�digo comentado para simula��o de falhas em testes automatizados
        /*if (valorEmprestimo <= 0)
            return GerarResultParamInvalido("Valor do Empr�stimo", req);
        if (numMeses <= 0)
            return GerarResultParamInvalido("N�mero de Meses", req);
        if (percTaxa <= 0)
            return GerarResultParamInvalido("Percentual da Taxa de Juros", req);*/

        var valorFinalJuros =
            CalculoFinanceiro.CalcularValorComJurosCompostos(
                valorEmprestimo, numMeses, percTaxa);
        _logger.LogInformation($"Valor Final com Juros: {valorFinalJuros}");

        var response = req.CreateResponse();
        response.WriteAsJsonAsync(new Emprestimo()
        {
            valorEmprestimo = valorEmprestimo.Value,
            numMeses = numMeses.Value,
            taxaPercentual = percTaxa.Value,
            valorFinalComJuros = valorFinalJuros
        });
        return response;
    }

    private HttpResponseData GerarResultParamInvalido(
        string nomeCampo, HttpRequestData req)
    {
        var erro = $"O {nomeCampo} deve ser maior do que zero!";
        _logger.LogError(erro);

        var response = req.CreateResponse();
        response.WriteAsJsonAsync(new FalhaCalculo() { mensagem = erro });
        response.StatusCode = HttpStatusCode.BadRequest;
        return response;
    }
}