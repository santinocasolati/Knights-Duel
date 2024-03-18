using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : NetworkBehaviour
{
    private static PlayerManager _instance;

    [SerializeField] private float respawnTime = 3f;
    [SerializeField] private List<Transform> spawnPoints;

    private Dictionary<int, Player> _players = new();
    private List<int> _deadPlayers = new();

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
    }

    private void Update()
    {
        for (int i = 0; i < _deadPlayers.Count; i++)
        {
            if (_players[_deadPlayers[i]].deathTime < Time.time - respawnTime)
            {
                RespawnPlayer(_deadPlayers[i]);
                _deadPlayers.RemoveAt(i);
                return;
            }
        }
    }

    private void RespawnPlayer(int clientId)
    {
        PlayerController.TogglePlayer(clientId, true);
        PlayerController.RespawnPlayer(clientId, spawnPoints[Random.Range(0, spawnPoints.Count)].position);
    }

    public static void InitializeNewPlayer(int clientId)
    {
        _instance._players.Add(clientId, new Player());
    }

    public static void DeletePlayer(int clientId)
    {
        _instance._players.Remove(clientId);
    }

    public static void PlayerDied(int player, int killer)
    {
        if (_instance._players.TryGetValue(killer, out Player killerPlayer))
        {
            killerPlayer.score++;
            // Sending the score to the GameManager to check win conditions
            GameManager.CheckScore(killer, killerPlayer.score);
        }

        if (_instance._players.TryGetValue(player, out Player deadPlayer)) deadPlayer.deathTime = Time.time;

        // By adding dead players to a list, the Update method can loop them and check if the death cooldown has passed
        _instance._deadPlayers.Add(player);
    }
}

public class Player
{
    public int score = 0;
    public float deathTime = -99;
}
