using Faction_Territories.Config;
using Faction_Territories.Network;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Inventory;
using Sandbox.Game.SessionComponents;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Collections;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Faction_Territories
{
    public static class Triggers
    {
        public static bool initAreaTrigger;

        public static void CreateNewTriggers(IMyBeacon beacon)
        {
            if (beacon == null) return;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(beacon.EntityId, out settings)) return;
            if (!settings.Enabled) return;


            if (!initAreaTrigger)
            {
                initAreaTrigger = true;
                MyVisualScriptLogicProvider.AreaTrigger_Entered += PlayerEntered;
                MyVisualScriptLogicProvider.AreaTrigger_EntityEntered += EntityEntered;
                MyVisualScriptLogicProvider.AreaTrigger_EntityLeft += EntityLeft;
                MyVisualScriptLogicProvider.AreaTrigger_Left += PlayerLeft;
                //MyVisualScriptLogicProvider.ShowNotificationToAll("Area Triggered", 15000, "Green");
            }

            MyVisualScriptLogicProvider.RemoveAllTriggersFromEntity(beacon.Name);

            if (!settings.CenterToPlanet)
                MyVisualScriptLogicProvider.CreateAreaTriggerOnEntity(beacon.Name, settings.ClaimRadius, beacon.EntityId.ToString());
            else
                MyVisualScriptLogicProvider.CreateAreaTriggerOnEntity(settings.PlanetName, settings.ClaimRadius, beacon.EntityId.ToString());

            //settings.TriggerInit = true;

            string text = !settings.IsClaimed ? $"Unclaimed Territory: {settings.TerritoryName}" : $"Claimed Territory: {settings.ClaimZoneName} ({settings.ClaimedFaction})";
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            foreach (var player in players)
            {
                if (player.SteamUserId <= 0 || player.IsBot) continue;

                GPS.UpdateBlockText(settings, text, player.IdentityId);
            }

            if (settings.IsClaimed)
            {
                Utils.AddPerks(settings);
            }
        }

        public static void RemoveTriggerData(long beaconId)
        {
            if (beaconId == 0)
                return;

            IMyEntity beacon = null;
            if (MyAPIGateway.Entities.TryGetEntityById(beaconId, out beacon))
                MyVisualScriptLogicProvider.RemoveAllTriggersFromEntity(beacon.Name);
            else
                MyVisualScriptLogicProvider.RemoveAllTriggersFromEntity(beaconId.ToString());


			ClaimBlockSettings settings;
            Session.Instance.claimBlocks.TryGetValue(beaconId, out settings);
            if (settings == null) 
                return;

            if (settings.IsClaimed)
            {
                Utils.RemoveSafeZone(settings);
                Utils.RemovePerks(settings);
            }

            GPS.RemoveCachedGps(0, GpsType.All, settings);
            /*if (settings.Enabled)
                GPS.RemoveCachedGps(0, GpsType.Tag, settings);
            else
                GPS.RemoveCachedGps(0, GpsType.All, settings);*/

            Utils.RemoveGridData(settings);
            settings.UpdatePlayerInside(0, false);
        }

        public static void PlayerEntered(string area, long playerId)
        {
            long entityId = 0;
            IMyEntity entity;
            IMyTerminalBlock block;
            long.TryParse(area, out entityId);
            if (entityId == 0) 
                return;

            MyAPIGateway.Entities.TryGetEntityById(entityId, out entity);
            if (entity == null)
                return;

            block = entity as IMyTerminalBlock;
            if (block == null)
                return;

            ClaimBlockSettings settings;
            Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings);
            if (settings == null) 
                return;
            if (!settings.Enabled) 
                return;

            if (settings.GetPlayersInside.ContainsKey(playerId)) 
                return;
            settings.UpdatePlayerInside(playerId, true);
            //settings.PlayersInside.Add(playerId);
            //settings.Sync = true;

            IMyPlayer player = GetPlayerFromId(playerId);
            if (player == null) 
                return;

			if (settings.IsSieging || settings.IsSiegingFinal)
				Utils.SiegeTimer(settings.TerritoryName, settings.SiegeTimer, false, playerId);
			else if (settings.IsClaiming)
				Utils.ClaimTimer(settings.TerritoryName, settings.Timer, false, playerId);

			//GPS.AddBlockLocation(block.GetPosition(), playerId, settings);
			if (settings.IsClaimed)
            {
                //MyVisualScriptLogicProvider.SendChatMessageColored($"{player.DisplayName} - Territory Entered: {settings.ClaimZoneName} ({settings.ClaimedFaction}).", Color.Violet, "[Faction Territories]", 0L, "Red");
                
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId);

                if (faction != null)
                {
                    if (!Utils.IsFactionEnemy(settings, faction)) 
                        return;

                    GPS.TagEnemyPlayer(player, settings);
                    new ModMessage(settings.EntityId, settings.TerritoryName, $"[{faction.Tag}]{player.DisplayName} - Territory Entered: {settings.ClaimZoneName} ({settings.ClaimedFaction}).", "[Faction Territories]", Color.Red, settings.DiscordChannelId, settings.FactionId);
					//MyVisualScriptLogicProvider.SendChatMessageColored($"You have entered {settings.ClaimZoneName}, controlled by {settings.ClaimedFaction}. Proceed with caution!", Color.Violet, "[Faction Territories]", player.IdentityId, "Red");
                    CustomChatMsg.SendChatMessage(player.SteamUserId, "[Faction Territories]", $"You have entered {settings.ClaimZoneName}, controlled by {settings.ClaimedFaction}. Proceed with caution!");

                    return;
                }

                //GPS.TagEnemyPlayer(player, settings);
                //new ModMessage(settings.EntityId, settings.TerritoryName, $"{player.DisplayName} - Territory Entered: {settings.ClaimZoneName} ({settings.ClaimedFaction}).", "[Faction Territories]", Color.Red);

                //Utils.EnterTerritoryMessage(settings, player);

                /*IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                IMyFaction playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId);
                if (faction == null) return;

                VRage.Game.MyRelationsBetweenFactions relation = VRage.Game.MyRelationsBetweenFactions.Enemies;
                if (playerFaction != null)
                {
                    relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(faction.FactionId, playerFaction.FactionId);
                }

                if(playerFaction != faction || playerFaction == null || relation == VRage.Game.MyRelationsBetweenFactions.Enemies)
                {
                    //Comms.DisableClientTools(playerId);
                    
                    MyVisualScriptLogicProvider.SendChatMessageColored($"{player.DisplayName}: Now entering territory: {settings.ClaimZoneName} ({settings.ClaimedFaction})", Color.Violet, "[Faction Territories]", 0L, "Red");
                }*/

            }
            else
            {
                //MyVisualScriptLogicProvider.SendChatMessageColored($"{player.DisplayName} - Territory Entered: Unclaimed {settings.UnclaimName}", Color.Violet, "[Faction Territories]", playerId, "Red");
                new ModMessage(settings.EntityId, settings.TerritoryName, $"{player.DisplayName} - Territory Entered: Unclaimed {settings.TerritoryName}", "[Faction Territories]", Color.Red);
            }

            Utils.EnterTerritoryMessage(settings, player);
        }

        public static void EntityEntered(string area, long entityId, string entityName)
        {
            long claimEntityId;
            IMyEntity claimEntity;
            IMyEntity entityEntered;
            long.TryParse(area, out claimEntityId);

            MyAPIGateway.Entities.TryGetEntityById(claimEntityId, out claimEntity);
            MyAPIGateway.Entities.TryGetEntityById(entityId, out entityEntered);
            if (claimEntity == null || entityEntered == null) 
                return;

            ClaimBlockSettings settings;
            Session.Instance.claimBlocks.TryGetValue(claimEntity.EntityId, out settings);
            if (settings == null) 
                return;
            if (!settings.Enabled) 
                return;

			MyCubeGrid cubeGrid = entityEntered as MyCubeGrid;
            if (cubeGrid == null || cubeGrid.Physics == null) 
                return;
            settings.UpdateGridsInside(entityId, cubeGrid, true);
            //MyVisualScriptLogicProvider.ShowNotificationToAll($"Entity Entered = {cubeGrid.DisplayName}", 10000, "Green");
            //cubeGrid.OnFatBlockAdded += Events.FatBlockAdded;
            //cubeGrid.OnFatBlockClosed += Events.FatBlockClosed;
            //cubeGrid.OnBlockIntegrityChanged += Events.FatBlockCheckFunctional;

            /*
            bool foundController = false;
            bool foundPower = false;
            ListReader<MyCubeBlock> blocks = cubeGrid.GetFatBlocks();
            foreach(var block in blocks)
            {
                if (block as IMyShipController != null)
                {
                    var controller = block as IMyShipController;
                    settings.UpdatesBlocksMonitored(cubeGrid, block, true);

                    if(controller.IsFunctional)
                        foundController = true;

                    continue;
                }

                if (block as IMyPowerProducer != null)
                {
                    var power = block as IMyPowerProducer;
                    settings.UpdatesBlocksMonitored(cubeGrid, block, true);

                    if(power.IsFunctional)
                        foundPower = true;

                    continue;
                }

                if (block as IMyShipToolBase != null)
                {
                    settings.UpdatesBlocksMonitored(cubeGrid, block, true);
                    Utils.DisableBlock(block, settings);
                    continue;
                }

                if (block as IMyShipDrill != null)
                {
                    settings.UpdatesBlocksMonitored(cubeGrid, block, true);
                    Utils.DisableBlock(block, settings);
                    continue;
                }

                if (block as IMyProductionBlock != null)
                {
                    settings.UpdatesBlocksMonitored(cubeGrid, block, true);
                    continue;
                }
            }
            */

            bool ignore = true;

            if (cubeGrid.OccupiedBlocks.Count > 0)
            {
                settings.UpdateGridData(cubeGrid.EntityId, ClaimBlockSettings.GridChangeType.Controller, true);
                ignore = false;
            }

            if (cubeGrid.IsPowered)
            {
                settings.UpdateGridData(cubeGrid.EntityId, ClaimBlockSettings.GridChangeType.Power, true);
                ignore = false;
            }


            if (ignore) return;
            //if (!foundController || !foundPower) 
            //    return;

            /*List<IMyShipController> controllers = new List<IMyShipController>();
            List<IMyPowerProducer> power = new List<IMyPowerProducer>();
            List<IMyShipToolBase> tools = new List<IMyShipToolBase>();
            MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(largest).GetBlocksOfType(controllers);
            MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(largest).GetBlocksOfType(power);
            MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(largest).GetBlocksOfType(tools);
            
            if (tools.Count != 0)
            {
                foreach (var tool in tools)
                {
                    tool.IsWorkingChanged += Events.IsWorkingChanged;
                }
            }

            if (controllers.Count != 0)
                settings.UpdateGridData(cubeGrid.EntityId, ClaimBlockSettings.GridChangeType.Controller, true);

            if (power.Count != 0)
                settings.UpdateGridData(cubeGrid.EntityId, ClaimBlockSettings.GridChangeType.Power, true);

            if (controllers.Count == 0 || power.Count == 0) return;*/

            if (!settings.IsClaimed) return;

            if (cubeGrid.OccupiedBlocks.Count > 0)
            {
                IMyPlayer player = MyAPIGateway.Players.GetPlayerControllingEntity(cubeGrid);
                if (player != null)
                {
                    IMyFaction fac = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId);
                    if (fac != null && !Utils.IsFactionEnemy(settings, fac)) return;

                    GPS.TagEnemyGrid(cubeGrid, settings);
                    return;
                }
            }
			MyCubeGrid largest = cubeGrid.GetBiggestGridInGroup();
			var owner = largest.BigOwners.FirstOrDefault();
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);

            if (faction != null && !Utils.IsFactionEnemy(settings, faction)) return;

            GPS.TagEnemyGrid(largest, settings);

            /*if (myFaction == null || faction == null)
            {
                if (owner == settings.PlayerClaimingId) return;
                GPS.TagEnemyGrid(largest, settings);
                return;
            }

            if (myFaction == faction) return;
            var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(myFaction.FactionId, faction.FactionId);
            if (relation == MyRelationsBetweenFactions.Enemies)
                GPS.TagEnemyGrid(largest, settings);*/
        }

        public static void EntityLeft(string area, long entityId, string entityName)
        {
            long claimEntityId;
            IMyEntity claimEntity;
            IMyEntity entityEntered;
            long.TryParse(area, out claimEntityId);

            MyAPIGateway.Entities.TryGetEntityById(claimEntityId, out claimEntity);
            MyAPIGateway.Entities.TryGetEntityById(entityId, out entityEntered);
            if (claimEntity == null) 
                return;

            ClaimBlockSettings settings;
            Session.Instance.claimBlocks.TryGetValue(claimEntity.EntityId, out settings);
            if (settings == null) 
                return;
            if (!settings.Enabled) 
                return;
            if (entityEntered == null || entityEntered.MarkedForClose)
            {
                settings.UpdateGridsInside(entityId, null, false);
                return;
            }

            MyCubeGrid cubeGrid = entityEntered as MyCubeGrid;
            if (cubeGrid == null) 
                return;
            //MyCubeGrid largest = cubeGrid.GetBiggestGridInGroup();
            settings.UpdateGridsInside(entityId, cubeGrid, false);

            //cubeGrid.OnFatBlockAdded -= Events.FatBlockAdded;
            //cubeGrid.OnFatBlockClosed -= Events.FatBlockClosed;
            //cubeGrid.OnBlockIntegrityChanged -= Events.FatBlockCheckFunctional;
            //MyVisualScriptLogicProvider.ShowNotification("Grid Left Triggered", 5000);
        }

        public static void PlayerLeft(string area, long playerId)
		{
			Utils.ClaimTimer(null, 0, true, playerId);
			Utils.SiegeTimer(null, 0, true, playerId);
			//MyVisualScriptLogicProvider.ShowNotification("Player Left", 5000);
			long entityId = 0;
            IMyEntity entity;
            IMyTerminalBlock block;
            long.TryParse(area, out entityId);
            if (entityId == 0) 
                return;

            MyAPIGateway.Entities.TryGetEntityById(entityId, out entity);
            if (entity == null) 
                return;

            block = entity as IMyTerminalBlock;
            if (block == null) 
                return;

            ClaimBlockSettings settings;
            Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings);
            if (settings == null) 
                return;
            if (!settings.Enabled) 
                return;

            if (!settings.GetPlayersInside.ContainsKey(playerId)) 
                return;
            settings.UpdatePlayerInside(playerId, false);

            IMyPlayer player = GetPlayerFromId(playerId);
            if (player == null) 
                return; 

            GPS.RemoveCachedGps(playerId, GpsType.Player, settings);
            if (settings.IsClaimed)
            {
                //MyVisualScriptLogicProvider.SendChatMessageColored($"{player.DisplayName} - Territory Leaving: {settings.ClaimZoneName} ({settings.ClaimedFaction}).", Color.Violet, "[Faction Territories]", 0L, "Green");
                
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId);

                if (faction != null)
                {
                    if (!Utils.IsFactionEnemy(settings, faction)) return;

                    new ModMessage(settings.EntityId, settings.TerritoryName, $"[{faction.Tag}]{player.DisplayName} - Territory Leaving: {settings.ClaimZoneName} ({settings.ClaimedFaction}).", "[Faction Territories]", Color.Red, settings.DiscordChannelId, settings.FactionId);
                }
                /*IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                IMyFaction playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId);
                if (faction == null) return;

                VRage.Game.MyRelationsBetweenFactions relation = VRage.Game.MyRelationsBetweenFactions.Enemies;
                if (playerFaction != null)
                {
                    relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(faction.FactionId, playerFaction.FactionId);
                }

                if (playerFaction != faction || playerFaction == null || relation == VRage.Game.MyRelationsBetweenFactions.Enemies)
                {
                    //Comms.DisableClientTools(playerId);
                    

                    MyVisualScriptLogicProvider.SendChatMessageColored($"{player.DisplayName} - Now leaving territory: {settings.ClaimZoneName} ({settings.ClaimedFaction})", Color.Violet, "[Faction Territories]", 0L, "Green");
                }*/

            }
            else
            {
                //MyVisualScriptLogicProvider.SendChatMessageColored($"{player.DisplayName} - Territory Leaving: Unclaimed - {settings.UnclaimName}", Color.Violet, "[Faction Territories]", 0L, "Green");
                new ModMessage(settings.EntityId, settings.TerritoryName, $"{player.DisplayName} - Territory Leaving: Unclaimed - {settings.TerritoryName}", "[Faction Territories]", Color.Red);
            }
        }

        public static IMyPlayer GetPlayerFromId(long playerId)
        {
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (player.IdentityId == playerId) return player;
            }

            return null;
        }
    }
}
