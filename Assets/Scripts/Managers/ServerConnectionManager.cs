using FishNet.Transporting.Tugboat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerConnectionManager : MonoBehaviour
{
    [SerializeField] private Tugboat _tugboat;

    public void HostGame()
    {
        _tugboat.StartConnection(true);
        JoinGame();
    }

    public void JoinGame()
    {
        _tugboat.StartConnection(false);
    }
}
