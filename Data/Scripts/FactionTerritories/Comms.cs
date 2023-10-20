using Faction_Territories.Config;
using ProtoBuf;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Library.Collections;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using Faction_Territories.Network;

namespace Faction_Territories.Network
{
    public enum NexusDataType
    {
        FactionJoin,
        FactionLeave,
        FactionRemove,
        FactionEdited,
        Chat,
        SettingsRequested,
        SettingsReply,
        GPS,
        ServerConfig
    }

    public enum SyncType
    {
        DetailInfo,
        Timer,
        SiegeTimer,
        Emissive,
        AddProductionPerk,
        EnableProductionPerk,
        DisableProductionPerk,
        SyncProductionAttached,
        SyncProductionRunning,
        FinalSiegeDateTime,
        FactionWhitelist
    }

    public enum DataType
    {
        None,
        ClaimSettings,
        InitClaim,
        InitSiege,
        Sync,
        SingleSync,
        ColorGps,
        RemoveClaimSettings,
        SendClientSettings,
        RequestSettings,
        UpdateDetailInfo,
        UpdateBlockText,    
        CreateTrigger,
        RemoveTrigger,
        SendGps,
        AddTerritory,
        SendAudio,
        UpdateEmissives,
        ResetTerritory,
        SyncBillboard,
        SyncParticle,
        UpdateProduction,
        SyncProduction,
        AddProduction,
        RemoveProduction,
        ManualTerritory,
        InitFinalSiege,
        ConsumeDelayTokens,
        UpdateSafeZoneAllies,
        DisableSafeZone,
        EnableSafeZone,
        ResetModData,
        PBMonitor,
        ChatMessage
    }

    [ProtoContract]
    public class ObjectContainer
    {
        [ProtoMember(1)]
        public long entityId = 0;
        [ProtoMember(2)]
        public long playerId = 0;
        [ProtoMember(3)]
        public long claimBlockId = 0;
        [ProtoMember(4)]
        public ClaimBlockSettings settings = new ClaimBlockSettings();
        [ProtoMember(5)]
        public string stringData;
        [ProtoMember(6)]
        public Vector3D location = new Vector3D(0, 0, 0);
        [ProtoMember(7)]
        public string factionTag;
        [ProtoMember(8)]
        public ulong steamId;
        [ProtoMember(9)]
        public SyncType syncType;
        [ProtoMember(10)]
        public float floatingNum;
        [ProtoMember(11)]
        public long fromFaction;
        [ProtoMember(12)]
        public long toFaction;
        [ProtoMember(13)]
        public ModMessage modMessage = new ModMessage();
        [ProtoMember(14)]
        public int nexusServerId;
    }

    [ProtoContract]
    public class NexusGPSMessage
    {
        [ProtoMember(1)]
		public string Name;

        [ProtoMember(2)]
		public string Description;

        [ProtoMember(3)]
		public Vector3D Pos;

        [ProtoMember(4)]
		public Color Color;

        [ProtoMember(5)]
		public List<long> SendToPlayerIds;

        [ProtoMember(6)]
        public bool Removing;

        public NexusGPSMessage() {}

        public NexusGPSMessage(string name, string description, Vector3D pos, Color color, List<long> sendToPlayerIds, bool removing = false)
		{
			Name = name;
			Description = description;
			Pos = pos;
			SendToPlayerIds = sendToPlayerIds;
            Removing = removing;
		}

        public NexusGPSMessage(IMyGps gps, List<long> sendToPlayerIds, bool removing = false)
		{
			Name = gps.Name;
			Description = gps.Description;
			Pos = gps.Coords;
			Color = gps.GPSColor;
			SendToPlayerIds = sendToPlayerIds;
			Removing = removing;
		}

		public static void SendAddGPS(ClaimBlockSettings settings, List<long> players, string description)
		{
            NexusGPSMessage msg = new NexusGPSMessage($"!INTRUDER! {settings.TerritoryName}", description, settings.BlockPos, Color.Red, players, false);
            msg.Removing = false;
			Session.Nexus.SendMessageToAllServers(msg.GetPackage());
		}

		public static void SendRemoveGPS(ClaimBlockSettings settings, List<long> players)
        {
            if (players == null)
            {
                players = NexusAPI.GetAllOnlinePlayers().Select(p => p.IdentityID).ToList();
            }
            NexusGPSMessage msg = new NexusGPSMessage($"!INTRUDER! {settings.TerritoryName}", "", settings.BlockPos, Color.Red, players, false);
			msg.Removing = true;
            Session.Nexus.SendMessageToAllServers(msg.GetPackage());
		}

		public IMyGps GetGPS()
        {
			IMyGps gps = MyAPIGateway.Session.GPS.Create(Name, Description, Pos, true, false);
			gps.GPSColor = Color;
            return gps;
		}

        public byte[] GetMessage()
        {
            return MyAPIGateway.Utilities.SerializeToBinary(this);
        }

        public byte[] GetPackage()
        {
			return new CommsPackage(NexusDataType.GPS, GetMessage()).GetMessage();
		}

        public static void Process(CommsPackage pkg)
        {
            if (pkg.NexusType != NexusDataType.GPS) return;
            NexusGPSMessage msg = MyAPIGateway.Utilities.SerializeFromBinary<NexusGPSMessage>(pkg.Data);
            if (msg == null) return;

            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(players, p => { return msg.SendToPlayerIds.Contains(p.IdentityId); });

            IMyGps gps = msg.GetGPS();

            foreach (IMyPlayer player in players)
            {
                if (msg.Removing)
                    MyAPIGateway.Session.GPS.RemoveGps(player.IdentityId, gps);
                else
				    MyAPIGateway.Session.GPS.AddGps(player.IdentityId, gps);
			}
        }
	}


    [ProtoContract]
    public class CommsPackage
    {
        [ProtoMember(1)]
        public DataType Type;

        [ProtoMember(2)]
        public byte[] Data;

        [ProtoMember(3)]
        public NexusDataType NexusType;

        public CommsPackage()
        {
            Type = DataType.None;
            Data = new byte[0];
        }

        public CommsPackage(DataType type, ObjectContainer objectContainer)
        {
            Type = type;
            Data = MyAPIGateway.Utilities.SerializeToBinary(objectContainer);
        }

		public CommsPackage(DataType type, byte[] data)
		{
			Type = type;
			Data = data;
		}

		public CommsPackage(NexusDataType type, ObjectContainer objectContainer)
        {
            NexusType = type;
            Data = MyAPIGateway.Utilities.SerializeToBinary(objectContainer);
        }

		public CommsPackage(NexusDataType type, byte[] data)
		{
			NexusType = type;
			Data = data;
		}

		public byte[] GetMessage()
		{
			return MyAPIGateway.Utilities.SerializeToBinary(this);
		}
	}

    public static class Comms
    {
        public static void DisablePBMonitor(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.PBMonitor, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void ResetClientModData(ulong steamId)
        {
            ObjectContainer objectContainer = new ObjectContainer();

            CommsPackage package = new CommsPackage(DataType.ResetModData, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, steamId);
        }

        public static void InitClaimToServer(long jdID, long claimBlockId, long playerId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = jdID;
            objectContainer.playerId = playerId;
            objectContainer.claimBlockId = claimBlockId;

            CommsPackage package = new CommsPackage(DataType.InitClaim, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void InitSiegeToServer(long jdID, long claimBlockId, long playerId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = jdID;
            objectContainer.playerId = playerId;
            objectContainer.claimBlockId = claimBlockId;

            CommsPackage package = new CommsPackage(DataType.InitSiege, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void FinalInitSiegeToServer(long jdID, long claimBlockId, long playerId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = jdID;
            objectContainer.playerId = playerId;
            objectContainer.claimBlockId = claimBlockId;

            CommsPackage package = new CommsPackage(DataType.InitFinalSiege, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void ManualTerritorySet(ClaimBlockSettings settings, string tag)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;
            objectContainer.factionTag = tag;

            CommsPackage package = new CommsPackage(DataType.ManualTerritory, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SendTriggerToServer(IMyTerminalBlock block, ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.claimBlockId = block.EntityId;
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.CreateTrigger, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SendRemoveTriggerToServer(IMyTerminalBlock block)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.claimBlockId = block.EntityId;

            CommsPackage package = new CommsPackage(DataType.RemoveTrigger, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SyncSettingsToOthers(ClaimBlockSettings settings, IMyPlayer client)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.Sync, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);

			MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);

			if (MyAPIGateway.Session.IsServer)
				Session.Instance.SaveClaimData(settings);
		}

        /*public static void SyncParticles(ClaimBlockSettings settings, IMyPlayer client)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.SyncParticles, objectContainer);
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);

            foreach (var player in players)
            {
                if (player == client) continue;
                if (player.SteamUserId <= 0 || player.IsBot) continue;
                //if (Vector3D.Distance(player.GetPosition(), settings.BlockPos) > 3000) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }
        }*/

        /*public static void CreateGpsOnClient(string description, Vector3D pos, long entityId, long playerId, ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;
            objectContainer.stringData = description;
            objectContainer.location = pos;
            objectContainer.entityId = entityId;
            objectContainer.playerId = playerId;

            CommsPackage package = new CommsPackage(DataType.CreateGps, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            IMyPlayer toClient = Triggers.GetPlayerFromId(playerId);
            if (toClient == null) return;

            MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, toClient.SteamUserId);
        }*/

        public static void SendChangeColorToServer(string description, long playerId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.stringData = description;
            objectContainer.playerId = playerId;

            CommsPackage package = new CommsPackage(DataType.ColorGps, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SendRemoveBlockToOthers(long entityId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = entityId;

            CommsPackage package = new CommsPackage(DataType.RemoveClaimSettings, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);
            /*List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (player.SteamUserId <= 0 || player.IsBot) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }*/
        }

        public static void UpdateDetailInfo(string info, long entityId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = entityId;
            objectContainer.stringData = info;

            CommsPackage package = new CommsPackage(DataType.UpdateDetailInfo, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            //List<IMyPlayer> players = new List<IMyPlayer>();
            //MyAPIGateway.Players.GetPlayers(players);

            long senderId = 0;
            if (!Session.Instance.isDedicated)
                senderId = MyAPIGateway.Session.LocalHumanPlayer.IdentityId;

            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);


            /*foreach (var player in players)
            {
                if (senderId == player.IdentityId) continue;
                if (player.SteamUserId <= 0 || player.IsBot) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }*/
        }

        public static void SyncSettingType(ClaimBlockSettings settings, IMyPlayer client, SyncType syncType)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;
            objectContainer.syncType = syncType;

            CommsPackage package = new CommsPackage(DataType.SingleSync, objectContainer);
            //List<IMyPlayer> players = new List<IMyPlayer>();
           // MyAPIGateway.Players.GetPlayers(players);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);

            /*foreach (var player in players)
            {
                if (player == client) continue;
                if (player.SteamUserId <= 0 || player.IsBot) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }*/

            if (MyAPIGateway.Session.IsServer)
                Session.Instance.SaveClaimData(settings);
            //else
                //MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void UpdateBlockText(ClaimBlockSettings settings, long playerId = 0)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;
            objectContainer.playerId = playerId;

            CommsPackage package = new CommsPackage(DataType.UpdateBlockText, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SendAudioToClient(IMyPlayer player, long playerId, string clip)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.stringData = clip;

            CommsPackage package = new CommsPackage(DataType.SendAudio, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);

            if (player == null && playerId != 0)
                player = Triggers.GetPlayerFromId(playerId);

            if (player == null) return;
            MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
        }

        public static void SendGpsToClient(long playerId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.playerId = playerId;

            CommsPackage package = new CommsPackage(DataType.SendGps, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void AddTerritoryToClient(long playerId, ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.playerId = playerId;
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.AddTerritory, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void UpdateEmissiveToClients(long claimId, IMyPlayer omitPlayer)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.claimBlockId = claimId;

            CommsPackage package = new CommsPackage(DataType.UpdateEmissives, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);
            /*List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (omitPlayer == player) continue;
                if (player.SteamUserId <= 0 || player.IsBot) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }*/
        }

        public static void SendClientsSettings(ClaimBlockSettings settings, ulong steamId = 0)
        {
			if (!Session.Instance.isDedicated && !Session.Instance.isServer)
				return;

			ObjectContainer objectContainer = new ObjectContainer();
			objectContainer.settings = settings;

			CommsPackage package = new CommsPackage(DataType.SendClientSettings, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);

            if(steamId == 0)
            {
                List<IMyPlayer> players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players);

                foreach (var player in players)
                {
                    //if (MyAPIGateway.Session.LocalHumanPlayer == player) continue;
                    //if (player.SteamUserId <= 0 || player.IsBot) continue;
                    MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
                }
            }
            else
            {
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, steamId);
            }
        }

        public static void RequestSettings(ulong steamId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.steamId = steamId;

            CommsPackage package = new CommsPackage(DataType.RequestSettings, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SendResetToServer(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.ResetTerritory, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void UpdateProductionMultipliers(long blockId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.claimBlockId = blockId;

            CommsPackage package = new CommsPackage(DataType.UpdateProduction, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SendApplyProductionPerkToServer(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.AddProduction, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SendRemoveProductionPerkToServer(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.RemoveProduction, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void SyncBillBoard(long entityId, IMyPlayer omitPlayer, string factionTag = "")
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = entityId;
            objectContainer.factionTag = factionTag;

            CommsPackage package = new CommsPackage(DataType.SyncBillboard, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);
            /*List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (omitPlayer == player) continue;
                if (player.SteamUserId <= 0 || player.IsBot) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }*/
        }

        public static void SyncParticleEffect(string effect, Vector3D pos)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.stringData = effect;
            objectContainer.location = pos;

            CommsPackage package = new CommsPackage(DataType.SyncParticle, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                //if (player.SteamUserId <= 0 || player.IsBot) continue;
                if (Vector3D.Distance(player.GetPosition(), pos) > 5000) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }
        }

        public static void SyncProductionPerk(long blockId, string type, float value)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.entityId = blockId;
            objectContainer.stringData = type;
            objectContainer.floatingNum = value;

            CommsPackage package = new CommsPackage(DataType.SyncProduction, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToOthers(4910, sendData);
            /*List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (player.SteamUserId <= 0 || player.IsBot) continue;
                MyAPIGateway.Multiplayer.SendMessageTo(4910, sendData, player.SteamUserId);
            }

            if (Session.Instance.isServer)
            {
                MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
            }*/
        }

        public static void ConsumeDelayTokens(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.ConsumeDelayTokens, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void UpdateSafeZoneAllies(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.UpdateSafeZoneAllies, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void DisableSafeZoneToServer(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.DisableSafeZone, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        public static void EnableSafeZoneToServer(ClaimBlockSettings settings)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.settings = settings;

            CommsPackage package = new CommsPackage(DataType.EnableSafeZone, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            MyAPIGateway.Multiplayer.SendMessageToServer(4910, sendData);
        }

        /*public static void SendNexusFactionChange(NexusDataType nexusDataType, long fromFaction, long toFaction, long playerId)
        {
            ObjectContainer objectContainer = new ObjectContainer();
            objectContainer.fromFaction = fromFaction;
            objectContainer.toFaction = toFaction;
            objectContainer.playerId = playerId;

            CommsPackage package = new CommsPackage(nexusDataType, objectContainer);
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(package);
            Session.Instance.Nexus.SendMessage(sendData);
        }*/


        public static T TryDeserialize<T>(byte[] data)
        {
            try
            {
                return MyAPIGateway.Utilities.SerializeFromBinary<T>(data);
            }
            catch
            {
                return default(T);
            }
        }

        public static void MessageHandler(ushort id, byte[] data, ulong sender, bool fromServer)
        {
            CommsPackage package;
            try
            {
                package = TryDeserialize<CommsPackage>(data);
            }
            catch { return; }
            if (package == null || package.Data.Length == 0) return;

            if (package.Type == DataType.ChatMessage)
            {
                CustomChatMsg.ShowMessage(data);
            }

            // To Server
            if (package.Type == DataType.InitClaim)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                IMyEntity entity = null;
                ClaimBlockSettings settings;

                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.claimBlockId, out settings))
                {
                    //MyVisualScriptLogicProvider.ShowNotification("Failed to get claim block", 5000, "Red");
                    return;
                }

                if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.entityId, out entity))
                {
                    //MyVisualScriptLogicProvider.ShowNotification("Failed to get entity", 5000, "Red");
                    return;
                }

                if (!Utils.TakeTokens(entity, settings))
                {
                    //MyVisualScriptLogicProvider.ShowNotification("Failed to get token", 5000, "Red");
                    return;
                }

                settings.IsClaiming = true;
                settings.Timer = settings.ToClaimTimer;
                settings.PlayerClaimingId = encasedData.playerId;
                //settings.PlayerClaiming = Triggers.GetPlayerFromId(encasedData.playerId);
                settings.JDClaimingId = encasedData.entityId;
                settings.JDClaiming = entity;
                Utils.EnableHighlight(settings, entity);
                Utils.DrainAllJDs(entity);

                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerClaimingId);
                Vector3D pos = settings.BlockPos;
                if (faction != null)
                    //MyVisualScriptLogicProvider.SendChatMessageColored($"({faction.Tag}) Claiming Territory: {settings.UnclaimName} with {TimeSpan.FromSeconds(settings.Timer)} to claim", Color.Violet, "[Faction Territories]", 0L, "Red");
                    new ModMessage(settings.EntityId, settings.TerritoryName, $"({faction.Tag}) Claiming Territory: {settings.TerritoryName} with {TimeSpan.FromSeconds(settings.Timer)} to claim", "[Faction Territories]", Color.Red);
                //else
                //MyVisualScriptLogicProvider.SendChatMessageColored($"({settings.PlayerClaiming.DisplayName}) Claiming Territory: {settings.UnclaimName} with {TimeSpan.FromHours(settings.Timer)} to claim", Color.Violet, "[Faction Territories]", 0L, "Red");

                //Session.Instance.UpdateParticlesToRun(settings, true);
                return;
            }

            // To Server
            if (package.Type == DataType.InitSiege)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                IMyEntity entity = null;
                ClaimBlockSettings settings;

                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.claimBlockId, out settings))
                {
                    //MyVisualScriptLogicProvider.ShowNotification("Failed to get claim block", 5000, "Red");
                    return;
                }

                if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.entityId, out entity))
                {
                    //MyVisualScriptLogicProvider.ShowNotification("Failed to get entity", 5000, "Red");
                    return;
                }

                if (!Utils.TakeTokens(entity, settings))
                {
                    //MyVisualScriptLogicProvider.ShowNotification("Failed to get token", 5000, "Red");
                    return;
                }

                Utils.MonitorSafeZonePBs(settings);

                if (!settings.IsSieged)
                {
                    settings.IsSieging = true;
                    settings.SiegeTimer = settings.ToSiegeTimer;
                    settings.PlayerSiegingId = encasedData.playerId;
                    settings.JDSiegingId = encasedData.entityId;
                    settings.JDSieging = entity;
                    Utils.EnableHighlight(settings, entity);
                    Utils.DrainAllJDs(entity);
                    //Utils.RemoveSafeZone(settings);
                    //Utils.AddSafeZone(settings, false);

                    IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(encasedData.playerId);
                    Vector3D pos = settings.BlockPos;
                    if (faction != null)
                        //MyVisualScriptLogicProvider.SendChatMessageColored($"({faction.Tag}) Sieging Territory: {settings.ClaimZoneName} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", Color.Violet, "[Faction Territories]", 0L, "Red");
                        new ModMessage(settings.EntityId, settings.TerritoryName, $"({faction.Tag}) Sieging Territory: {settings.TerritoryName} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red);
                    else
                    {
                        IMyPlayer myPlayer = Triggers.GetPlayerFromId(encasedData.playerId);
                        if (myPlayer == null) return;
                        //MyVisualScriptLogicProvider.SendChatMessageColored($"({myPlayer.DisplayName}) Sieging Territory: {settings.ClaimZoneName} with {TimeSpan.FromHours(settings.SiegeTimer)} to siege", Color.Violet, "[Faction Territories]", 0L, "Red");
                        new ModMessage(settings.EntityId, settings.TerritoryName, $"({myPlayer.DisplayName}) Sieging Territory: {settings.TerritoryName} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red);
                    }
                }

                if (settings.IsSieged)
                {
                    settings.IsSiegingFinal = true;
                    settings.SiegeTimer = settings.SiegeFinalTimer;
                    settings.PlayerSiegingId = encasedData.playerId;
                    settings.JDSiegingId = encasedData.entityId;
                    settings.JDSieging = entity;
                    Utils.EnableHighlight(settings, entity);
                    Utils.DrainAllJDs(entity);
                    //Utils.RemoveSafeZone(settings);
                    //Utils.AddSafeZone(settings, false);

                    IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(encasedData.playerId);
                    Vector3D pos = settings.BlockPos;
                    if (faction != null)
                        //MyVisualScriptLogicProvider.SendChatMessageColored($"({faction.Tag}) Sieging Territory: {settings.ClaimZoneName} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", Color.Violet, "[Faction Territories]", 0L, "Red");
                        new ModMessage(settings.EntityId, settings.TerritoryName, $"({faction.Tag}) Final Sieging Territory: {settings.TerritoryName} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red);
                    else
                    {
                        IMyPlayer myPlayer = Triggers.GetPlayerFromId(encasedData.playerId);
                        if (myPlayer == null) return;
                        //MyVisualScriptLogicProvider.SendChatMessageColored($"({myPlayer.DisplayName}) Sieging Territory: {settings.ClaimZoneName} with {TimeSpan.FromHours(settings.SiegeTimer)} to siege", Color.Violet, "[Faction Territories]", 0L, "Red");
                        new ModMessage(settings.EntityId, settings.TerritoryName, $"({myPlayer.DisplayName}) Final Sieging Territory: {settings.TerritoryName} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red);
                    }
                }

                return;
            }

            // To Server
            if (package.Type == DataType.InitFinalSiege)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                IMyEntity entity = null;
                ClaimBlockSettings settings;

                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.claimBlockId, out settings)) return;
                if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.entityId, out entity)) return;
                if (!Utils.TakeTokens(entity, settings)) return;

                Utils.MonitorSafeZonePBs(settings);

                settings.ReadyToSiege = false;
                settings.IsSiegingFinal = true;
                settings.SiegeTimer = settings.SiegeFinalTimer;
                settings.PlayerSiegingId = encasedData.playerId;
                settings.JDSiegingId = encasedData.entityId;
                settings.JDSieging = entity;
                Utils.EnableHighlight(settings, entity);
                Utils.DrainAllJDs(entity);
                //Utils.RemoveSafeZone(settings);
                //Utils.AddSafeZone(settings, false);

                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(encasedData.playerId);
                Vector3D pos = settings.BlockPos;
                if (faction != null)
                    //MyVisualScriptLogicProvider.SendChatMessageColored($"({faction.Tag}) Sieging Territory: {settings.ClaimZoneName} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", Color.Violet, "[Faction Territories]", 0L, "Red");
                    new ModMessage(settings.EntityId, settings.TerritoryName, $"({faction.Tag}) Final Sieging Territory: {settings.TerritoryName} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red);
                else
                {
                    IMyPlayer myPlayer = Triggers.GetPlayerFromId(encasedData.playerId);
                    if (myPlayer == null) return;
                    //MyVisualScriptLogicProvider.SendChatMessageColored($"({myPlayer.DisplayName}) Sieging Territory: {settings.ClaimZoneName} with {TimeSpan.FromHours(settings.SiegeTimer)} to siege", Color.Violet, "[Faction Territories]", 0L, "Red");
                    new ModMessage(settings.EntityId, settings.TerritoryName, $"({myPlayer.DisplayName}) Final Sieging Territory: {settings.TerritoryName} with {TimeSpan.FromHours(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red);
                }

                return;
            }

            // To Server
            if (package.Type == DataType.ManualTerritory)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings mySettings;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out mySettings)) return;

                Utils.ManuallySetTerritory(mySettings, encasedData.factionTag);
            }

            // To Everyone
            if (package.Type == DataType.Sync)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings settings = null;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

                ServerData hold = settings.Server;
                Session.Instance.claimBlocks[encasedData.settings.EntityId] = encasedData.settings;
                Session.Instance.claimBlocks[encasedData.settings.EntityId].Server = hold;

                if (MyAPIGateway.Multiplayer.IsServer)
                {
                    Session.Instance.SaveClaimData(settings);
                }

                IMyEntity entity;
                if (MyAPIGateway.Entities.TryGetEntityById(encasedData.entityId, out entity) && entity is IMyTerminalBlock)
                    settings.Block = entity as IMyTerminalBlock;

                return;
            }

            // To Everyone
            if (package.Type == DataType.SingleSync)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings mySettings;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out mySettings)) return;

                if (encasedData.syncType == SyncType.DetailInfo)
                {
                    mySettings._detailInfo = encasedData.settings.DetailInfo;

                    IMyEntity entity = null;
                    if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.settings.EntityId, out entity)) return;
                    var block = entity as IMyTerminalBlock;
                    if (block == null) return;

                    block.RefreshCustomInfo();
                    ActionControls.RefreshControls(block, false);
                    //return;
                }

                if (encasedData.syncType == SyncType.Emissive)
                {
                    mySettings._emissiveState = encasedData.settings.BlockEmissive;

                    IMyEntity block;
                    if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.settings.EntityId, out block)) return;
                    Utils.SetEmissive(mySettings.BlockEmissive, block as IMyBeacon);
                    //return;
                }

                if (encasedData.syncType == SyncType.SiegeTimer)
                {
                    mySettings._siegeTimer = encasedData.settings.SiegeTimer;
                    //return;
                }

                if (encasedData.syncType == SyncType.Timer)
                {
                    mySettings._timer = encasedData.settings.Timer;
                    //return;
                }

                if (encasedData.syncType == SyncType.AddProductionPerk)
                {
                    mySettings._perks = encasedData.settings.GetPerks;
                    //return;
                }

                if (encasedData.syncType == SyncType.EnableProductionPerk)
                {
                    mySettings._perks[PerkType.Production].enabled = true;
                    //return;
                }

                if (encasedData.syncType == SyncType.DisableProductionPerk)
                {
                    if (mySettings._perks.ContainsKey(PerkType.Production))
                    {
                        mySettings._perks[PerkType.Production].enabled = false;
                    }

                    //return;
                }

                if (encasedData.syncType == SyncType.SyncProductionAttached)
                {
                    if (mySettings._perks.ContainsKey(PerkType.Production) && encasedData.settings._perks.ContainsKey(PerkType.Production))
                    {
                        mySettings._perks[PerkType.Production].perk.productionPerk.attachedEntities = encasedData.settings._perks[PerkType.Production].perk.productionPerk.attachedEntities;
                    }

                    //return;
                }

                if (encasedData.syncType == SyncType.SyncProductionRunning)
                {
                    if (mySettings._perks.ContainsKey(PerkType.Production) && encasedData.settings._perks.ContainsKey(PerkType.Production))
                    {
                        mySettings._perks[PerkType.Production].perk.productionPerk.ProductionRunning = encasedData.settings._perks[PerkType.Production].perk.productionPerk.ProductionRunning;
                    }

                    //return;
                }

                if (encasedData.syncType == SyncType.FinalSiegeDateTime)
                {
                    mySettings._finalSiegeDateTime = encasedData.settings.FinalSiegeDateTime;
                    //return;
                }

                if (encasedData.syncType == SyncType.FactionWhitelist)
                {
                    mySettings._factions = encasedData.settings.Factions;
                    IMyEntity entity;
                    if (MyAPIGateway.Entities.TryGetEntityById(encasedData.settings.SafeZoneEntity, out entity) && entity is MySafeZone)
                    {
                        MySafeZone zone = entity as MySafeZone;
                        List<IMyFaction> list = new List<IMyFaction>();
                        foreach (long fid in encasedData.settings.Factions)
                        {
                            IMyFaction f = MyAPIGateway.Session.Factions.TryGetFactionById(fid);
                            if (f != null)
                                list.Add(f);
                        }
                        zone.Factions = ListKonverter(zone.Factions[0], list);
                    }
                }

                if (MyAPIGateway.Multiplayer.IsServer)
                {
                    Session.Instance.SaveClaimData(encasedData.settings);
                }

                return;
            }

            // To Server
            if (package.Type == DataType.ColorGps)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                MyVisualScriptLogicProvider.SetGPSColor(encasedData.stringData, Color.Red, encasedData.playerId);
                return;
            }

            // To Everyone
            if (package.Type == DataType.RemoveClaimSettings)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                //ClaimBlockSettings settings;
                //Session.Instance.claimBlocks.TryGetValue(encasedData.entityId, out settings);
                //if (settings != null) Session.Instance.UpdateParticlesToRun(settings, false);

                if (Session.Instance.claimBlocks.ContainsKey(encasedData.entityId))
                    Session.Instance.claimBlocks.Remove(encasedData.entityId);

                return;
            }

            // To Everyone
            if (package.Type == DataType.UpdateDetailInfo)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                IMyEntity entity = null;
                if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.entityId, out entity)) return;

                var block = entity as IMyTerminalBlock;
                if (block == null) return;

                block.RefreshCustomInfo();
                ActionControls.RefreshControls(block);
                return;
            }

            // To Everyone
            if (package.Type == DataType.SyncBillboard)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                IMyEntity entity = null;
                if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.entityId, out entity)) return;

                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(encasedData.factionTag);

                Utils.SetScreen(entity as IMyBeacon, faction);
                return;
            }

            // To Everyone
            if (package.Type == DataType.SyncParticle)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                Utils.PlayParticle(encasedData.stringData, encasedData.location);
                return;
            }

            // To Server
            if (package.Type == DataType.UpdateBlockText)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings mySettings;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out mySettings)) return;

                if (!mySettings.IsClaimed)
                    GPS.UpdateBlockText(mySettings, $"Unclaimed Territory: {encasedData.settings.TerritoryName}", encasedData.playerId);
                else
                    GPS.UpdateBlockText(mySettings, $"Claimed Territory: {encasedData.settings.ClaimZoneName} ({encasedData.settings.ClaimedFaction})", encasedData.playerId);

                return;
            }

            // To Client
            if (package.Type == DataType.SendAudio)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                Audio.PlayClip(encasedData.stringData);
                return;
            }

            // To Server
            if (package.Type == DataType.CreateTrigger)
            {
                if (!MyAPIGateway.Multiplayer.IsServer) return;
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                IMyEntity entity = null;
                if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.claimBlockId, out entity)) return;

                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(encasedData.settings.BlockOwner);
                if (faction != null && faction.IsEveryoneNpc())
                    Session.Instance.blockOwner = faction;

                Events.CheckOwnership(entity as IMyTerminalBlock);
                //ClaimBlockSettings temp = null;
                // if (!Session.Instance.claimBlocks.TryGetValue(encasedData.claimBlockId, out temp)) return;
                //ServerData hold = temp.Server;
                //if (!Session.Instance.claimBlocks.ContainsKey(encasedData.claimBlockId)) return;


                //Session.Instance.claimBlocks[encasedData.claimBlockId] = encasedData.settings;
                //Session.Instance.claimBlocks[encasedData.claimBlockId].Server = hold;
                Triggers.CreateNewTriggers(entity as IMyBeacon);
                return;
            }

            // To Server
            if (package.Type == DataType.RemoveTrigger)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                Triggers.RemoveTriggerData(encasedData.claimBlockId);
                return;
            }

            // To Server
            if (package.Type == DataType.SendGps)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                Utils.CheckGridsToTag(encasedData.playerId);
                Utils.CheckPlayersToTag(encasedData.playerId);
                return;
            }

            // To Server
            if (package.Type == DataType.AddTerritory)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings mySettings;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out mySettings)) return;

                GPS.AddBlockLocation(mySettings.BlockPos, encasedData.playerId, mySettings);
                return;
            }

            // To Everyone
            if (package.Type == DataType.UpdateEmissives)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings mySettings;
                IMyEntity block;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.claimBlockId, out mySettings)) return;
                if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.claimBlockId, out block)) return;

                Utils.SetEmissive(mySettings.BlockEmissive, block as IMyBeacon);
                return;
            }

            // To Clients
            if (package.Type == DataType.SyncProduction)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                IMyEntity block;
                if (!MyAPIGateway.Entities.TryGetEntityById(encasedData.entityId, out block)) return;

                Utils.SetUpgradeValue(block as MyCubeBlock, encasedData.stringData, encasedData.floatingNum);
                return;
            }

            // To Client/Everyone
            if (package.Type == DataType.SendClientSettings)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                if (Session.Instance.claimBlocks.ContainsKey(encasedData.settings.EntityId)) return;

                IMyEntity entity;
                if (MyAPIGateway.Entities.TryGetEntityById(encasedData.settings.EntityId, out entity))
                    encasedData.settings.Block = entity as IMyTerminalBlock;

                Session.Instance.claimBlocks.Add(encasedData.settings.EntityId, encasedData.settings);
                return;
            }

            // To Server
            if (package.Type == DataType.RequestSettings)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                //IMyPlayer toPlayer = Triggers.GetPlayerFromId(encasedData.playerId);
                foreach (var item in Session.Instance.claimBlocks.Values)
                {
                    SendClientsSettings(item, encasedData.steamId);
                }

                return;
            }

            // To Server
            if (package.Type == DataType.ResetTerritory)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings serverSettings;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out serverSettings)) return;

                if (encasedData.settings.IsClaimed)
                    Utils.RemoveSafeZone(serverSettings);

                Utils.ResetClaim(serverSettings);
                Triggers.RemoveTriggerData(serverSettings.EntityId);
                serverSettings.TriggerInit = false;
                return;
            }

            // To Server
            if (package.Type == DataType.UpdateProduction)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings settings;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.claimBlockId, out settings)) return;

                Utils.UpdateProductionMultipliers(settings);
                return;
            }

            // To Server
            if (package.Type == DataType.AddProduction)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings settings;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

                Utils.AddAllProductionMultipliers(settings, null, true);
                return;
            }

            // To Server
            if (package.Type == DataType.RemoveProduction)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings settings;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

                Utils.RemoveProductionMultipliers(settings, null, true);
                return;
            }

            // To Server
            if (package.Type == DataType.ConsumeDelayTokens)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings settings;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

                Utils.DelaySiegeTokenConsumption(settings, true);
            }

            // To Server
            if (package.Type == DataType.UpdateSafeZoneAllies)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings settings;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

                
                    Utils.RemoveSafeZone(settings);
                    Utils.AddSafeZone(settings);
            }

            // To Server
            if (package.Type == DataType.DisableSafeZone)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings settings;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;


                Utils.RemoveSafeZone(settings);
            }

            // To Server
            if (package.Type == DataType.EnableSafeZone)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings settings;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

					Utils.AddSafeZone(settings);
            }

            // To Client
            if (package.Type == DataType.ResetModData)
            {
                if (Session.Instance.isServer) return;
                if (!Session.Instance.init)
                {
                    MyLog.Default.WriteLineAndConsole("Territories: Client Connected, Init False, Returning");
                    return;
                }

                MyLog.Default.WriteLineAndConsole("Territories: Client Connected, Init True, Clearing Data/Reloading");
                Session.Instance.claimBlocks.Clear();
                RequestSettings(MyAPIGateway.Multiplayer.MyId);
                MyAPIGateway.Parallel.StartBackground(() =>
                {
                    MyAPIGateway.Parallel.Sleep(1500);
                    Session.Instance.init = false;
                });
            }

            // To Server
            if (package.Type == DataType.PBMonitor)
            {
                var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                if (encasedData == null) return;

                ClaimBlockSettings settings;
                if (!Session.Instance.claimBlocks.TryGetValue(encasedData.settings.EntityId, out settings)) return;

                Utils.MonitorSafeZonePBs(settings, true);
            }
        }


		public static List<T> ListKonverter<T>(T obj, List<IMyFaction> factions)
		{
			List<T> list = new List<T>();
			foreach (IMyFaction faction in factions)
				if (faction is T)
					list.Add((T)faction);
			return list;
		}

		/*public static void Nexus_NexusMessageRecieved(object sender, NexusAPI.NexusMessage e)
        {
            try
            {
                var package = TryDeserialize<CommsPackage>(e.MessageData);
                if (package == null) return;

                if (package.NexusType == NexusDataType.FactionJoin)
                {
                    var encasedData = TryDeserialize<ObjectContainer>(package.Data);
                    if (encasedData == null) return;

                    foreach (var item in Session.Instance.claimBlocks.Values)
                    {
                        if (!item.IsClaimed) continue;
                        if (item.FactionId != encasedData.fromFaction) continue;

                        Utils.CheckGridsToTag(encasedData.playerId);
                        Utils.CheckPlayersToTag(encasedData.playerId);

                        return;
                    }

                    return;
                }

            }
            catch (Exception ex)
            {

            }
        }*/
	}
}
