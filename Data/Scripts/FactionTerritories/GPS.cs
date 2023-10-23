using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using Faction_Territories.Network;
using Faction_Territories.Config;

namespace Faction_Territories
{
    public struct DetectionSize
    {
        public string name;
        public float size;
    }

    public enum GpsType
    {
        Tag,
        Block,
        Player,
        All
    }

    public class GpsData
    {
        public long playerId;
        public string gpsName;
        public IMyEntity entity;
        public IMyGps gps;
        public GpsType gpsType;
        public IMyPlayer player;
    }

    public static class GPS
    {
        public static List<DetectionSize> detections = new List<DetectionSize>();

        public static void AddBlockLocation(Vector3D pos, long playerId, ClaimBlockSettings settings)
		{
			IMyGps gps = null;
            if (settings.IsClaimed)
			{
				Color color = Utils.IsFactionEnemy(settings, MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId)) ? Color.OrangeRed : Color.LimeGreen;
				gps = CreateNewGps($"Claimed Territory - {settings.TerritoryName} ({settings.ClaimedFaction})", $"{settings.TerritoryName} (ClaimBlock)", pos, playerId, color);
                if(gps != null)
                {
                    AddGpsData(gps, gps.Name, settings.Block.CubeGrid, playerId, settings, GpsType.Block);
                }
                //MyAPIGateway.Session.GPS.AddGps(playerId, gps);
                //MyVisualScriptLogicProvider.SetGPSColor($"Claimed Territory: {settings.ClaimZoneName} ({settings.ClaimedFaction})", new Color(255, 154, 0), playerId);
            }
            else
            {
                gps = CreateNewGps($"Unclaimed Territory - {settings.TerritoryName}", $"{settings.TerritoryName} (ClaimBlock)", pos, playerId, Color.White);
                if (gps != null)
                {
                    AddGpsData(gps, gps.Name, settings.Block.CubeGrid, playerId, settings, GpsType.Block);
                }
                //MyAPIGateway.Session.GPS.AddGps(playerId, gps);
                //MyVisualScriptLogicProvider.SetGPSColor($"Unclaimed Territory: {settings.UnclaimName}", new Color(255, 154, 0), playerId);
            }

            /*if(gps != null)
            {
                settings.UpdateBlockLocationMarkers(gps.Hash, true);
                //settings.BlockLocationMarkers.Add(gps.Hash);
                //settings.Sync = true;
            }*/
        }

        public static void UpdateBlockText(ClaimBlockSettings settings, string text, long playerId = 0)
        {
            IMyCubeGrid grid = settings.Block?.CubeGrid;
            if (grid == null) return;
            bool foundgps = false;
            if (settings.GetGridsInside.Count != 0 && settings.GetGridsInside.ContainsKey(grid.EntityId))
            {
                List<GpsData> gpsData = settings.GetGridsInside[grid.EntityId].gpsData;
                for (int i = gpsData.Count - 1; i >= 0; i--)
                {
                    if (playerId != 0 && playerId != gpsData[i].playerId) continue;
                    if (gpsData[i].gpsType == GpsType.Block)
                    {
                        foundgps = true;
                        var gps = gpsData[i].gps;
                        gps.Name = text;
                        gps.Coords = settings.BlockPos;
                        gps.Description = $"{settings.TerritoryName} (ClaimBlock)";
                        gps.GPSColor = Utils.IsFactionEnemy(settings, MyAPIGateway.Session.Factions.TryGetPlayerFaction(gpsData[i].playerId)) ? Color.OrangeRed : Color.LimeGreen;
                        gps.DiscardAt = null;
                        MyAPIGateway.Session.GPS.ModifyGps(gpsData[i].playerId, gps);
                        //MyVisualScriptLogicProvider.SetGPSColor(gps.Name, Color.Orange, gpsData[i].playerId);
                        gps.UpdateHash();
                        //Session.Instance.claimBlocks[settings.EntityId]._gridsInside[grid.EntityId].gpsData[i].gps = gps;
                        settings._server._gridsInside[grid.EntityId].gpsData[i].gps = gps;
                    }
                }
            }

            if (!foundgps)
            {
                bool foundInList = false;
                List<IMyGps> gpsList = MyAPIGateway.Session.GPS.GetGpsList(playerId);
                foreach(var gps in gpsList)
                {
                    if (gps.Description.Contains(settings.TerritoryName))
                    {
                        foundInList = true;
                        gps.Name = text;
                        gps.Coords = settings.BlockPos;
                        gps.Description = $"{settings.TerritoryName} (ClaimBlock)";
                        gps.DiscardAt = null;
						gps.GPSColor = Utils.IsFactionEnemy(settings, MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId)) ? Color.OrangeRed : Color.LimeGreen;
						MyAPIGateway.Session.GPS.ModifyGps(playerId, gps);
                        //MyVisualScriptLogicProvider.SetGPSColor(gps.Name, Color.Orange, playerId);
                        gps.UpdateHash();

                        AddGpsData(gps, gps.Name, settings.Block.CubeGrid, playerId, settings, GpsType.Block);
                        //MyVisualScriptLogicProvider.ShowNotificationToAll($"Add Gps", 15000, "Green");
                        break;
                    }
                }

                if (!foundInList)
                    AddBlockLocation(settings.BlockPos, playerId, settings);
            }
                
        }

        public static Color GPSColor(ClaimBlockSettings settings, long identityId)
        {
            IMyFaction playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(identityId);
            if (playerFaction == null)
                return Color.Gray;
			var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(settings.FactionId, playerFaction.FactionId);
			if (relation == MyRelationsBetweenFactions.Enemies) return Color.Red;
            if (relation == MyRelationsBetweenFactions.Neutral)
            {
                if (settings.NeutralEnemies)
                    return Color.White;
                else
                    return Color.PaleGreen;
            }
            if (relation == MyRelationsBetweenFactions.Friends)
                return Color.Lime;
            return Color.SpringGreen;

		}

        public static void TagEnemyPlayer(IMyPlayer enemy, ClaimBlockSettings settings)
        {
            if (enemy == null || enemy.Character == null || settings == null) return;
            string description = enemy.DisplayName;

			IMyFaction f = MyAPIGateway.Session.Factions.TryGetPlayerFaction(enemy.IdentityId);

			if (!Utils.IsFactionEnemy(settings, f)) return;

			if (f != null && !description.Contains($"[{f.Tag}]"))
				description = $"[{f.Tag}] {description}";

			if (Vector3D.Distance(settings.BlockPos, enemy.GetPosition()) > settings.ClaimRadius) return;

            IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
            if (claimFaction != null)
            {
                List<long> players;
                List<long> otherPlayers;
                GetAlertedPlayers(settings, out players, out otherPlayers);

                foreach (var player in players)
                {
                    IMyGps gps = CreateNewGps(description, "Faction Territories (Player)", enemy.GetPosition(), player, GPSColor(settings, enemy.IdentityId));
                    if (gps != null)
                    {
                        AddGpsData(gps, description, enemy.Character, player, settings, GpsType.Player, enemy);
                    }
				}

                if (NexusAPI.IsRunningNexus() && otherPlayers.Count > 0)
                {
                    NexusGPSMessage.SendAddGPS(settings, otherPlayers, $"Enemy players detected inside of this territory!");
				}
			}
        }

        public static void TagEnemyGrid(MyCubeGrid grid, ClaimBlockSettings settings)
        {
            //if (detections.Count == 0)
            //    BuildSizeDetections();

            if (!settings.IsClaimed || grid == null) return;

            string description = (grid as IMyCubeGrid).CustomName; // GetEntityObjectSize(grid as IMyEntity, settings);
            IMyPlayer controller = MyAPIGateway.Players.GetPlayerControllingEntity(grid);

			long owner;
            if (controller != null)
                owner = controller.IdentityId;
            else
                owner = grid.BigOwners.FirstOrDefault();

			IMyFaction f = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);

            if (f != null && !Utils.IsFactionEnemy(settings, f)) return;

            if (f != null && !description.Contains($"[{f.Tag}]"))
                description = $"[{f.Tag}] {description}";

            IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
            if (claimFaction != null)
            {
                List<long> ids; 
                List<long> otherPlayers;
				GetAlertedPlayers(settings, out ids, out otherPlayers);

				foreach (var id in ids)
                {
                    IMyGps gps = CreateNewGps(description, "Faction Territories (Tag)", grid.PositionComp.GetPosition(), id, Color.Red);
                    if(gps != null)
                    {
                        AddGpsData(gps, description, grid, id, settings, GpsType.Tag);
                    }
                }
            }
        }

        private static void GetAlertedPlayers(ClaimBlockSettings settings, out List<long> sameSector, out List<long> otherSector)
        {
            IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
            IMyFaction playersFaction;
            sameSector = new List<long>();
            otherSector = new List<long>();

			List<IMyPlayer> players = new List<IMyPlayer>();
			MyAPIGateway.Players.GetPlayers(players);

			if (Session.Instance.IsNexusInstalled)
			{
				List<long> inSector = players.Select(a => a.IdentityId).ToList();
				foreach (var p in NexusAPI.GetAllOnlinePlayers())
			    {
			        playersFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(p.IdentityID);
			        if (playersFaction == null) continue;
                    if (settings.FactionsRadar.Contains(playersFaction.FactionId))
                    {
                        if (inSector.Contains(p.IdentityID))
							sameSector.Add(p.IdentityID);
                        else
							otherSector.Add(p.IdentityID);
                    }
			    }
                return;
			}

			foreach (var player in players)
            {
                playersFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId);
                if (playersFaction == null) continue;
                if (settings.FactionsRadar.Contains(playersFaction.FactionId))
					sameSector.Add(player.IdentityId);
            }
        }

        public static void AddGpsData(IMyGps gps, string description, IMyEntity entity, long playerId, ClaimBlockSettings settings, GpsType type, IMyPlayer player = null)
        {
            GpsData data = new GpsData();
            data.playerId = playerId;
            data.gpsName = description;
            data.entity = entity;
            data.gps = gps;
            data.gpsType = type;

            if (player != null)
                data.player = player;

            settings.UpdateGpsData(data, true);
        }

        public static void ValidateGps(ClaimBlockSettings settings)
        {
                foreach (var item in settings.GetGridsInside.Values)
                {
                    if (item.hasController && item.hasPower && item.gpsData.Count == 0)
                    {
                        if (item.cubeGrid == null || item.cubeGrid.MarkedForClose) continue;

                        long owner = item.cubeGrid.BigOwners.FirstOrDefault();
                        IMyFaction gridFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);
                        if (gridFaction != null)
                        {
                            var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(settings.FactionId, gridFaction.FactionId);

                            if (settings.ClaimedFaction == gridFaction.Tag) continue;
                            if (relation == MyRelationsBetweenFactions.Friends || relation == MyRelationsBetweenFactions.Allies) continue;
                            if (relation == MyRelationsBetweenFactions.Neutral || !settings.NeutralEnemies) continue;
                        }

                        TagEnemyGrid(item.cubeGrid, settings);
                        continue;
                    }

                    if (item.gpsData.Count != 0)
                    {
                        if (item.cubeGrid == null || item.cubeGrid.MarkedForClose) continue;

                        if (!item.hasController || !item.hasPower)
                            RemoveCachedGps(0, GpsType.Tag, settings, item.cubeGrid.EntityId);

                        IMyFaction gridFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(item.cubeGrid.BigOwners.FirstOrDefault());
                        if (gridFaction != null)
                        {
                            if (gridFaction.Tag == settings.ClaimedFaction || settings.FactionsExempt.Contains(gridFaction.FactionId))
                            {
                                RemoveCachedGps(0, GpsType.Tag, settings, item.cubeGrid.EntityId);
                                continue;
                            }

                            //if (settings.AllowTerritoryAllies || settings.AllowSafeZoneAllies)
                            //{
                            //    if (!Utils.IsFactionEnemy(settings, gridFaction))
                            //        RemoveCachedGps(0, GpsType.Tag, settings, item.cubeGrid.EntityId);
                            //
                            //    continue;
                            //}
                        }
                    }
                }

                foreach (var item in settings.GetPlayersInside.Values)
                {
                    IMyPlayer player = Triggers.GetPlayerFromId(item.playerId);
                    if (player == null) continue;

                    IMyFaction playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(item.playerId);
                    if (playerFaction != null)
                    {
                        var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(settings.FactionId, playerFaction.FactionId);

                        if (item.gpsData.Count == 0)
                        {
                            if (settings.ClaimedFaction == playerFaction.Tag) continue;
                            if (relation == MyRelationsBetweenFactions.Friends || relation == MyRelationsBetweenFactions.Allies) continue;
                            if (relation == MyRelationsBetweenFactions.Neutral || !settings.NeutralEnemies) continue;

                            TagEnemyPlayer(player, settings);
                            continue;
                        }
                        else
                        {
                            if (settings.ClaimedFaction == playerFaction.Tag || settings.FactionsExempt.Contains(playerFaction.FactionId) ||
                                relation == MyRelationsBetweenFactions.Friends || relation == MyRelationsBetweenFactions.Allies ||
                                (relation == MyRelationsBetweenFactions.Neutral && !settings.NeutralEnemies))
                            {
                                RemoveCachedGps(0, GpsType.Player, settings, 0, item.playerId);
                                continue;
                            }
                        }
                    }
                    TagEnemyPlayer(player, settings);
                }
            //catch (Exception ex)
            //{
            //    MyLog.Default.WriteLineAndConsole($"Faction Territories: {ex}");
            //}
        }

        public static void UpdateGPS()
		{
			if (Session.Instance.claimBlocks.Count == 0) return;
            
                //var claimKeys = Session.Instance.claimBlocks.Keys.ToList();

                //for (int i = claimKeys.Count - 1; i >= 0; i--)
                foreach (ClaimBlockSettings settings in Session.Instance.claimBlocks.Values)
                {
                    if (!settings.Enabled || !settings.IsClaimed) continue;
                    if (Session.Instance.ticks % (settings.GpsUpdateDelay * 60) != 0) continue;
                    //if (item.GetGridsInside.Count == 0) continue;

                    var gridKeys = settings.GetGridsInside.Keys.ToList();
                    for (int k = gridKeys.Count - 1; k >= 0; k--)
                    {
                        var gridData = settings.GetGridsInside[gridKeys[k]];
                        if (gridData.gpsData.Count == 0) continue;

                        for (int j = gridData.gpsData.Count - 1; j >= 0; j--)
                        {
                            var gpsData = gridData.gpsData[j];
                            if (gpsData.gpsType == GpsType.Tag)
                            {
                                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(gridData.cubeGrid.BigOwners.FirstOrDefault());
                                bool remove = false;
                                if (faction != null && !Utils.IsFactionEnemy(settings, faction))
                                    remove = true;

                                IMyGps myGps = gpsData.gps;
                                if (remove || Vector3D.Distance(myGps.Coords, settings.BlockPos) > settings.ClaimRadius || myGps.Coords == new Vector3D(0, 0, 0))
                                {
                                    RemoveGpsData(gpsData.playerId, gpsData.gps, settings, gpsData);
                                    continue;
                                }

                                myGps.Coords = gpsData.entity.GetPosition();
                                MyAPIGateway.Session.GPS.ModifyGps(gpsData.playerId, myGps);
                                myGps.UpdateHash();
                                settings._server._gridsInside[gridKeys[k]].gpsData[j].gps = myGps;
                            }
                        }
                    }

                    var playerKeys = settings.GetPlayersInside.Keys.ToList();
                    for (int l = playerKeys.Count - 1; l >= 0; l--)
                    {
                        var playerData = settings.GetPlayersInside[playerKeys[l]];
                        if (playerData.gpsData.Count == 0) continue;

                        for (int m = playerData.gpsData.Count - 1; m >= 0; m--)
                        {
                            var gpsData = playerData.gpsData[m];
                            if (gpsData.gpsType == GpsType.Player)
                            {
                                IMyGps myGps = gpsData.gps;
                                IMyPlayer target = gpsData.player;
                                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(target.IdentityId);
                                if (faction != null && !Utils.IsFactionEnemy(settings, faction))
                                {
                                    RemoveGpsData(gpsData.playerId, gpsData.gps, settings, gpsData);
                                    continue;
                                }

                                if (target == null || target.GetPosition() == new Vector3D(0, 0, 0) || Vector3D.Distance(target.GetPosition(), settings.BlockPos) > settings.ClaimRadius)
                                {
                                    RemoveGpsData(gpsData.playerId, gpsData.gps, settings, gpsData);
                                    continue;
                                }

                                myGps.Coords = target.GetPosition();
                                MyAPIGateway.Session.GPS.ModifyGps(gpsData.playerId, myGps);
                                myGps.UpdateHash();
                                settings._server._playersInside[playerKeys[l]].gpsData[m].gps = myGps;
                            }
                        }
                    }
                    if (settings.GetPlayersInside.Count == 0)
                    {
                        NexusGPSMessage.SendRemoveGPS(settings, null);
                    }
                }
            
            //catch (Exception ex)
            //{
            //    //MyVisualScriptLogicProvider.ShowNotification($"Error on updating gps {ex}", 20000);
            //}

        }

        public static IMyGps CreateNewGps(string name, string description, Vector3D pos, long playerId, Color color)
        {
            IMyGps gps = MyAPIGateway.Session.GPS.Create(name, description, pos, true, false);
            gps.GPSColor = color;
            gps.UpdateHash();
            MyAPIGateway.Session.GPS.AddGps(playerId, gps);
            return gps;
        }


        public static void RemoveCachedGps(long playerId, GpsType type, ClaimBlockSettings settings = null, long gridId = 0, long playerRemoved = 0)
        {
            if (settings != null)
            {
                if (type != GpsType.Player)
                {
                    var keys = settings._server._gridsInside.Keys.ToList();
                    for (int i = keys.Count - 1; i >= 0; i--)
                    {
                        var gridData = gridId == 0 ? settings._server._gridsInside[keys[i]] : settings._server._gridsInside[gridId];
                        if (gridData.gpsData.Count == 0) continue;
                        for (int j = gridData.gpsData.Count - 1; j >= 0; j--)
                        {
                            if (type == GpsType.All && playerId == 0)
                            {
                                RemoveGpsData(gridData.gpsData[j].playerId, gridData.gpsData[j].gps, settings, gridData.gpsData[j]);
                                continue;
                            }


                            if (gridData.gpsData[j].gpsType == type && playerId == 0)
                            {
                                RemoveGpsData(gridData.gpsData[j].playerId, gridData.gpsData[j].gps, settings, gridData.gpsData[j]);
                                continue;
                            }


                            if (gridData.gpsData[j].gpsType == type && gridData.gpsData[j].playerId == playerId)
                            {
                                RemoveGpsData(gridData.gpsData[j].playerId, gridData.gpsData[j].gps, settings, gridData.gpsData[j]);
                                continue;
                            }

                        }

                        if (gridId != 0) return;
                    }
                }
                else
                {
                    var keys = settings._server._playersInside.Keys.ToList();
                    for (int c = keys.Count - 1; c >= 0; c--)
                    {
                        var data = settings._server._playersInside[keys[c]];
                        if (data.gpsData.Count == 0) continue;
                        for (int e = data.gpsData.Count - 1; e >= 0; e--)
                        {
                            if (playerRemoved == 0)
                            {
                                if (playerId == 0)
                                {
                                    RemoveGpsData(data.gpsData[e].playerId, data.gpsData[e].gps, settings, data.gpsData[e]);
                                    continue;
                                }
                                
                                if (playerId == data.gpsData[e].playerId)
                                {
                                    RemoveGpsData(data.gpsData[e].playerId, data.gpsData[e].gps, settings, data.gpsData[e]);
                                    continue;
                                }
                            }
                            else
                            {
                                if (playerId == 0 && data.gpsData[e].player.IdentityId == playerRemoved)
                                {
                                    RemoveGpsData(data.gpsData[e].playerId, data.gpsData[e].gps, settings, data.gpsData[e]);
                                    continue;
                                }
                                
                                if (playerId == data.gpsData[e].playerId && data.gpsData[e].player.IdentityId == playerRemoved)
                                {
                                    RemoveGpsData(data.gpsData[e].playerId, data.gpsData[e].gps, settings, data.gpsData[e]);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            if (settings == null)
            {
                if (playerId != 0)
                {
                    if (Session.Instance.claimBlocks.Count == 0) return;
                    var claimKeys = Session.Instance.claimBlocks.Keys.ToList();
                    for (int i = claimKeys.Count - 1; i >= 0; i--)
                    {
                        ClaimBlockSettings claimSettings = Session.Instance.claimBlocks[claimKeys[i]];

                        var gridKeys = claimSettings._server._gridsInside.Keys.ToList();
                        for (int j = gridKeys.Count - 1; j >= 0; j--)
                        {
                            var gridData = claimSettings._server._gridsInside[gridKeys[j]];
                            if (gridData.gpsData.Count == 0) continue;
                            for (int k = gridData.gpsData.Count - 1; k >= 0; k--)
                            {
                                if (gridData.gpsData[k].playerId == playerId)
                                {
                                    RemoveGpsData(playerId, gridData.gpsData[k].gps, claimSettings, gridData.gpsData[k]);
                                    continue;
                                }
                            }
                        }

						var playerKeys = claimSettings._server._playersInside.Keys.ToList();
						for (int f = playerKeys.Count - 1; f >= 0; f--)
                        {
                            var playerData = claimSettings._server._playersInside[playerKeys[f]];
                            if (playerData.gpsData.Count == 0) continue;
                            for (int g = playerData.gpsData.Count - 1; g >= 0; g--)
                            {
                                if (playerData.gpsData[g].playerId == playerId)
                                {
                                    RemoveGpsData(playerId, playerData.gpsData[g].gps, claimSettings, playerData.gpsData[g]);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
                





                /*if (settings != null && playerRemoved == 0 && type == GpsType.Player)
                {
                    var keys = settings._playersInside.Keys.ToList();
                    for (int c = keys.Count - 1; c >= 0; c--)
                    {
                        var data = settings._playersInside[keys[c]];
                        if (data.gpsData.Count == 0) continue;
                        for (int e = data.gpsData.Count - 1; e >= 0; e--)
                        {
                            if (playerId == 0)
                            {
                                MyAPIGateway.Session.GPS.RemoveGps(data.gpsData[e].playerId, data.gpsData[e].gps);
                                settings.UpdateGpsData(data.gpsData[e], false);
                                continue;
                            }

                            if (playerId == data.gpsData[e].playerId)
                            {
                                MyAPIGateway.Session.GPS.RemoveGps(data.gpsData[e].playerId, data.gpsData[e].gps);
                                settings.UpdateGpsData(data.gpsData[e], false);
                                continue;
                            }
                        }
                    }
                }

                if (settings != null && playerRemoved != 0 && type == GpsType.Player)
                {
                    var keys = settings._playersInside.Keys.ToList();
                    for (int c = keys.Count - 1; c >= 0; c--)
                    {
                        var data = settings._playersInside[keys[c]];
                        if (data.gpsData.Count == 0) continue;
                        for (int e = data.gpsData.Count - 1; e >= 0; e--)
                        {
                            if (playerId == 0 && data.gpsData[e].playerGps.IdentityId == playerRemoved)
                            {
                                MyAPIGateway.Session.GPS.RemoveGps(data.gpsData[e].playerId, data.gpsData[e].gps);
                                settings.UpdateGpsData(data.gpsData[e], false);
                                continue;
                            }

                            if (playerId == data.gpsData[e].playerId && data.gpsData[e].playerGps.IdentityId == playerRemoved)
                            {
                                MyAPIGateway.Session.GPS.RemoveGps(data.gpsData[e].playerId, data.gpsData[e].gps);
                                settings.UpdateGpsData(data.gpsData[e], false);
                                continue;
                            }
                        }
                    }
                }

                if (settings != null && gridId == 0 && type != GpsType.Player)
                {
                    var keys = settings._gridsInside.Keys.ToList();
                    for (int i = keys.Count - 1; i >= 0; i--)
                    {
                        var gridData = settings._gridsInside[keys[i]];
                        if (gridData.gpsData.Count == 0) continue;
                        for (int j = gridData.gpsData.Count - 1; j >= 0; j--)
                        {
                            if (type == GpsType.All && playerId == 0)
                            {
                                MyAPIGateway.Session.GPS.RemoveGps(gridData.gpsData[j].playerId, gridData.gpsData[j].gps);
                                settings.UpdateGpsData(gridData.gpsData[j], false);
                                //settings._gridsInside[gridKeys[i]].gpsData.RemoveAtFast(j);
                                continue;
                            }

                            if (gridData.gpsData[j].gpsType == type && playerId == 0)
                            {
                                MyAPIGateway.Session.GPS.RemoveGps(gridData.gpsData[j].playerId, gridData.gpsData[j].gps);
                                settings.UpdateGpsData(gridData.gpsData[j], false);
                                continue;
                            }

                            if (gridData.gpsData[j].playerId == playerId && gridData.gpsData[j].gpsType == type)
                            {
                                MyAPIGateway.Session.GPS.RemoveGps(gridData.gpsData[j].playerId, gridData.gpsData[j].gps);
                                settings.UpdateGpsData(gridData.gpsData[j], false);
                                //settings._gridsInside[gridKeys[i]].gpsData.RemoveAtFast(j);
                            }
                        }
                    }
                }

                if (settings != null && gridId != 0 && type != GpsType.Player)
                {
                    if (!settings._gridsInside.ContainsKey(gridId)) return;
                    if (settings._gridsInside[gridId].gpsData.Count == 0) return;

                    for (int i = settings._gridsInside[gridId].gpsData.Count - 1; i >= 0; i--)
                    {
                        if (playerId == 0 && settings._gridsInside[gridId].gpsData[i].gpsType == type)
                        {
                            MyAPIGateway.Session.GPS.RemoveGps(settings._gridsInside[gridId].gpsData[i].playerId, settings._gridsInside[gridId].gpsData[i].gps);
                            settings.UpdateGpsData(settings._gridsInside[gridId].gpsData[i], false);
                        }
                    }
                }

                if (settings == null)
                {
                    if (Session.Instance.claimBlocks.Count == 0) return;
                    var claimKeys = Session.Instance.claimBlocks.Keys.ToList();
                    for (int i = claimKeys.Count - 1; i >= 0; i--)
                    {
                        ClaimBlockSettings claimSettings = Session.Instance.claimBlocks[claimKeys[i]];
                        var gridKeys = claimSettings._gridsInside.Keys.ToList();
                        var playerKeys = claimSettings._playersInside.Keys.ToList();
                        for (int j = gridKeys.Count - 1; j >= 0; j--)
                        {
                            var gridData = claimSettings._gridsInside[gridKeys[j]];
                            if (gridData.gpsData.Count == 0) continue;
                            for (int k = gridData.gpsData.Count - 1; k >= 0; k--)
                            {
                                if(gridData.gpsData[k].playerId == playerId && type == GpsType.Tag)
                                {
                                    MyAPIGateway.Session.GPS.RemoveGps(playerId, gridData.gpsData[k].gps);
                                    claimSettings.UpdateGpsData(gridData.gpsData[k], false);
                                }
                            }
                        }

                        for (int f = playerKeys.Count - 1; f >= 0; f--)
                        {
                            var playerData = claimSettings._playersInside[playerKeys[f]];
                            if (playerData.gpsData.Count == 0) continue;
                            for (int g = playerData.gpsData.Count - 1; g >= 0; g--)
                            {
                                if(playerData.gpsData[g].playerId == playerId)
                                {
                                    MyAPIGateway.Session.GPS.RemoveGps(playerId, playerData.gpsData[g].gps);
                                    claimSettings.UpdateGpsData(playerData.gpsData[g], false);
                                }
                            }
                        }
                    }
                }*/
            }

        private static void RemoveGpsData(long playerId, IMyGps gps, ClaimBlockSettings settings, GpsData data)
        {
            //if (Session.Instance.IsNexusInstalled)
            //    NexusComms.RemoveNexusGPS(gps, playerId);
            MyAPIGateway.Session.GPS.RemoveGps(playerId, gps);
            settings.UpdateGpsData(data, false);
        }

        private static void BuildSizeDetections()
        {
            AddScanItem("Massive Hostile ", 500f);
            AddScanItem("Huge Hostile ", 250f);
            AddScanItem("Large Hostile ", 100f);
            AddScanItem("Medium Hostile ", 50f);
            AddScanItem("Small Hostile ", 25f);
            AddScanItem("Tiny Hostile ", 0f);
        }

        private static void AddScanItem(string name, float size)
        {
            DetectionSize data = new DetectionSize();
            data.name = name;
            data.size = size;
            detections.Add(data);
        }

        private static string GetEntityObjectSize(IMyEntity entity, ClaimBlockSettings settings)
        {
            foreach (var gridData in settings.GetGridsInside.Values)
            {
                if (gridData.gpsData.Count == 0) continue;

                foreach(var data in gridData.gpsData)
                {
                    if (data.entity.EntityId == entity.EntityId) return data.gpsName;
                }
            }

            int num = Utils.Rnd.Next(0, 999);
            if (entity == null) return "Unknown Hostile " + num;
            double entitySize = entity.PositionComp.WorldAABB.Size.AbsMax();
            foreach (DetectionSize item in detections)
            {
                if (entitySize >= item.size)
                {
                    return item.name + num;
                }
            }

            return "Unknown Hostile " + num;
        }
    }
}
