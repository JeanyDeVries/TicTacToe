using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private enum Turn
    {
        PLAYER,
        AI
    }

    private enum Difficulty
    { 
        EASY,
        NORMAL,
        UNBEATABLE
    }

    [SerializeField] private Difficulty difficulty; //The difficulty of the AI
    [SerializeField] private Sprite[] playerSprites;
    [SerializeField] private Button[] gridSpaces; //The spaces in the grid that can be clicked
    [SerializeField] private GameObject[] turnIcons; //Displays whose turn it is
    [SerializeField] private GameObject[] winningLines; //All the lines for the possible winning outcomes
    [SerializeField] private GameObject winningPanel;
    [SerializeField] private TMP_Text winningText;
    [SerializeField] private TMP_Text xScoreTxt, oScoreTxt;

    private Turn turnIndicator; //A variable that keeps track whose turn it is
    private int turnsCounter; //Counts the number of turns played this game
    private int playerXScore, playerOScore;
    private const int EMPTY_SPACE = -100;  //Set the index for an empty space to -100, because 0 is the index of the player
    private const float RANDOM_AMOUNT_EASY = 0.3f; //Set a random noise amount for the AI to make less optimal moves
    private const float RANDOM_AMOUNT_NORMAL = 0.1f; //Set a random noise amount for the AI to make less optimal moves
    private int[] filledSpaces; //ID's which space is filled by which player

    void Start()
    {
        GameSetup();
    }

    /// <summary>
    /// A setup for the game, which sets the UI correct and set some values to the correct starting point.
    /// </summary>
    void GameSetup()
    {
        turnIndicator = Turn.PLAYER; //X (the player) always starts at ticTacToe
        turnsCounter = 0;
        turnIcons[0].SetActive(true);
        turnIcons[1].SetActive(false);

        foreach (var space in gridSpaces)
        {
            space.interactable = true;
            space.GetComponent<Image>().sprite = null;
        }
        filledSpaces = new int[gridSpaces.Length];
        for (int i = 0; i < filledSpaces.Length; i++)
        {
            filledSpaces[i] = EMPTY_SPACE;
        }
    }

    /// <summary>
    /// When a space is clicked, change the space with the turnIndicator and switch turns.
    /// </summary>
    /// <param name="gridNumber">This is the number of the buttons which are in the grid, the first one is 0 and the last one is 8.</param>
    public void TicTacToeSpaceClicked(int gridNumber)
    {
        gridSpaces[gridNumber].image.sprite = playerSprites[(int)turnIndicator]; //Set the image to the correct player sprite
        gridSpaces[gridNumber].interactable = false;

        filledSpaces[gridNumber] = (int)turnIndicator +1; //Add plus 
        turnsCounter++;
        if (turnsCounter > 4) //Only after 4 turns is it possible to win
        {
            if (CheckDraw() || WinCheck(turnIndicator, false))
            {
                UpdateGridInteractability(); //set the grid interactabily one last time, so that the non interactable buttons show
                return; //if there is a draw or a win, no need to run the rest of the code
            }
        }
        ChangeTurn();
        UpdateGridInteractability();

        if (turnIndicator == Turn.AI)
            StartCoroutine(AIMove());
    }

    /// <summary>
    /// Sets the move of what the AI should do
    /// </summary>
    IEnumerator AIMove()
    {
        yield return new WaitForSeconds(1);

        CalculateBestOption();
    }

    /// <summary>
    /// Change turns. 
    /// </summary>
    void ChangeTurn()
    {
        turnIndicator = (turnIndicator == Turn.PLAYER) ? Turn.AI : Turn.PLAYER;

        turnIcons[0].SetActive(turnIndicator == Turn.PLAYER);
        turnIcons[1].SetActive(turnIndicator == Turn.AI);
    }


    #region AI calculations

    /// <summary>
    /// Calculates the best possible position using the minimax algorithm
    /// </summary>
    void CalculateBestOption()
    {
        // Find the index of the best move for the AI
        int bestScore = -1, bestPos = -1, score;
        for (int i = 0; i < filledSpaces.Length; i++)
        {
            if (filledSpaces[i] < 0) //space is empty
            {
                filledSpaces[i] = (int)Turn.AI + 1;
                score = MiniMax(Turn.PLAYER, -1000, 1000);
                filledSpaces[i] = EMPTY_SPACE;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPos = i;
                }
                else if (score < bestScore) 
                {
                    // add some randomness to the move by occasionally making a suboptimal move      
                    if (difficulty == Difficulty.EASY && Random.Range(0f, 1f) < RANDOM_AMOUNT_EASY)
                        bestPos = i;                  
                    if(difficulty == Difficulty.NORMAL && Random.Range(0f, 1f) < RANDOM_AMOUNT_NORMAL)
                        bestPos = i; 
                }
            }
        }

        if (bestPos <= -1) //Check this and do a random option, for if something went wrong in the code (this is just a fallback)
        {
            CalculateRandomOption();
            return;
        }


        TicTacToeSpaceClicked(bestPos);
    }

    /// <summary>
    /// Calculates a score per route of the game. The score would be higher if it is more beneficial for the AI.
    /// </summary>
    /// <param name="player">The turn of the user, player or AI.</param>
    /// <param name="alpha">The score of the best optimal option.</param>
    /// <param name="beta">The score of the least optimal option.</param>
    /// <returns>Returns a score of the outcome of the current option.</returns>
    int MiniMax(Turn player, int alpha, int beta)
    {
        if (WinCheck(Turn.AI, false))
        {
            return +100;
        }
        else if (WinCheck(Turn.PLAYER, false))
        {
            return -100;
        }
        else if (IsBoardFull())
        {
            return 0;
        }

        int score;
        int playerMark = (player == Turn.PLAYER) ? ((int)Turn.PLAYER + 1) : ((int)Turn.AI + 1);

        if (player == Turn.AI)
        {

            for (int i = 0; i < filledSpaces.Length; i++)
            {
                if (filledSpaces[i] < 0) // Space is empty
                {
                    filledSpaces[i] = playerMark;
                    score = MiniMax(Turn.PLAYER, alpha, beta);
                    filledSpaces[i] = EMPTY_SPACE;

                    if (score > alpha)
                        alpha = score;

                    if (alpha > beta)
                        break;
                }
            }
            return alpha;
        }
        else
        {
            for (int i = 0; i < filledSpaces.Length; i++)
            {
                if (filledSpaces[i] < 0) // Space is empty
                {
                    filledSpaces[i] = playerMark;
                    score = MiniMax(Turn.AI, alpha, beta);
                    filledSpaces[i] = EMPTY_SPACE;

                    if (score < beta)
                        beta = score;

                    if (alpha > beta)
                        break;
                }
            }
            return beta;
        }
    }

    /// <summary>
    /// Calculates a random possible option and places the AI move there
    /// </summary>
    void CalculateRandomOption()
    {
        List<int> optionalOptions = new List<int>();
        for (int i = 0; i < filledSpaces.Length; i++)
        {
            if (filledSpaces[i] < 0) //space is empty
            {
                optionalOptions.Add(i);
            }
        }

        int randomSpace = Random.Range(0, optionalOptions.Count);
        TicTacToeSpaceClicked(optionalOptions[randomSpace]);
    }

    #endregion

    #region Board status checks

    /// <summary>
    /// Checks if the last move caused the game to be finished with a win. 
    /// </summary>
    /// <param name="player">The turn of the user, player or AI.</param>
    /// <param name="checkAIScore">A check to see if this method is called for a check for the AI, if not then it is a true win of the player or AI.</param>
    /// <returns>Returns true when there is a win.</returns>
    bool WinCheck(Turn player, bool showWinnerDisplay)
    {
        int playerMark = (player == Turn.PLAYER) ? ((int)Turn.PLAYER + 1) : ((int)Turn.AI + 1);

        int solution1 = filledSpaces[0] + filledSpaces[1] + filledSpaces[2];
        int solution2 = filledSpaces[3] + filledSpaces[4] + filledSpaces[5];
        int solution3 = filledSpaces[6] + filledSpaces[7] + filledSpaces[8];
        int solution4 = filledSpaces[0] + filledSpaces[3] + filledSpaces[6];
        int solution5 = filledSpaces[1] + filledSpaces[4] + filledSpaces[7];
        int solution6 = filledSpaces[2] + filledSpaces[5] + filledSpaces[8];
        int solution7 = filledSpaces[0] + filledSpaces[4] + filledSpaces[8];
        int solution8 = filledSpaces[2] + filledSpaces[4] + filledSpaces[6];
        var solutions = new int[] { solution1, solution2, solution3, solution4, solution5, solution6, solution7, solution8 };
        for (int i = 0; i < solutions.Length; i++)
        {
            if (solutions[i] == 3 * playerMark)
            {
                if (showWinnerDisplay)
                    WinnerDisplay(i);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks to see if there has been a draw in the game.
    /// </summary>
    /// <returns>Returns true when there is a draw in the game.</returns>
    bool CheckDraw()
    {
        bool didPlayerWin = WinCheck(turnIndicator, true);
        if (turnsCounter < 9 || didPlayerWin) return false; //When the player won or all the spaces are not filled in yet, return false (no draw)

        winningText.text = "DRAW";
        winningPanel.gameObject.SetActive(true);
        return true;
    }

    /// <summary>
    /// Checks if all spaces are filled in the game. 
    /// </summary>
    /// <returns>Returns true all spaces are filled.</returns>
    bool IsBoardFull()
    {
        foreach (int space in filledSpaces)
        {
            if (space < 0)
            {
                return false;
            }
        }

        return true;
    }

    #endregion

    #region UI 

    /// <summary>
    /// This method is to show the UI of the winner. 
    /// </summary>
    void WinnerDisplay(int indexSolution)
    {
        if (turnIndicator == Turn.PLAYER)
        {
            winningText.text = "You won the game!";
            playerXScore++;
            xScoreTxt.text = playerXScore.ToString();
        }
        else if (turnIndicator == Turn.AI)
        {
            winningText.text = "The AI won the game!";
            playerOScore++;
            oScoreTxt.text = playerOScore.ToString();
        }
        winningPanel.gameObject.SetActive(true);

        winningLines[indexSolution].SetActive(true);

        UpdateGridInteractability();
    }

    /// <summary>
    /// This method is to update the interactability of the grid spaces
    /// </summary>
    public void UpdateGridInteractability()
    {
        for (int i = 0; i < filledSpaces.Length; i++)
        {
            if (filledSpaces[i] < 0) //space is empty
            {
                gridSpaces[i].interactable = (turnIndicator == Turn.PLAYER);

                ChangeInteractableAlpha(gridSpaces[i], 0);
            }
            else
            {
                ChangeInteractableAlpha(gridSpaces[i], 1);
            }
        }
    }

    /// <summary>
    /// Sets the alpha of the non interactable button.
    /// </summary>
    /// <param name="button">The button in the grid.</param>
    /// <param name="alpha">The alpha amount we want to set to the button.</param>
    void ChangeInteractableAlpha(Button button, int alpha)
    {
        ColorBlock colors = button.colors;
        Color disabledColor = colors.disabledColor;
        disabledColor.a = alpha;
        colors.disabledColor = disabledColor;
        button.colors = colors;
    }

    /// <summary>
    /// This method sets all the settings to restart the round
    /// </summary>
    public void Rematch(int difficulty) //Cannot set it to enum diffuculty so int is the only way
    {
        switch (difficulty)
        {
            case 0:
                this.difficulty = Difficulty.EASY;
                break;
            case 1:
                this.difficulty = Difficulty.NORMAL;
                break;
            case 2:
                this.difficulty = Difficulty.UNBEATABLE;
                break;
            default:
                break;
        }
        GameSetup();
        foreach (var line in winningLines)
        {
            line.SetActive(false);
        }
        winningPanel.SetActive(false);
    }

    /// <summary>
    /// This method sets all the settings to restart the whole game
    /// /// </summary>
    public void Restart()
    {
        Rematch((int)difficulty); //Do a reset with the current difficulty
        playerOScore = 0;
        playerXScore = 0;
        xScoreTxt.text = "0";
        oScoreTxt.text = "0";
    }

    #endregion
}
