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

    [SerializeField] private Sprite[] playerSprites;
    [SerializeField] private Button[] gridSpaces; //The spaces in the grid that can be clicked
    [SerializeField] private GameObject[] turnIcons; //Displays whose turn it is
    [SerializeField] private GameObject winningPanel;
    [SerializeField] private TMP_Text winningText;
    [SerializeField] private GameObject[] winningLines; //All the lines for the possible winning outcomes
    [SerializeField] private TMP_Text xScoreTxt, oScoreTxt;

    private Turn turnIndicator;
    private int turnsCounter; //Counts the number of turns played this game
    private int playerXScore, playerOScore;
    private int[] filledSpaces; //ID's which space is filled by which player
    private bool isPlayerTurn = true;

    void Start()
    {
        GameSetup();
    }

    void GameSetup()
    {
        turnIndicator = Turn.PLAYER; //X always starts at ticTacToe
        turnsCounter = 0;
        turnIcons[0].SetActive(true);
        turnIcons[1].SetActive(false);
        isPlayerTurn = false;

        foreach (var space in gridSpaces)
        {
            space.interactable = true;
            space.GetComponent<Image>().sprite = null;
        }
        filledSpaces = new int[gridSpaces.Length];
        for (int i = 0; i < filledSpaces.Length; i++)
        {
            filledSpaces[i] = -100; //Set it to -100 instead of 0, because 0 means player X filled it in
        }
    }

    void ChangeTurn()
    {
        turnIndicator = (turnIndicator == Turn.PLAYER) ? Turn.AI : Turn.PLAYER;

        isPlayerTurn = (turnIndicator == Turn.PLAYER);
        turnIcons[0].SetActive(isPlayerTurn);
        turnIcons[1].SetActive(!isPlayerTurn);
    }

    public void TicTacToeSpaceClicked(int gridNumber)
    {
        gridSpaces[gridNumber].image.sprite = playerSprites[(int)turnIndicator];
        gridSpaces[gridNumber].interactable = false;

        filledSpaces[gridNumber] = (int)turnIndicator +1; //Add +1 to prevent logic errors
        turnsCounter++;
        if (turnsCounter > 4)
        {
            bool didPlayerWin = WinCheck(filledSpaces, turnIndicator, false);
            if(turnsCounter >= 9 && !didPlayerWin)
            {
                winningText.text = "DRAW";
                winningPanel.gameObject.SetActive(true);
                return;
            }
            else if(didPlayerWin)
            {
                return;
            }
        }
        ChangeTurn();
        UpdateGridInteractability();

        if (turnIndicator == Turn.AI)
            StartCoroutine(AIMove());
    }

    IEnumerator AIMove()
    {
        yield return new WaitForSeconds(2);

        // Find the index of the best move for the AI
        int bestMoveIndex = 0;
        int bestMoveScore = int.MinValue;
        for (int i = 0; i < filledSpaces.Length; i++)
        {
            if (filledSpaces[i] < 0) //space is empty
            {
                filledSpaces[i] = (int)Turn.AI + 1;
                int moveScore = MiniMax(filledSpaces, turnIndicator, -1000, 1000);
                filledSpaces[i] = -100;
                Debug.Log("move score : " + moveScore);
                if (moveScore > bestMoveScore)
                {
                    bestMoveScore = moveScore;
                    bestMoveIndex = i;
                }
            }
        }
        Debug.Log("best score : " + bestMoveScore);
        TicTacToeSpaceClicked(bestMoveIndex);
    }

    int MiniMax(int[] gameState, Turn player, int alpha, int beta)
    {
        if (WinCheck(gameState, Turn.AI, true))
        {
            return 1;
        }
        else if (WinCheck(gameState, Turn.PLAYER, true))
        {
            return -1;
        }
        else if (IsBoardFull(gameState))
        {
            return 0;
        }

        int score;
        int playerMark = (player == Turn.PLAYER) ? ((int)Turn.PLAYER + 1) : ((int)Turn.AI + 1);

        if(player == Turn.AI)
        {
            for (int i = 0; i < gameState.Length; i++)
            {
                if (gameState[i] < 0) // Space is empty
                {
                    gameState[i] = playerMark;
                    score = MiniMax(gameState, Turn.PLAYER, alpha, beta);
                    gameState[i] = -100;

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
            for (int i = 0; i < gameState.Length; i++)
            {
                if (gameState[i] < 0) // Space is empty
                {
                    gameState[i] = playerMark;
                    score = MiniMax(gameState, Turn.AI, alpha, beta);
                    gameState[i] = -100;

                    if (score < beta)
                        beta = score;

                    if (alpha > beta)
                        break;
                }
            }
            return beta;
        }
    }

    bool WinCheck(int[] gameState, Turn player, bool checkAIScore)
    {
        int playerMark = (player == Turn.PLAYER) ? ((int)Turn.PLAYER + 1) : ((int)Turn.AI + 1);

        int solution1 = gameState[0] + gameState[1] + gameState[2];
        int solution2 = gameState[3] + gameState[4] + gameState[5];
        int solution3 = gameState[6] + gameState[7] + gameState[8];
        int solution4 = gameState[0] + gameState[3] + gameState[6];
        int solution5 = gameState[1] + gameState[4] + gameState[7];
        int solution6 = gameState[2] + gameState[5] + gameState[8];
        int solution7 = gameState[0] + gameState[4] + gameState[8];
        int solution8 = gameState[2] + gameState[4] + gameState[6];
        var solutions = new int[] { solution1, solution2, solution3, solution4, solution5, solution6, solution7, solution8 };
        for (int i = 0; i < solutions.Length; i++)
        {
            if (solutions[i] == 3 * playerMark)
            {
                if(!checkAIScore)
                    WinnerDisplay(i);
                return true;
            }
        }

        return false;
    }

    bool IsBoardFull(int[] gameState)
    {
        foreach (int space in gameState)
        {
            if (space < 0)
            {
                return false;
            }
        }

        return true;
    }

    void WinnerDisplay(int indexSolution)
    {
        if (turnIndicator == Turn.PLAYER)
        {
            winningText.text = "Player X has won the game!";
            playerXScore++;
            xScoreTxt.text = playerXScore.ToString();
        }
        else if (turnIndicator == Turn.AI)
        {
            winningText.text = "Play O has won the game!";
            playerOScore++;
            oScoreTxt.text = playerOScore.ToString();
        }
        winningPanel.gameObject.SetActive(true);

        winningLines[indexSolution].SetActive(true);

        isPlayerTurn = false;
        UpdateGridInteractability();
    }

    // Call this method to update the interactability of the grid spaces
    public void UpdateGridInteractability()
    {
        for (int i = 0; i < filledSpaces.Length; i++)
        {
            if (filledSpaces[i] < 0) //space is empty
            {
                gridSpaces[i].interactable = isPlayerTurn;
            }
        }
    }

    public void Rematch()
    {
        GameSetup();
        foreach (var line in winningLines)
        {
            line.SetActive(false);
        }
        winningPanel.SetActive(false);
    }

    public void Restart()
    {
        Rematch();
        playerOScore = 0;
        playerXScore = 0;
        xScoreTxt.text = "0";
        oScoreTxt.text = "0";
    }
}
