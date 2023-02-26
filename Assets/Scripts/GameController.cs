using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Tooltip("0 = player X and 1 = player O")]
    [SerializeField] private int turnIndicator;
    [SerializeField] private Sprite[] playerSprites;
    [SerializeField] private Button[] gridSpaces; //The spaces in the grid that can be clicked
    [SerializeField] private GameObject[] turnIcons; //Displays whose turn it is
    [SerializeField] private GameObject winningPanel;
    [SerializeField] private TMP_Text winningText;
    [SerializeField] private GameObject[] winningLines; //All the lines for the possible winning outcomes
    [SerializeField] private TMP_Text xScoreTxt, oScoreTxt;

    private int turnsCounter; //Counts the number of turns played this game
    private int playerXScore, playerOScore;
    private int[] filledSpaces; //ID's which space is filled by which player

    void Start()
    {
        GameSetup();
    }

    void GameSetup()
    {
        turnIndicator = 0; //X always starts at ticTacToe
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
            filledSpaces[i] = -100; //Set it to -100 instead of 0, because 0 means player X filled it in
        }
    }

    public void TicTacToeSpaceClicked(int gridNumber)
    {
        gridSpaces[gridNumber].image.sprite = playerSprites[turnIndicator];
        gridSpaces[gridNumber].interactable = false;

        filledSpaces[gridNumber] = turnIndicator+1; //Add +1 to prevent logic errors
        turnsCounter++;
        if (turnsCounter > 4)
        {
            bool didPlayerWin = WinCheck();
            if(turnsCounter >= 9 && !didPlayerWin)
            {
                winningText.text = "DRAW";
                winningPanel.gameObject.SetActive(true);
            }
        }

        // Change turns
        if (turnIndicator == 0)
        {
            turnIndicator = 1;
            turnIcons[0].SetActive(false);
            turnIcons[1].SetActive(true);
        }
        else
        {
            turnIndicator = 0;
            turnIcons[0].SetActive(true);
            turnIcons[1].SetActive(false);
        }
    }

    bool WinCheck()
    {
        int solution1 = filledSpaces[0] + filledSpaces[1] + filledSpaces[2];
        int solution2 = filledSpaces[3] + filledSpaces[4] + filledSpaces[5];
        int solution3 = filledSpaces[6] + filledSpaces[7] + filledSpaces[8];
        int solution4 = filledSpaces[0] + filledSpaces[3] + filledSpaces[6];
        int solution5 = filledSpaces[1] + filledSpaces[4] + filledSpaces[7];
        int solution6 = filledSpaces[2] + filledSpaces[5] + filledSpaces[8];
        int solution7 = filledSpaces[0] + filledSpaces[4] + filledSpaces[8];
        int solution8 = filledSpaces[0] + filledSpaces[4] + filledSpaces[6];
        var solutions = new int[] { solution1, solution2, solution3, solution4, solution5, solution6, solution7, solution8 };
        for (int i = 0; i < solutions.Length; i++)
        {
            if (solutions[i] == 3 * (turnIndicator + 1))
            {
                WinnerDisplay(i);
                return true;
            }
        }

        return false;
    }

    void WinnerDisplay(int indexSolution)
    {
        if (turnIndicator == 0)
        {
            winningText.text = "Player X has won the game!";
            playerXScore++;
            xScoreTxt.text = playerXScore.ToString();
        }
        else
        {
            winningText.text = "Play O has won the game!";
            playerOScore++;
            oScoreTxt.text = playerOScore.ToString();
        }
        winningPanel.gameObject.SetActive(true);

        winningLines[indexSolution].SetActive(true);

        foreach (var space in gridSpaces)
        {
            space.interactable = false;
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
