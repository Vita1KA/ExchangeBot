using System.Net;
using System.Net.Http.Json;
using Moq;
using Moq.Protected;

public class ExchangeRateServiceTests
{
    [Fact]
    public async Task GetExchangeRate_Test()
    {
        var currencyCode = "USD";
        var date = new DateTime(2024, 1, 1);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains($"{date:dd.MM.yyyy}")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new PrivatBankApiResponse
                {
                    Date = $"{date:dd.MM.yyyy}",
                    ExchangeRate = new List<ExchangeRate>
                    {
                        new ExchangeRate { Currency = "USD", SaleRateNB = 27.5m }
                    }
                })
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new ExchangeRateService(httpClient);

        var result = await service.GetExchangeRate(currencyCode, date);

        Assert.NotNull(result);
        Assert.Equal(27.5m, result);
    }

    [Fact]
    public async Task GetExchangeRate_ShouldReturnNull_Test()
    {
        var currencyCode = "EUR";
        var date = new DateTime(2024, 1, 1);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new PrivatBankApiResponse
                {
                    Date = $"{date:dd.MM.yyyy}",
                    ExchangeRate = new List<ExchangeRate> { } 
                })
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new ExchangeRateService(httpClient);

        var result = await service.GetExchangeRate(currencyCode, date);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetExchangeRate_ShouldThrowException_Test()
    {
        var currencyCode = "USD";
        var date = new DateTime(2024, 1, 1);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError 
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new ExchangeRateService(httpClient);

        await Assert.ThrowsAsync<Exception>(() => service.GetExchangeRate(currencyCode, date));
    }
}