using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    private static PlayerManager instance;

    [SerializeField] private float respawnTime = 3f;
    [SerializeField] private List<Transform> spawnPoints;

    private Dictionary<int, Player> _players = new();
    private List<int> _deadPlayers = new();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
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
        instance._players.Add(clientId, new Player());
    }

    public static void DeletePlayer(int clientId)
    {
        instance._players.Remove(clientId);
    }

    public static void PlayerDied(int player, int killer)
    {
        if (instance._players.TryGetValue(killer, out Player killerPlayer)) killerPlayer.score++;

        if (instance._players.TryGetValue(player, out Player deadPlayer)) deadPlayer.deathTime = Time.time;

        instance._deadPlayers.Add(player);
    }

    private class Player
    {
        public int score = 0;
        public float deathTime = -99;
    }
}
