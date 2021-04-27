﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using NMAP;

namespace HackChat
{
	public class Chat
	{
		public const int DefaultPort = 31337;

		private readonly byte[] PingMsg = new byte[1];
		private readonly ConcurrentDictionary<IPEndPoint, (TcpClient Client, NetworkStream Stream)> Connections = new();
		private readonly SequentialScannerOpen scanner = new();

		private IPAddress[] ips;
		private readonly int port;
		private readonly TcpListener tcpListener;

		public Chat(int port) => tcpListener = new TcpListener(IPAddress.Any, this.port = port);

		public void Start()
		{
			ips = new IPAddress[256];
			for (int i = 0; i < 255; i++)
			{
				ips[i] = new IPAddress(new byte[] {192, 168, 11, (byte)i});
			}
			
			Task.Run(DiscoverLoop);
			Task.Run(() =>
			{
				string line;
				while((line = Console.ReadLine()) != null)
					Task.Run(() => BroadcastAsync(line));
			});
			Task.Run(() =>
			{
				tcpListener.Start(100500);
				while(true)
				{
					var tcpClient = tcpListener.AcceptTcpClient();
					Task.Run(() => ProcessClientAsync(tcpClient));
				}
			});
		}

		private async Task BroadcastAsync(string message)
		{
			throw new NotImplementedException();
		}

		private async void DiscoverLoop()
		{
			while(true)
			{
				try { await Discover(); } catch { /* ignored */ }
				await Task.Delay(3000);
			}
		}

		private async Task Discover()
		{
			var clients = await scanner.Scan(ips, port);

			//await Task.WhenAll(clients.Where( c => c))
		}

		private static async Task ProcessClientAsync(TcpClient tcpClient)
		{
			EndPoint endpoint = null;
			try { endpoint = tcpClient.Client.RemoteEndPoint; } catch { /* ignored */ }
			await Console.Out.WriteLineAsync($"[{endpoint}] connected");
			try
			{
				using(tcpClient)
				{
					var stream = tcpClient.GetStream();
					await ReadLinesToConsoleAsync(stream);
				}
			}
			catch { /* ignored */ }
			await Console.Out.WriteLineAsync($"[{endpoint}] disconnected");
		}

		private static async Task ReadLinesToConsoleAsync(Stream stream)
		{
			string line;
			using var sr = new StreamReader(stream);
			while((line = await sr.ReadLineAsync()) != null)
				await Console.Out.WriteLineAsync($"[{((NetworkStream)stream).Socket.RemoteEndPoint}] {line}");
		}
	}
}