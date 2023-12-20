namespace open_ai_game;
using OpenAI.API;
using Microsoft.Extensions.Configuration;
using OpenAI.API.Completions;

public partial class open_ai_api : ContentPage
{
    private OpenAIAPI client;
    public string promptText { get; set; }
    public string generatedText { get; set; }
    public open_ai_api()
    {
        InitializeComponent();
        client = new OpenAIAPI(MauiProgram.apiKey);
        lblGeneratedText.Text = string.IsNullOrEmpty(MauiProgram.apiKey) ? "API key not found" : "";
    }

    private async Task GenerateText()
    {
        var parameters = new CompletionRequest
        {
            Model = "text-davinci-002",
            Prompt = txtGeneratedText.Text,
            Temperature = 0.7,
            MaxTokens = 3000
        };

        var response = await client.Completions.CreateCompletionAsync(parameters);
        //iterated thru the array and showed all results
        generatedText = response.Completions[0].Text;
        lblGeneratedText.Text = generatedText;
    }

    private async void btnGenerate_Clicked(object sender, EventArgs e)
    {
        await GenerateText();
    }
}