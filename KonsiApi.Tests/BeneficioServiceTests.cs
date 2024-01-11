using Xunit;
using Moq;
using KonsiApi.Services;
using System.Threading.Tasks;
using KonsiApi.Models;
using KonsiApi.Repositories;
using Microsoft.Extensions.Options;

public class BeneficioServiceTests
{
    [Fact]
    public async Task BuscarDadosBeneficio_RetornaDadosQuandoCpfValido()
    {
        var mockHttpClient = new Mock<HttpClient>();
        var mockConfiguration = new Mock<IOptions<ExternalApiConfig>>();
        var elasticsearchService = new Mock<ElasticsearchService>(mockConfiguration.Object);
        var beneficioService = new BeneficioService(mockHttpClient.Object, mockConfiguration.Object, elasticsearchService.Object);

        var mockRepo = new Mock<ICpfRepository>();


        //Faltaram a configuração dos mocks para retornar dados específicos ou comportamentos

        var result = await beneficioService.BuscarDadosBeneficio(new Cpf { Numero = "12345678901" });
        Assert.NotNull(result);
    }
}
