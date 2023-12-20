using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace open_ai_game;

public static class MauiProgram
{
   public static string apiKey = "";

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../secret-appsettings.json");

        string jsonString = File.ReadAllText(path);

        JsonDocument jsonDocument = JsonDocument.Parse(jsonString);

        if (jsonDocument.RootElement.TryGetProperty("OpenAI", out JsonElement openAIProperty))
        {
            if (openAIProperty.TryGetProperty("ApiKey", out JsonElement apiKeyProperty))
            {
                apiKey = apiKeyProperty.GetString();
            }
        }
        jsonDocument.Dispose();


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

