using OscJack;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace OscJackExpansion
{
	public class OscSenderEx : MonoBehaviour
	{
		private OscClient _client;
		private Socket _blobSocket;
		public OscClient Client => _client;

		public void Connect(string ipAddress, int port)
		{
			Disconnect();

			_client = new OscClient(ipAddress, port);

			_blobSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			if (ipAddress == "255.255.255.255")
			{
				_blobSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
			}
			_blobSocket.Connect(new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port));
		}

		public void Disconnect()
		{
			if (_client != null)
			{
				_client.Dispose();
				_client = null;
			}
			if (_blobSocket != null)
			{
				_blobSocket.Close();
				_blobSocket = null;
			}
		}

		public void Send(string address)
		{
			if (_client == null) return;
			_client.Send(address);
		}

		public void Send(string address, int data)
		{
			if (_client == null) return;
			_client.Send(address, data);
		}

		public void Send(string address, float data)
		{
			if (_client == null) return;
			_client.Send(address, data);
		}

		public void Send(string address, string data)
		{
			if (_client == null) return;
			_client.Send(address, data);
		}

		public void Send(string address, bool data)
		{
			if (_client == null) return;
			_client.Send(address, data ? 1 : 0);
		}

		public void Send(string address, byte[] blob)
		{
			if (_blobSocket == null) return;

			var packet = BuildOscBlobPacket(address, blob);
			_blobSocket.Send(packet, packet.Length, SocketFlags.None);
		}

		private static byte[] BuildOscBlobPacket(string address, byte[] blob)
		{
			int addrLen = address.Length;
			int addrPadded = Align4(addrLen + 1);
			int typePadded = Align4(3);
			int blobPadded = Align4(blob.Length);

			int totalSize = addrPadded + typePadded + 4 + blobPadded;
			byte[] packet = new byte[totalSize];
			int pos = 0;

			for (int i = 0; i < addrLen; i++)
			{
				packet[pos++] = (byte)address[i];
			}
			while (pos < addrPadded)
			{
				packet[pos++] = 0;
			}


			int typeStart = pos;
			packet[pos++] = (byte)',';
			packet[pos++] = (byte)'b';
			while (pos < typeStart + typePadded)
			{
				packet[pos++] = 0;
			}

			int size = blob.Length;
			packet[pos++] = (byte)(size >> 24);
			packet[pos++] = (byte)(size >> 16);
			packet[pos++] = (byte)(size >> 8);
			packet[pos++] = (byte)(size);

			Buffer.BlockCopy(blob, 0, packet, pos, blob.Length);

			return packet;
		}

		private static int Align4(int value)
		{
			return (value + 3) & ~3;
		}

		private void OnDestroy()
		{
			Disconnect();
		}
	}
}
