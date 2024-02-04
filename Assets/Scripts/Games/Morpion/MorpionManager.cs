using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MorpionManager : MonoBehaviour
{

    #region Unity Members
    public GameObject[] AllButtons;

    public Color TeamColor = Color.red;

    public ServerTCP server;
    public ClientTCP client;

    private List<bool> State = new List<bool>();
    #endregion

    public void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        foreach (GameObject button in AllButtons)
        {
            button.GetComponent<Button>().interactable = true;
            button.GetComponent<Image>().color = Color.white;
        }

        foreach (GameObject button in AllButtons)
        {
            State.Add(GetButtonState(button));
        }
        string data = ".DataGame_MorpionStateInfo_";
        foreach (bool state in State)
        {
            data += state.ToString() + "_";
        }
        data += GetIntColor() + "_";
        MorpionSendData(data);
        MorpionSendData(data);
        State.Clear();
    }

    public void ClickOnGrid(GameObject button)
    {
        button.GetComponent<Button>().interactable = false;
        button.GetComponent<Image>().color = TeamColor;

        foreach (GameObject b in AllButtons)
        {
            State.Add(GetButtonState(b));
        }
        string data = ".DataGame_MorpionStateInfo_";
        foreach (bool state in State)
        {
            data += state.ToString() + "_";
        }

        data += GetIntColor() + "_";

        MorpionSendData(data);
        MorpionSendData(data);
        State.Clear();
    }

    public bool GetButtonState(GameObject button)
    {
        if(button.GetComponent<Button>().interactable == false) return false;
        else return true;
    }

    public string GetIntColor()
    {
        if (TeamColor == Color.red) { return "color1"; }
        if (TeamColor == Color.green) { return "color2"; }
        if (TeamColor == Color.blue) { return "color3"; }
        else { return "color0"; }
    }

    public void MorpionSendData(string Data)
    {
        if (client.enabled)
        {
            client.SendDataGame(Data);
        }

        if (server.enabled)
        {
            server.SendDataGame(Data);
        }
    }

    public void MorpionRecvData(string Data)
    {
        string[] DataSplited = Data.Split('_');
        List<bool> NewState = new List<bool>();
        Color RecvColor = Color.white;
        if ("MorpionStateInfo" == DataSplited[1])
        {
            foreach (string d in DataSplited)
            {
                if (d == "True" || d == "False")
                {
                    NewState.Add(Convert.ToBoolean(d));
                }

                if (d == "color1") { RecvColor = Color.red; }
                if (d == "color2") { RecvColor = Color.green; }
                if (d == "color3") { RecvColor = Color.blue; }
            }

            int StateCount = 0;

            foreach (GameObject button in AllButtons)
            {
                if (button.GetComponent<Image>().color == Color.white) { button.GetComponent<Image>().color = RecvColor; }
                button.GetComponent<Button>().interactable = NewState[StateCount];
                StateCount += 1;
            }
        }
        NewState.Clear();
    }

    public void SetTeamColor(int color)
    {
        if (color == 1) { TeamColor = Color.red; }
        if (color == 2) { TeamColor = Color.green; }
        if (color == 3) { TeamColor = Color.blue; }
    }

    public void Update()
    {
        foreach (GameObject button in AllButtons)
        {
            if (GetButtonState(button)) { button.GetComponent<Image>().color = Color.white; }
        }
    }
}
