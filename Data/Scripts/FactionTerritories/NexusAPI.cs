using Faction_Territories.Config;
using Faction_Territories.Network;
using ProtoBuf;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace Faction_Territories.Network
{
	public class NexusAPI
	{
		public ushort CrossServerModID;

		/*  For recieving custom messages you have to register a message handler with a different unique ID then what you use server to client. (It should be the same as this class)
         *  
         *  NexusAPI(5432){
         *  CrossServerModID = 5432
         *  }
         *  
         *  
         *  Register this somewhere in your comms code. (This will only be raised when it recieves a message from another server)
         *  MyAPIGateway.Multiplayer.RegisterMessageHandler(5432, MessageHandler);
         */

		public NexusAPI(ushort SocketID)
		{
			CrossServerModID = SocketID;
		}

		public static bool IsRunningNexus()
		{
			return false;
		}

		public static bool IsPlayerOnline(long IdentityID)
		{
			return false;
		}

		private static List<object[]> GetSectorsObject()
		{
			List<object[]> APISectors = new List<object[]>();
			return APISectors;
		}

		private static List<object[]> GetAllOnlinePlayersObject()
		{
			List<object[]> OnlinePlayers = new List<object[]>();
			return OnlinePlayers;
		}

		private static List<object[]> GetAllServersObject()
		{
			List<object[]> Servers = new List<object[]>();
			return Servers;
		}

		private static List<object[]> GetAllOnlineServersObject()
		{
			List<object[]> Servers = new List<object[]>();
			return Servers;
		}

		private static object[] GetThisServerObject()
		{
			object[] OnlinePlayers = new object[6];
			return OnlinePlayers;
		}

		public static Server GetThisServer()
		{
			object[] obj = GetThisServerObject();
			return new Server((string)obj[0], (int)obj[1], (short)obj[2], (int)obj[3], (int)obj[4], (List<ulong>)obj[5]);
		}

		public static List<Sector> GetSectors()
		{
			List<object[]> Objs = GetSectorsObject();

			List<Sector> Sectors = new List<Sector>();
			foreach (var obj in Objs)
			{
				Sectors.Add(new Sector((string)obj[0], (string)obj[1], (int)obj[2], (bool)obj[3], (Vector3D)obj[4], (double)obj[5], (int)obj[6]));
			}
			return Sectors;
		}

		public static int GetServerIDFromPosition(Vector3D Position)
		{
			return 0;
		}

		public static List<Player> GetAllOnlinePlayers()
		{
			List<object[]> Objs = GetAllOnlinePlayersObject();

			List<Player> Players = new List<Player>();
			foreach (var obj in Objs)
			{
				Players.Add(new Player((string)obj[0], (ulong)obj[1], (long)obj[2], (int)obj[3]));
			}
			return Players;
		}

		public static List<Server> GetAllServers()
		{
			List<object[]> Objs = GetAllServersObject();

			List<Server> Servers = new List<Server>();
			foreach (var obj in Objs)
			{
				Servers.Add(new Server((string)obj[0], (int)obj[1], (int)obj[2], (string)obj[3]));
			}
			return Servers;
		}

		public static List<Server> GetAllOnlineServers()
		{
			List<object[]> Objs = GetAllOnlineServersObject();

			List<Server> Servers = new List<Server>();
			foreach (var obj in Objs)
			{
				Servers.Add(new Server((string)obj[0], (int)obj[1], (int)obj[2], (float)obj[3], (int)obj[4], (List<ulong>)obj[5]));
			}
			return Servers;
		}

		public static bool IsServerOnline(int ServerID)
		{
			return false;
		}

		public static void BackupGrid(List<MyObjectBuilder_CubeGrid> GridObjectBuilders, long OnwerIdentity)
		{
			return;
		}

		public static void SendChatMessageToDiscord(ulong ChannelID, string Author, string Message) { }

		public static void SendEmbedMessageToDiscord(ulong ChannelID, string EmbedTitle, string EmbedMsg, string EmbedFooter, string EmbedColor = null) { }

		public void SendMessageToServer(int ServerID, byte[] Message)
		{
			return;
		}

		public void SendMessageToAllServers(byte[] Message)
		{
			return;
		}

		public class Sector
		{
			public readonly string Name;
			public readonly string IPAddress;
			public readonly int Port;
			public readonly bool IsGeneralSpace;
			public readonly Vector3D Center;
			public readonly double Radius;
			public readonly int ServerID;

			public Sector(string Name, string IPAddress, int Port, bool IsGeneralSpace, Vector3D Center, double Radius, int ServerID)
			{
				this.Name = Name;
				this.IPAddress = IPAddress;
				this.Port = Port;
				this.IsGeneralSpace = IsGeneralSpace;
				this.Center = Center;
				this.Radius = Radius;
				this.ServerID = ServerID;
			}
		}

		public class Player
		{
			public readonly string PlayerName;
			public readonly ulong SteamID;
			public readonly long IdentityID;
			public readonly int OnServer;

			public Player(string PlayerName, ulong SteamID, long IdentityID, int OnServer)
			{
				this.PlayerName = PlayerName;
				this.SteamID = SteamID;
				this.IdentityID = IdentityID;
				this.OnServer = OnServer;
			}
		}

		public partial class Server
		{
			public readonly string Name;
			public readonly int ServerID;
			public readonly int ServerType;
			public readonly string ServerIP;

			public readonly int MaxPlayers;
			public readonly float ServerSS;
			public readonly int TotalGrids;
			public readonly List<ulong> ReservedPlayers;

			/*  Possible Server Types
             * 
             *  0 - SyncedSectored
             *  1 - SyncedNon-Sectored
             *  2 - Non-Synced & Non-Sectored
             * 
             */

			public Server(string Name, int ServerID, int ServerType, string IP)
			{
				this.Name = Name;
				this.ServerID = ServerID;
				this.ServerType = ServerType;
				this.ServerIP = IP;
			}

			//Online Server
			public Server(string Name, int ServerID, int MaxPlayers, float SimSpeed, int TotalGrids, List<ulong> ReservedPlayers)
			{
				this.Name = Name;
				this.ServerID = ServerID;
				this.MaxPlayers = MaxPlayers;
				this.ServerSS = SimSpeed;
				this.TotalGrids = TotalGrids;
				this.ReservedPlayers = ReservedPlayers;
			}
		}

		[ProtoContract]
		public class CrossServerMessage
		{
			[ProtoMember(1)] public readonly int ToServerID;
			[ProtoMember(2)] public readonly int FromServerID;
			[ProtoMember(3)] public readonly ushort UniqueMessageID;
			[ProtoMember(4)] public readonly byte[] Message;

			public CrossServerMessage(ushort UniqueMessageID, int ToServerID, int FromServerID, byte[] Message)
			{
				this.UniqueMessageID = UniqueMessageID;
				this.ToServerID = ToServerID;
				this.FromServerID = FromServerID;
				this.Message = Message;
			}

			public CrossServerMessage() { }
		}
	}

	public static class NexusComms
    {
		public static List<NexusAPI.Server> Servers = new List<NexusAPI.Server>();

        public static void SendChatAllServers(ModMessage message)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.modMessage = message;

            CommsPackage package = new CommsPackage(NexusDataType.Chat, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
			foreach (var server in Servers)
				Session.Nexus.SendMessageToServer(server.ServerID, sendData);
           // MyLog.Default.WriteLineAndConsole($"NexusAPI: Nexus Message Sent");
            //MyAPIGateway.Multiplayer.SendMessageToServer(Session.Instance.ModId, sendData);
        }

        public static void RequestTerritoryStatus()
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.nexusServerId = Session.Instance.NexusServer.ServerID;

            CommsPackage package = new CommsPackage(NexusDataType.SettingsRequested, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
			foreach (var server in Servers)
				Session.Nexus.SendMessageToServer(server.ServerID, sendData);
			//Session.Nexus.SendMessageToAllServers(sendData);
        }

        public static void SendTerritoryStatus(int serverId)
        {
            foreach(var item in Session.Instance.claimBlocks.Values)
            {
                ObjectContainer objectContainer = new ObjectContainer();
                objectContainer.settings = item;

                CommsPackage package = new CommsPackage(NexusDataType.SettingsReply, objectContainer);
                var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
                Session.Nexus.SendMessageToServer(serverId, sendData);
            }
        }

		public static void MessageHandler(ushort id, byte[] data, ulong sender, bool fromServer)
		{
			CommsPackage package = null;

			try
			{
				package = MyAPIGateway.Utilities.SerializeFromBinary<CommsPackage>(data);
			}
			catch (Exception ex)
			{
				package = null;
			}

			if (package == null) return;

			if (package.NexusType == NexusDataType.Chat)
			{
				var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
				if (encasedData == null) return;

				encasedData.modMessage.ProcessMsg();

				//if (encasedData.modMessage.FactionId == 0)
				//	MyVisualScriptLogicProvider.SendChatMessageColored($"<@&{encasedData.modMessage.TerritoryEntityId}> [{encasedData.modMessage.TerritoryName}]: {encasedData.modMessage.Message}", encasedData.modMessage.Color, encasedData.modMessage.Author, 0, "Red");
				//else
				//{
				//	IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionById(encasedData.modMessage.FactionId);
				//	if (faction == null) return;
				//	List<IMyPlayer> players = new List<IMyPlayer>();
				//	MyAPIGateway.Multiplayer.Players.GetPlayers(players, p => faction.Members.Keys.Contains(p.IdentityId));
				//	foreach (var player in players)
				//	{
				//		MyVisualScriptLogicProvider.SendChatMessageColored($"<@&{encasedData.modMessage.TerritoryEntityId}> [{encasedData.modMessage.TerritoryName}]: {encasedData.modMessage.Message}", encasedData.modMessage.Color, encasedData.modMessage.Author, player.IdentityId, "Red");
				//	}
				//}
				return;
			}

			if (package.NexusType == NexusDataType.SettingsRequested)
			{
				var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
				if (encasedData == null) return;

				SendTerritoryStatus(encasedData.nexusServerId);
				return;
			}

			if (package.NexusType == NexusDataType.SettingsReply)
			{
				var encasedData = MyAPIGateway.Utilities.SerializeFromBinary<ObjectContainer>(package.Data);
				if (encasedData == null) return;

				Session.Instance.crossServerClaimSettings.Add(encasedData.settings);
				return;
			}

			if (package.NexusType == NexusDataType.GPS)
			{
				NexusGPSMessage.Process(package);
			}

			if (package.NexusType == NexusDataType.ServerConfig)
			{
				ServerConfig config = MyAPIGateway.Utilities.SerializeFromBinary<ServerConfig>(package.Data);
				if (config != null && config is ServerConfig)
				{
					Session.Config = config;
					Session.Config.Save();
				}
			}
		}
    }
}
