﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NetworkGameLobby : NetworkBehaviour {

    public MyNetworkManager mynetworkManager { get; private set; }
    float timer = 1f;
    float refreshTime = 1f;

    private void Awake()
    {
        DontDestroyOnLoad(this);

    }


    void Start()
    {
        mynetworkManager = NetworkManager.singleton.GetComponent<MyNetworkManager>();
       
    }

    void Update()
    {

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = refreshTime;
            if (isServer)
            {
                ServerUpdate();
            }

            if (isClient)
            {
                ClientUpdate();
            }
        }       


    }

    private void ClientUpdate()
    {
        UpdatePlayersOnClient();
    }


    private void ServerUpdate()
    {
        UpdatePlayersNameOnServer();

    }

    private void UpdatePlayersNameOnServer()
    {

        RpcAskPlayerForName();

    }

    private void UpdatePlayersOnClient()
    {
        if(isLocalPlayer)
            CmdUpdateMatchData();
    }

    public void StartGameOnClients()
    {
        RpcStartGame(mynetworkManager.currentMatch, GameManager.boardSize);
    }


    [ClientRpc]
    private void RpcAskPlayerForName()
    {
        if(isLocalPlayer)
            CmdUpdatePlayerName(mynetworkManager.currentPlayer);
    }

    [Command]
    private void CmdUpdatePlayerName(NetworkPlayer player)
    {
        for (int i = 0; i < mynetworkManager.currentMatch.playersOnLobby.Length; i++)
        {
            if (mynetworkManager.currentMatch.playersOnLobby[i].connectionID == player.connectionID)
            {
                mynetworkManager.currentMatch.playersOnLobby[i].playerName = player.playerName;
                break;
            }
        }
    }


    [ClientRpc]
    private void RpcSendMatchDataToClients(NetworkMatchData currentMatchData)
    {
        if(!NetworkServer.active)
            mynetworkManager.currentMatch = currentMatchData;

        for (int i = 0; i < currentMatchData.playersOnLobby.Length; i++)
        {
            if (currentMatchData.playersOnLobby[i].connectionID == mynetworkManager.currentPlayer.connectionID)
            {
                mynetworkManager.currentPlayer.playerSymbol = currentMatchData.playersOnLobby[i].playerSymbol;
                mynetworkManager.currentPlayer.color = currentMatchData.playersOnLobby[i].color;
                break;
            }
        }
    }

    [Command]
    private void CmdUpdateMatchData()
    {
        RpcSendMatchDataToClients(mynetworkManager.currentMatch);
    }

    [ClientRpc]
    public void RpcChangeBoardSize(int newBoardSize)
    {
        MenuManager.Instance.EffectivateBoardSizeChange();
        GameManager.boardSize = newBoardSize;
    }

    [ClientRpc]
    private void RpcStartGame(NetworkMatchData matchData, int boardsize)
    {
        Debug.Log("Received RPC start game!");
        mynetworkManager.currentMatch = matchData;
        GameManager.boardSize = boardsize;
        GameManager.Instance.SetNetworkPlayers(matchData.playersOnLobby);
        GameManager.Instance.StartGame();
        AudioManager.Instance.PlayStartGameSFX();
    }
}

