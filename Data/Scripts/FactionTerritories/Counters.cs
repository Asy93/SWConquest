using Sandbox.Game;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;
using Faction_Territories.Network;

namespace Faction_Territories
{
    public static class Counters
    {
        public static void IsClaimingCounter(ClaimBlockSettings settings)
        {
            settings.Timer--;
            foreach (long id in settings.GetPlayersInside.Keys)
			    Utils.ClaimTimer(settings.TerritoryName, settings.Timer, false, id);
			if (!Utils.CheckPlayerandBlock(settings))
            {
                if (settings.RecoveryTimer == 0)
                {
                    IMyPlayer playerClaim = Triggers.GetPlayerFromId(settings.PlayerClaimingId);
                    if (playerClaim != null)
                        MyVisualScriptLogicProvider.SendChatMessageColored($"WARNING - Return to range of claim area within 60 seconds or claiming will fail", Color.Violet, "[Faction Territories]", settings.PlayerClaimingId, "Red");
                }

                settings.RecoveryTimer++;
                if(settings.RecoveryTimer == 60)
                {
                    IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerClaimingId);
                    if (faction != null)
                        //MyVisualScriptLogicProvider.SendChatMessageColored($"({faction.Tag}) Failed To Claim: {settings.UnclaimName}", Color.Violet, "[Faction Territories]", 0L, "Red");
                        new ModMessage(settings.EntityId, settings.TerritoryName, $"({faction.Tag}) Failed To Claim: {settings.TerritoryName}", "[Faction Territories]", Color.Red);

					Utils.ResetClaim(settings);
                }
            }
            else
            {
                settings.RecoveryTimer = 0;
            }

            if (settings.Timer % 60 == 0 && settings.Timer != 0)
            {
                var pos = settings.BlockPos;
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerClaimingId);

                if (faction != null)
                    //MyVisualScriptLogicProvider.SendChatMessageColored($"({faction.Tag}) Claiming Territory: {settings.UnclaimName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromSeconds(settings.Timer)} to claim", Color.Violet, "[Faction Territories]", 0L, "Red");
                    new ModMessage(settings.EntityId, settings.TerritoryName, $"({faction.Tag}) Claiming Territory: {settings.TerritoryName} with {TimeSpan.FromSeconds(settings.Timer)} to claim", "[Faction Territories]", Color.Red, 0, faction.FactionId);
                //else
                    //MyVisualScriptLogicProvider.SendChatMessageColored($"({settings.PlayerClaiming.DisplayName}) Claiming Territory: {settings.UnclaimName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromHours(settings.Timer)} to claim", Color.Violet, "[Faction Territories]", 0L, "Red");
            }

            if (settings.Timer == 3)
            {
                Comms.SyncParticleEffect("Claimed", settings.BlockPos);
            }

            if (settings.Timer == 0)
            {
                if (!Utils.CheckPlayerandBlock(settings))
                {
                    IMyFaction factionA = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerClaimingId);
                    if (factionA != null)
                        //MyVisualScriptLogicProvider.SendChatMessageColored($"({factionA.Tag}) Failed To Claim: {settings.UnclaimName}", Color.Violet, "[Faction Territories]", 0L, "Red");
                        new ModMessage(settings.EntityId, settings.TerritoryName, $"({factionA.Tag}) Failed To Claim: {settings.TerritoryName}", "[Faction Territories]", Color.Red);

                    Utils.ResetClaim(settings);
                    return;
                }

                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerClaimingId);

				if (settings.JDClaiming != null)
					Utils.DisableHighlight(settings, settings.JDClaiming);

				settings.RecoveryTimer = 0;
                settings.IsClaiming = false;
                settings.IsClaimed = true;
                settings.ClaimZoneName = settings.TerritoryName;
                settings.Timer = settings.ConsumeTokenTimer;
                settings.BlockEmissive = EmissiveState.Claimed;
                if(faction != null)
                {
                    settings.ClaimedFaction = faction.Tag;
                    settings.FactionId = faction.FactionId;

                    var icon = faction.FactionIcon;
                    Utils.SetScreen(settings.Block as IMyBeacon, faction, true);
                }
                    

                /*if (settings.Block.HasInventory)
                {
                    var blockInv = settings.Block.GetInventory();
                    blockInv.Clear();
                }*/

                Utils.SetOwner(settings.Block, faction);
                Utils.AddSafeZone(settings);
                Utils.GetSurroundingSafeZones(settings);
                Utils.TagEnemyGrids(settings);
                Utils.TagEnemyPlayers(settings);
                Utils.StopHandTools();
                Utils.AddPerks(settings);
                GPS.UpdateBlockText(settings, $"Claimed Territory: {settings.ClaimZoneName} ({settings.ClaimedFaction})");

				var pos = settings.BlockPos;
                if (faction != null)
                {
                    //MyVisualScriptLogicProvider.SendChatMessageColored($"({faction.Tag}) Claimed Territory: {settings.UnclaimName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)}", Color.Violet, "[Faction Territories]", 0L, "Red");
                    //MyVisualScriptLogicProvider.SendChatMessageColored($"WARNING - Claimed Territory: {settings.ClaimZoneName} is out of tokens, 1 hour until territory is unclaimed", Color.Violet, "[Faction Territories]", 0L, "Red");
                    new ModMessage(settings.EntityId, settings.TerritoryName, $"({faction.Tag}) Claimed Territory: {settings.TerritoryName}", "[Faction Territories]", Color.Red);
                    new ModMessage(settings.EntityId, settings.TerritoryName, $"WARNING - Claimed Territory: {settings.TerritoryName} is out of tokens, {TimeSpan.FromSeconds(settings.ConsumeTokenTimer)} until territory is unclaimed", "[Faction Territories]", Color.Red, 1, settings.FactionId);
                }
                 //else
                    //MyVisualScriptLogicProvider.SendChatMessageColored($"({settings.PlayerClaiming.DisplayName}) Claimed Territory: {settings.UnclaimName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)}", Color.Violet, "[Faction Territories]", 0L, "Red");
            }
        }

        public static void IsClaimedCounter(ClaimBlockSettings settings)
        {
            if (!settings.IsSieging && !settings.IsSiegingFinal)
                settings.Timer--;

            if (settings.Timer == 0)
            {
                if (!Utils.ConsumeToken(settings))
                {
                    Utils.RemoveSafeZone(settings);
                    Utils.ResetClaim(settings);
					return;
                }

                Utils.UpdatePlayerPerks(settings);
                settings.Timer = settings.ConsumeTokenTimer;
            }

            /*if (!settings.IsSieged)
                settings.DetailInfo = Utils.GetCounterDetails(settings);
            else
                settings.DetailInfo += Utils.GetCounterDetails(settings);*/

            settings.DetailInfo = Utils.GetCounterDetails(settings);

            //GPS.ValidateGps(settings);
        }

        public static void IsSiegingCounter(ClaimBlockSettings settings)
        {
            settings.SiegeTimer--;
			foreach (long id in settings.GetPlayersInside.Keys)
				Utils.SiegeTimer(settings.TerritoryName, settings.SiegeTimer, false, id);
            if (!Utils.CheckPlayerandBlock(settings))
            {
                settings.RecoveryTimer++;

                MyVisualScriptLogicProvider.ShowNotificationToAll($"Siege grid is too far away! {60 - settings.RecoveryTimer:N0} seconds until sieging fails!", 900, "Red");

                if (settings.RecoveryTimer == 60)
                {
                    IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerSiegingId);
                    if (faction != null)
                    {
                        if (!settings.IsSieged)
                            new ModMessage(settings.EntityId, settings.TerritoryName, $"({faction.Tag}) Failed To Siege: {settings.TerritoryName}", "[Faction Territories]", Color.Red);
                        else
                            new ModMessage(settings.EntityId, settings.TerritoryName, $"({faction.Tag}) Failed To Siege: {settings.TerritoryName} - Final Siege Cooldown Started {TimeSpan.FromSeconds(settings.SiegeCoolingTime)}", "[Faction Territories]", Color.Red);
                    }

                    if (settings.IsSieged)
                    {
                        settings.SiegeTimer = settings.SiegeCoolingTime;
                        settings.IsSiegeCooling = true;
                    }

                    Utils.ResetSiegeData(settings);
                    return;
                }
            }

			else
            {
                settings.RecoveryTimer = 0;
            }

            if ((settings.SiegeTimer % (settings.SiegeNotificationFreq * 60) == 0 && settings.SiegeTimer > 0) || settings.SiegeTimer == 60)
            {
                var pos = settings.BlockPos;
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerSiegingId);

                if (!settings.IsSiegingFinal)
                {
                    if (faction != null)
                        //MyVisualScriptLogicProvider.SendChatMessageColored($"({faction.Tag}) Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", Color.Violet, "[Faction Territories]", 0L, "Red");
                        new ModMessage(settings.EntityId, settings.TerritoryName, $"({faction.Tag}) Sieging Territory: {settings.TerritoryName} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red, 0, faction.FactionId);
                    else
                    {
                        IMyPlayer playerSiege = Triggers.GetPlayerFromId(settings.PlayerSiegingId);
                        //MyVisualScriptLogicProvider.SendChatMessageColored($"({playerSiege?.DisplayName}) Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromHours(settings.SiegeTimer)} to siege", Color.Violet, "[Faction Territories]", 0L, "Red");
                        new ModMessage(settings.EntityId, settings.TerritoryName, $"({playerSiege?.DisplayName}) Sieging Territory: {settings.TerritoryName} with {TimeSpan.FromHours(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red, 0, faction.FactionId);

                    }
                }
                else
                {
                    if (faction != null)
                        //MyVisualScriptLogicProvider.SendChatMessageColored($"({faction.Tag}) Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", Color.Violet, "[Faction Territories]", 0L, "Red");
                        new ModMessage(settings.EntityId, settings.TerritoryName, $"({faction.Tag}) Final Sieging Territory: {settings.TerritoryName} with {TimeSpan.FromSeconds(settings.SiegeTimer)} until final sieging is complete", "[Faction Territories]", Color.Red, 0, faction.FactionId);
                    else
                    {
                        IMyPlayer playerSiege = Triggers.GetPlayerFromId(settings.PlayerSiegingId);
                        //MyVisualScriptLogicProvider.SendChatMessageColored($"({playerSiege?.DisplayName}) Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromHours(settings.SiegeTimer)} to siege", Color.Violet, "[Faction Territories]", 0L, "Red");
                        new ModMessage(settings.EntityId, settings.TerritoryName, $"({playerSiege?.DisplayName}) Final Sieging Territory: {settings.TerritoryName} with {TimeSpan.FromHours(settings.SiegeTimer)} until final sieging is complete", "[Faction Territories]", Color.Red, 0, faction.FactionId);

                    }
                } 
            }

            if (settings.SiegeTimer <= 0)
            {
                if (!Utils.CheckPlayerandBlock(settings))
                {
                    IMyFaction factionA = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerSiegingId);
                    if (factionA != null)
                        //MyVisualScriptLogicProvider.SendChatMessageColored($"({factionA.Tag}) Failed To Siege: {settings.ClaimZoneName}", Color.Violet, "[Faction Territories]", 0L, "Red");
                        new ModMessage(settings.EntityId, settings.TerritoryName, $"({factionA.Tag}) Failed To Siege: {settings.TerritoryName}", "[Faction Territories]", Color.Red);

                    settings.SiegeTimer = 0;
					Utils.ResetSiegeData(settings);
                    return;
                }

                IMyFaction factionB = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerSiegingId);

                if (!settings.IsSiegingFinal)
                {
                    settings.FinalSiegeDateTime = DateTime.Now + TimeSpan.FromSeconds(settings.ZoneDeactivationTimer);

					if (factionB != null)
                        new ModMessage(settings.EntityId, settings.TerritoryName, $"({factionB.Tag}) Successfully Init Sieged: {settings.TerritoryName} - Territory can be final sieged in {TimeSpan.FromSeconds(settings.ZoneDeactivationTimer)}", "[Faction Territories]", Color.Red);

                    settings.RecoveryTimer = 0;
                    settings.IsSieged = true;
                    settings.IsSieging = false;
                    settings.SiegeTimer = settings.ZoneDeactivationTimer;
                    settings.SiegedBy = factionB != null ? factionB.Tag : settings.PlayerSiegingId.ToString();
                    settings.BlockEmissive = EmissiveState.Sieged;
                    if (settings.JDSieging != null)
                    {
                        Utils.DisableHighlight(settings, settings.JDSieging);
                    }
                    Utils.RemoveSafeZone(settings);
                    Utils.AddSafeZone(settings);
                }
                else
				{
					if (settings.JDSieging != null)
					{
						Utils.DisableHighlight(settings, settings.JDSieging);
					}
                    settings._previousOwnerId = settings.FactionId;
					Utils.RemoveSafeZone(settings);
                    Utils.ResetClaim(settings);
					settings.IsCooling = true;
                    settings.Timer = settings.CooldownTimer;
                    settings.FinalSiegeDateTime = null;

                    if (factionB != null)
                        new ModMessage(settings.EntityId, settings.TerritoryName, $"{factionB.Tag} Successfully sieged {settings.TerritoryName}, the territory safe zone has fallen! Cooldown in effect, must wait {TimeSpan.FromSeconds(settings.Timer)} before territory can be claimed", "[Faction Territories]", Color.Red);

                }

                //Utils.MonitorSafeZonePBs(settings, true);
                Comms.SyncParticleEffect("Claimed", settings.BlockPos);
            }
        }

        public static void IsSiegedCounter(ClaimBlockSettings settings)
        {
            settings.SiegeTimer--;
            if (settings.FinalSiegeDateTime != null && settings.FinalSiegeDateTime.Value - DateTime.Now <= TimeSpan.Zero && settings.IsSieged && !settings.ReadyToSiege)
            {
                settings.FinalSiegeDateTime = null;
				settings.SiegeTimer = settings.TimeframeToSiege;
                settings.ReadyToSiege = true;
                new ModMessage(settings.EntityId, settings.TerritoryName, $"Territory Sieged Init: {settings.TerritoryName} - Territory is ready to be sieged by ({settings.SiegedBy}), time left to final siege {TimeSpan.FromSeconds(settings.SiegeTimer)}", "[Faction Territories]", Color.Red);
                //new ModMessage(settings.EntityId, settings.UnclaimName, $"Territory Sieged Init: {settings.UnclaimName} - Territory is ready to be sieged by ({settings.SiegedBy}), time left to final siege {TimeSpan.FromSeconds(settings.SiegeTimer)}", "[Faction Territories]", Color.Red);
                //Utils.RemoveSafeZone(settings);
                //Utils.ResetClaim(settings);
                //new ModMessage(settings.EntityId, settings.UnclaimName, $"Territory is now unclaimed: {settings.UnclaimName}", "[Faction Territories]", Color.Red);
                //return;
            }

            if (settings.ReadyToSiege)
            {
                if (settings.SiegeTimer % 300 == 0 && settings.SiegeTimer > 0)
                {
                    new ModMessage(settings.EntityId, settings.TerritoryName, $"Territory Sieged Init: {settings.TerritoryName} - Territory is ready to be sieged by ({settings.SiegedBy}), time left to final siege {TimeSpan.FromSeconds(settings.SiegeTimer)}", "[Faction Territories]", Color.Red);
                }

                if (settings.SiegeTimer <= 0)
                {
                    settings.SiegeTimer = 0;
					Utils.ResetSiegeData(settings, false);
                    new ModMessage(settings.EntityId, settings.TerritoryName, $"Territory Siege Failed: {settings.TerritoryName} - Territory siege has been reset", "[Faction Territories]", Color.Red);

                    return;
                }

                //settings.DetailInfo = $"\n[Territory Can Be Final Sieged For]:\n{TimeSpan.FromSeconds(settings.SiegeTimer)}\n";
            }

            if (settings.FinalSiegeDateTime != null && settings.IsSieged && !settings.ReadyToSiege && !settings.IsSiegingFinal)
            {
                if (settings.SiegeTimer % 3600 == 0)
                {
                    TimeSpan time = settings.FinalSiegeDateTime.Value - DateTime.Now;
                    string timestring = "";
                    if (time.TotalHours >= 1)
                        timestring += $"{time.Hours} hours, ";
                    if (time.TotalMinutes >= 1)
                        timestring += $"{time.Minutes} minutes, ";
                    timestring += $"{time.Seconds} seconds.";

                    new ModMessage(settings.EntityId, settings.TerritoryName, $"Territory Sieged Init: {settings.TerritoryName} - Territory has been initial sieged, time until it can be final sieged {timestring}", "[Faction Territories]", Color.Red);
                }

                //settings.DetailInfo = $"\n[Time Until Territory Can Be Final Sieged]:\n{TimeSpan.FromSeconds(settings.SiegeTimer)}\n";
                //settings.DetailInfo += $"\n[Siege Time Extended]: {settings.HoursToDelay * settings.SiegedDelayedHit} hrs ({settings.SiegedDelayedHit}/{settings.SiegeDelayAllow}) used\n";
            }
        }

        public static void IsCoolingCounter(ClaimBlockSettings settings)
        {
            settings.Timer--;
            if (settings.Timer <= 0)
            {
                settings.Timer = 0;
				settings.IsCooling = false;
                new ModMessage(settings.EntityId, settings.TerritoryName, $"Territory: {settings.TerritoryName} - Cooldown timer has expired, territory can now be claimed", "[Faction Territories]", Color.Red);
            }
        }

        public static void IsSiegeCoolingCounter(ClaimBlockSettings settings)
        {
            settings.SiegeTimer--;
            if (settings.SiegeTimer <= 0)
            {
                settings.SiegeTimer = 0;
				settings.IsSiegeCooling = false;
                new ModMessage(settings.EntityId, settings.TerritoryName, $"Territory {settings.TerritoryName}: Siege cooldown has expired and can now be init sieged", "[Faction Territories]", Color.Red);
            }
        }

        public static void CheckSafeZoneDelay()
        {
            foreach(var item in Session.Instance.claimBlocks.Values)
            {
                if (!item.Enabled || !item.IsClaimed) continue;
                if (item.GetZonesDelayRemove.Count == 0) continue;

                for (int i = item.GetZonesDelayRemove.Count - 1; i >= 0; i--)
                {
                    var zoneData = item.GetZonesDelayRemove[i];
                    IMySafeZoneBlock zoneBlock = null;
                    Session.Instance.safeZoneBlocks.TryGetValue(zoneData.zoneId, out zoneBlock);
                    if (zoneBlock == null) continue;

                    var elapsed = DateTime.Now - zoneData.time;
                    TimeSpan totalTime = new TimeSpan(1, 0, 0, 0);
                    TimeSpan timeLeft = totalTime.Subtract(elapsed);
                    if (timeLeft <= TimeSpan.Zero)
                    {
                        item.UpdateZonesDelayRemove(zoneData.zoneId, DateTime.Now, false);
                        //item.UpdateSafeZoneBlocks(zoneData.zoneId, true);
                        //zoneBlock.EnableSafeZone(false);
                        var prop = zoneBlock.GetProperty("SafeZoneCreate");
                        var prop2 = prop as ITerminalProperty<bool>;

                        if (prop2 != null)
                        {
                            prop2.SetValue(zoneBlock, false);
                        }
                        continue;
                    }

                    if (!zoneBlock.IsSafeZoneEnabled())
                    {
                        item.UpdateZonesDelayRemove(zoneData.zoneId, DateTime.Now, false);
                        //item.UpdateSafeZoneBlocks(zoneData.zoneId, true);
                    }
                }

            }
        }
    }
}
