using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public ClientTCP client;
    public ServerTCP server;
    public MorpionManager morpion;

    public Text ServIP;
    public Text Console;
    public Text ServerMessage;
    public Text ClientMessage;
    public Text NickNameText;

    public GameObject[] HideOnConnectToServer;
    public GameObject[] HideOnBeTheServer;

    public GameObject[] ShowOnConnectToServer;
    public GameObject[] ShowOnBeTheServer;

    public RectTransform ConsoleContent;

    public string Data;

    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        foreach (GameObject g in ShowOnConnectToServer)
        {
            g.SetActive(false);
        }
        
        foreach (GameObject g in ShowOnBeTheServer)
        {
            g.SetActive(false);
        }
        
        foreach (GameObject g in HideOnBeTheServer)
        {
            g.SetActive(true);
        }
        
        foreach (GameObject g in HideOnConnectToServer)
        {
            g.SetActive(true);
        }
    }

    public void Host()
    {
        server.enabled = true;

        foreach (GameObject g in HideOnBeTheServer)
        {
            g.SetActive(false);
        }
            
        foreach (GameObject g in ShowOnBeTheServer)
        {
            g.SetActive(true);
        }
    }

    public void Conect()
    {
        if (ServIP.text != "")
        {
            client.enabled = true;
            client.IP = ServIP.text;

            foreach (GameObject g in HideOnConnectToServer)
            {
                g.SetActive(false);
            }

            foreach (GameObject g in ShowOnConnectToServer)
            {
                g.SetActive(true);
            }
        }
        else print("EMPTY !!!!!");
    }

    public void EndHost()
    {
        server.enabled = false;
        foreach (GameObject g in HideOnBeTheServer)
        {
            g.SetActive(true);
        }

        foreach (GameObject g in ShowOnBeTheServer)
        {
            g.SetActive(false);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void EndClientConnection()
    {
        client.enabled = false;

        foreach (GameObject g in HideOnConnectToServer)
        {
            g.SetActive(true);
        }

        foreach (GameObject g in ShowOnConnectToServer)
        {
            g.SetActive(false);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void console(string text)
    {
        Console.text = Console.text + text + "\n";
        ConsoleContent.sizeDelta = new Vector2(ConsoleContent.sizeDelta.x, ConsoleContent.sizeDelta.y + 50);
    }

    public void ServerSendMessage()
    {
        server.SendMessageServer(GetNickName() + " : " + ServerMessage.text + "\n");
        console(GetNickName() + " : " + ServerMessage.text + "\n");
        ServerMessage.text = string.Empty;         
    }
    
    public void ClientSendMessage()
    {
        client.SendMessageClient(ClientMessage.text);
        ClientMessage.text = string.Empty;
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    public string GetNickName()
    {
        return NickNameText.text;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Submit"))
        {
            if (client.enabled)
            {
                ClientSendMessage();
            }

            if (server.enabled)
            {
                ServerSendMessage();
            }
        }
        string[] DataSplited = Data.Split('_');
        foreach (string d in DataSplited)
        {
            if (d == "MorpionStateInfo")
            {
                morpion.MorpionRecvData(Data);
            }
        }
        Data = "EMPTY";
        Array.Clear(DataSplited,0,DataSplited.Length-1);
    }
}
