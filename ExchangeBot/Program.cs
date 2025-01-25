using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Program
{
    private static readonly string BotToken = "7608346129:AAEg7l6EKua2mLE3A_p51FLdc5xBxbORbXM";

    static async Task Main(string[] args)
    {
        var botClient = new TelegramBotClient(BotToken);
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        botClient.StartReceiving(Update, Error, receiverOptions);

        var botInfo = await botClient.GetMeAsync();
        Console.WriteLine($"{botInfo.FirstName} is up and running :)");
        Console.ReadLine();
    }

    private static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { Text: { } messageText })
            return;

        var chatId = update.Message.Chat.Id;
        var inputParts = messageText.Split(' ');

        if (inputParts.Length != 2)
        {
            await botClient.SendTextMessageAsync(chatId, "Please enter input in the format: `<Currency Code> <Date>` (e.g., `USD 01.01.2024`).");
            return;
        }

        var currencyCode = inputParts[0].ToUpper();
        var dateString = inputParts[1];

        if (!DateTime.TryParseExact(dateString, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            await botClient.SendTextMessageAsync(chatId, "Invalid date format. Use `dd.MM.yyyy` (e.g., `27.01.2024`).");
            return;
        }

        try
        {
            var exchangeRateService = new ExchangeRateService(new HttpClient());
            var exchangeRate = await exchangeRateService.GetExchangeRate(currencyCode, date);

            if (exchangeRate.HasValue)
            {
                await botClient.SendTextMessageAsync(chatId, $"Exchange rate for {currencyCode} to UAH on {date:dd.MM.yyyy}: {exchangeRate:F2} UAH");
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, $"No exchange rate data available for {currencyCode} on {date:dd.MM.yyyy}.");
            }
        }
        catch (Exception ex)
        {
            await botClient.SendTextMessageAsync(chatId, $"An error occurred: {ex.Message}");
        }
    }

    private static Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
}
