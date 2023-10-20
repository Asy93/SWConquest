using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using Sandbox.ModAPI;

namespace Faction_Territories.Network

{
	[ProtoContract]
	public class CustomChatMsg
	{
		[ProtoMember(1)]
		public string Sender;

		[ProtoMember(2)]
		public string Message;

		public CustomChatMsg()
		{
			Sender = "";
			Message = "";
		}

		public CustomChatMsg(string sender, string message)
		{
			Sender = sender;
			Message = message;
		}

		public static void SendChatMessage(ulong steamId, string sender, string message)
		{
			if (MyAPIGateway.Multiplayer.IsServer)
				MyAPIGateway.Multiplayer.SendMessageTo(4910, new CommsPackage(DataType.ChatMessage, MyAPIGateway.Utilities.SerializeToBinary(new CustomChatMsg(sender, message))).GetMessage(), steamId);
		}

		public static void ShowMessage(byte[] data)
		{
			if (MyAPIGateway.Session.LocalHumanPlayer == null) return;
			CustomChatMsg message;
			try
			{
				message = MyAPIGateway.Utilities.SerializeFromBinary<CustomChatMsg>(data);
			}
			catch { return; }

			if (message != null)
				MyAPIGateway.Utilities.ShowMessage(message.Sender, message.Message);
		}
	}
}
