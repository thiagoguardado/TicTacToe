﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class MyNetworkManager : NetworkManager {


    public static MyNetworkDiscovery Discovery
    {
        get
        {
            return singleton.GetComponent<MyNetworkDiscovery>();
        }
    }

    public static bool isBroadcasting = false;

    public PlayerSymbols possibleSymbols;
    public NetworkMatchData currentMatch;
    public Dictionary<NetworkConnection,int> currentConnections = new Dictionary<NetworkConnection,int>();
    public string playerName = "Player";
    public NetworkPlayer currentPlayer;
    private int nextPlayerID = 0;

    public bool isConnected { get; private set; }

    
    private void Start()
    {
        isConnected = false;
    }

    //private void Update()
    //{

    //}

    #region Internet
    public void StartLookingForMatchesOnInternet()
    {
        throw new NotImplementedException();
    }
    #endregion

    #region LAN
    public void StartLookingForMatchesOnLAN()
    {
        Discovery.InitializeAsClient();
    }

    public void StartBroadcastingNewMatch(string newMatchName)
    {
        currentMatch = new NetworkMatchData(newMatchName, singleton.networkAddress);
        singleton.StartHost();
        isConnected = true;

        UnityEngine.SceneManagement.SceneManager.LoadScene("NetworkGameLobby");

    }

    public void ConnectToAMatch(NetworkMatchData matchData)
    {

        singleton.networkAddress = matchData.serverAddress;
        singleton.StartClient();
        isConnected = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("NetworkGameLobby");
    }
    #endregion


    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        if (!NetworkServer.active)
        {
            Discovery.MyStopBroadcast();
        }

        currentPlayer = new NetworkPlayer(playerName, -1);
    }


    public override void OnClientDisconnect(NetworkConnection conn)
    {

        base.OnClientDisconnect(conn);

        Debug.Log("Server disconnected");

        currentPlayer = null;

        ClientDisconnectAll();

        GameManager.Instance.ReturnToTitleScreen();

    }

    public override void OnServerConnect(NetworkConnection conn)
    {

        currentMatch.AddPlayer(new NetworkPlayer(ChooseRandomNewPlayerSymbol(), nextPlayerID));
        currentConnections.Add(conn, nextPlayerID);
        nextPlayerID++;

        base.OnServerConnect(conn);

        StartCoroutine(RefreshBroadcastInfo());

        NetworkGameLobbyView netView = GetComponent<NetworkGameLobbyView>();
        if (netView != null)
        {
            netView.UpdateView();
        }

        if (currentMatch.playersOnLobby.Length >= 4)
        {
            //Stop broadcast
            Discovery.MyStopBroadcast();
        }

    }


    public override void OnServerDisconnect(NetworkConnection conn)
    {

        // if in game, remove player from current match and change to next player
        if (GameManager.GameState == GameState.GameStarted)
        {
            NetworkPlayer disconnectedPlayer = new NetworkPlayer();
            for (int i = 0; i < currentMatch.playersOnLobby.Length; i++)
            {
                if (currentMatch.playersOnLobby[i].playerID == currentConnections[conn])
                {
                    disconnectedPlayer = currentMatch.playersOnLobby[i];
                    break;
                }
            }

            /*/
            if (BoardManager.Instance.Board.CurrentPlayer.playerSymbol == disconnectedPlayer.playerSymbol)
            {
                BoardManager.Instance.AddPlayerToBoard(-1);
            }
            /*/

            // remove player from game
            BoardManager.Instance.TriggerPlayerRemoval(disconnectedPlayer.playerSymbol);

        }
        
        currentMatch.RemovePlayer(currentConnections[conn]);
        currentConnections.Remove(conn);

        base.OnServerDisconnect(conn);

        if (currentMatch.playersOnLobby.Length <4)
        {
            //restart broadcast
            StartCoroutine(RefreshBroadcastInfo());
        }

        //foreach (var netID in FindObjectsOfType<NetworkIdentity>())
        //{
        //    if (netID.netId.Value == conn.connectionId) Destroy(netID.gameObject);
        //}

    }

    private IEnumerator RefreshBroadcastInfo()
    {
        Discovery.MyStopBroadcast();
        Discovery.broadcastData = NetworkMatchData.CreateMatchBroadcastData(currentMatch);
        yield return null;
        Discovery.RestartAsServer();
    }

    public static void ClientDisconnectAll()
    {
        singleton.StopClient();
        singleton.StopHost();
        singleton.StopMatchMaker();
        Network.Disconnect();
    }

    public static void ServerDiscconectAll()
    {
        singleton.StopClient();
        singleton.StopHost();
        singleton.StopMatchMaker();
        Discovery.MyStopBroadcast();
        Network.Disconnect();
    }

    public PlayerSymbol ChooseRandomNewPlayerSymbol()
    {
        PlayerSymbol[] currentSymbols = new PlayerSymbol[currentMatch.playersOnLobby.Length];
        for (int i = 0; i < currentMatch.playersOnLobby.Length; i++)
        {
            currentSymbols[i] = currentMatch.playersOnLobby[i].playerSymbol;
        }

        return GameManager.FindDifferentPlayerSymbol(currentSymbols);
    }
}
