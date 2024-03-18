using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : NetworkBehaviour
{
    private static GameManager _instance;

    [SerializeField] private TMPro.TMP_Text scoreText;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

    public UnityEvent OnGameFinished;

    public int scoreToWin = 3;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
    }

    public static void ResetGame()
    {
        _instance.scoreText.text = 0.ToString();
    }

    public static void CheckScore(int scorerId, int score)
    {
        PlayerController clientPlayer = PlayerController.GetClientPlayer();

        if (clientPlayer == null) return;

        if (scorerId == clientPlayer.OwnerId)
        {
            // Update the score UI
            _instance.scoreText.text = score.ToString();
        }

        // This part of the code checks the win condition. In this case, is the times a player has killed other players
        if (score == _instance.scoreToWin)
        {
            _instance.HandleGameFinish(scorerId == clientPlayer.OwnerId);
        }
    }

    private void HandleGameFinish(bool isWinner)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _instance.OnGameFinished.Invoke();
       
        if (isWinner)
        {
            _instance.winScreen.SetActive(true);
        } else
        {
            _instance.loseScreen.SetActive(true);
        }
    }
}
