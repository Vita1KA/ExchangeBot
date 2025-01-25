using System.Net.Http.Json;

public class ExchangeRateService
{
    private readonly HttpClient _httpClient;

    private const string PrivatBankApiUrl = "https://api.privatbank.ua/p24api/exchange_rates?json&date=";

    public ExchangeRateService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<decimal?> GetExchangeRate(string currencyCode, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
        {
            throw new ArgumentException("Код валюти повинен складатися з 3 символів.", nameof(currencyCode));
        }

        currencyCode = currencyCode.ToUpperInvariant();

        var requestUrl = $"{PrivatBankApiUrl}{date:dd.MM.yyyy}";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<PrivatBankApiResponse>(requestUrl);

            if (response?.ExchangeRate == null)
            {
                Console.WriteLine("Дані про курси валют відсутні.");
                return null;
            }

            var rate = response.ExchangeRate.FirstOrDefault(r => r.Currency == currencyCode);
            return rate?.SaleRateNB;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Помилка HTTP-запиту: {ex.Message}");
            throw new Exception("Не вдалося отримати дані з API ПриватБанку.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Неочікувана помилка: {ex.Message}");
            throw;
        }
    }
}
