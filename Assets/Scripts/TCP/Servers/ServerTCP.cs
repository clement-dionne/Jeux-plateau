using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ServerTCP : MonoBehaviour
{

	#region private members
	private Socket Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	private Thread NewClient;
	private Thread ServerStart;
	private List<Socket> AllClient = new List<Socket>();
	#endregion

	#region unity members
	public NetworkManager networkManager;
	public ExecuteOnMainThread MainThread;
	public int DefaultPort = 4912;
	#endregion

	void Start()
	{
		bool error = false;
		try
		{
			// Create and start Server TCP.
			IPHostEntry host = Dns.GetHostEntry(GetLocalIPAddress());
			IPEndPoint ServerIP = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), DefaultPort);


			Debug.Log(ServerIP + " or  " + IPAddress.Parse(GetLocalIPAddress()));

			Server.Bind(ServerIP);
			Server.Listen(DefaultPort);

			Debug.Log("Server is listening on : " + GetLocalIPAddress() + " port : " + DefaultPort.ToString());
			networkManager.console("Server is listening on : " + GetLocalIPAddress() + " port : " + DefaultPort.ToString() + "\n");
		}
		catch (SocketException socketException)
		{
			Debug.Log("SocketException " + socketException.ToString());
			//networkManager.console("SocketException " + socketException.ToString() + "\n");
			error = true;
		}
        if (!error)
        {
			ServerStart = new Thread(() => WaitForClient());
			ServerStart.IsBackground = true;
			ServerStart.Start();

			Debug.Log("End Start");
		}
	}

	private void Update()
	{
		foreach (Socket c in AllClient)
		{
			if (!CheckConnectionState(c))
            {
				SendMessageServer(((IPEndPoint)c.RemoteEndPoint).Address.ToString() + " Left the Server !");
				networkManager.console(((IPEndPoint)c.RemoteEndPoint).Address.ToString() + " Left the Server !");
				c.Close();
				AllClient.Remove(c);
            }
		}
	}
	private void WaitForClient()
    {
        while (true)
        {
			try
			{
				// create buffer
				Byte[] bytes = new Byte[999999999];

				// accept connections and run client listener
				Socket client = Server.Accept();

				Debug.Log("Connection accepted from " + ((IPEndPoint)client.RemoteEndPoint).Address.ToString());
				ExecuteOnMainThread.RunOnMainThread.Enqueue(() => { networkManager.console("Connection accepted from " + ((IPEndPoint)client.RemoteEndPoint).Address.ToString() + "\n"); });
				ExecuteOnMainThread.RunOnMainThread.Enqueue(() => { AllClient.Add(client); });

				NewClient = new Thread(() => ReadClientMessages(client, bytes));
				NewClient.IsBackground = true;
				NewClient.Start();

				SendMessageServer( ((IPEndPoint)client.RemoteEndPoint).Address.ToString() + " Join the Server !");
				Debug.Log(((IPEndPoint)client.RemoteEndPoint).Address.ToString() + " Join the Server !");
				ExecuteOnMainThread.RunOnMainThread.Enqueue(() => { networkManager.console(((IPEndPoint)client.RemoteEndPoint).Address.ToString() + " Join the Server !" + "\n"); });

			}
			catch (SocketException socketException)
			{
				Debug.Log("SocketException " + socketException.ToString());
				//networkManager.console("SocketException " + socketException.ToString() + "\n");
			}
		}
	}

	public void SendMessageServer(string TextMessageUI)
	{
		// BrodCast message to all client conected
		foreach (Socket ClientSocket in AllClient)
        {
			try
			{
				if (TextMessageUI != "")
				{
					ClientSocket.Send(Encoding.ASCII.GetBytes(TextMessageUI));
					ClientSocket.Send(Encoding.ASCII.GetBytes(TextMessageUI));
					Debug.Log(TextMessageUI);
					// ExecuteOnMainThread.RunOnMainThread.Enqueue(() => { networkManager.console(TextMessageUI + "\n"); });
				}
			}
			catch (SocketException socketException)
			{
				Debug.Log("Socket exception: " + socketException);
				ExecuteOnMainThread.RunOnMainThread.Enqueue(() => { networkManager.console("Socket exception: " + socketException + "\n"); });
				ClientSocket.Close();
				AllClient.Remove(ClientSocket);
			}
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

	private void ReadClientMessages(Socket client, Byte[] bytes)
    {
		bool NotADataGame = true;
		while (true)
        {
            try
            {
				NotADataGame = true;
				// receive client message
				int Incoming = client.Receive(bytes);

				string ClientMessage = string.Empty;

				// convert client message
				for (int i = 0; i < Incoming; i++)
				{
					ClientMessage = ClientMessage + Convert.ToChar(bytes[i]);
				}

				string[] DataSplited = ClientMessage.Split('_');
				foreach (string d in DataSplited)
				{
					if (d == ".DataGame")
					{
						SendDataGame(ClientMessage);
						networkManager.Data = ClientMessage;
						NotADataGame = false;
					}
					Array.Clear(DataSplited, 0, DataSplited.Length - 1);
				}
				// show client message
				if (ClientMessage != "" && NotADataGame)
				{
					Debug.Log(ClientMessage);
					ExecuteOnMainThread.RunOnMainThread.Enqueue(() => { networkManager.console(ClientMessage + "\n"); });

					// client.Send(Encoding.ASCII.GetBytes(ClientMessage + " test ok j s p q m"));
					SendMessageServer(ClientMessage);
				}
			}
            catch
            {
				client.Close();
				AllClient.Remove(client);
				break;
			}
		}
		// client.Close();
	}

    private void OnApplicationQuit()
    {
		Server.Shutdown(SocketShutdown.Send);
		Server.Close();
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

	public void EndServer()
    {
        foreach (Socket c in AllClient)
        {
			c.Close();
			AllClient.Remove(c);
        }
		Server.Shutdown(SocketShutdown.Send);
		Server.Close();
		networkManager.console("Server Closed "+"\n");
		networkManager.EndHost();
		KillAllThread();
	}

	public void KillAllThread()
	{
		ServerStart.Abort();
		NewClient.Abort();
	}

	public void SendDataGame(string DataToSend)
    {
		foreach (Socket ClientSocket in AllClient)
		{
			try
			{
				if (DataToSend != "")
				{
					ClientSocket.Send(Encoding.ASCII.GetBytes(DataToSend));
				}
			}
			catch (SocketException socketException)
			{
				Debug.Log("Socket exception: " + socketException);
				ClientSocket.Close();
				AllClient.Remove(ClientSocket);
			}
		}
	}
}