using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ClientTCP : MonoBehaviour
{
	#region private members
	private Thread clientThread;
	private Thread clientListenerThread;
	private Thread ReadDataGameThread;
	private Socket server;
	private bool EndStart;
	#endregion

	#region Unity members
	public NetworkManager networkManager;
	public ExecuteOnMainThread MainThread;
	public string IP = "localhost";
	public int DefaultPort = 4912;
	#endregion

    // Setup socket connection. 	
    private void Start()
    {
		try
		{
			clientThread = new Thread(new ThreadStart(ConnectToServer));
			clientThread.IsBackground = true;
			clientThread.Start();
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
			networkManager.console("On client connect exception " + e + "\n");
		}
	}
    // Runs in background clientReceiveThread; Listens for incomming data.

    private void ConnectToServer()
    {
		EndStart = false;
		// Get server IP:Port

		ExecuteOnMainThread.RunOnMainThread.Enqueue(() => { networkManager.console("Try to connect to " + IP + "\n"); });

		IPHostEntry host = Dns.GetHostEntry(IP);
		IPAddress ipAddress = host.AddressList[0];
		IPEndPoint ServerIP = new IPEndPoint(ipAddress, DefaultPort);

		// connect to server
		server = new Socket(AddressFamily.InterNetwork , SocketType.Stream, ProtocolType.Tcp);

		try { server.Connect(ServerIP); }
        catch
		{
			try { server.Connect(IP, DefaultPort); }
			catch (SocketException e)
			{
				server.Close();
				ExecuteOnMainThread.RunOnMainThread.Enqueue(() => { networkManager.console("Can't connect to server : " + e + "\n"); });
				networkManager.EndClientConnection();
			}
		}

		Debug.Log("Connected to server on " + IP + " on port " + DefaultPort);
		ExecuteOnMainThread.RunOnMainThread.Enqueue(() => { networkManager.console("Connected to server on " + IP + " on port " + DefaultPort); });

		// create bufer and listen to server sended data
		Byte[] bytes = new Byte[9999999];

		EndStart = true;

		clientListenerThread = new Thread(() => ListenForData(bytes));
		clientListenerThread.IsBackground = true;
		clientListenerThread.Start();

		ReadDataGameThread = new Thread(() => ReadDataGame(bytes));
		ReadDataGameThread.IsBackground = true;
		ReadDataGameThread.Start();
	}

	private void ListenForData(Byte[] bytes)
	{
		bool NotADataGame = true;
		try
		{
			while (true)
			{
				NotADataGame = true;
				int Incoming = server.Receive(bytes);
				string ServerMessage = string.Empty;
				for (int i = 0; i < Incoming; i++)
				{
					ServerMessage = ServerMessage + Convert.ToChar(bytes[i]);
				}

				string[] DataSplited = ServerMessage.Split('_');
				foreach (string d in DataSplited)
				{
					if (d == ".DataGame")
					{
						NotADataGame = false;
					}
					Array.Clear(DataSplited, 0, DataSplited.Length - 1);
				}

				if (ServerMessage != "" && NotADataGame)
                {
					Debug.Log(ServerMessage);
					networkManager.console(ServerMessage + "\n");
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
			networkManager.console("Socket exception: " + socketException + "\n");
		}
	}
		
	// Send message to server using socket connection. 	
	public void SendMessageClient(string TextMessageUI)
	{
		try
		{
            if (TextMessageUI != "")
            {
				server.Send(Encoding.ASCII.GetBytes(networkManager.GetNickName() + " : " + TextMessageUI));
				Debug.Log(TextMessageUI);
				// networkManager.console(TextMessageUI + "\n");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
			networkManager.console("Socket exception: " + socketException + "\n");
		}
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

	private void OnApplicationQuit()
	{
		server.Close();
	}

	bool CheckConnectionState(Socket s)
	{
		bool part1 = s.Poll(1000, SelectMode.SelectRead);
		bool part2 = (s.Available == 0);
		if (part1 && part2)
			return false;
		else
			return true;
	}

	private void Update()
	{
        if (EndStart)
        {
			if (!CheckConnectionState(server))
			{
				KillAllThread();
				server.Disconnect(true);
				server.Close();
				networkManager.console("Check Connection State == false" + "\n");
				networkManager.EndClientConnection();
			}
		}
	}

	public void KillAllThread()
    {
		clientThread.Abort();
		clientListenerThread.Abort();
		ReadDataGameThread.Abort();
	}

	public void ReadDataGame(Byte[] bytes)
    {
		try
		{
			while (true)
			{
				int Incoming = server.Receive(bytes);
				string Data = string.Empty;
				for (int i = 0; i < Incoming; i++)
				{
					Data = Data + Convert.ToChar(bytes[i]);
				}
				if (Data != "")
				{
					Debug.Log(Data);
					networkManager.Data = Data;
				}
				Data = "";
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}

	public void SendDataGame(string Data)
	{
		try
		{
			if (Data != "")
			{
				server.Send(Encoding.ASCII.GetBytes(Data));
				// networkManager.console(TextMessageUI + "\n");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
}
