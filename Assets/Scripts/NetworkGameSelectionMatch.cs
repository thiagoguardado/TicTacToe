﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NetworkGameSelectionMatch : MonoBehaviour {

    public NetworkMatchData thisMatchData;
    public Text matchName;
    public int maxCharsInName = 15;
    public List<Image> playerSymbols = new List<Image>();
    public PlayerSymbols possibleSymbols;

    public void Setup(NetworkMatchData _matchData)
    {
        thisMatchData = _matchData;

        SetName(thisMatchData.matchName);
        SetPlayers(thisMatchData.playersOnLobby.ToList());

    }

    private void SetName(string newName)
    {
        matchName.text = newName.Substring(0, Mathf.Min(newName.Length,maxCharsInName));
    }

    private void SetPlayers(List<NetworkPlayer> players)
    {
        foreach (Image image in playerSymbols)
        {
            image.gameObject.SetActive(false);
        }

        for (int i = 0; i < players.Count; i++)
        {
            playerSymbols[i].gameObject.SetActive(true);
            playerSymbols[i].sprite = possibleSymbols.GetSprite(players[i].playerSymbol);
            playerSymbols[i].color = players[i].color;


        }
    }

    public void OnClick()
    {
        MyNetworkManager myNetworkManager = FindObjectOfType<MyNetworkManager>();

        if(myNetworkManager!=null){
            if(GameManager.networkType == NetworkType.LAN)
            {
                myNetworkManager.ConnectToAMatchLAN(thisMatchData);
            } else {
                myNetworkManager.ConnectToAMatchInternet(thisMatchData);
            }
        }

    }

}
