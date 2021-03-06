﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls game configuration menu screen appearence
/// </summary>
public class MenuView : MonoBehaviour {

    public MenuPlayerBox playerBoxPrefab;
    public Text numberOfPlayers;
    public Image numberOfPlayersGreaterSign;
    public Image numberOfPlayersLesserSign;
    public Text boardSize;
    public Image boardSizeGreaterSign;
    public Image boardSizeLesserSign;
    public Color signColorWhenDisabled;
    public Transform menuPlayerBoxesCenter;
    private List<MenuPlayerBox> menuPlayerBoxes = new List<MenuPlayerBox>();


    public void UpdateView()
    {
        UpdateBoardSize();
        UpdateNumberOfPlayers();
        UpdateMenuPlayerBoxes();

    }

    private void UpdateBoardSize()
    {
        boardSize.text = GameManager.boardSize.ToString();

        // checkButtons
        CheckLesserAndGreaterSigns(GameManager.boardSize, MenuManager.Instance.PossibleBoardSize, boardSizeLesserSign,boardSizeGreaterSign);

    }

    private void CheckLesserAndGreaterSigns(int currentNumber, int[] possibleNumbers, Image numberLesserSign, Image numberGreaterSign)
    {
        if (currentNumber == possibleNumbers[0])
        {
            numberLesserSign.color = signColorWhenDisabled;
            numberGreaterSign.color = Color.white;

        }
        else if (currentNumber == possibleNumbers[possibleNumbers.Length-1])
        {
            numberLesserSign.color = Color.white;
            numberGreaterSign.color = signColorWhenDisabled;

        }
        else
        {
            numberLesserSign.color = Color.white;
            numberGreaterSign.color = Color.white;
        }
    }


    private void UpdateNumberOfPlayers()
    {
        numberOfPlayers.text = GameManager.numberOfPlayers.ToString();

        // check buttons
        CheckLesserAndGreaterSigns(GameManager.numberOfPlayers, MenuManager.Instance.PossibleNumberOfPlayers, numberOfPlayersLesserSign, numberOfPlayersGreaterSign);
    }

    private void UpdateMenuPlayerBoxes()
    {
        // check if number of player boxes is equal to players
        if (menuPlayerBoxes.Count != GameManager.numberOfPlayers)
        {
            CreateNewPlayerBoxes();
        }

        for (int i = 0; i < GameManager.numberOfPlayers; i++)
        {
            Player p = GameManager.players[i];
            menuPlayerBoxes[i].Setup(p.playerSymbolAndSprite.playerSprite, p.color, p.playerType,i,this);

        }
    }

    private void CreateNewPlayerBoxes()
    {
        for (int i = 0; i < menuPlayerBoxes.Count; i++)
        {
            Destroy(menuPlayerBoxes[i].gameObject);
        }

        menuPlayerBoxes.Clear();

        for (int i = 0; i < GameManager.numberOfPlayers; i++)
        {
            MenuPlayerBox go = Instantiate(playerBoxPrefab, menuPlayerBoxesCenter.position,Quaternion.identity,menuPlayerBoxesCenter);
            go.transform.localPosition = new Vector3((i - ((float)GameManager.numberOfPlayers-1) / 2f) * 450f, 0, 0);
            menuPlayerBoxes.Add(go);
        }

    }
}
