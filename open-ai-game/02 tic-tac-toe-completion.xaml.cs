using Newtonsoft.Json;
using OpenAI.API;
using OpenAI.API.Completions;
using System;
using System.Text.Json;

namespace open_ai_game;

public partial class tic_tac_toe_completion : ContentPage
{

    enum GameStatusEnum { NotStarted, Playing, Tie, Winner }
    enum PlayerEnum { User, Assistant };
    PlayerEnum player = PlayerEnum.Assistant;
    GameStatusEnum gameStatus = GameStatusEnum.NotStarted;
    string[] playerSymbols = { "X", "O" };

    OpenAIAPI api = new OpenAIAPI(new APIAuthentication(MauiProgram.apiKey));
    Button[,] boardArray;
    List<Button> lstBoard;
    string apiPrompt;
    bool aiCompleteMove = true;
    public tic_tac_toe_completion()
    {
        InitializeComponent();
        BindingContext = this;
        SetUpGame();
    }
    private void SetUpGame()
    {
        boardArray = new Button[,]
        {
            { btn1, btn2, btn3 },
            { btn4, btn5, btn6 },
            { btn7, btn8, btn9 }
        };
        lstBoard = boardArray.Cast<Button>().ToList();
        lstBoard.ForEach(b => { b.Text = ""; b.IsEnabled = false; });
        apiPrompt = $"{apiSystemPrompt} \n\n{apiPromptStartGame}";
        DisplayGameStatus();
        lblStatus.Text = string.IsNullOrEmpty(MauiProgram.apiKey) ? "API key not found" : "";

    }
    private void StartGame()
    {
        if (gameStatus == GameStatusEnum.Playing)
        {
            SetUpGame();
            aiCompleteMove = true;
        }
        gameStatus = GameStatusEnum.Playing;
        lstBoard.ForEach(b => { b.Text = ""; b.IsEnabled = true; b.Clicked += spotButton_Click; });
        DisplayGameStatus();
        lblChat.Text = "";
        player = PlayerEnum.Assistant;
    }
    private void DoMove(Button btn = null)
    {
        if (gameStatus == GameStatusEnum.Playing)
        {
            player = player == PlayerEnum.User ? PlayerEnum.Assistant : PlayerEnum.User;
            if (player == PlayerEnum.User && btn.Text == "")
            {
                UserMove(btn);
            }
            else if (player == PlayerEnum.Assistant)
            {
                AIMoveAsync();
            }

        }
    }
    private void UserMove(Button btn)
    {
        btn.Text = playerSymbols[(int)player];
        int[] buttonIndex = GetButtonIndex(btn);

        if (buttonIndex != null)
        {
            int row = buttonIndex[0];
            int col = buttonIndex[1];

            boardArray[row, col] = btn;
        }
        CheckWinnerTie();
        DoMove();
    }
    private async Task AIMoveAsync()
    {
        lblStatus.Text = "AI processing";
        apiPrompt = apiPrompt == "" ? apiPromptMain : apiPrompt;
        aiCompleteMove = false;

        while (!aiCompleteMove)
        {
            GetChat(PlayerEnum.User, apiPrompt);

            var response = await CallAPIAsync();

            GetChat(PlayerEnum.Assistant, response.Completions[0].Text);

            var apiResponses = response.Completions[0].Text.Split(",");

            //validation
            if (apiResponses.Length == 2 && int.TryParse(apiResponses[0], out int parsedRow) && int.TryParse(apiResponses[1], out int parsedCol))
            {
                if (parsedRow >= 0 && parsedRow < 3 && parsedCol >= 0 && parsedCol < 3)
                {
                    if (boardArray[parsedRow, parsedCol].Text == "")
                    {
                        boardArray[parsedRow, parsedCol].Text = playerSymbols[(int)player];
                        CheckWinnerTie();
                        aiCompleteMove = true;
                        HandleError("AI just moved!", "");
                    }
                    else
                    {
                        HandleError("Incorrect move. Spot already taken. AI retrying...", apiPromptTakenSpot);
                        continue;
                    }
                }
                else
                {
                    HandleError("Incorrect move. Spot doesn't exist on the board. AI retrying...", apiPromptNotExistingSpot);
                    continue;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(response.Completions[0].Text))
                {
                    HandleError("No response. AI retrying...", apiPromptNoResponse);
                }
                else
                {
                    HandleError("Incorrect response. AI retrying...", apiPromptFormatWarning);
                }
                continue;
            }
        }
    }
    private async Task<CompletionResult> CallAPIAsync()
    {
        CompletionResult response = null;
        while (response == null)
        {
            try
            {
                var parameters = new CompletionRequest
                {
                    Model = "text-davinci-003",
                    Prompt = apiPrompt,
                    Temperature = 1.0,
                    MaxTokens = 800
                };

                response = await api.Completions.CreateCompletionAsync(parameters);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("TooManyRequests"))
                {
                    lblStatus.Text = "Exceeded maximum retries without success. Try again later....";
                }
                else
                {
                    lblStatus.Text = ex.Message;
                }
            }
        }
        return response;
    }
    private string GetTicTacToeJsonArray()
    {
        string[,] ticTacToeTextArray = new string[3, 3];
        for (int i = 0; i < boardArray.GetLength(0); i++)
        {
            for (int j = 0; j < boardArray.GetLength(1); j++)
            {
                ticTacToeTextArray[i, j] = boardArray[i, j].Text;
            }
        }
        return JsonConvert.SerializeObject(ticTacToeTextArray);
    }
    private int[] GetButtonIndex(Button button)
    {
        for (int row = 0; row < boardArray.GetLength(0); row++)
        {
            for (int col = 0; col < boardArray.GetLength(1); col++)
            {
                if (boardArray[row, col] == button)
                {
                    return new int[] { row, col };
                }
            }
        }
        // Button not found in the array
        return null;
    }
    private void GetChat(PlayerEnum player, string apiResponse = "")
    {
        string chat = "";
        switch (player)
        {
            case PlayerEnum.User:
                chat = $"{PlayerEnum.User}: \n{apiPrompt}";
                break;
            case PlayerEnum.Assistant:
                chat = $"{PlayerEnum.Assistant}: \n{apiResponse.Replace("\n", "")} \n\n -------------------------";
                break;
        }
        lblChat.Text = string.IsNullOrEmpty(lblChat.Text) ? chat : lblChat.Text += $"\n\n{chat}";
    }
    private void HandleError(string message, string prompt)
    {
        lblStatus.Text = message;
        apiPrompt = prompt;
    }
    private void CheckWinnerTie()
    {
        bool winner = false;
        bool tie = false;

        //check rows
        for (int row = 0; row < 3; row++)
        {
            if (boardArray[row, 0].Text != "" && boardArray[row, 0].Text == boardArray[row, 1].Text && boardArray[row, 1].Text == boardArray[row, 2].Text)
            {
                winner = true;
                break;
            }
        }
        //check columns
        for (int col = 0; col < 3 && winner == false; col++)
        {
            if (boardArray[0, col].Text != "" && boardArray[0, col].Text == boardArray[1, col].Text && boardArray[1, col].Text == boardArray[2, col].Text)
            {
                winner = true;
                break;
            }
        }
        if (!winner)
        {
            //check diagonals
            if (boardArray[0, 0].Text != "" && boardArray[0, 0].Text == boardArray[1, 1].Text && boardArray[1, 1].Text == boardArray[2, 2].Text)
            {
                winner = true;
            }
            else if (boardArray[0, 2].Text != "" && boardArray[0, 2].Text == boardArray[1, 1].Text && boardArray[1, 1].Text == boardArray[2, 0].Text)
            {
                winner = true;
            }
            //check tie
            else if (lstBoard.Exists(b => b.Text == "") == false)
            {
                tie = true;
            }
        }
        gameStatus = winner ? GameStatusEnum.Winner : tie ? GameStatusEnum.Tie : GameStatusEnum.Playing;
        DisplayGameStatus();
        if (winner || tie)
        {
            lstBoard.ForEach(b => { b.IsEnabled = false; });
        }
    }
    private void DisplayGameStatus()
    {
        string msg = "";
        switch (gameStatus)
        {
            case GameStatusEnum.NotStarted:
                msg = "Click start to begin game";
                break;
            case GameStatusEnum.Playing:

                msg = "Current turn: " + (player == PlayerEnum.User ? playerSymbols[(int)player + 1] : playerSymbols[(int)player - 1]);
                lblStatus.Text = "";
                break;
            case GameStatusEnum.Tie:
                msg = GameStatusEnum.Tie.ToString();
                break;
            case GameStatusEnum.Winner:
                msg = "Winner is: " + playerSymbols[(int)player];
                break;
        }
        lblTurn.Text = msg;
    }
    private void spotButton_Click(object? sender, EventArgs e)
    {
        if (sender is Button && aiCompleteMove)
        {
            DoMove((Button)sender);
        }
    }
    private void btnStart_Clicked(object sender, EventArgs e)
    {
        StartGame();
    }
    private string apiSystemPrompt { get => $"We're playing Tic Tac Toe!\nYou are 'O' and I am 'X'. Your goal is to win by getting three in a row, whether diagonally, vertically, or horizontally. Always respond in the format \"row, col\" without extra text (e.g., \"2, 2\").\nBe strategic, aim for victory, and block the user from winning.\nGood luck!"; }
    private string apiPromptStartGame { get => $"The current 3x3 game board is represented as a JSON 2D array, displaying blank spaces, 'X', and 'O':\n{GetTicTacToeJsonArray()}\nYour next move? Respond strictly in the format \"row, col\" (e.g. \"2, 2\")."; }
    private string apiPromptMain { get => $"Updated board state:\n{GetTicTacToeJsonArray()}\nYour next move? Reply strictly in the format \"row, col\" (e.g. \"2, 2\") without extra text!"; }
    private string apiPromptNotExistingSpot { get => "You've selected an invalid spot. Please pick a valid position within the 3x3 grid in the format: \"row, col\" among available spots.  (e.g. \"2, 2\")"; }
    private string apiPromptTakenSpot { get => "The spot you've chosen is already taken. Choose an empty position and provide your move as \"row, col\" from available spots.  (e.g. \"2, 2\")"; }
    private string apiPromptFormatWarning { get => $"Your response is in an incorrect format. Remember to reply strictly as \"row, col\". (e.g. \"2, 2\"). No additional text will be considered.\nUpdated board state:\n{GetTicTacToeJsonArray()}\nYour next move?"; }
    private string apiPromptNoResponse { get => $"You haven't provided an answer. Please give your move as \"row, col\". (e.g. \"2, 2\"). No additional text will be considered.\nUpdated board state:\n{GetTicTacToeJsonArray()}\nYour next move?"; }
}