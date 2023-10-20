using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Faction_Territories.Config;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.Gui;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;
using Faction_Territories.Network;
using VSL = Sandbox.Game.MyVisualScriptLogicProvider;

namespace Faction_Territories
{
    [Flags]
    public enum CustomSafeZoneAction
    {
        Damage = 1,
        Shooting = 2,
        Drilling = 4,
        Welding = 8,
        Grinding = 16,
        VoxelHand = 32,
        Building = 64,
        LandingGearLock = 128,
        ConvertToStation = 256,
        BuildingProjections = 512,
        All = 1023,
        AdminIgnore = 894
    }

    public enum EmissiveState
    {
        Online,
        Claimed,
        Sieged,
        Offline
    }

    public enum MyTextEnum
    {
        SafeZoneAllies,
        TerritoryAllies
    }


    public static class EnumFlagExtensions
    {
        public static TEnum NewFlag<TEnum>(this TEnum enumVar, int newFlag)
            where TEnum : struct
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), newFlag);
        }
    }

    public static class Utils
    {
        private readonly static string emissiveName = "Emissive";
        private readonly static Color Online = Color.Green;
        private readonly static Color Offline = Color.Red;
        private readonly static Color Claimed = Color.Aqua;
        private readonly static Color Sieged = Color.Orange;
        //private readonly static string UnClaimedLogo = $@"{MyAPIGateway.Utilities.GamePaths.ContentPath}\244850\2187340824\Textures\Logo\GVLogoScaled.dds";
        private readonly static string UnClaimedLogo = $@"{Session.Instance.modPath}\Textures\FactionLogo\GVLogoScaled.dds";
        //private readonly static string UnClaimedLogo = $@"Textures\FactionLogo\PirateIcon.dds";
        public static int territoryStatusDelay;

        public static Random Rnd = new Random();

        public static bool TakeTokens(IMyEntity entity, ClaimBlockSettings settings)
        {
            var jd = entity as IMyJumpDrive;
            if (jd == null) return false;

            IMyCubeGrid cubeGrid = jd.CubeGrid;
            if (cubeGrid == null) return false;

            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Players.GetPlayerControllingEntity(cubeGrid).IdentityId);
            if (!settings.IsClaimed && faction != null && settings._previousOwnerId == faction.FactionId)
            {
                return true;
            }

            int tokensToRemove = 0;
            if (!settings.IsClaimed)
                tokensToRemove = settings.TokensToClaim;
            else
                tokensToRemove = !settings.IsSiegingFinal ? settings.TokensToSiege : settings._tokensToSiegeFinal;


            //tokensToRemove = !settings.IsClaimed ? settings.TokensToClaim : settings.TokensToSiege;
            if (tokensToRemove == 0) return true;

            MyDefinitionId tokenId;
            if (!MyDefinitionId.TryParse(settings.ConsumptionItem, out tokenId)) return false;

            //Dictionary<IMyInventory, List<IMyInventoryItem>> cachedItems = new Dictionary<IMyInventory, List<IMyInventoryItem>>();
            List<IMyInventory> cachedInventory = new List<IMyInventory>();
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid).GetBlocksOfType(Blocks, x => x.HasInventory);
            MyFixedPoint tokens = 0;

            foreach (var tblock in Blocks)
            {
                tokens = 0;
                IMyInventory blockInv = tblock.GetInventory();
                tokens = blockInv.GetItemAmount(tokenId);

                if (tokens != 0)
                {
                    tokensToRemove -= (int)tokens;
                    if (cachedInventory.Contains(blockInv)) continue;
                    cachedInventory.Add(blockInv);
                }

                if (tokensToRemove <= 0) break;
            }

            if (tokensToRemove > 0) return false;

            if (!settings.IsClaimed)
                tokensToRemove = settings.TokensToClaim;
            else
                tokensToRemove = !settings.IsSiegingFinal ? settings.TokensToSiege : settings._tokensToSiegeFinal;
            foreach (MyInventory inventory in cachedInventory)
            {
                var removed = (int)inventory.RemoveItemsOfType(tokensToRemove, tokenId);
                tokensToRemove -= removed;
                if (tokensToRemove <= 0) return true;
            }

            if (tokensToRemove <= 0) return true;

                /*var invList = blockInv.GetItems();

                foreach (var item in invList)
                {
                    if (item.Content.SubtypeName.Contains(tokenId.SubtypeName))
                    {
                        tokens += item.Amount;
                        if (cachedItems.ContainsKey(blockInv as IMyInventory))
                        {
                            cachedItems[blockInv as IMyInventory].Add(item as IMyInventoryItem);
                        }
                        else
                        {
                            List<IMyInventoryItem> temp = new List<IMyInventoryItem>() { item };
                            cachedItems.Add(blockInv as IMyInventory, temp);
                        }

                        if (tokens >= tokensToRemove) break;
                    }
                }

                if (tokens >= tokensToRemove) break;*/

            /*if (tokens < tokensToRemove) return false;
            MyFixedPoint amountNeeded = tokensToRemove;
            foreach (var cache in cachedItems.Keys)
            {
                foreach (var item in cachedItems[cache])
                {
                    if (item.Amount == amountNeeded)
                    {
                        cache.RemoveItemAmount(item, item.Amount);
                        return true;
                    }

                    if (item.Amount < amountNeeded)
                    {
                        amountNeeded -= item.Amount;
                        cache.RemoveItemAmount(item, item.Amount);
                    }
                    else
                    {
                        cache.RemoveItemAmount(item, amountNeeded);
                        return true;
                    }

                    if (amountNeeded <= 0) return true;
                }*/


                /*if (cachedItems[cache].Amount == amountNeeded)
                {
                    cache.RemoveItemAmount(cachedItems[cache], cachedItems[cache].Amount);
                    return true;
                }

                if (cachedItems[cache].Amount < amountNeeded)
                {
                    amountNeeded -= cachedItems[cache].Amount;
                    cache.RemoveItemAmount(cachedItems[cache], cachedItems[cache].Amount);
                }
                else
                {
                    cache.RemoveItemAmount(cachedItems[cache], amountNeeded);
                }*/

                //if (amountNeeded <= 0) return true;

            return false;
        }

        public static string GetPendingPerksAdd(ClaimBlockSettings settings)
        {
            string result = "";
            if (settings == null) return "";
            foreach (var perk in settings.GetPerks.Keys)
            {
                PerkBase perkBase;
                if (!settings.GetPerks.TryGetValue(perk, out perkBase)) return "";
                if (perk == PerkType.Production)
                {
                    for (int i = 0; i < perkBase.perk.productionPerk.GetPendingAddUpgrades.Count; i++)
                    {
                        result += perkBase.perk.productionPerk.GetPendingAddUpgrades[i];
                        if (i < perkBase.perk.productionPerk.GetPendingAddUpgrades.Count - 1)
                            result += ", ";
                    }
                }
            }

            return result;
        }

        public static bool AnyPerksEnabled(ClaimBlockSettings settings)
        {
            if (settings == null) return false;
            if (settings.GetPerks.Count == 0) return false;

            foreach (var perk in settings.GetPerks.Keys)
            {
                if (settings.GetPerks[perk].Enable) return true;
            }

            return false;
        }

        public static string GetPendingPerksRemove(ClaimBlockSettings settings)
        {
            string result = "";
            if (settings == null) return result;
            foreach (var perk in settings.GetPerks.Keys)
            {
                PerkBase perkBase;
                if (!settings.GetPerks.TryGetValue(perk, out perkBase)) return result;
                if (perk == PerkType.Production)
                {
                    for (int i = 0; i < perkBase.perk.productionPerk.GetPendingRemoveUpgrades.Count; i++)
                    {
                        result += perkBase.perk.productionPerk.GetPendingRemoveUpgrades[i];
                        if (i < perkBase.perk.productionPerk.GetPendingRemoveUpgrades.Count - 1)
                            result += ", ";
                    }
                }
            }

            return result;
        }

        public static string GetActivePerks(ClaimBlockSettings settings)
        {
            string result = "";
            if (settings == null) return result;
            foreach (var perk in settings.GetPerks.Keys)
            {
                PerkBase perkBase;
                if (!settings.GetPerks.TryGetValue(perk, out perkBase)) return result;
                if (perk == PerkType.Production)
                {
                    for (int i = 0; i < perkBase.perk.productionPerk.GetActiveUpgrades.Count; i++)
                    {
                        result += perkBase.perk.productionPerk.GetActiveUpgrades[i];
                        if (i < perkBase.perk.productionPerk.GetActiveUpgrades.Count - 1)
                            result += ", ";
                    }
                }
            }

            return result;
        }

        public static int GetPerkCost(ClaimBlockSettings settings)
        {
            int cost = 0;
            if (settings == null) return 0;
            if (!settings.Enabled || !settings.IsClaimed) return 0;

            foreach (var perk in settings.GetPerks.Keys)
            {
                PerkBase perkBase;
                if (!settings.GetPerks.TryGetValue(perk, out perkBase)) return 0;

                cost += perkBase.PendingPerkCost;
            }

            return cost;
        }

        public static int GetActivePerkCost(ClaimBlockSettings settings)
        {
            int cost = 0;
            if (settings == null) return 0;
            if (!settings.Enabled || !settings.IsClaimed) return 0;

            foreach (var perk in settings.GetPerks.Keys)
            {
                PerkBase perkBase;
                if (!settings.GetPerks.TryGetValue(perk, out perkBase)) return 0;

                cost += perkBase.ActivePerkCost;
            }

            return cost;
        }

        public static int GetPendingPerkCost(ClaimBlockSettings settings)
        {
            int cost = 0;
            if (settings == null) return cost;
            if (!settings.Enabled || !settings.IsClaimed) return cost;

            foreach (var perk in settings.GetPerks.Keys)
            {
                PerkBase perkBase;
                if (!settings.GetPerks.TryGetValue(perk, out perkBase)) return cost;

                cost += perkBase.PendingPerkCost;
            }

            return cost;
        }

        public static bool ConsumeToken(ClaimBlockSettings settings)
        {
            int perkCost = GetPerkCost(settings) + settings.ConsumptinAmt;
            MyFixedPoint tokensNeeded = (MyFixedPoint)perkCost;
            MyDefinitionId tokenId;
            if (!MyDefinitionId.TryParse(settings.ConsumptionItem, out tokenId)) return false;
            IMyInventory blockInv = settings.Block.GetInventory();
            if (blockInv == null) return false;

            MyFixedPoint tokens = blockInv.GetItemAmount(tokenId);

            if (tokens >= tokensNeeded)
                blockInv.RemoveItemsOfType(tokensNeeded, tokenId);
            else { return false; }

            /*var invList = blockInv.GetItems();
            foreach (var item in invList)
            {
                if (item.Content.SubtypeName.Contains("ZoneChip"))
                    tokens += item.Amount;
            }

            if (tokens >= tokensNeeded)
            {
                IMyInventory inv = blockInv as IMyInventory;
                if (inv == null) return false;

                foreach (var item in invList)
                {
                    if (tokensNeeded == item.Amount)
                    {
                        inv.RemoveItemAmount((IMyInventoryItem)item, item.Amount);
                        break;
                    }

                    if (tokensNeeded > item.Amount)
                    {
                        tokensNeeded -= (int)item.Amount;
                        inv.RemoveItemAmount(item, item.Amount);
                        continue;
                    }
                    else
                    {
                        inv.RemoveItemAmount(item, tokensNeeded);
                        break;
                    }
                }
            }
            else { return false; }*/

            if (!settings.Server._perkWarning)
            {
                int perkWarningCost = perkCost * 24;

                if (perkWarningCost >= tokens)
                {
                    settings.Server._perkWarning = true;
                    new ModMessage(settings.EntityId, settings.TerritoryName, $"WARNING - Claimed Territory: {settings.ClaimZoneName} has less than 24 hours worth of tokens left before territory is reset.", "[Faction Territories]", Color.Red, settings.DiscordChannelId);

                }
            }

            /*var item = inv.FindItem(token);
            if (item == null) return false;

            inv.RemoveItemAmount(item, 1);

            inv = settings.Block.GetInventory();
            if (inv == null)
            {
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                if (faction != null)
                {
                    //MyVisualScriptLogicProvider.SendChatMessageColored($"WARNING - Claimed Territory: {settings.ClaimZoneName} is out of tokens, 1 hour until territory is unclaimed", Color.Violet, "[Faction Territories]", 0L, "Red");
                    new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"WARNING - Claimed Territory: {settings.ClaimZoneName} is out of tokens, 1 hour until territory is unclaimed", "[Faction Territories]", Color.Red);
                }

                return true;
            }

            item = inv.FindItem(token);
            if (item == null)
            {
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                if (faction != null)
                {
                    //MyVisualScriptLogicProvider.SendChatMessageColored($"WARNING - Claimed Territory: {settings.ClaimZoneName} is out of tokens, 1 hour until territory is unclaimed", Color.Violet, "[Faction Territories]", 0L, "Red");
                    new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"WARNING - Claimed Territory: {settings.ClaimZoneName} is out of tokens, 1 hour until territory is unclaimed", "[Faction Territories]", Color.Red);

                }

                return true;
            }

            if (item.Amount == 24)
            {
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                if (faction != null)
                {
                    //MyVisualScriptLogicProvider.SendChatMessageColored($"WARNING - Claimed Territory: {settings.ClaimZoneName} is low in tokens, 24 hours until territory is unclaimed", Color.Violet, "[Faction Territories]", 0L, "Red");
                    new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"WARNING - Claimed Territory: {settings.ClaimZoneName} is low in tokens, 24 hours until territory is unclaimed", "[Faction Territories]", Color.Red);

                }
            }*/

            return true;
        }

        public static bool DelaySiegeTokenConsumption(ClaimBlockSettings settings, bool consume = false)
        {
            MyFixedPoint tokensNeeded = (MyFixedPoint)settings.TokensSiegeDelay;
            MyDefinitionId tokenId;
            if (!MyDefinitionId.TryParse(settings.ConsumptionItem, out tokenId)) return false;
            IMyInventory blockInv = settings.Block.GetInventory();
            if (blockInv == null) return false;

            MyFixedPoint tokens = blockInv.GetItemAmount(tokenId);

            if (!consume)
                if (tokens >= tokensNeeded) return true;
                else return false;

            if (tokens >= tokensNeeded)
                blockInv.RemoveItemsOfType(tokensNeeded, tokenId);
            else { return false; }

            /*var invList = blockInv.GetItems();
            foreach (var item in invList)
            {
                if (item.Content.SubtypeName.Contains(tokenId.SubtypeName))
                    tokens += item.Amount;
            }

            if (!consume)
                if (tokens >= tokensNeeded) return true;
                else return false;

            if (tokens >= tokensNeeded)
            {
                IMyInventory inv = blockInv as IMyInventory;
                if (inv == null) return false;

                foreach (var item in invList)
                {
                    if (tokensNeeded == item.Amount)
                    {
                        inv.RemoveItemAmount((IMyInventoryItem)item, item.Amount);
                        break;
                    }

                    if (tokensNeeded > item.Amount)
                    {
                        tokensNeeded -= (int)item.Amount;
                        inv.RemoveItemAmount(item, item.Amount);
                        continue;
                    }
                    else
                    {
                        inv.RemoveItemAmount(item, tokensNeeded);
                        break;
                    }
                }
            }
            else { return false; }*/

            int perkCost = GetPerkCost(settings) + 1;
            if (!settings.Server._perkWarning)
            {
                int perkWarningCost = perkCost * 24;

                if (perkWarningCost >= tokens)
                {
                    settings.Server._perkWarning = true;
                    new ModMessage(settings.EntityId, settings.TerritoryName, $"WARNING - Claimed Territory: {settings.ClaimZoneName} has less than 24 hours worth of tokens left before territory is reset.", "[Faction Territories]", Color.Red);
                }
            }

            new ModMessage(settings.EntityId, settings.TerritoryName, $"Sieged Territory: {settings.ClaimZoneName} - Siege time has been exteneded additional {settings.HoursToDelay} hours, time to final siege is now {TimeSpan.FromSeconds(settings.SiegeTimer + settings.HoursToDelay * 3600)}", "[Faction Territories]", Color.Red);
            return true;
        }

        public static void AddSafeZone(ClaimBlockSettings settings, bool allowActions = true)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
            var ob = new MyObjectBuilder_SafeZone();

            ob.PositionAndOrientation = new MyPositionAndOrientation(settings.BlockPos, Vector3.Forward, Vector3.Up);
            ob.PersistentFlags = MyPersistentEntityFlags2.InScene;
            if (faction != null)
            {
                List<long> factions = new List<long> { faction.FactionId };
                if (settings.Factions.Count == 0)
                {
                    foreach (var kvp in MyAPIGateway.Session.Factions.Factions)
                    {
                        if (kvp.Key == faction.FactionId || kvp.Value.IsEveryoneNpc()) continue;
                        var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(faction.FactionId, kvp.Key);
                        if (relation == MyRelationsBetweenFactions.Enemies) continue;
                        if (settings.NeutralEnemies && relation == MyRelationsBetweenFactions.Neutral) continue;
						if (!factions.Contains(kvp.Key))
							factions.Add(kvp.Key);
                    }
                    settings._factions.Clear();
                    settings._factions.AddList(factions);
                }
                else
                {
                    foreach (long id in settings.Factions)
                    {
                        if (!factions.Contains(id))
                            factions.Add(id);
                    }
                }
                ob.Factions = factions.ToArray();
                ob.AccessTypeFactions = MySafeZoneAccess.Whitelist;
            }
            else
            {
                ob.Players = new long[] { settings.PlayerClaimingId };
                ob.AccessTypePlayers = MySafeZoneAccess.Whitelist;
            }

            ob.Shape = MySafeZoneShape.Sphere;
            ob.Radius = settings.SafeZoneSize;
            ob.DisplayName = "(FactionTerritory)" + "_" + settings.TerritoryName + "_" + settings.EntityId.ToString();
            ob.Enabled = true;
            if (!settings.VisibleSZ)
            {
                ob.Texture = "SafeZone_Texture_Disabled";
                ob.ModelColor = Color.Black.ToVector3();
            }
            else if (ActionControls.SZTextures.Count > 0)
            {
                ob.Texture = ActionControls.SZTextures[settings.SZTexture].Value.String;
                ob.ModelColor = settings.SZColor.ToVector3();
            }

            if (allowActions)
            {
                int allowedFlags = (int)ob.AllowedActions;
                allowedFlags = allowedFlags | (int)CustomSafeZoneAction.Grinding;
                allowedFlags = allowedFlags | (int)CustomSafeZoneAction.Welding;
                allowedFlags = allowedFlags | (int)CustomSafeZoneAction.Building;
                allowedFlags = allowedFlags | (int)CustomSafeZoneAction.Drilling;
                allowedFlags = allowedFlags | (int)CustomSafeZoneAction.ConvertToStation;
                allowedFlags = allowedFlags | (int)CustomSafeZoneAction.BuildingProjections;
                allowedFlags = allowedFlags | (int)CustomSafeZoneAction.LandingGearLock;
                ob.AllowedActions = MySessionComponentSafeZones.AllowedActions.NewFlag(allowedFlags);
            }

			MySafeZone zone = MyEntities.CreateFromObjectBuilderAndAdd(ob, true) as MySafeZone;
            if (zone == null) return;

            zone.AccessTypeGrids = KeenumKonverter(zone.AccessTypeGrids, 1);
            zone.AccessTypeFloatingObjects = KeenumKonverter(zone.AccessTypeFloatingObjects, 1);
            zone.Radius = settings.SafeZoneSize;
            zone.DisplayName = "(FactionTerritory)" + "_" + settings.TerritoryName + "_" + settings.EntityId.ToString();
			zone.RecreatePhysics();
            settings.SafeZoneEntity = zone.EntityId;
        }

        private static T KeenumKonverter<T>(T obj, int value)
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        public static void RemoveSafeZone(ClaimBlockSettings settings)
        {
            IMyEntity entity;
            if (!MyAPIGateway.Entities.TryGetEntityById(settings.SafeZoneEntity, out entity))
            {
                var zones = MySessionComponentSafeZones.SafeZones;
                foreach(var sz in zones)
                {
                    if (sz == null || sz.MarkedForClose) continue;
                    if (sz.DisplayName == null) continue;
                    if (sz.DisplayName.Contains(settings.TerritoryName))
                    {
                        entity = sz as IMyEntity;
                        break;
                    }
                    if (sz.PositionComp.WorldVolume.Contains(settings.BlockPos) == ContainmentType.Contains)
                    {
                        entity = sz as IMyEntity;
                        break;
                    }
                }
            }

            settings.SafeZoneEntity = 0;
            if (entity == null) return;
            MySafeZone zone = entity as MySafeZone;
            if (zone == null || zone.MarkedForClose) return;

            zone.Close();
        }

        public static void ResetClaim(ClaimBlockSettings settings)
		{
			if (settings.JDClaiming != null)
				DisableHighlight(settings, settings.JDClaiming);
			RemovePerks(settings);
			settings.FinalSiegeDateTime = null;
			settings.RecoveryTimer = 0;
            settings.IsClaiming = false;
            settings.IsClaimed = false;
            settings.IsSieging = false;
            settings.IsSieged = false;
            settings.PlayerClaimingId = 0;
            settings.PlayerSiegingId = 0;
            settings.JDClaimingId = 0;
            settings.JDClaiming = null;
            settings.JDSieging = null;
            settings.JDSiegingId = 0;
            settings.ClaimedFaction = "";
            settings.FactionId = 0;
            settings.Factions.Clear();
            settings.FactionsExempt.Clear();
            settings.FactionsRadar.Clear();
            settings.SafeZoneEntity = 0;
            settings.ClaimZoneName = settings.TerritoryName;
            settings.DetailInfo = "";
            settings.ReadyToSiege = false;
            settings.IsSiegingFinal = false;
            settings.IsSiegeFinal = false;
            settings.SiegedDelayedHit = 0;
            settings.SiegedBy = "";
            settings._safeZones.Clear();
            settings._zonesDelay.Clear();
            settings._allowSafeZoneAllies = false;
            settings._allowTerritoryAllies = false;
            settings._allianceId = 0;
            settings.IsCooling = false;
            settings.IsSiegeCooling = false;
            settings.BlockEmissive = settings.Enabled ? EmissiveState.Online : EmissiveState.Offline;
            settings.GetTerritoryStatus = TerritoryStatus.Neutral;
            GPS.RemoveCachedGps(0, GpsType.Tag, settings);
            GPS.RemoveCachedGps(0, GpsType.Player, settings);
            GPS.UpdateBlockText(settings, $"Unclaimed Territory: {settings.TerritoryName}");
            //SetScreen(settings.Block as IMyBeacon, null, true);
            SetOwner(settings.Block);
            //MonitorSafeZonePBs(settings, true);
        }

        public static void ResetSiegeData(ClaimBlockSettings settings, bool resetSafeZone = true)
		{
			if (settings.JDSieging != null)
				DisableHighlight(settings, settings.JDSieging);
            settings.FinalSiegeDateTime = null;
			settings.RecoveryTimer = 0;
            settings.IsSieging = false;
            settings.IsSieged = false;
            settings.ReadyToSiege = false;
            settings.IsSiegingFinal = false;
            settings.IsSiegeFinal = false;
            settings.SiegedBy = "";
            settings.JDSieging = null;
            settings.JDSiegingId = 0;
            settings.PlayerSiegingId = 0;
            settings.SiegedDelayedHit = 0;
            settings.BlockEmissive = EmissiveState.Claimed;
            //MonitorSafeZonePBs(settings, true);

            if (!resetSafeZone) return;
            RemoveSafeZone(settings);
            AddSafeZone(settings);
        }

        public static void DrainAllJDs(IMyEntity entity)
        {
            if (entity == null) return;
            IMyJumpDrive jd = entity as IMyJumpDrive;
            if (jd == null) return;
            IMyCubeGrid cubeGrid = jd.CubeGrid;
            if (cubeGrid == null) return;

            List<IMyJumpDrive> Blocks = new List<IMyJumpDrive>();
            MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid).GetBlocksOfType(Blocks, x => x.IsFunctional);

            foreach (var item in Blocks)
            {
                item.CurrentStoredPower = 0f;
            }

        }

        public static bool CheckPlayerandBlock(ClaimBlockSettings settings)
        {
            IMyEntity targetEntity = settings.IsClaiming ? settings.JDClaiming : settings.JDSieging;
            //IMyPlayer targetPlayer = settings.IsClaiming ? Triggers.GetPlayerFromId(settings.PlayerClaimingId) : Triggers.GetPlayerFromId(settings.PlayerSiegingId);
            double targetDistance = settings.IsClaiming ? settings.DistanceToClaim : settings.DistanceToSiege;
            //if (targetPlayer == null) return false;

            if (targetEntity == null)
            {
                long targetId = settings.IsClaiming ? settings.JDClaimingId : settings.JDSiegingId;
                if (!MyAPIGateway.Entities.TryGetEntityById(targetId, out targetEntity)) return false;

                if (settings.IsClaiming)
                    settings.JDClaiming = targetEntity;
                else
                    settings.JDSieging = targetEntity;
            }

            /*if (targetPlayer.Character == null || targetPlayer.Character.IsDead)
            {
                //MyVisualScriptLogicProvider.ShowNotification($"Character is null", 3000);
                return false;
            }*/

            //if (Vector3D.Distance(targetPlayer.Character.GetPosition(), settings.BlockPos) > targetDistance) return false;

            IMyJumpDrive jd = targetEntity as IMyJumpDrive;
            if (jd == null || jd.MarkedForClose) return false;

            if (!jd.IsFunctional) return false;
            if (Vector3D.Distance(jd.GetPosition(), settings.BlockPos) > targetDistance) return false;
            return true;
        }

        public static void TagEnemyGrids(ClaimBlockSettings settings)
        {
            if (!settings.IsClaimed) return;
            if (settings.GetGridsInside.Count == 0) return;
            foreach (var data in settings.GetGridsInside.Values)
            {

                MyCubeGrid cubeGrid = data.cubeGrid;
                if (cubeGrid == null) continue;

                if (!data.hasController || !data.hasPower) continue;
                var owner = cubeGrid.BigOwners.FirstOrDefault();
                //IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);

                if (faction != null && faction.FactionId != settings.FactionId)
                {
					var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(settings.FactionId, faction.FactionId);
					if (relation == MyRelationsBetweenFactions.Enemies || (settings.NeutralEnemies && relation == MyRelationsBetweenFactions.Neutral))
					{
						GPS.TagEnemyGrid(cubeGrid, settings);
					}
					continue;
                }

                GPS.TagEnemyGrid(cubeGrid, settings);
            }
        }

        public static void TagEnemyPlayers(ClaimBlockSettings settings)
        {
            if (!settings.IsClaimed) return;
            if (settings.GetPlayersInside.Count == 0) return;
            foreach (var data in settings.GetPlayersInside.Keys)
            {
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(data);
                IMyPlayer player = Triggers.GetPlayerFromId(data);
                if (player == null) continue;

                if (faction != null && faction.FactionId != settings.FactionId)
				{
					var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(settings.FactionId, faction.FactionId);
					if (relation == MyRelationsBetweenFactions.Enemies || (settings.NeutralEnemies && relation == MyRelationsBetweenFactions.Neutral))
                    {
						GPS.TagEnemyPlayer(player, settings);
                    }
                    continue;
				}

				GPS.TagEnemyPlayer(player, settings);
			}
        }

        public static void CheckGridsToTag(long playerId)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId);
            if (faction == null) return;

            foreach (var settings in Session.Instance.claimBlocks.Values)
            {
                if (settings.ClaimedFaction == faction.Tag)
                {
                    foreach (var gridData in settings.GetGridsInside.Values)
                    {
                        MyCubeGrid cubeGrid = gridData.cubeGrid;
                        if (cubeGrid == null) continue;
                        if (!gridData.hasController || !gridData.hasPower) continue;

                        var owner = cubeGrid.BigOwners.FirstOrDefault();
                        IMyFaction gridFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);

                        if (gridFaction != null && gridFaction.FactionId != settings.FactionId)
						{
							var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(settings.FactionId, gridFaction.FactionId);
							if (relation == MyRelationsBetweenFactions.Enemies || (settings.NeutralEnemies && relation == MyRelationsBetweenFactions.Neutral))
							{
								GPS.TagEnemyGrid(cubeGrid, settings);
                            }
                            continue;
						}
						GPS.TagEnemyGrid(cubeGrid, settings);
					}
                }
            }
        }

        public static void CheckPlayersToTag(long playerId)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId);
            if (faction == null) return;

            foreach (var settings in Session.Instance.claimBlocks.Values)
            {
                if (settings.ClaimedFaction == faction.Tag)
                {
                    foreach (var player in settings.GetPlayersInside.Keys)
                    {
                        IMyFaction playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player);
                        IMyPlayer p = Triggers.GetPlayerFromId(player);
                        if (p == null) continue;

                        if (playerFaction != null)
                        {
                            var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(settings.FactionId, playerFaction.FactionId);
							if (relation == MyRelationsBetweenFactions.Enemies || (settings.NeutralEnemies && relation == MyRelationsBetweenFactions.Neutral))
                            {
                                GPS.TagEnemyPlayer(p, settings);
                            }
                            continue;
						}
						GPS.TagEnemyPlayer(p, settings);
					}
                }
            }
        }

        public static void StopHandTools()
        {
            //List<IMyPlayer> players = new List<IMyPlayer>();
            //MyAPIGateway.Players.GetPlayers(players);
            //
            //foreach (var player in players)
            //{
            //    if (player.SteamUserId <= 0 || player.IsBot) continue;
            //    Session.Instance.ToolEquipped(player.IdentityId, "", "");
            //}
        }

        public static void RemoveGridData(ClaimBlockSettings settings, MyCubeGrid cubeGrid = null)
        {
            var keys = settings.GetGridsInside.Keys.ToList();
            //MyVisualScriptLogicProvider.ShowNotification($"Grid Count Before = {keys.Count}", 10000);
            if (cubeGrid == null)
            {
                for (int i = keys.Count - 1; i >= 0; i--)
                {
                    settings.UpdateGridsInside(keys[i], settings._server._gridsInside[keys[i]].cubeGrid, false);
                }
            }
            else
            {
                for (int i = keys.Count - 1; i >= 0; i--)
                {
                    if (settings._server._gridsInside[keys[i]].cubeGrid == cubeGrid)
                    {
                        settings.UpdateGridsInside(keys[i], settings._server._gridsInside[keys[i]].cubeGrid, false);
                        break;
                    }
                }
            }

            //MyVisualScriptLogicProvider.ShowNotification($"Grid Count After = {Session.Instance.claimBlocks[settings.EntityId].GetGridsInside.Count}", 10000);
        }

        public static void GetSurroundingSafeZones(ClaimBlockSettings settings)
        {
            BoundingSphereD sphere = new BoundingSphereD(settings.CenterToPlanet ? settings.PlanetCenter : settings.BlockPos, settings.ClaimRadius);
            List<IMyEntity> entities = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref sphere);

            foreach (var entity in entities)
            {
                MySafeZone zone = entity as MySafeZone;
                if (zone == null) continue;

                long zoneBlockId = zone.SafeZoneBlockId;
                settings.UpdateSafeZones(zone.EntityId, true);
                if (zoneBlockId == 0) continue;

                IMySafeZoneBlock zoneBlock = null;
                Session.Instance.safeZoneBlocks.TryGetValue(zoneBlockId, out zoneBlock);
                if (zoneBlock == null) continue;

                IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(zoneBlock.OwnerId);

                if (claimFaction != null && blockFaction != null && claimFaction != blockFaction)
                {
                    //MyVisualScriptLogicProvider.ShowNotification("Added Safe Zone Info", 5000);
                    if (zoneBlock.IsSafeZoneEnabled())
                        settings.UpdateZonesDelayRemove(zoneBlockId, DateTime.Now, true);
                    //else
                    //settings.UpdateSafeZoneBlocks(zoneBlockId, true);
                }
            }
        }

        public static void CheckTools(ClaimBlockSettings settings)
        {
            if (settings.GetGridsInside.Count == 0 || settings.AllowTools) return;

            foreach (var data in settings.GetGridsInside.Values)
            {
                foreach (var drill in data.blocksMonitored.drills)
                {
                    if (settings.AllowDrilling) return;
                    if (drill == null || drill.MarkedForClose) continue;
                    IMyFaction toolFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(drill.OwnerId);
                    if (toolFaction != null)
                    {
                        if (toolFaction.Tag == settings.ClaimedFaction || settings.FactionsExempt.Contains(toolFaction.FactionId)) continue;
                        //if (settings.AllowSafeZoneAllies || settings.AllowTerritoryAllies)
                        //{
                        //    if (!Utils.IsFactionEnemy(settings, toolFaction)) continue;
                        //}
                    }

                    var toolbase = (IMyGunObject<MyToolBase>)drill;
                    if (toolbase == null) continue;
                    if (!toolbase.IsShooting) continue;
                    toolbase.EndShoot(MyShootActionEnum.PrimaryAction);
                    toolbase.EndShoot(MyShootActionEnum.SecondaryAction);

                    var player = MyAPIGateway.Players.GetPlayerControllingEntity(drill.CubeGrid);
                    if (player == null) continue;

                    //MyVisualScriptLogicProvider.PlayHudSoundLocal(VRage.Audio.MyGuiSounds.HudErrorMessage, player.IdentityId);
                    Comms.SendAudioToClient(player, player.IdentityId, "RealHudUnable");
                    //MyVisualScriptLogicProvider.ShowNotification("No Tools Allowed Inside Claimed Territory...", 3000, "Red", player.IdentityId);
                }

                foreach (var tool in data.blocksMonitored.tools)
                {
                    if (settings.AllowWelding && tool is IMyShipWelder) continue;
                    if (settings.AllowGrinding && tool is IMyShipGrinder) continue;

                    if (tool == null || tool.MarkedForClose) continue;
                    IMyFaction toolFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(tool.OwnerId);
                    if (toolFaction != null)
                    {
                        if (toolFaction.Tag == settings.ClaimedFaction || settings.FactionsExempt.Contains(toolFaction.FactionId)) continue;
                        if (settings.AllowSafeZoneAllies || settings.AllowTerritoryAllies)
                        {
                            if (!Utils.IsFactionEnemy(settings, toolFaction)) continue;
                        }
                    }

                    var toolbase = (IMyGunObject<MyToolBase>)tool;
                    if (toolbase == null) continue;
                    if (!toolbase.IsShooting) continue;
                    toolbase.EndShoot(MyShootActionEnum.PrimaryAction);

                    var player = MyAPIGateway.Players.GetPlayerControllingEntity(tool.CubeGrid);
                    if (player == null) continue;

                    //MyVisualScriptLogicProvider.PlayHudSoundLocal(VRage.Audio.MyGuiSounds.HudErrorMessage, player.IdentityId);
                    Comms.SendAudioToClient(player, player.IdentityId, "RealHudUnable");
                    //MyVisualScriptLogicProvider.ShowNotification("No Tools Allowed Inside Claimed Territory...", 3000, "Red", player.IdentityId);
                }
            }
        }

        public static void SetEmissive(EmissiveState state, IMyBeacon myBeacon)
        {
            if (myBeacon == null) return;
            if (state == EmissiveState.Online)
            {
                myBeacon.SetEmissiveParts(emissiveName, Online, 1f);
                return;
            }

            if (state == EmissiveState.Offline)
            {
                myBeacon.SetEmissiveParts(emissiveName, Offline, 1f);
                return;
            }

            if (state == EmissiveState.Claimed)

            {
                myBeacon.SetEmissiveParts(emissiveName, Claimed, 1f);
                return;
            }

            if (state == EmissiveState.Sieged)
            {
                myBeacon.SetEmissiveParts(emissiveName, Sieged, 1f);
                return;
            }
        }

        public static void SetScreen(IMyBeacon beacon, IMyFaction faction = null, bool sync = false)
        {
            try
            {
                return;
                if (beacon == null) return;
                var screenAreaRender = beacon.Render.GetAs<MyRenderComponentScreenAreas>();

                if (screenAreaRender == null)
                {
                    var renderId = beacon.Render.RenderObjectIDs[0];
                    MyVisualScriptLogicProvider.ShowNotification($"Render Id Before = {beacon.Render.RenderObjectIDs[0]}", 25000);
                    screenAreaRender = new MyRenderComponentScreenAreas((MyEntity)beacon);
                    beacon.Render.Container.Add(screenAreaRender);
                    //beacon.Render.SetRenderObjectID(0, renderId);
                    beacon.Render.AddRenderObjects();
                    screenAreaRender = beacon.Render.GetAs<MyRenderComponentScreenAreas>();

                    //screenAreaRender.AddRenderObjects();
                    MyVisualScriptLogicProvider.ShowNotification($"Render Id AFter = {beacon.Render.RenderObjectIDs[0]}", 25000);
                    //screenAreaRender.AddScreenArea(beacon.Render.RenderObjectIDs, "ScreenArea");
                    //screenAreaRender.UpdateModelProperties();

                    //IMyCubeBlock block = beacon as IMyCubeBlock;
                    //var useObject = block.UseObjectsComponent;
                    //useObject.LoadDetectorsFromModel();
                    //block.ReloadDetectors();


                }

                if (screenAreaRender != null)
                {
                    if (faction == null)
                    {
                        //MyVisualScriptLogicProvider.ShowNotification($"Logo = {UnClaimedLogo}", 15000);
                        screenAreaRender.ChangeTexture(0, UnClaimedLogo);
                    }
                    else
                    {
                        screenAreaRender.ChangeTexture(0, $"{faction.FactionIcon}");
                        //MyAPIGateway.Utilities.ShowNotification($"{faction.FactionIcon}", 25000, "Green");
                    }
                }

                if (sync)
                {
                    if (faction != null)
                        Comms.SyncBillBoard(beacon.EntityId, MyAPIGateway.Session.LocalHumanPlayer, faction?.Tag);
                    else
                        Comms.SyncBillBoard(beacon.EntityId, MyAPIGateway.Session.LocalHumanPlayer);
                }

            }
            catch (Exception ex)
            {
                MyVisualScriptLogicProvider.ShowNotification($"{ex.ToString()}", 25000);
                //MyLog.Default.WriteLineAndConsole($"{ex.StackTrace}");
            }

        }

        public static void PlayParticle(string effect, Vector3D pos)
        {
            MatrixD hitParticleMatrix = MatrixD.CreateWorld(pos, Vector3.Forward, Vector3.Up);
            MyParticleEffect particle = null;
            MyParticlesManager.TryCreateParticleEffect(effect, ref hitParticleMatrix, ref pos, uint.MaxValue, out particle);

            if (particle == null) return;
            particle.UserScale = 10f;
        }

        public static float GetAttachedUpgradeModules(MyCubeBlock block, string type)
        {
            float num = 0;
            var modules = block.CurrentAttachedUpgradeModules;
            if (modules == null) return 0f;
            if (modules.Count == 0) return 0f;
            foreach (var module in modules.Values)
            {
                if (!module.Block.IsWorking) continue;
                int slotCount = module.SlotCount;
                //uint connections = module.Block.Connections;
                //uint upgradeCount = module.Block.UpgradeCount;

                var upgradeModule = module.Block as MyCubeBlock;
                var def = upgradeModule.BlockDefinition;
                var defModule = def as MyUpgradeModuleDefinition;
                if (defModule != null)
                {
                    var upgrades = defModule.Upgrades;
                    foreach (var upgrade in upgrades)
                    {
                        if (type == upgrade.UpgradeType)
                        {
                            if (upgrade.ModifierType == MyUpgradeModifierType.Additive)
                                num += upgrade.Modifier * slotCount;
                            else
                                num *= upgrade.Modifier * slotCount;
                        }
                    }

                    //MyVisualScriptLogicProvider.ShowNotificationToAll($"Upgrade connections = {connections} | SlotCount = {slotCount} | UpgradeCount = {upgradeCount} | Value = {num} | Type = {type}", 25000, "Green");
                }
            }

            return num;
        }

        public static float GetCurrentUpgradeValue(MyCubeBlock block, string type)
        {
            float num = 0;
            var upgradeValues = block.UpgradeValues;
            foreach (var upgrade in upgradeValues.Keys)
            {
                if (upgrade == type)
                {
                    num += upgradeValues[upgrade];
                }
            }

            return num;
        }

        public static void SetUpgradeValue(MyCubeBlock block, string type, float value, bool sync = false)
        {
            if (!block.UpgradeValues.ContainsKey(type)) return;
            block.UpgradeValues[type] = value;
            block.CommitUpgradeValues();

            if (sync)
                Comms.SyncProductionPerk(block.EntityId, type, value);
            //MyVisualScriptLogicProvider.ShowNotificationToAll($"Production Perk Added = {value} - Server = {Session.Instance.isServer}", 20000, "Green");
        }

        public static void AddPerks(ClaimBlockSettings settings, MyCubeBlock block = null)
        {
            if (settings == null) return;
            if (settings.GetPerks == null || settings.GetPerks.Count == 0) return;

            foreach (var item in settings.GetPerks.Values)
            {
                if (item.type == PerkType.Production)
                {
                    AddAllProductionMultipliers(settings, block, true);
                }
            }
        }

        public static void RemovePerks(ClaimBlockSettings settings)
        {
            if (settings == null) return;
            if (settings.GetPerks == null || settings.GetPerks.Count == 0)
            {
                //MyVisualScriptLogicProvider.ShowNotification("No perks here!!", 8000, "Red");
                return;
            }

            foreach (var item in settings.GetPerks.Values)
            {
                if (item.type == PerkType.Production)
                {
                    RemoveProductionMultipliers(settings, null, true);
                    settings.GetPerks[PerkType.Production].perk.productionPerk.pendingAddUpgrades.Clear();
                    settings.GetPerks[PerkType.Production].perk.productionPerk.pendingRemoveUpgrades.Clear();
                    settings.GetPerks[PerkType.Production].perk.productionPerk.activeUpgrades.Clear();
                    settings.GetPerks[PerkType.Production].perk.productionPerk.enableClientControlSpeed = false;
                    settings.GetPerks[PerkType.Production].perk.productionPerk.enableClientControlYield = false;
                    settings.GetPerks[PerkType.Production].perk.productionPerk.enableClientControlEnergy = false;
                }
            }
        }

        public static void RemovePerksFromGrid(ClaimBlockSettings settings, MyCubeGrid grid)
        {
            if (settings == null) return;
            if (settings.GetGridsInside.Count == 0 || settings.GetGridsInside.ContainsKey(grid.EntityId)) return;

            foreach (var item in settings.GetPerks.Values)
            {
                if (item.type == PerkType.Production)
                {
                    foreach (var gridData in settings.GetGridsInside.Values)
                    {
                        foreach (var production in gridData.blocksMonitored.production)
                        {
                            RemoveProductionMultipliers(settings, production, true);
                        }
                    }
                }
            }
        }

        public static void RemovePerkType(ClaimBlockSettings settings, PerkType perkType)
        {
            if (settings == null) return;
            if (settings.GetPerks == null || settings.GetPerks.Count == 0) return;

            foreach (var item in settings.GetPerks.Values)
            {
                if (item.type == perkType)
                {
                    RemoveProductionMultipliers(settings, null, true);
                    settings.GetPerks[PerkType.Production].perk.productionPerk.pendingAddUpgrades.Clear();
                    settings.GetPerks[PerkType.Production].perk.productionPerk.pendingRemoveUpgrades.Clear();
                    settings.GetPerks[PerkType.Production].perk.productionPerk.activeUpgrades.Clear();
                    settings.GetPerks[PerkType.Production].perk.productionPerk.enableClientControlSpeed = false;
                    settings.GetPerks[PerkType.Production].perk.productionPerk.enableClientControlYield = false;
                    settings.GetPerks[PerkType.Production].perk.productionPerk.enableClientControlEnergy = false;
                }
            }

            settings.Server.Sync = true;
        }

        public static void UpdateSingleProductionMultiplier(ClaimBlockSettings settings, MyCubeBlock block, string upgradeName, bool add = false, bool sync = false)
        {
            if (settings == null) return;
            if (settings.GetGridsInside.Count == 0) return;
            if (!settings.GetPerks.ContainsKey(PerkType.Production)) return;
            if (!settings.GetPerks[PerkType.Production].enabled) return;

            float speed = settings.GetPerks[PerkType.Production].perk.productionPerk.Speed;
            float yield = settings.GetPerks[PerkType.Production].perk.productionPerk.Yield;
            float energy = settings.GetPerks[PerkType.Production].perk.productionPerk.Energy;

            if (block != null)
            {
                if (!block.IsFunctional) return;
                //if (settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Contains(block.EntityId)) return;
                IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                if (blockFaction == null) return;

                if (settings.ClaimedFaction == blockFaction.Tag)
                {

                    float num3 = GetAttachedUpgradeModules(block, "Productivity");
                    float num4 = GetAttachedUpgradeModules(block, "Effectiveness");
                    float num5 = GetAttachedUpgradeModules(block, "PowerEfficiency");

                    if (!settings.GetPerks[PerkType.Production].perk.productionPerk.allowStandAlone)
                    {
                        if (upgradeName == "Productivity")
                        {
                            if (add)
                                SetUpgradeValue(block, "Productivity", speed + num3, sync);
                            else
                                SetUpgradeValue(block, "Productivity", num3, sync);
                        }


                        if (upgradeName == "Effectiveness")
                        {
                            if (add)
                                SetUpgradeValue(block, "Effectiveness", yield + num4 + 1, sync);
                            else
                                SetUpgradeValue(block, "Effectiveness", num4 + 1, sync);
                        }

                        if (upgradeName == "PowerEfficiency")
                        {
                            if (add)
                                SetUpgradeValue(block, "PowerEfficiency", energy + num5 + 1, sync);
                            else
                                SetUpgradeValue(block, "PowerEfficiency", num5 + 1, sync);
                        }

                        if (settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades.Count == 0)
                            settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(block.EntityId, false);
                        else
                            settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(block.EntityId, true);

                        Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionAttached);
                    }
                }

                return;
            }

            foreach (var gridData in settings.GetGridsInside.Values)
            {
                if (gridData.blocksMonitored.production.Count == 0) continue;
                foreach (var production in gridData.blocksMonitored.production)
                {
                    //if (settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Contains(production.EntityId)) continue;
                    if (!production.IsFunctional) return;
                    IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(production.OwnerId);
                    if (blockFaction == null) continue;

                    if (settings.ClaimedFaction == blockFaction.Tag)
                    {
                        float num3 = GetAttachedUpgradeModules(production as MyCubeBlock, "Productivity");
                        float num4 = GetAttachedUpgradeModules(production as MyCubeBlock, "Effectiveness");
                        float num5 = GetAttachedUpgradeModules(production as MyCubeBlock, "PowerEfficiency");

                        if (!settings.GetPerks[PerkType.Production].perk.productionPerk.allowStandAlone)
                        {
                            if (upgradeName == "Productivity")
                            {
                                if (add)
                                    SetUpgradeValue(production as MyCubeBlock, "Productivity", speed + num3, sync);
                                else
                                    SetUpgradeValue(production as MyCubeBlock, "Productivity", num3, sync);
                            }


                            if (upgradeName == "Effectiveness")
                            {
                                if (add)
                                    SetUpgradeValue(production as MyCubeBlock, "Effectiveness", yield + num4 + 1, sync);
                                else
                                    SetUpgradeValue(production as MyCubeBlock, "Effectiveness", num4 + 1, sync);
                            }

                            if (upgradeName == "PowerEfficiency")
                            {
                                if (add)
                                    SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", energy + num5 + 1, sync);
                                else
                                    SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", num5 + 1, sync);
                            }

                            if (settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades.Count == 0)
                            {
                                //settings.GetPerks[PerkType.Production].perk.productionPerk.ProductionRunning = false;
                                settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(production.EntityId, false);
                            }

                            else
                            {
                                //settings.GetPerks[PerkType.Production].perk.productionPerk.ProductionRunning = true;
                                settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(production.EntityId, true);
                            }

                        }

                        continue;
                    }
                }
            }

            Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionAttached);
            //Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionRunning);
        }

        public static void AddProductionMultipliersToClient(MyCubeBlock block, ClaimBlockSettings settings)
        {
            if (block == null || settings == null) return;
            if (!block.IsFunctional) return;
            if (!settings.GetPerks.ContainsKey(PerkType.Production)) return;
            if (!settings.GetPerks[PerkType.Production].enabled) return;

            float speed = settings.GetPerks[PerkType.Production].perk.productionPerk.Speed;
            float yield = settings.GetPerks[PerkType.Production].perk.productionPerk.Yield;
            float energy = settings.GetPerks[PerkType.Production].perk.productionPerk.Energy;

            float num3 = GetAttachedUpgradeModules(block, "Productivity");
            float num4 = GetAttachedUpgradeModules(block, "Effectiveness");
            float num5 = GetAttachedUpgradeModules(block, "PowerEfficiency");

            if (settings.GetPerks[PerkType.Production].perk.productionPerk.allowStandAlone)
            {
                SetUpgradeValue(block, "Productivity", speed + num3);
                SetUpgradeValue(block, "Effectiveness", yield + num4 + 1);
                SetUpgradeValue(block, "PowerEfficiency", energy + num5 + 1);
            }
            else
            {
                if (settings.GetPerks[PerkType.Production].perk.productionPerk.ProductionRunning)
                {
                    foreach (var upgrade in settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades)
                    {
                        if (upgrade == "Productivity")
                            SetUpgradeValue(block, "Productivity", speed + num3);

                        if (upgrade == "Effectiveness")
                            SetUpgradeValue(block, "Effectiveness", yield + num4 + 1);

                        if (upgrade == "PowerEfficiency")
                            SetUpgradeValue(block, "PowerEfficiency", energy + num5 + 1);
                    }
                }
            }
        }

        public static void AddAllProductionMultipliers(ClaimBlockSettings settings, MyCubeBlock block, bool sync = false)
        {
            if (settings == null) return;
            if (settings.GetGridsInside.Count == 0)
            {
                //MyVisualScriptLogicProvider.ShowNotificationToAll($"no grids", 8000);
                return;
            }
            if (!settings.GetPerks.ContainsKey(PerkType.Production))
            {
                //MyVisualScriptLogicProvider.ShowNotificationToAll($"no perks", 8000);
                return;
            }

            if (!settings.GetPerks[PerkType.Production].enabled) return;

            float speed = settings.GetPerks[PerkType.Production].perk.productionPerk.Speed;
            float yield = settings.GetPerks[PerkType.Production].perk.productionPerk.Yield;
            float energy = settings.GetPerks[PerkType.Production].perk.productionPerk.Energy;

            if (block != null)
            {
                if (!block.IsFunctional) return;
                //if (settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Contains(block.EntityId)) return;
                IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                if (blockFaction == null)
                {
                    //MyLog.Default.WriteLineAndConsole($"Block Faction is null = {block.OwnerId}");
                    return;
                    /*blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.CubeGrid.BigOwners.FirstOrDefault());
                    if (blockFaction == null)
                    {
                        MyLog.Default.WriteLineAndConsole($"Grid has no faction");
                        return;
                    }*/
                }

                if (settings.ClaimedFaction == blockFaction.Tag)
                {
                    //float num = GetCurrentUpgradeValue(block, "Productivity");
                    //float num1 = GetCurrentUpgradeValue(block, "Effectiveness");
                    //float num2 = GetCurrentUpgradeValue(block, "PowerEfficiency");

                    float num3 = GetAttachedUpgradeModules(block, "Productivity");
                    float num4 = GetAttachedUpgradeModules(block, "Effectiveness");
                    float num5 = GetAttachedUpgradeModules(block, "PowerEfficiency");

                    if (settings.GetPerks[PerkType.Production].perk.productionPerk.allowStandAlone)
                    {
                        SetUpgradeValue(block, "Productivity", speed + num3, sync);
                        SetUpgradeValue(block, "Effectiveness", yield + num4 + 1, sync);
                        SetUpgradeValue(block, "PowerEfficiency", energy + num5 + 1, sync);
                    }
                    else
                    {
                        if (settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades.Count == 0) return;
                        if (!settings.GetPerks[PerkType.Production].perk.productionPerk.ProductionRunning) return;
                        foreach (var item in settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades)
                        {
                            if (item == "Productivity")
                                SetUpgradeValue(block, "Productivity", speed + num3, sync);

                            if (item == "Effectiveness")
                                SetUpgradeValue(block, "Effectiveness", yield + num4 + 1, sync);

                            if (item == "PowerEfficiency")
                                SetUpgradeValue(block, "PowerEfficiency", energy + num5 + 1, sync);

                        }
                    }


                    settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(block.EntityId, true);
                    Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionAttached);
                    //settings.Server.Sync = true;
                }

                return;
            }

            foreach (var gridData in settings.GetGridsInside.Values)
            {
                if (gridData.blocksMonitored.production.Count == 0)
                {
                    //MyVisualScriptLogicProvider.ShowNotificationToAll($"no production | isServer = {MyAPIGateway.Multiplayer.IsServer}", 8000);
                    continue;
                }
                foreach (var production in gridData.blocksMonitored.production)
                {
                    //if (settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Contains(production.EntityId)) continue;
                    if (!production.IsFunctional) continue;
                    IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(production.OwnerId);
                    if (blockFaction == null) continue;

                    if (settings.ClaimedFaction == blockFaction.Tag)
                    {
                        float num3 = GetAttachedUpgradeModules(production as MyCubeBlock, "Productivity");
                        float num4 = GetAttachedUpgradeModules(production as MyCubeBlock, "Effectiveness");
                        float num5 = GetAttachedUpgradeModules(production as MyCubeBlock, "PowerEfficiency");

                        if (settings.GetPerks[PerkType.Production].perk.productionPerk.allowStandAlone)
                        {
                            SetUpgradeValue(production as MyCubeBlock, "Productivity", speed + num3, sync);
                            SetUpgradeValue(production as MyCubeBlock, "Effectiveness", yield + num4 + 1, sync);
                            SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", energy + num5 + 1, sync);
                        }
                        else
                        {
                            if (settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades.Count == 0) return;
                            if (!settings.GetPerks[PerkType.Production].perk.productionPerk.ProductionRunning) return;
                            foreach (var item in settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades)
                            {
                                if (item == "Productivity")
                                    SetUpgradeValue(production as MyCubeBlock, "Productivity", speed + num3, sync);

                                if (item == "Effectiveness")
                                    SetUpgradeValue(production as MyCubeBlock, "Effectiveness", yield + num4 + 1, sync);

                                if (item == "PowerEfficiency")
                                    SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", energy + num5 + 1, sync);

                            }
                        }


                        settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(production.EntityId, true);
                        //MyVisualScriptLogicProvider.ShowNotificationToAll($"Production Attached = {settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Count}", 12000, "Red");

                        //settings.Server.Sync = true;

                        continue;
                    }
                }
            }

            Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionAttached);
        }

        public static void UpdateProductionMultipliers(ClaimBlockSettings settings)
        {
            if (!settings.IsClaimed) return;
            if (!settings.GetPerks.ContainsKey(PerkType.Production)) return;
            if (settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Count == 0) return;
            if (!settings.GetPerks[PerkType.Production].enabled) return;

            foreach (var productionId in settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities)
            {
                IMyEntity entity;
                if (!MyAPIGateway.Entities.TryGetEntityById(productionId, out entity)) continue;

                float num3 = GetAttachedUpgradeModules(entity as MyCubeBlock, "Productivity");
                float num4 = GetAttachedUpgradeModules(entity as MyCubeBlock, "Effectiveness");
                float num5 = GetAttachedUpgradeModules(entity as MyCubeBlock, "PowerEfficiency");

                float speed = settings.GetPerks[PerkType.Production].perk.productionPerk.Speed;
                float yield = settings.GetPerks[PerkType.Production].perk.productionPerk.Yield;
                float energy = settings.GetPerks[PerkType.Production].perk.productionPerk.Energy;

                if (settings.GetPerks[PerkType.Production].perk.productionPerk.allowStandAlone)
                {
                    SetUpgradeValue(entity as MyCubeBlock, "Productivity", speed + num3, true);
                    SetUpgradeValue(entity as MyCubeBlock, "Effectiveness", yield + num4 + 1, true);
                    SetUpgradeValue(entity as MyCubeBlock, "PowerEfficiency", energy + num5 + 1, true);
                }
                else
                {
                    if (settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades.Count == 0) return;
                    if (!settings.GetPerks[PerkType.Production].perk.productionPerk.ProductionRunning) return;
                    foreach (var item in settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades)
                    {
                        if (item == "Productivity")
                            SetUpgradeValue(entity as MyCubeBlock, "Productivity", speed + num3, true);

                        if (item == "Effectiveness")
                            SetUpgradeValue(entity as MyCubeBlock, "Effectiveness", yield + num4 + 1, true);

                        if (item == "PowerEfficiency")
                            SetUpgradeValue(entity as MyCubeBlock, "PowerEfficiency", energy + num5 + 1, true);

                    }
                }
            }
        }

        public static void RemoveProductionMultipliers(ClaimBlockSettings settings, IMyProductionBlock block = null, bool sync = false)
        {
            if (settings == null) return;
            if (settings.GetGridsInside.Count == 0) return;
            if (!settings.GetPerks.ContainsKey(PerkType.Production)) return;

            if (block != null)
            {
                if (!settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Contains(block.EntityId)) return;
                float num = GetAttachedUpgradeModules(block as MyCubeBlock, "Productivity");
                float num1 = GetAttachedUpgradeModules(block as MyCubeBlock, "Effectiveness");
                float num2 = GetAttachedUpgradeModules(block as MyCubeBlock, "PowerEfficiency");

                if (num > 0)
                    SetUpgradeValue(block as MyCubeBlock, "Productivity", num, sync);
                else
                    SetUpgradeValue(block as MyCubeBlock, "Productivity", 0f, sync);

                if (num1 > 0)
                    SetUpgradeValue(block as MyCubeBlock, "Effectiveness", num1, sync);
                else
                    SetUpgradeValue(block as MyCubeBlock, "Effectiveness", 1f, sync);

                if (num2 > 0)
                    SetUpgradeValue(block as MyCubeBlock, "PowerEfficiency", num2, sync);
                else
                    SetUpgradeValue(block as MyCubeBlock, "PowerEfficiency", 1f, sync);

                settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(block.EntityId, false);
                Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionAttached);
                //settings.Server.Sync = true;
                return;
            }

            foreach (var gridData in settings.GetGridsInside.Values)
            {
                if (gridData.blocksMonitored.production.Count == 0) continue;
                foreach (var production in gridData.blocksMonitored.production)
                {
                    if (!settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Contains(production.EntityId)) continue;
                    float num = GetAttachedUpgradeModules(production as MyCubeBlock, "Productivity");
                    float num1 = GetAttachedUpgradeModules(production as MyCubeBlock, "Effectiveness");
                    float num2 = GetAttachedUpgradeModules(production as MyCubeBlock, "PowerEfficiency");

                    if (num > 0)
                        SetUpgradeValue(production as MyCubeBlock, "Productivity", num, sync);
                    else
                        SetUpgradeValue(production as MyCubeBlock, "Productivity", 0f, sync);

                    if (num1 > 0)
                        SetUpgradeValue(production as MyCubeBlock, "Effectiveness", num1, sync);
                    else
                        SetUpgradeValue(production as MyCubeBlock, "Effectiveness", 1f, sync);

                    if (num2 > 0)
                        SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", num2, sync);
                    else
                        SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", 1f, sync);

                    settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(production.EntityId, false);

                    //settings.Server.Sync = true;
                    continue;
                }
            }

            settings.GetPerks[PerkType.Production].perk.productionPerk.ProductionRunning = false;
            Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionAttached);
            Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionRunning);
            //settings.Server.Sync = true;
        }

        public static void RemoveProductionMultipliersByPlayer(ClaimBlockSettings settings, long playerId, bool sync = false)
        {
            if (settings == null) return;
            if (settings.GetGridsInside.Count == 0) return;
            if (!settings.GetPerks.ContainsKey(PerkType.Production)) return;

            foreach (var gridData in settings.GetGridsInside.Values)
            {
                if (gridData.blocksMonitored.production.Count == 0) continue;
                foreach (var production in gridData.blocksMonitored.production)
                {
                    if (!settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Contains(production.EntityId)) continue;
                    if (production.OwnerId != playerId) continue;

                    float num = GetAttachedUpgradeModules(production as MyCubeBlock, "Productivity");
                    float num1 = GetAttachedUpgradeModules(production as MyCubeBlock, "Effectiveness");
                    float num2 = GetAttachedUpgradeModules(production as MyCubeBlock, "PowerEfficiency");

                    if (num > 0)
                        SetUpgradeValue(production as MyCubeBlock, "Productivity", num, sync);
                    else
                        SetUpgradeValue(production as MyCubeBlock, "Productivity", 0f, sync);

                    if (num1 > 0)
                        SetUpgradeValue(production as MyCubeBlock, "Effectiveness", num1, sync);
                    else
                        SetUpgradeValue(production as MyCubeBlock, "Effectiveness", 1f, sync);

                    if (num2 > 0)
                        SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", num2, sync);
                    else
                        SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", 1f, sync);

                    settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(production.EntityId, false);
                    continue;
                }
            }

            Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionAttached);

            if (settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Count == 0)
            {
                settings.GetPerks[PerkType.Production].perk.productionPerk.ProductionRunning = false;
                Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionRunning);
            }
        }

        public static void SetDefaultProduction(MyCubeBlock block)
        {
            if (block == null) return;
            float num = GetAttachedUpgradeModules(block as MyCubeBlock, "Productivity");
            float num1 = GetAttachedUpgradeModules(block as MyCubeBlock, "Effectiveness");
            float num2 = GetAttachedUpgradeModules(block as MyCubeBlock, "PowerEfficiency");

            if (num > 0)
                SetUpgradeValue(block as MyCubeBlock, "Productivity", num);
            else
                SetUpgradeValue(block as MyCubeBlock, "Productivity", 0f);

            if (num1 > 0)
                SetUpgradeValue(block as MyCubeBlock, "Effectiveness", num1);
            else
                SetUpgradeValue(block as MyCubeBlock, "Effectiveness", 1f);

            if (num2 > 0)
                SetUpgradeValue(block as MyCubeBlock, "PowerEfficiency", num2);
            else
                SetUpgradeValue(block as MyCubeBlock, "PowerEfficiency", 1f);
        }

        public static void UpdatePlayerPerks(ClaimBlockSettings settings)
        {
            if (settings == null) return;

            foreach (var perk in settings.GetPerks.Keys)
            {
                if (perk == PerkType.Production)
                {
                    PerkBase perkbase;
                    if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) continue;

                    if (!perkbase.Enable) continue;
                    if (perkbase.perk.productionPerk.allowStandAlone) continue;
                    if (perkbase.perk.productionPerk.GetPendingAddUpgrades.Count != 0)
                    {
                        foreach (var upgrade in perkbase.perk.productionPerk.GetPendingAddUpgrades)
                        {
                            if (perkbase.perk.productionPerk.GetActiveUpgrades.Contains(upgrade)) continue;

                            settings.GetPerks[PerkType.Production].perk.productionPerk.ActiveUprades(upgrade, true);
                            UpdateSingleProductionMultiplier(settings, null, upgrade, true, true);
                        }
                    }

                    if (perkbase.perk.productionPerk.GetPendingRemoveUpgrades.Count != 0)
                    {
                        foreach (var upgrade in perkbase.perk.productionPerk.GetPendingRemoveUpgrades)
                        {
                            if (!perkbase.perk.productionPerk.GetActiveUpgrades.Contains(upgrade)) continue;

                            settings.GetPerks[PerkType.Production].perk.productionPerk.ActiveUprades(upgrade, false);
                            UpdateSingleProductionMultiplier(settings, null, upgrade, false, true);
                        }
                    }



                    /*foreach(var upgrade in perkbase.perk.productionPerk.GetActiveUpgrades)
                    {
                        if (perkbase.perk.productionPerk.GetPendingUpgrades.Contains(upgrade)) continue;

                        settings.GetPerks[PerkType.Production].perk.productionPerk.ActiveUprades(upgrade, false);
                        UpdateSingleProductionMultiplier(settings, null, upgrade, false, true);
                    }*/

                    perkbase.perk.productionPerk.pendingAddUpgrades.Clear();
                    perkbase.perk.productionPerk.pendingRemoveUpgrades.Clear();

                    if (perkbase.perk.productionPerk.GetActiveUpgrades.Count != 0)
                        perkbase.perk.productionPerk.ProductionRunning = true;
                    else
                        perkbase.perk.productionPerk.ProductionRunning = false;

                    settings.Server.Sync = true;
                    Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionRunning);
                }
            }
        }

        public static void UpdateActiveStandAlonePerks(ClaimBlockSettings settings)
        {
            if (settings == null) return;
            foreach (var perk in settings.GetPerks.Keys)
            {
                if (perk == PerkType.Production)
                {
                    PerkBase perkbase;
                    if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) continue;
                    perkbase.perk.productionPerk.activeUpgrades.Clear();

                    if (perkbase.perk.productionPerk.allowStandAlone)
                    {
                        if (perkbase.perk.productionPerk.Speed != 0)
                            settings.GetPerks[PerkType.Production].perk.productionPerk.ActiveUprades("Productivity", true);

                        if (perkbase.perk.productionPerk.Yield != 0)
                            settings.GetPerks[PerkType.Production].perk.productionPerk.ActiveUprades("Effectiveness", true);

                        if (perkbase.perk.productionPerk.Energy != 0)
                            settings.GetPerks[PerkType.Production].perk.productionPerk.ActiveUprades("PowerEfficiency", true);
                    }

                    if (perkbase.perk.productionPerk.GetActiveUpgrades.Count != 0)
                        perkbase.perk.productionPerk.ProductionRunning = true;
                    else
                        perkbase.perk.productionPerk.ProductionRunning = false;

                    settings.Server.Sync = true;
                    Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionRunning);
                }
            }
        }

        public static string GetCounterDetails(ClaimBlockSettings settings)
        {
            string result = "";
            bool perkEnabled = Utils.AnyPerksEnabled(settings);
            int pendingCost = 0;
            string token = settings.ConsumptionItem;
            token = token.Replace("MyObjectBuilder_", "");

            result += $"\n A Token = {token}\n";

            if (settings.ReadyToSiege)
                result += $"\n[Territory Can Be Final Sieged For]:\n{TimeSpan.FromSeconds(settings.SiegeTimer)}\n";

            if (settings.IsSieged && !settings.ReadyToSiege && !settings.IsSiegingFinal)
            {
                result += $"\n[Time Until Territory Can Be Final Sieged]:\n{TimeSpan.FromSeconds(settings.SiegeTimer)}\n";
                result += $"\n[Siege Time Extended]: {settings.HoursToDelay * settings.SiegedDelayedHit} hrs ({settings.SiegedDelayedHit}/{settings.SiegeDelayAllow}) used\n";
            }

            if (settings.IsSiegeCooling)
            {
                result += $"\n[Siege Cooldown]: {TimeSpan.FromSeconds(settings.SiegeTimer)}\n";
            }

            pendingCost = Utils.GetPendingPerkCost(settings);
            result += $"\n[Time Until {pendingCost + settings.ConsumptinAmt} Token(s) Consumed]:\n{TimeSpan.FromSeconds(settings.Timer)}\n";
            result += $"\n[Safe Zone Upkeep Cost]: {settings.ConsumptinAmt} Token(s)\n[Total Cost Next Cycle]: {pendingCost + settings.ConsumptinAmt} Token(s)\n";

            if (perkEnabled)
            {
                string pendingPerksAdd = Utils.GetPendingPerksAdd(settings);
                string pendingPerksRemove = Utils.GetPendingPerksRemove(settings);
                string activePerks = Utils.GetActivePerks(settings);
                string activePerksText = string.IsNullOrEmpty(activePerks) ? "None" : activePerks;
                int currentCost = Utils.GetActivePerkCost(settings);

                result += $"\n*Player configurable perks will NOT enable/disabled until next token consumption*\n";
                if (!string.IsNullOrEmpty(pendingPerksAdd))
                    result += $"\n[Pending Perks To Be Added]: {pendingPerksAdd}";

                if (!string.IsNullOrEmpty(pendingPerksRemove))
                    result += $"\n[Pending Perks To Be Removed]: {pendingPerksRemove}";

                if (!string.IsNullOrEmpty(pendingPerksAdd) || !string.IsNullOrEmpty(pendingPerksRemove))
                    result += $"\n[Pending Perk Cost]: {pendingCost} Token(s)\n";

                result += $"\n[Active Perks]: {activePerksText}";
                result += $"\n[Current Perks Cost]: {currentCost} Token(s)";
            }

            return result;
        }

        public static void ManuallySetTerritory(ClaimBlockSettings settings, string tag)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(tag);
            if (faction == null) return;

            settings.RecoveryTimer = 0;
            settings.IsClaiming = false;
            settings.IsClaimed = true;
            settings.ClaimZoneName = settings.TerritoryName;
            settings.Timer = settings.ConsumeTokenTimer;
            settings.BlockEmissive = EmissiveState.Claimed;
            settings.ClaimedFaction = faction.Tag;
            settings.FactionId = faction.FactionId;

            var icon = faction.FactionIcon;
            Utils.SetScreen(settings.Block as IMyBeacon, faction, true);

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
        }

        public static void DisableBlock(MyCubeBlock block, ClaimBlockSettings settings)
        {
            if (settings.AllowTools) return;
            IMyFunctionalBlock fBlock = block as IMyFunctionalBlock;
            if (fBlock == null) return;

            if (block is IMyShipDrill && settings.AllowDrilling) return;
            if (block is IMyShipWelder && settings.AllowWelding) return;
            if (block is IMyShipGrinder && settings.AllowGrinding) return;

            IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
            if (blockFaction == null)
            {
                fBlock.Enabled = false;
                return;
			}

			if (settings.IsClaimed)
			{
				IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
				if (claimFaction != null && claimFaction != blockFaction && !settings.FactionsExempt.Contains(blockFaction.FactionId))
                {
                    //if (settings.AllowTerritoryAllies || settings.AllowSafeZoneAllies)
                    //{
                    //    if (IsFactionEnemy(settings, blockFaction))
                    //    {
                    //        fBlock.Enabled = false;
                    //        return;
                    //    }
                    //
                    //    return;
                    //}

                    fBlock.Enabled = false;
                    return;
                }
            }
        }

        public static bool IsFactionEnemy(ClaimBlockSettings settings, IMyFaction faction2)
        {
            if (settings == null || faction2 == null || settings.FactionId == 0) return true;
            if (settings.FactionId == faction2.FactionId) return false;
            IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionById(settings.FactionId);
            if (claimFaction == null) return true;

            var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(claimFaction.FactionId, faction2.FactionId);
			if (relation == MyRelationsBetweenFactions.Enemies) return true;
            if (relation == MyRelationsBetweenFactions.Neutral && settings.NeutralEnemies) return true;
            
            return true;
        }

        public static void SetOwner(IMyTerminalBlock block, IMyFaction myFaction = null)
        {
            MyCubeBlock cubeblock = block as MyCubeBlock;
            if (cubeblock == null) return;

            cubeblock.ChangeBlockOwnerRequest(0, MyOwnershipShareModeEnum.Faction);

            if (myFaction != null)
                cubeblock.ChangeBlockOwnerRequest(myFaction.FounderId, MyOwnershipShareModeEnum.Faction);
            else
            {
                IMyFaction npc = MyAPIGateway.Session.Factions.TryGetFactionByTag("SPRT");
                if (npc != null)
                    cubeblock.ChangeBlockOwnerRequest(npc.FounderId, MyOwnershipShareModeEnum.Faction);
            }
        }

        public static void MonitorSafeZonePBs(ClaimBlockSettings settings, bool disable = false)
        {
            if (disable)
            {
                foreach(var pb in settings.Server._pbList)
                    pb.IsWorkingChanged -= Events.PbWatcher;

                settings.Server._pbList.Clear();
                return;
            }

            if (settings.SafeZoneEntity == 0) return;
            BoundingSphereD sphere = new BoundingSphereD(settings.BlockPos, settings.SafeZoneSize);
            var ents = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref sphere);
            foreach(var ent in ents)
            {
                if (ent == null) continue;
                IMyCubeGrid cubeGrid = ent as IMyCubeGrid;
                if (cubeGrid == null) continue;

                List<IMyProgrammableBlock> blocks = new List<IMyProgrammableBlock>();
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid).GetBlocksOfType(blocks);
                foreach(var pb in blocks)
                {
                    if (pb == null) continue;
                    pb.Enabled = false;
                    pb.IsWorkingChanged += Events.PbWatcher;
                    settings.Server._pbList.Add(pb);
                }
            }
        }

        public static string GetPopupText(MyTextEnum text)
        {
            StringBuilder builder = new StringBuilder();

            if (text == MyTextEnum.SafeZoneAllies)
            {
                builder.Append("\n            ****** WARNING ******\n\n");
                builder.Append(" Enabling this will give all of your faction's allies access inside the safe zone with the ability to drill/weld/grind.");
                builder.Append(" Enabling this option will also force enable 'Allow Territory Allies'. Which means that will be able to access your whole territory to drill/weld/grind without being detected.");
                builder.Append(" Allies will NOT be able to access the claim block to change settings.\n\n");
                builder.Append("This option is NOT dynamic, so if a faction is newly allied to you after this option is enabled, the safe zone whitelist does NOT automatically update its whitelist.");
                builder.Append(" You will need to disable/enable this option to refresh the safe zone with your current allies. Same goes with a faction that is no longer an allie with you.");
                return builder.ToString();
            }

            if (text == MyTextEnum.TerritoryAllies)
            {
                builder.Append("\n            ****** WARNING ******\n\n");
                builder.Append(" Enabling this option will allow all of your faction's allies to access your whole territory, they will be able to drill/weld/grind without being detected.");
                builder.Append(" Allies will NOT be able to access the claim block to change settings.");
                return builder.ToString();
            }

            return builder.ToString();
        }

        private static readonly string[] NotNames = new string[]
        {
            "Nobody loves me :(",
            "That one territory that needs a name still",
            "I have been neglected and don't have a name",
            "The admins have been too busy explaining to players why conveyor loops are HORRIBLE for sim speed so I haven't been named yet",
            "I will have a name soon™",
            "yrotirreT demannU"
        };

        public static void EnterTerritoryMessage(ClaimBlockSettings settings, IMyPlayer player)
        {
            if (settings == null) return;
            if (MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId) != null && !settings.FactionsExempt.Contains(MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId).FactionId))
                return;

            string name = settings.ClaimZoneName;
            if (string.IsNullOrEmpty(name))
                name = NotNames[MyAPIGateway.Session.GameplayFrameCounter % NotNames.Length];

			CustomChatMsg.SendChatMessage(player.SteamUserId, "[Faction Territories]", $"You have entered the unclaimed territory of {name}.");
            

			//MyVisualScriptLogicProvider.SendChatMessageColored($"Claimed territories prevent your free offline safezone, or safezone generators, from working unless you are the owner or ally. You will be seen in real-time by the territory owners. Proceed with caution!", Color.Violet, "[Faction Territories]", player.IdentityId, "Red");

            //if (settings.AllowTools) 
            //    return;
            //
            //if (!settings.AllowDrilling && !settings.AllowGrinding && !settings.AllowWelding)
            //{
            //    MyVisualScriptLogicProvider.SendChatMessageColored($"All ship/hand tools are NOT allowed inside claimed territories!", Color.Violet, "[Faction Territories]", player.IdentityId, "Red");
            //    return;
            //}
            //
            //if (!settings.AllowDrilling)
            //    MyVisualScriptLogicProvider.SendChatMessageColored($"Drilling is NOT allowed inside claimed territories!", Color.Violet, "[Faction Territories]", player.IdentityId, "Red");
            //
            //if (!settings.AllowGrinding)
            //    MyVisualScriptLogicProvider.SendChatMessageColored($"Grinding is NOT allowed inside claimed territories!", Color.Violet, "[Faction Territories]", player.IdentityId, "Red");
            //
            //if (!settings.AllowWelding)
            //    MyVisualScriptLogicProvider.SendChatMessageColored($"Welding is NOT allowed inside claimed territories!", Color.Violet, "[Faction Territories]", player.IdentityId, "Red");

        }

        public static void GetTerritoriesStatus(IMyTerminalBlock block = null)
        {
            MyAPIGateway.Parallel.StartBackground(() =>
            {
                if (Session.Instance.IsNexusInstalled)
                {
                    if (Session.Instance.crossServerClaimSettings.Count == 0)
                    {
                        MyAPIGateway.Utilities.InvokeOnGameThread(() => NexusComms.RequestTerritoryStatus());
                        MyAPIGateway.Parallel.Sleep(1000);
                    }
                }

                if (block != null)
                {
                    IMyTextSurface panel = block as IMyTextSurface;
                    if (panel == null) return;
                    WriteStatusToLCD(panel);
                }
                else
                {
                    foreach (IMyTextSurface panel in Session.Instance.activeLCDs)
                        WriteStatusToLCD(panel);
                }
            });
        }

        public static void WriteStatusToLCD(IMyTextSurface panel)
        {
            if (panel == null) return;
            string title = " Territory Status\n ------------------\n";
            panel.WriteText(title);

            string text = "";
            foreach (var item in Session.Instance.claimBlocks.Values)
            {
                text += $"\n {item.TerritoryName}: ";
            }
            
        }

        public static float RandomFloat(float minValue, float maxValue)
        {
            var minInflatedValue = (float)Math.Round(minValue, 3) * 1000;
            var maxInflatedValue = (float)Math.Round(maxValue, 3) * 1000;
            var randomValue = (float)Rnd.Next((int)minInflatedValue, (int)maxInflatedValue) / 1000;
            return randomValue;
        }

        public static void EnableHighlight(ClaimBlockSettings settings, IMyEntity Entity)
        {
            if (Entity == null || settings == null) return;
            MyCubeGrid grid = null;
            if (Entity is MyCubeBlock)
            {
                grid = (Entity as MyCubeBlock).CubeGrid;
                VSL.SetHighlightForAll(grid.Name, true, 10, 60, Color.Yellow.Alpha(0.3f));
            }
            else
                VSL.SetHighlightForAll(Entity.Name, true, 10, 60, Color.Yellow.Alpha(0.3f));

            if (settings.JDClaiming != null)
                ClaimTimer(settings.TerritoryName, settings.Timer);
            else
                SiegeTimer(settings.TerritoryName, settings.SiegeTimer);
        }

        public static void DisableHighlight(ClaimBlockSettings settings, IMyEntity Entity)
		{
            if (Entity == null || settings == null) return;
			MyCubeGrid grid = null;
			if (Entity is MyCubeBlock)
			{
				grid = (Entity as MyCubeBlock).CubeGrid;
				VSL.SetHighlightForAll(grid.Name, false, -1);
			}
			else
				VSL.SetHighlightForAll(Entity.Name, false, -1);

            if (settings.JDClaiming != null)
                ClaimTimer(settings.TerritoryName, settings.Timer, true);
            else
		        SiegeTimer(settings.TerritoryName, settings.SiegeTimer, true);
		}

        public static float TimerPOSX = 0.05f;
        public static float TimerPOSY = 0.0001f;
        public static float TimerTextScale = 1.0f;

		public static void SiegeTimer(string name, int time, bool hide = false, long playerId = -1)
		{
			VSL.RemoveUIString(1, playerId);

			if (hide) return;

			VSL.CreateUIString(1, $"{name} is under siege! {TimeSpan.FromSeconds(time)}", TimerPOSX, TimerPOSY, TimerTextScale, "Red", VRage.Utils.MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP, playerId);
		}

		public static void ClaimTimer(string name, int time, bool hide = false, long playerId = -1)
		{
			VSL.RemoveUIString(0, playerId);

			if (hide) return;

			VSL.CreateUIString(0, $"Claiming {name}: {TimeSpan.FromSeconds(time)}", TimerPOSX, TimerPOSY, TimerTextScale, "Red", VRage.Utils.MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP, playerId);
		}
	}
}
