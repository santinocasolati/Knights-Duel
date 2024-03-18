using FishNet.Object;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerConnectionManager : MonoBehaviour
{
    private static ServerConnectionManager _instance;

    [SerializeField] private Tugboat _tugboat;
    [SerializeField] private GameObject mainMenu;

    private bool isHost = false;

    // The functionality of this class it to only handle connections. This methods are called by the main menu UI mostly

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
    }

    public void HostGame()
    {
        _tugboat.StartConnection(true);
        isHost = true;
        JoinGame();
    }

    public void JoinGame()
    {
        GameManager.ResetGame();
        _tugboat.StartConnection(false);
        mainMenu.SetActive(false);
    }

    public void ExitGame()
    {
        _tugboat.StopConnection(false);
        if (isHost) _tugboat.StopConnection(true);
    }
}
