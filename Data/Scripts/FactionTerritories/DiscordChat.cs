using Faction_Territories.Config;
using ProtoBuf;
using Sandbox.Game;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Gui;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Network;
using VRage.Utils;
using VRageMath;

namespace Faction_Territories.Network
{
    [ProtoContract]
    public class ModMessage
    {
        [ProtoMember(1)] public long TerritoryEntityId;
        [ProtoMember(2)] public string TerritoryName;
        [ProtoMember(3)] public string Message;
        [ProtoMember(4)] public string Author;
        [ProtoMember(5)] public bool BroadCastToDiscordOnly;
        [ProtoMember(6)] public Color Color;
        [ProtoMember(7)] public long FactionId;
        [ProtoMember(8)] public ulong ChannelId;
        [ProtoMember(9)] public string FactionTag;
        [ProtoMember(10)] public List<long> NotifyIds = new List<long>();

		public ModMessage(long territoryEntityId, string territoryName, string MessageTxt, string MsgAuthor, Color color, ulong channelId = 0, long factionId = 0, bool BrodcastDiscordOnly = false)
		{
			TerritoryEntityId = territoryEntityId;
			TerritoryName = territoryName;
			Author = MsgAuthor;
			Color = color;
			BroadCastToDiscordOnly = BrodcastDiscordOnly;
			Message = MessageTxt;
			FactionId = factionId;
			ChannelId = channelId;

            ulong globalDiscordChatId = 0;

			ClaimBlockSettings settings = null;
            if (Session.Instance.claimBlocks.TryGetValue(TerritoryEntityId, out settings))
            {
                FactionTag = settings.ClaimedFaction;

                if (factionId != 0)
                {
                    List<IMyFaction> factionsToNotify = new List<IMyFaction>();
                    foreach (long id in settings.FactionsRadar)
                    {
                        IMyFaction f = MyAPIGateway.Session.Factions.TryGetFactionById(id);
                        if (f == null) continue;
                        factionsToNotify.Add(f);
                    }
                    foreach (var fac in factionsToNotify)
                    {
                        NotifyIds.AddRange(fac.Members.Keys);
                    }
                }

                globalDiscordChatId = settings.DiscordGlobalChannelId;
			}

			if (Session.Instance.IsNexusInstalled)
			{
				if (ChannelId != 0)
					NexusAPI.SendChatMessageToDiscord(ChannelId, Author, $"<{FactionTag}> [{TerritoryName}]: {Message}");
                else if (globalDiscordChatId != 0)
					NexusAPI.SendChatMessageToDiscord(globalDiscordChatId, Author, $"<{FactionTag}> [{TerritoryName}]: {Message}");
                
				NexusComms.SendChatAllServers(this);
			}

			ProcessMsg();
		}

		public void ProcessMsg()
        { 
            if (FactionId == 0)
            {
				MyVisualScriptLogicProvider.SendChatMessageColored($"<{FactionTag}> [{TerritoryName}]: {Message}", Color, Author, 0, "Red");
                return;
            }

            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(players, p => NotifyIds.Contains(p.IdentityId));
            foreach (var player in players)
            {
                //MyVisualScriptLogicProvider.SendChatMessageColored($"<{FactionTag}> [{TerritoryName}]: {Message}", Color, Author, player.IdentityId, "Red");
                CustomChatMsg.SendChatMessage(player.SteamUserId, $"<{FactionTag}> [{TerritoryName}]", Message);
            }
		}

        public ModMessage() { }
    }

    
}
