using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using SpaceEngineers.Game.Entities.Weapons;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using Sandbox.ModAPI.Interfaces.Terminal;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.Entities.Cube;
using Faction_Territories.Network;

namespace Faction_Territories
{
    public static class Events
    {

        public static void InitProduction(MyEntity entity)
        {
            if (entity as IMyProductionBlock != null)
            {
                var production = entity as IMyProductionBlock;
                Utils.SetDefaultProduction(production as MyCubeBlock);
                if (Session.Instance.isServer) return;
                foreach (var setting in Session.Instance.claimBlocks.Values)
                {
                    if (!setting.Enabled || !setting.IsClaimed) continue;
                    if (Vector3D.Distance(setting.CenterToPlanet ? setting.PlanetCenter : setting.BlockPos, production.GetPosition()) > setting.ClaimRadius) continue;
                    if (!setting.GetPerks.ContainsKey(PerkType.Production)) continue;
                    if (!setting.GetPerks[PerkType.Production].enabled) continue;

                    IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(production.OwnerId);
                    if (blockFaction == null)
                    {
                        MyLog.Default.WriteLineAndConsole($"EntityCreate Event Faction is null");
                        return;
                    }
                    if (blockFaction.Tag != setting.ClaimedFaction) return;

                    Utils.AddProductionMultipliersToClient(production as MyCubeBlock, setting);
                    return;
                }
            }
        }

        public static void EntityAdd(IMyEntity entity)
        {
            MyAPIGateway.Parallel.StartBackground(() =>
            {
                MyCubeGrid grid = entity as MyCubeGrid;
                if (grid != null && grid.BigOwners.Count > 0 && !MyAPIGateway.Session.Factions.TryGetPlayerFaction(grid.BigOwners[0]).IsEveryoneNpc())
                {
                    foreach (var block in grid.GetFatBlocks())
                    {
                        if (block as IMyProductionBlock != null)
                        {
                            MyAPIGateway.Utilities.InvokeOnGameThread(() => InitProduction(block as MyEntity));
                            continue;
                        }
                    }

                    return;
                }

                MySafeZone zone = entity as MySafeZone;
                if (zone != null)
                {
                    //if (!MyAPIGateway.Session.IsServer) return;
                    if (zone.DisplayName != null)
                        if (zone.DisplayName.Contains("(FactionTerritory)"))
                        {
                            //MyLog.Default.WriteLineAndConsole($"Found Faction Safe Zone | IsServer = {MyAPIGateway.Multiplayer.IsServer}");
                            //MyVisualScriptLogicProvider.ShowNotification($"Got Safezone", 20000);
                            var split = zone.DisplayName.Split('_');
                            if (split.Length == 3)
                            {
                                long claimId;
                                ClaimBlockSettings settings;
                                long.TryParse(split[2], out claimId);
                                if (!Session.Instance.claimBlocks.TryGetValue(claimId, out settings))
                                {
                                    //MyLog.Default.WriteLineAndConsole($"No Settings Yet!!!!!");
                                    return;
                                }

                                MyAPIGateway.Utilities.InvokeOnGameThread(() =>
								{
									zone.Radius = settings.SafeZoneSize;
									zone.RecreatePhysics();
                                });
                            }

                            return;
                        }

                    if (!MyAPIGateway.Multiplayer.IsServer) return;
                    if (!Session.Instance.init) return;

                    foreach (var claim in Session.Instance.claimBlocks.Values)
                    {
                        if (Vector3D.Distance(claim.BlockPos, zone.PositionComp.GetPosition()) <= 10000)
                        {
                            long blockId = zone.SafeZoneBlockId;
                            if (blockId == 0)
                            {
                                MyAPIGateway.Utilities.InvokeOnGameThread(() => entity.Close());
                                return;
                            }

                            IMySafeZoneBlock block = null;
                            Session.Instance.safeZoneBlocks.TryGetValue(blockId, out block);
                            if (block != null)
                            {
                                //block.EnableSafeZone(false);
                                MyAPIGateway.Parallel.Sleep(5);
                                MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                                {
                                    var prop = block.GetProperty("SafeZoneCreate");
                                    var prop2 = prop as ITerminalProperty<bool>;

                                    if (prop2 != null)
                                    {
                                        prop2.SetValue(block, false);
                                    }
                                    return;
                                });

                                return;
                            }

                            MyAPIGateway.Utilities.InvokeOnGameThread(() => entity.Close());
                            return;
                        }

                        if (claim.IsClaimed)
                        {
                            if (Vector3D.Distance(claim.CenterToPlanet ? claim.PlanetCenter : claim.BlockPos, zone.PositionComp.GetPosition()) <= claim.ClaimRadius)
                            {
                                if (zone.DisplayName != null && zone.DisplayName.Contains("FSZ"))
                                {
                                    var split = zone.DisplayName.Split('_');
                                    IMyFaction zoneFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(split[1]);
                                    IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(claim.ClaimedFaction);
                                    if (zoneFaction == null)
                                    {
                                        MyAPIGateway.Utilities.InvokeOnGameThread(() => entity.Close());
                                        return;
                                    }

                                    if (claimFaction == zoneFaction) return;
                                }
                                else
                                {
                                    if (claim.GetSafeZones.Contains(zone.EntityId)) return;

                                    long zoneBlockId = zone.SafeZoneBlockId;
                                    IMySafeZoneBlock zoneBlock = null;

                                    Session.Instance.safeZoneBlocks.TryGetValue(zoneBlockId, out zoneBlock);
                                    if (zoneBlock != null)
                                    {
                                        IMyFaction zoneFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(zoneBlock.OwnerId);
                                        if (zoneFaction != null)
                                        {
                                            if (zoneFaction.Tag == claim.ClaimedFaction) return;
                                        }

                                        foreach (var item in claim.GetZonesDelayRemove)
                                        {
                                            if (item.zoneId == zoneBlockId) return;
                                        }

                                        //zoneBlock.EnableSafeZone(false);
                                        MyAPIGateway.Parallel.Sleep(5);
                                        MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                                        {
                                            var prop = zoneBlock.GetProperty("SafeZoneCreate");
                                            var prop2 = prop as ITerminalProperty<bool>;

                                            if (prop2 != null)
                                            {
                                                prop2.SetValue(zoneBlock, false);
                                            }
                                            return;
                                        });

                                        return;
                                    }
                                }

                                MyAPIGateway.Utilities.InvokeOnGameThread(() => entity.Close());
                                return;
                            }
                        }
                    }
                }
            }); 
        }

        public static void EntityCreate(MyEntity entity)
        {
            IMyTextSurface panel = entity as IMyTextSurface;
            if (panel == null) return;
            var t = panel as IMyTerminalBlock;
            if (t.CubeGrid?.Physics == null) return;

            if (t.CustomName.Contains("[Territory]"))
            {
                if (!Session.Instance.activeLCDs.Contains(t))
                    Session.Instance.activeLCDs.Add(t);

                Utils.GetTerritoriesStatus(t);
            }

            t.CustomNameChanged += CheckBlockName;
        }

        public static void EntityRemoved(MyEntity entity)
        {
            IMyTextSurface panel = entity as IMyTextSurface;
            if (panel == null) return;
            var t = panel as IMyTerminalBlock;

            if (Session.Instance.activeLCDs.Contains(t))
                Session.Instance.activeLCDs.Remove(t);

            t.CustomNameChanged -= CheckBlockName;
        }

        public static void CheckBlockName(IMyTerminalBlock block)
        {

        }

        public static void FatBlockAdded(MyCubeBlock block)
        {
            MyAPIGateway.Parallel.StartBackground(() =>
            {
                MyAPIGateway.Parallel.Sleep(5);
                if (block as IMyShipController != null ||
                block as IMyPowerProducer != null ||
                block as IMyShipToolBase != null ||
                block as IMyShipDrill != null ||
                block as IMyProductionBlock != null)
                {
                    ClaimBlockSettings settings = null;
                    foreach (var data in Session.Instance.claimBlocks.Values)
                    {
                        if (!data.Enabled || !data.IsClaimed) continue;
                        if (Vector3D.Distance(block.PositionComp.GetPosition(), data.CenterToPlanet ? data.PlanetCenter : data.BlockPos) > data.ClaimRadius) continue;
                        settings = data;
                        break;
                    }

                    if (settings == null) return;
                    MyCubeGrid grid = block.CubeGrid;
                    settings.UpdatesBlocksMonitored(grid, block, true);
                    //MyVisualScriptLogicProvider.ShowNotificationToAll($"Fat Block Added | {MyAPIGateway.Multiplayer.IsServer}", 5000, "Green");
                }
            });
        }

        public static void FatBlockClosed(MyCubeBlock block)
        {
            if (block as IMyShipController != null ||
                block as IMyPowerProducer != null ||
                block as IMyShipToolBase != null ||
                block as IMyShipDrill != null ||
                block as IMyProductionBlock != null)
            {
                ClaimBlockSettings settings = null;
                foreach (var data in Session.Instance.claimBlocks.Values)
                {
                    if (!data.Enabled || !data.IsClaimed) continue;
                    if (Vector3D.Distance(block.PositionComp.GetPosition(), data.CenterToPlanet ? data.PlanetCenter : data.BlockPos) > data.ClaimRadius) continue;
                    settings = data;
                    break;
                }

                if (settings == null) return;
                MyCubeGrid grid = block.CubeGrid;
                settings.UpdatesBlocksMonitored(grid, block, false);
                CheckForWorkingBlock(block, settings);
                //MyVisualScriptLogicProvider.ShowNotificationToAll($"Fat Block Closed | {MyAPIGateway.Multiplayer.IsServer}", 5000, "Green");
            }
        }

        /*public static void FatBlockCheckFunctional(IMySlimBlock slim)
        {
            IMyCubeBlock block = slim.FatBlock;
            if (block == null) return;
   
            if (block as IMyShipController != null ||
                block as IMyPowerProducer != null ||
                block as IMyShipToolBase != null ||
                block as IMyShipDrill != null ||
                block as IMyProductionBlock != null)
            {
                if (block.IsFunctional) return;
                ClaimBlockSettings settings = null;
                foreach (var data in Session.Instance.claimBlocks.Values)
                {
                    if (!data.Enabled || !data.IsClaimed) continue;
                    if (Vector3D.Distance(block.PositionComp.GetPosition(), data.BlockPos) > data.ClaimRadius) continue;
                    settings = data;
                    break;
                }

                if (settings == null) return;
                //MyCubeGrid grid = block.CubeGrid;
                //settings.UpdatesBlocksMonitored(grid, block, false);
                CheckForWorkingBlock(block, settings);
                MyVisualScriptLogicProvider.ShowNotificationToAll($"Block is no longer functional!!! | {MyAPIGateway.Multiplayer.IsServer}", 5000, "Green");
            }
        }*/

        public static void CheckForWorkingBlock(IMyCubeBlock block, ClaimBlockSettings settings)
        {
            try
            {
                if (settings == null) return;

                bool functional = false;
                MyCubeGrid grid = block.CubeGrid as MyCubeGrid;
                IMyShipController controller = block as IMyShipController;
                if (controller != null)
                {
                    functional = controller.IsFunctional;

                    if (!functional)
                    {
                        if (!settings._server._gridsInside.ContainsKey(grid.EntityId))
                        {
                            MyLog.Default.WriteLineAndConsole($"Territories: Key does not exist for controllers --{grid.EntityId}--");
                            return;
                        }
                        foreach (var t in settings._server._gridsInside[grid.EntityId].blocksMonitored.controllers)
                        {
                            functional = t.IsFunctional;
                            if (functional)
                            {
                                IMyFaction blockOwner = MyAPIGateway.Session.Factions.TryGetPlayerFaction(t.OwnerId);
                                if (blockOwner != null)
                                {
                                    functional = blockOwner.Tag == settings.ClaimedFaction;
                                    break;
                                }
                                else
                                {
                                    functional = false;
                                }

                            }
                        }
                    }

                    settings.UpdateGridData(grid.EntityId, ClaimBlockSettings.GridChangeType.Controller, functional);
                    return;
                }

                IMyPowerProducer power = block as IMyPowerProducer;
                if (power != null)
                {
                    functional = power.IsFunctional;

                    if (!functional)
                    {
                        if (!settings._server._gridsInside.ContainsKey(grid.EntityId))
                        {
                            MyLog.Default.WriteLineAndConsole($"Territories: Key does not exist for power --{grid.EntityId}--");
                            return;
                        }
                        foreach (var t in settings._server._gridsInside[grid.EntityId].blocksMonitored.powers)
                        {
                            functional = t.IsFunctional;
                            if (functional)
                            {
                                IMyFaction blockOwner = MyAPIGateway.Session.Factions.TryGetPlayerFaction(t.OwnerId);
                                if (blockOwner != null)
                                {
                                    functional = blockOwner.Tag == settings.ClaimedFaction;
                                    break;
                                }
                                else
                                {
                                    functional = false;
                                }

                            }
                        }
                    }

                    settings.UpdateGridData(grid.EntityId, ClaimBlockSettings.GridChangeType.Power, functional);
                    return;
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLineAndConsole($"Territories: Crash Logged --- {ex.StackTrace}");
            }
            
        }

        public static void IsWorkingChanged(IMyCubeBlock block)
        {
            if (block as IMyShipController != null || block as IMyPowerProducer != null || block as IMyShipToolBase != null || block as IMyShipDrill != null || block as IMyProductionBlock != null)
            {
                IMyFunctionalBlock fBlock = block as IMyFunctionalBlock;
                ClaimBlockSettings settings = null;
                foreach (var data in Session.Instance.claimBlocks.Values)
                {
                    if (!data.Enabled || !data.IsClaimed) continue;
                    if (Vector3D.Distance(block.GetPosition(), data.CenterToPlanet ? data.PlanetCenter : data.BlockPos) > data.ClaimRadius) continue;
                    settings = data;
                    break;
                }

                if (settings == null) return;
                IMyShipToolBase tool = block as IMyShipToolBase;
                IMyShipDrill drill = block as IMyShipDrill;
                IMyProductionBlock production = block as IMyProductionBlock;

                if (fBlock != null)
                {
                    if (tool != null || drill != null)
                    {
                        if (settings.AllowTools) return;
                        if (block is IMyShipDrill && settings.AllowDrilling) return;
                        if (block is IMyShipWelder && settings.AllowWelding) return;
                        if (block is IMyShipGrinder && settings.AllowGrinding) return;

                        IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                        if (blockFaction != null)
                        {
                            if (blockFaction.Tag == settings.ClaimedFaction) return;

                            if (settings.AllowTerritoryAllies || settings.AllowSafeZoneAllies)
                            {
                                if (!Utils.IsFactionEnemy(settings, blockFaction)) return;
                            }
                        }

                        fBlock.Enabled = false;
                        return;
                    }

                    if (production != null)
                    {
                        if (!fBlock.Enabled) return;
                        Utils.AddAllProductionMultipliers(settings, block as MyCubeBlock, true);
                        return;
                    }
                }
                
                CheckForWorkingBlock(block, settings);
            }
        }

        public static void PbWatcher(IMyCubeBlock block)
        {
            if (block == null) return;

            IMyProgrammableBlock pb = block as IMyProgrammableBlock;
            if (pb == null) return;

            if (pb.Enabled)
            {
                foreach(var settings in Session.Instance.claimBlocks.Values)
                {
                    if (Vector3D.Distance(block.GetPosition(), settings.BlockPos) > settings.SafeZoneSize) continue;
                    pb.Enabled = false;
                }
            }
        }

        public static void CheckOwnership(IMyTerminalBlock block)
        {
            //MyAPIGateway.Parallel.StartBackground(() =>
            //{
            //MyAPIGateway.Parallel.Sleep(20);
           /* MyCubeBlock cubeblock = block as MyCubeBlock;
            if (cubeblock == null) return;

            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            if (settings.IsClaimed)
            {
                IMyFaction blockOwner = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                if (blockOwner == null || blockOwner.Tag != settings.ClaimedFaction)
                {
                    IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                    if (faction != null)
                        cubeblock.ChangeBlockOwnerRequest(faction.FounderId, VRage.Game.MyOwnershipShareModeEnum.Faction);
                    else
                        cubeblock.ChangeBlockOwnerRequest(Session.Instance.blockOwner.FounderId, VRage.Game.MyOwnershipShareModeEnum.All);
                }
            }
            else
            {
                cubeblock.ChangeBlockOwnerRequest(Session.Instance.blockOwner.FounderId, VRage.Game.MyOwnershipShareModeEnum.Faction);
            }*/

                //var share = cubeblock.IDModule.ShareMode;
                //if (share == VRage.Game.MyOwnershipShareModeEnum.All) return;

                /*IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                if (faction == null || !faction.IsEveryoneNpc())
                {
                    IMyFaction npcFaction = Session.Instance.blockOwner;
                    if (npcFaction == null)
                    {
                        Session.Instance.blockOwner = MyAPIGateway.Session.Factions.TryGetFactionByTag("SPRT");
                        npcFaction = Session.Instance.blockOwner;
                    }
                }*/

                
            //});
        }

        public static void ProductionOwnershipChanged(IMyTerminalBlock block)
        {
            if (block == null) return;
            IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
            foreach(var item in Session.Instance.claimBlocks.Values)
            {
                if (!item.Enabled || (!item.IsClaimed)) continue;
                if (Vector3D.Distance(block.GetPosition(), item.CenterToPlanet ? item.PlanetCenter : item.BlockPos) > item.ClaimRadius) continue;
                if (!item.GetPerks.ContainsKey(PerkType.Production)) continue;
                if (!item.GetPerks[PerkType.Production].perk.productionPerk.GetAttachedEntities.Contains(block.EntityId))
                {
                    if (blockFaction != null && blockFaction.Tag == item.ClaimedFaction)
                        Utils.AddAllProductionMultipliers(item, block as MyCubeBlock, true);
                }
                else
                {
                    if(blockFaction == null)
                        Utils.RemoveProductionMultipliers(item, block as IMyProductionBlock, true);
                    else
                    {
                        if(blockFaction.Tag != item.ClaimedFaction)
                            Utils.RemoveProductionMultipliers(item, block as IMyProductionBlock, true);
                    }
                }
            }
        }

        public static void CheckGridStatic(IMyCubeGrid cubeGrid, bool isStatic)
        {
            if (!isStatic)
                cubeGrid.IsStatic = true;
        }

        private static bool IsAdmin(long owner)
        {
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (player.IdentityId == owner)
                {
                    if (player.PromoteLevel == MyPromoteLevel.Owner || player.PromoteLevel == MyPromoteLevel.Admin) return true;
                    if (MyAPIGateway.Session.LocalHumanPlayer != null && player.IdentityId == MyAPIGateway.Session.LocalHumanPlayer.IdentityId && MyAPIGateway.Session.HasCreativeRights) return true;
                    break;
                }
            }

            return false;
        }

        public static bool BeaconSetup(IMyBeacon beacon)
        {
            if (beacon == null || beacon.CubeGrid == null) return false;
			MyCubeGrid cubeGrid = beacon.CubeGrid as MyCubeGrid;
            if (cubeGrid == null) return false;
			beacon.AppendingCustomInfo += UpdateCustomInfo;

			//CheckOwnership(beacon);
			if (MyAPIGateway.Session.IsServer)
            {
				beacon.Enabled = true;
				beacon.CubeGrid.IsStatic = true;
				cubeGrid.Editable = true;   
				cubeGrid.DestructibleBlocks = false;
				//beacon.CubeGrid.CustomName = "Faction Territory Claim [NPC-IGNORE]";
				beacon.CustomName = "Faction Territory Claim";
				beacon.OwnershipChanged += CheckOwnership;
				beacon.CubeGrid.OnIsStaticChanged += CheckGridStatic;
                beacon.Radius = 0f;

				ClaimBlockSettings data = Session.Instance.LoadClaimData(beacon);
                if (data != null)
                {
                    Comms.SendClientsSettings(data);

                    if (data.IsClaimed && data.ClaimedFaction != null)
                        Utils.SetOwner(beacon, MyAPIGateway.Session.Factions.TryGetFactionByTag(data.ClaimedFaction));
                    else
                        Utils.SetOwner(beacon);
                }
                else
                {
                    Utils.SetOwner(beacon);
                }

                Triggers.CreateNewTriggers(beacon);
            }

            ClaimBlockSettings settings = null;
            if (!Session.Instance.claimBlocks.TryGetValue(beacon.EntityId, out settings)) return false;
            settings.Block = beacon as IMyTerminalBlock;
            Utils.SetEmissive(settings.BlockEmissive, beacon);

            if (settings.IsClaimed && settings.ClaimedFaction != null)
            {
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                Utils.SetScreen(beacon, faction);
            }
            else
            {
                Utils.SetScreen(beacon);
            }
            return true;
        }

        public static void SafeZoneBlockSetup(IMySafeZoneBlock zoneBlock)
        {
            if (MyAPIGateway.Session.IsServer)
            {
                if (Session.Instance.safeZoneBlocks.ContainsKey(zoneBlock.EntityId)) return;
                Session.Instance.safeZoneBlocks.Add(zoneBlock.EntityId, zoneBlock);
            }
            //MyVisualScriptLogicProvider.ShowNotification("Found Safezone block", 5000);
        }

        public static void UpdateCustomInfo(IMyTerminalBlock block, StringBuilder sb)
        {
            if (block as IMyBeacon != null)
            {
                ClaimBlockSettings data;
                Session.Instance.claimBlocks.TryGetValue(block.EntityId, out data);
                if (data == null) return;

                sb.Clear();
                sb.Append(new StringBuilder(data.DetailInfo));
            }

            if (block as IMyJumpDrive != null)
            {
                sb.Clear();
                sb.AppendStringBuilder(Controls.sb);
                //sb.Append(new StringBuilder(Controls.text));
            }
        }

        public static void PlayerConnected(long playerId)
        {
            /*if (!Session.Instance.init)
            {
                MyLog.Default.WriteLineAndConsole("Territories: Client Connected, Init False, Returning");
                return;
            }

            MyLog.Default.WriteLineAndConsole("Territories: Client Connected, Init True, Clearing Data/Reloading");*/
            ulong steamId = MyAPIGateway.Players.TryGetSteamId(playerId);
            if (steamId == 0) return;

            Comms.ResetClientModData(steamId);
            //foreach (var item in Session.Instance.claimBlocks.Values)
                //Comms.SendClientsSettings(item, steamId);
        }

        public static void PlayerDisconnected(long playerId)
        {
            GPS.RemoveCachedGps(playerId, GpsType.Tag);
		}
    }
}
