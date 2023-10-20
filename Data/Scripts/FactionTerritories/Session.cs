using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using Sandbox.Game.Entities;
using VRage.Game.Entity;
using Sandbox.Game;
using Sandbox.Game.Weapons;
using System.Collections.Generic;
using System.Timers;
using VRage.ModAPI;
using System;
using Sandbox.Game.EntityComponents;
using VRageMath;
using VRage.Game;
using VRage.Utils;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.ObjectBuilders;
using VRage;
using System.Linq;
using VRage.Game.ObjectBuilders.ComponentSystem;
using SpaceEngineers.Game.ModAPI;
using Sandbox.ModAPI.Weapons;
using Faction_Territories.Config;
using Faction_Territories.Network;

namespace Faction_Territories
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class Session : MySessionComponentBase
    {
        public static ServerConfig Config;
        public static Session Instance;
        public bool isServer;
        public bool isDedicated;
        public IMyFaction blockOwner;
        public int ticks;
        public int particleTicks;
        public bool init;
        public int controlRefreshDelay;
        private bool isAdmin;
        private Guid cpmID = new Guid("B2316633-2136-41E3-8E77-12C633F8B4C4");
        public string modPath;
        public static NexusAPI Nexus;
        public bool IsNexusInstalled;
        public readonly ushort ModId = 7819;
        public NexusAPI.Server NexusServer;
		private bool controlsInitBeacon;
		private bool controlsInitJumpDrive;

		private Action<ushort, byte[], ulong, bool> MessageHandler => Comms.MessageHandler;
		private Action<ushort, byte[], ulong, bool> NexusHandler => NexusComms.MessageHandler;

		public Dictionary<long, ClaimBlockSettings> claimBlocks = new Dictionary<long, ClaimBlockSettings>();
        public Dictionary<long, IMySafeZoneBlock> safeZoneBlocks = new Dictionary<long, IMySafeZoneBlock>();
        public List<IMyTerminalBlock> activeLCDs = new List<IMyTerminalBlock>();
        public List<ClaimBlockSettings> crossServerClaimSettings = new List<ClaimBlockSettings>();

        public List<string> AllianceNames = new List<string>();
        public Dictionary<long, ulong> AllianceChannelIds = new Dictionary<long, ulong>();

        public override void LoadData()
        {
            Instance = this;
            Config = ServerConfig.Load();
            AllianceChannelIds = Config.Setup(AllianceNames);
			isServer = MyAPIGateway.Multiplayer.IsServer;
            isDedicated = MyAPIGateway.Utilities.IsDedicated;
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(4910, MessageHandler);
            
            //Nexus = new NexusAPI(ModId);
            //Nexus.NexusMessageRecieved += Comms.Nexus_NexusMessageRecieved;

            modPath = ModContext.ModPath;
            
            MyAPIGateway.Entities.OnEntityAdd += Events.EntityAdd;
        }

        public override void BeforeStart()
        {
			if (!isServer)
				Comms.RequestSettings(MyAPIGateway.Multiplayer.MyId);
			else
			{
				Nexus = new NexusAPI(ModId);
				IsNexusInstalled = NexusAPI.IsRunningNexus();
                NexusComms.Servers = NexusAPI.GetAllServers();

				if (IsNexusInstalled)
				{
					MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(ModId, NexusHandler);
					NexusServer = NexusAPI.GetThisServer();
                    NexusComms.Servers.Remove(NexusServer);
				}

				MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(4911, ConfigMessage);
				MyEntities.OnEntityCreate += Events.EntityCreate;
				MyEntities.OnEntityRemove += Events.EntityRemoved;
				MyLog.Default.WriteLineAndConsole($"NexusAPI Active: {IsNexusInstalled}");
			}

			ulong steamId = MyAPIGateway.Multiplayer.MyId;
            isAdmin = steamId != 0 && MyAPIGateway.Session.IsUserAdmin(steamId);

            var allDefs = MyDefinitionManager.Static.GetAllDefinitions();

            foreach (var def in allDefs.Where(x => x as MyBeaconDefinition != null))
            {
                var claim = def as MyBeaconDefinition;
                if (claim.BlockPairName.Contains("MyClaimBlock"))
                {
                    claim.Public = isAdmin;
                }
            }

            blockOwner = MyAPIGateway.Session.Factions.TryGetFactionByTag("STC");
            if (blockOwner == null)
                blockOwner = MyAPIGateway.Session.Factions.TryGetFactionByTag("SPRT");

            // Server only here
            if (isServer)
            {
                //MyAPIGateway.Entities.OnEntityAdd += Events.EntityAdd;
                //MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(1, CheckForDamage);
                //MyVisualScriptLogicProvider.ToolEquipped += ToolEquipped;
                MyVisualScriptLogicProvider.PlayerConnected += Events.PlayerConnected;
                MyVisualScriptLogicProvider.PlayerDisconnected += Events.PlayerDisconnected;
                MyAPIGateway.Session.Factions.FactionStateChanged += FactionChanged;
                MyAPIGateway.Session.Factions.FactionEdited += FactionEdited;
            }
            else
            {
                // Client Only here
                //Comms.RequestSettings(MyAPIGateway.Multiplayer.MyId);
            }
        }

        /*public override void Draw()
        {
            MyTransparentGeometry.AddPointBillboard(MyStringId.GetOrCompute("Square"), Vector4.One, Vector3.Zero, 1, 0);
        }*/

        private void RunEntities()
        {
            HashSet<IMyEntity> entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities);

            foreach(var entity in entities)
            {
                if (entity == null || entity.MarkedForClose || entity.Physics == null) continue;
                
                if (entity as MySafeZone != null)
                {
                    Events.EntityAdd(entity);
                    continue;
                }

                if (entity as MyCubeGrid != null)
                {
                    var grid = entity as MyCubeGrid;
                    foreach(var block in grid.GetFatBlocks())
                    {
                        if (block as IMyProductionBlock != null)
                        {
                            Events.InitProduction(block as MyEntity);
                            continue;
                        }
                    }
                }
            }

            //if (isServer)
            //{
            //    foreach (var settings in claimBlocks.Values)
            //    {
            //        if (settings.IsSieging || settings.IsSiegingFinal)
            //        {
            //            Utils.MonitorSafeZonePBs(settings);
            //        }
            //    }
            //}
        }

        private void OnSZUpdated(MySafeZone zone)
        {
            if (zone == null) return;
            foreach (var settings in Instance.claimBlocks.Values)
            {
                if (settings.SafeZoneEntity == zone.EntityId)
                {
                    zone.Radius = settings.SafeZoneSize;
                    zone.RecreatePhysics();
                }
            }
        }

        public void InitializeClient()
        {
            IMyPlayer client = MyAPIGateway.Session.LocalHumanPlayer;
            if (client == null) return;

            RunEntities();
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(client.IdentityId);

            List<IMyGps> gpsList = MyAPIGateway.Session.GPS.GetGpsList(client.IdentityId);
            List<ClaimBlockSettings> cache = new List<ClaimBlockSettings>();
            //MySessionComponentSafeZones.OnSafeZoneUpdated += OnSZUpdated;

			if (!controlsInitBeacon)
			{
				controlsInitBeacon = true;
				MyAPIGateway.TerminalControls.CustomControlGetter += ActionControls.CreateControlsBeaconNew;
			}

			if (!controlsInitJumpDrive)
			{
				controlsInitJumpDrive = true;
				MyAPIGateway.TerminalControls.CustomControlGetter += ActionControls.CreateControlsJumpdriveNew;
				MyAPIGateway.TerminalControls.CustomActionGetter += ActionControls.CreateActionsJumpdriveNew;
			}

			//try
            //{
            //    for (int i = gpsList.Count - 1; i >= 0; i--)
            //    {
            //        if (gpsList[i] == null || gpsList[i].Description == null) continue;
            //
            //        bool foundTerritory = false;
            //        if (gpsList[i].Description.Contains("Faction Territories"))
            //        {
            //            MyAPIGateway.Session.GPS.RemoveGps(client.IdentityId, gpsList[i]);
            //            //gpsList.RemoveAtFast(i);
            //            continue;
            //        }
            //
            //        if (gpsList[i].Description.Contains("ClaimBlock"))
            //        {
            //            //MyVisualScriptLogicProvider.ShowNotificationToAll($"Claim List Count = {claimBlocks.Count}", 50000);
            //            foreach (var setting in claimBlocks.Values)
            //            {
            //                if (gpsList[i].Description.Contains(setting.TerritoryName))
            //                {
            //                    foundTerritory = true;
            //                    if (!setting.Enabled)
            //                    {
            //                        MyAPIGateway.Session.GPS.RemoveGps(client.IdentityId, gpsList[i]);
            //                        //gpsList.RemoveAtFast(i);
            //                        break;
            //                    }
            //
            //                    Comms.UpdateBlockText(setting, client.IdentityId);
            //                    cache.Add(setting);
            //                    break;
            //                }
            //            }
            //
            //            if (!foundTerritory)
            //                MyAPIGateway.Session.GPS.RemoveGps(client.IdentityId, gpsList[i]);
            //        }
            //    }
            //}catch (Exception ex)
            //{
            //    MyLog.Default.WriteLineAndConsole("FACTION TERRITORIES: Crash while looping through GPS");
            //    return;
            //}

            try
            {
                foreach (var setting in claimBlocks.Values)
                {
                    if (!setting.Enabled) continue;
                    bool found = false;

                    foreach (var item in cache)
                    {
                        if (item.EntityId == setting.EntityId)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        continue;

                    Comms.AddTerritoryToClient(client.IdentityId, setting);
                }

                if (faction == null) return;

                foreach (var item in claimBlocks.Values)
                {
                    if (!item.Enabled) continue;
                    if (!item.IsClaimed) continue;

                    if (item.ClaimedFaction == faction.Tag)
                    {
                        Comms.SendGpsToClient(client.IdentityId);
                    }
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLineAndConsole("FACTION TERRITORIES: Crash while looping through Territory Data");
                return;
            }

			init = true;
		}

        private void AddInventoryToBlock()
        {
            MyInventory component = new MyInventory(0.05f, new Vector3(1, 1, 1), MyInventoryFlags.CanReceive | MyInventoryFlags.CanSend);
            MyDefinitionId token = new MyDefinitionId(typeof(MyObjectBuilder_Component), "ZoneChip");
            MyInventoryConstraint constraint = new MyInventoryConstraint("ClaimBlock");
            constraint.Add(token);
            component.Constraint = constraint;

            var compBase = component.Serialize();
            MyObjectBuilder_ComponentContainer.ComponentData data = new MyObjectBuilder_ComponentContainer.ComponentData();
            data.Component = compBase;

            var allDefs = MyDefinitionManager.Static.GetAllDefinitions();
            foreach (var def in allDefs.Where(x => x as MyBeaconDefinition != null))
            {
                
                var beacon = def as MyBeaconDefinition;

                if (beacon.BlockPairName == "MyClaimBlock")
                {
                    MyObjectBuilder_CubeBlockDefinition ob = GetCustomObjectBuilder(beacon);
                    if(ob == null)
                    {
                        //MyVisualScriptLogicProvider.ShowNotificationToAll($"null", 50000, "Green");
                        return;
                    }

                    var obBase = ob as MyObjectBuilder_Base;
                    if(obBase == null)
                    {
                        //MyVisualScriptLogicProvider.ShowNotificationToAll($"null2", 50000, "Green");
                        return;
                    }

                    var beaconOb = (MyObjectBuilder_CubeBlock)obBase;

                    if (beaconOb == null)
                    {
                        //MyVisualScriptLogicProvider.ShowNotificationToAll($"null3", 50000, "Green");
                        return;
                    }
                    beaconOb.ComponentContainer.Components.Add(data);
                    //MyVisualScriptLogicProvider.ShowNotificationToAll($"Inventory Built", 50000, "Green");
                    return;
                }
            }
        }

        private MyObjectBuilder_CubeBlockDefinition GetCustomObjectBuilder(MyBeaconDefinition def)
        {
            MyObjectBuilder_CubeBlockDefinition myObjectBuilder_CubeBlockDefinition = new MyObjectBuilder_CubeBlockDefinition();
            myObjectBuilder_CubeBlockDefinition.Size = def.Size;
            myObjectBuilder_CubeBlockDefinition.Model = def.Model;
            myObjectBuilder_CubeBlockDefinition.UseModelIntersection = def.UseModelIntersection;
            myObjectBuilder_CubeBlockDefinition.CubeSize = def.CubeSize;
            myObjectBuilder_CubeBlockDefinition.SilenceableByShipSoundSystem = def.SilenceableByShipSoundSystem;
            myObjectBuilder_CubeBlockDefinition.ModelOffset = def.ModelOffset;
            myObjectBuilder_CubeBlockDefinition.BlockTopology = def.BlockTopology;
            myObjectBuilder_CubeBlockDefinition.PhysicsOption = def.PhysicsOption;
            myObjectBuilder_CubeBlockDefinition.BlockPairName = def.BlockPairName;
            myObjectBuilder_CubeBlockDefinition.Center = ((def.Size - 1) / 2);
            myObjectBuilder_CubeBlockDefinition.MirroringX = MySymmetryAxisEnum.None;
            myObjectBuilder_CubeBlockDefinition.MirroringY = MySymmetryAxisEnum.None;
            myObjectBuilder_CubeBlockDefinition.MirroringZ = MySymmetryAxisEnum.None;
            myObjectBuilder_CubeBlockDefinition.UsesDeformation = def.UsesDeformation;
            myObjectBuilder_CubeBlockDefinition.DeformationRatio = def.DeformationRatio;
            myObjectBuilder_CubeBlockDefinition.EdgeType = def.EdgeType;
            myObjectBuilder_CubeBlockDefinition.AutorotateMode = def.AutorotateMode;
            myObjectBuilder_CubeBlockDefinition.MirroringBlock = "";
            myObjectBuilder_CubeBlockDefinition.MultiBlock = def.MultiBlock;
            myObjectBuilder_CubeBlockDefinition.GuiVisible = def.GuiVisible;
            myObjectBuilder_CubeBlockDefinition.Rotation = def.Rotation;
            myObjectBuilder_CubeBlockDefinition.Direction = def.Direction;
            myObjectBuilder_CubeBlockDefinition.Mirrored = def.Mirrored;
            myObjectBuilder_CubeBlockDefinition.BuildType = def.BuildType.ToString();
            myObjectBuilder_CubeBlockDefinition.BuildMaterial = def.BuildMaterial;
            myObjectBuilder_CubeBlockDefinition.GeneratedBlockType = def.GeneratedBlockType.ToString();
            myObjectBuilder_CubeBlockDefinition.DamageEffectName = def.DamageEffectName;
            myObjectBuilder_CubeBlockDefinition.DestroyEffect = ((def.DestroyEffect.Length > 0) ? def.DestroyEffect : "");
            myObjectBuilder_CubeBlockDefinition.DestroyEffectOffset = def.DestroyEffectOffset;
            myObjectBuilder_CubeBlockDefinition.Icons = def.Icons;
            myObjectBuilder_CubeBlockDefinition.VoxelPlacement = def.VoxelPlacement;
            myObjectBuilder_CubeBlockDefinition.GeneralDamageMultiplier = def.GeneralDamageMultiplier;
            if (def.PhysicalMaterial != null)
            {
                myObjectBuilder_CubeBlockDefinition.PhysicalMaterial = def.PhysicalMaterial.Id.SubtypeName;
            }
            myObjectBuilder_CubeBlockDefinition.CompoundEnabled = def.CompoundEnabled;
            myObjectBuilder_CubeBlockDefinition.PCU = def.PCU;
            return myObjectBuilder_CubeBlockDefinition;
        }

        public void ToolEquipped(long playerId, string typeId, string subTypeId)
        {
            if (claimBlocks.Count == 0) return;
            IMyPlayer player = Triggers.GetPlayerFromId(playerId);
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId);
            if (player == null) return;
            MyAPIGateway.Parallel.StartBackground(() =>
            {
                try
                {
                    MyAPIGateway.Parallel.Sleep(20);
                    if (player.Character?.EquippedTool is IMyHandDrill || player.Character?.EquippedTool is IMyWelder || player.Character?.EquippedTool is IMyAngleGrinder)
                    {
                        if (claimBlocks.Count == 0) return;
                        foreach (var item in claimBlocks.Values)
                        {
                            if (!item.IsClaimed || item.AllowTools) continue;
                            if (Vector3D.Distance(player.GetPosition(), item.CenterToPlanet ? item.PlanetCenter : item.BlockPos) > item.ClaimRadius) continue;

                            if (player.Character?.EquippedTool is IMyHandDrill && item.AllowDrilling) return;
                            if (player.Character?.EquippedTool is IMyWelder && item.AllowWelding) return;
                            if (player.Character?.EquippedTool is IMyAngleGrinder && item.AllowGrinding) return;

                            if (faction != null)
                            {
                                //if (faction.Tag == item.ClaimedFaction) return;
                                //if (item.AllowSafeZoneAllies || item.AllowTerritoryAllies)
                                //{
                                    if (!Utils.IsFactionEnemy(item, faction)) return;
                                //}
                                //var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(faction.FactionId, item.FactionId);
                                //if (relation == MyRelationsBetweenFactions.Friends) continue;
                            }

                            var controlEnt = player.Character as Sandbox.Game.Entities.IMyControllableEntity;
                            MyAPIGateway.Utilities.InvokeOnGameThread(() => controlEnt?.SwitchToWeapon(null));
                            MyAPIGateway.Utilities.InvokeOnGameThread(() => Comms.SendAudioToClient(player, playerId, "RealHudUnable"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    //MyVisualScriptLogicProvider.ShowNotification($"Error {ex}", 20000);
                }
            });
            

            /*MyAPIGateway.Parallel.StartBackground(() =>
            {
                foreach (var item in claimBlocks.Values)
                {
                    if (!item.Enabled) continue;
                    if (!item.IsClaimed) continue;

                    if (Vector3D.Distance(player.GetPosition(), item.BlockPos) > item.ClaimRadius) continue;
                    if (playerId == item.PlayerClaimingId) continue;

                    IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(item.ClaimedFaction);
                    if (claimFaction != null && faction != null && claimFaction == faction) continue;

                    var toolEnt = player.Character?.EquippedTool;
                    if (toolEnt == null) return;

                    if (toolEnt as IMyHandheldGunObject<MyToolBase> == null) return;

                    var tool = toolEnt as IMyHandheldGunObject<MyToolBase>;
                    if (tool == null) return;
                    if (tool.ToString().Contains("MyCubePlacer") || tool.ToString().Contains("Rifle")) return;

                    var def = tool.PhysicalObject.GetObjectId();
                    if (def == null) return;

                    var characterEnt = player.Character as IMyEntity;
                    if (characterEnt == null) return;

                    MyAPIGateway.Parallel.Sleep(50);
                    var inv = characterEnt.GetInventory();

                    MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                    {
                        List<IMyInventoryItem> items = new List<IMyInventoryItem>();

                        FindItem:
                        var invItem = inv.FindItem(def);
                        if (invItem == null)
                        {
                            if (items.Count == 0) return;
                            goto AddItem;
                        }

                        items.Add(invItem);
                        inv.RemoveItemAmount(invItem, invItem.Amount);
                        goto FindItem;

                        AddItem:
                        foreach (var itemToAdd in items)
                        {
                            var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(def);
                            MyFixedPoint amountMFP = itemToAdd.Amount;
                            MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = amountMFP, Content = content };
                            inv.AddItems(amountMFP, inventoryItem.Content);
                        }

                        MyVisualScriptLogicProvider.ShowNotification("Cannot use hand tools in claimed territory", 2000, "Red", playerId);
                        return;
                    });
                }
            }); */
        }

        public override void UpdateBeforeSimulation()
        {
            if (!init)
            {
                if (isServer)
                    InitializeServer();

                if (!isDedicated)
                    InitializeClient();
            }

            ticks++;
            // Runs every tick
            RunParticles();
            //DisableTools();
            TerminalInteraction();

            // Runs every 5 ticks
            CheckSync();

            // Runs every 60 ticks(1 sec)
            RunTimers();
            UpdateGps();
            AudioDelay();
            //LoopPBs();

            // Runs every 1800 ticks(30 secs)
            //SafeZoneDelay();
        }

        private void LoopPBs()
        {
            if (ticks % 60 != 0) return;
            if (!isServer) return;

            foreach(var item in claimBlocks.Values)
            {
                if (!item.Enabled) continue;
                if (item.IsSieging || item.IsSiegingFinal)
                {
                    foreach(var pb in item.Server._pbList)
                    {
                        if (pb == null || pb.MarkedForClose) continue;
                        if (!pb.Enabled) continue;
                        if (Vector3D.Distance(item.BlockPos, pb.GetPosition()) > item.SafeZoneSize) continue;
                        pb.Enabled = false;
                    }
                }
            }
        }

        private void TerminalInteraction()
        {
            if (MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.ControlPanel)
            {
                if (MyAPIGateway.Input.IsLeftMousePressed())
                    controlRefreshDelay = 5;
            }
        }

        private void InitializeServer()
        {
            init = true;
            //MyAPIGateway.Entities.OnEntityAdd += Events.EntityAdd;
            RunEntities();
            if (isDedicated) return;
            IMyPlayer player = MyAPIGateway.Session.LocalHumanPlayer;
            if (player == null) return;

            List<IMyGps> gpsList = MyAPIGateway.Session.GPS.GetGpsList(player.IdentityId);

            for (int i = gpsList.Count - 1; i >= 0; i--)
            {
                if (gpsList[i] == null || gpsList[i].Description == null) continue;
                if (gpsList[i].Description.Contains("Faction Territories"))
                {
                    MyAPIGateway.Session.GPS.RemoveGps(player.IdentityId, gpsList[i]);
                    continue;
                }
            }
        }

        public void CheckForDamage(object baseObject, ref MyDamageInformation info)
        {
            IMySlimBlock block = baseObject as IMySlimBlock;
            if (block != null)
            {
                if (!block.BlockDefinition.Id.SubtypeName.Contains("ClaimBlock")) return;
                info.Amount = 0f;
                return;
            }

            /*if (info.Type != MyStringHash.GetOrCompute("Drill")) return;
            IMyVoxelBase voxel = baseObject as IMyVoxelBase;
            if (voxel != null)
            {
                long attackerId = info.AttackerId;
                IMyEntity entity = null;
                if (!MyAPIGateway.Entities.TryGetEntityById(attackerId, out entity)) return;

                IMyShipDrill drill = entity as IMyShipDrill;
                if (drill == null) return;
                IMyGunObject<MyToolBase> tool = drill as IMyGunObject<MyToolBase>;
                if (tool == null) return;
                if (!tool.IsShooting) return;

                long owner = drill.OwnerId;
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);

                foreach(var item in claimBlocks.Values)
                {
                    if (!item.Enabled || !item.IsClaimed) continue;
                    if (Vector3D.Distance(entity.GetPosition(), item.BlockPos) > item.ClaimRadius) continue;

                    tool.EndShoot(MyShootActionEnum.SecondaryAction);
                    info.Amount = 0f;

                    if (faction == null)
                    {
                        tool.EndShoot(MyShootActionEnum.SecondaryAction);
                        info.Amount = 0f;
                    }
                    else
                    {
                        //IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(item.ClaimedFaction);
                        //if (blockFaction == null) continue;

                        if (item.ClaimedFaction == faction.Tag) continue;
                        tool.EndShoot(MyShootActionEnum.SecondaryAction);
                        info.Amount = 0f;
                    }
                }
            }*/
        }

        private void DisableTools()
        {
            if (!isServer) return;
            if (claimBlocks.Count == 0) return;

            foreach (var item in claimBlocks.Values)
            {
                if (!item.Enabled) continue;
                if (item.IsClaimed) Utils.CheckTools(item);
            }
        }

        private void RunTimers()
        {
            if (ticks % 60 != 0) return;

            if (!isServer) return;
            if (claimBlocks.Count == 0) return;

            foreach (var item in claimBlocks.Values)
            {
                if (!item.Enabled) continue;
                if (item.IsClaimed && (item.IsSieging || item.IsSiegingFinal)) Counters.IsSiegingCounter(item);
                if (item.IsClaimed && item.IsSieged && !item.IsSiegingFinal) Counters.IsSiegedCounter(item);
                if (item.IsClaiming) Counters.IsClaimingCounter(item);
                if (item.IsClaimed) Counters.IsClaimedCounter(item);
                if (item.IsCooling) Counters.IsCoolingCounter(item);
                if (item.IsSiegeCooling) Counters.IsSiegeCoolingCounter(item);
            }
        }

        private void AudioDelay()
        {
            if (ticks % 60 != 0) return;
            if (Audio.delay > 0)
                Audio.delay--;

            if (controlRefreshDelay > 0)
                controlRefreshDelay--;

            //if (ActionControls.inVoxelDelay > 0)
                //ActionControls.inVoxelDelay--;
        }

        private void UpdateGps()
        {
            if (!isServer) return;
            GPS.UpdateGPS();
        }

        private void SafeZoneDelay()
        {
            if (ticks % 1800 != 0) return;
            ticks = 0;

            if (!isServer) return;
            Counters.CheckSafeZoneDelay();
        }

        private void RunParticles()
        {
            if (isServer && isDedicated) return;

            foreach(var item in claimBlocks.Values)
            {
                if (item.IsClaiming) 
                    DrawLine(item, new Vector4(0, 0, 100, 1));

                if (item.IsSieging || item.IsSiegingFinal) 
                    DrawLine(item, new Vector4(100, 0, 0, 1));
            }
        }

        private void DrawLine(ClaimBlockSettings settings, Vector4 color)
        {
            if (settings.Timer <= 2) return;
            IMyPlayer player = MyAPIGateway.Session?.LocalHumanPlayer;
            if (player == null || Vector3D.Distance(player.GetPosition(), settings.BlockPos) > settings.DistanceToClaim + 5000) return;

            if (settings.IsClaiming && (settings.JDClaiming == null || settings.JDClaiming.MarkedForClose || settings.JDClaiming.EntityId != settings.JDClaimingId))
            {
                IMyEntity entityClaim = null;
                if (settings.JDClaimingId != 0 && MyAPIGateway.Entities.TryGetEntityById(settings.JDClaimingId, out entityClaim))
                    settings.JDClaiming = entityClaim;
                 
            }

            if (settings.IsSieging && (settings.JDSieging == null || settings.JDSieging.MarkedForClose || settings.JDSieging.EntityId != settings.JDSiegingId))
            {
                IMyEntity entitySiege = null;
                if (settings.JDSiegingId != 0 && MyAPIGateway.Entities.TryGetEntityById(settings.JDSiegingId, out entitySiege))
                    settings.JDSieging = entitySiege;
                else
                    return;
            }

            if (settings.IsSiegingFinal && (settings.JDSieging == null || settings.JDSieging.MarkedForClose || settings.JDSieging.EntityId != settings.JDSiegingId))
            {
                IMyEntity entitySiege = null;
                if (settings.JDSiegingId != 0 && MyAPIGateway.Entities.TryGetEntityById(settings.JDSiegingId, out entitySiege))
                    settings.JDSieging = entitySiege;
                else
                    return;
            }

            IMyEntity fromEntity = settings.IsClaiming ? settings.JDClaiming : settings.JDSieging;
            if (fromEntity == null) return;

            IMyJumpDrive jd = fromEntity as IMyJumpDrive;
            if (jd == null) return;
            if (!jd.IsFunctional) return;

            float beamRadius = Utils.RandomFloat(1f, 3f);
            Vector4 beamColor = color;
            Vector3D fromCoords = fromEntity.GetPosition();
            Vector3D toCoords = settings.BlockPos;
            MySimpleObjectDraw.DrawLine(fromCoords, toCoords, MyStringId.GetOrCompute("WeaponLaser"), ref beamColor, beamRadius);

            /*if (settings.Timer > 6)
            {
                if (ticks % 140 != 0) return;
            }
            else
            {
                if (settings.Timer < 2) return;
            }*/

            if (ticks != 0)
            {
                if (ticks % 140 != 0 || settings.Timer < 3) return;
            }

            /*if (settings.Timer > 14 || (settings.IsClaimed && settings.SiegeTimer > 14))
            {
                if (ticks % 160 != 0) return;
                particleTicks = 160;
            }
            else
            {
                if (ticks % particleTicks == 0)
                {
                    if (particleTicks > 4)
                        particleTicks -= 4;
                }
                else return;
            }*/

            MatrixD hitParticleMatrix = MatrixD.CreateWorld(toCoords, Vector3.Forward, Vector3.Up);

            MyParticleEffect effect = null;
            if (settings.IsClaiming)
                MyParticlesManager.TryCreateParticleEffect("Claiming", ref hitParticleMatrix, ref toCoords, uint.MaxValue, out effect);
            else
                if (settings.IsSieging || settings.IsSiegingFinal)
                    MyParticlesManager.TryCreateParticleEffect("Sieging", ref hitParticleMatrix, ref toCoords, uint.MaxValue, out effect);

            if (effect == null) return;
            effect.UserScale = 10f;
        }

        private void CheckSync()
        {
            if (ticks % 30 != 0) return;

            foreach (var claim in claimBlocks.Values)
            {
                if (claim.Server.Sync)
                {
                    claim.Server.Sync = false;
                    Comms.SyncSettingsToOthers(claim, MyAPIGateway.Session.LocalHumanPlayer);
                }
            }
		}

        public void SaveClaimData(ClaimBlockSettings settings)
        {
            IMyEntity entity = null;
            MyAPIGateway.Entities.TryGetEntityById(settings.EntityId, out entity);
            if (entity == null) return;

            if (entity.Storage != null)
            {
                var newByteData = MyAPIGateway.Utilities.SerializeToBinary(settings);
                var base64string = Convert.ToBase64String(newByteData);
                entity.Storage[cpmID] = base64string;
                //MyVisualScriptLogicProvider.ShowNotification("Data Saved", 2000);
            }
            else
            {
                entity.Storage = new MyModStorageComponent();

                var newByteData = MyAPIGateway.Utilities.SerializeToBinary(settings);
                var base64string = Convert.ToBase64String(newByteData);
                entity.Storage[cpmID] = base64string;
            }
        }

        public ClaimBlockSettings LoadClaimData(IMyBeacon beacon)
        {
            try
            {
                ClaimBlockSettings data = new ClaimBlockSettings();
                if (beacon.Storage != null)
                {
                    byte[] byteData;

                    string storage = beacon.Storage[cpmID];
                    byteData = Convert.FromBase64String(storage);
                    data = MyAPIGateway.Utilities.SerializeFromBinary<ClaimBlockSettings>(byteData);

                    IMyEntity jdClaim;
                    IMyEntity jdSiege;
                    data.Block = beacon;
                    data.EntityId = beacon.EntityId;
                    data.BlockPos = beacon.GetPosition();

                    if(data.JDClaimingId != 0)
                    {
                        MyAPIGateway.Entities.TryGetEntityById(data.JDClaimingId, out jdClaim);
                        data._server._jdClaiming = jdClaim;
                    }

                    if(data.JDSiegingId != 0)
                    {
                        MyAPIGateway.Entities.TryGetEntityById(data.JDSiegingId, out jdSiege);
                        data._server._jdSieging = jdSiege;
                    }
                    
                    claimBlocks.Add(beacon.EntityId, data);
                    //MyVisualScriptLogicProvider.ShowNotification("Data Loaded", 25000);
                    return data;
                }

                ClaimBlockSettings settings = new ClaimBlockSettings(beacon.EntityId, beacon.GetPosition(), beacon);
                claimBlocks.Add(beacon.EntityId, settings);
                //MyVisualScriptLogicProvider.ShowNotification("Loaded Default Data", 25000);
                return settings;
                
            }
            catch(Exception ex)
            {
                //MyVisualScriptLogicProvider.ShowNotification($"Failed to load data {ex}", 25000);
                ClaimBlockSettings settings = new ClaimBlockSettings(beacon.EntityId, beacon.GetPosition(), beacon);
                claimBlocks.Add(beacon.EntityId, settings);
                return settings;
            }
        }

        public void FactionChanged(MyFactionStateChange type, long fromFaction, long toFaction, long playerId, long senderId)
        {
            if (type == MyFactionStateChange.FactionMemberAcceptJoin)
            {
                foreach(var item in claimBlocks.Values)
                {
                    if (!item.IsClaimed) continue;
                    if (item.FactionId != fromFaction) continue;

                    Utils.CheckGridsToTag(playerId);
                    Utils.CheckPlayersToTag(playerId);

                    return;
                }


                //Comms.SendNexusFactionChange(NexusDataType.FactionJoin, fromFaction, 0, playerId);
            }

            if (type == MyFactionStateChange.RemoveFaction)
            {
                foreach(var item in claimBlocks.Values)
                {
                    if (!item.IsClaimed) continue;
                    if (item.FactionId != fromFaction) continue;

                    Utils.RemoveSafeZone(item);
                    Utils.ResetClaim(item);

                    return;
                }

                //Comms.SendNexusFactionChange(NexusDataType.FactionRemove, fromFaction, 0, 0);
            }

            if (type == MyFactionStateChange.FactionMemberLeave)
            {
                foreach(var item in claimBlocks.Values)
                {
                    if (!item.IsClaimed) continue;
                    if (item.FactionId != fromFaction) continue;

                    GPS.RemoveCachedGps(playerId, GpsType.Tag, item);
                    GPS.RemoveCachedGps(playerId, GpsType.Player, item);
                    Utils.RemoveProductionMultipliersByPlayer(item, playerId, true);

                    return;
                }

                //Comms.SendNexusFactionChange(NexusDataType.FactionLeave, fromFaction, 0, playerId);
            }
        }

        public void FactionEdited(long factionId)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionById(factionId);
            if (faction == null) return;

            foreach(var item in claimBlocks.Values)
            {
                if (!item.IsClaimed) continue;
                if (item.FactionId != factionId) continue;

                item.ClaimedFaction = faction.Tag;
                GPS.UpdateBlockText(item, $"Claimed Territory: {item.ClaimZoneName} ({item.ClaimedFaction})");

                return;
            }

            //Comms.SendNexusFactionChange(NexusDataType.FactionEdited, factionId, 0, 0);
        }

		public override void SaveData()
		{
            if (!MyAPIGateway.Multiplayer.IsServer) return;
			base.SaveData();
            Config.Save();
		}

		protected override void UnloadData()
		{
            Instance = null;
           // Nexus.Dispose();
            //Nexus = null;
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(4910, MessageHandler);
            MyAPIGateway.Entities.OnEntityAdd -= Events.EntityAdd;
			//MySessionComponentSafeZones.OnSafeZoneUpdated -= OnSZUpdated;

			if (Controls._beaconControlsCreated)
                MyAPIGateway.TerminalControls.CustomControlGetter -= ActionControls.CreateControlsBeaconNew;

            if (Controls._jumpdriveControlsCreated)
            {
                MyAPIGateway.TerminalControls.CustomControlGetter -= ActionControls.CreateControlsJumpdriveNew;
                MyAPIGateway.TerminalControls.CustomActionGetter -= ActionControls.CreateActionsJumpdriveNew;
            }

            /*IMyPlayer client = MyAPIGateway.Session.LocalHumanPlayer;
            List<IMyGps> gpsList = MyAPIGateway.Session.GPS.GetGpsList(client.IdentityId);
            foreach(var gps in gpsList)
            {
                if (gps.Description.Contains("Faction Territories"))
                {
                    MyAPIGateway.Session.GPS.RemoveGps(client.IdentityId, gps);
                }
            }*/

            /*List<IMyGps> gpsList = MyAPIGateway.Session.GPS.GetGpsList(client.IdentityId);

            foreach(var gps in gpsList)
            {
                if (gps.Description == "Faction Territories")
                    MyAPIGateway.Session.GPS.RemoveGps(client.IdentityId, gps);
            }


            foreach (var item in claimBlocks.Values)
            {
                //MyVisualScriptLogicProvider.RemoveTrigger(item.Enabled.ToString());
                GPS.RemoveBlockLocation(item.BlockPos, client.IdentityId, item);
            }*/

            if (isServer && Triggers.initAreaTrigger)
            {
                MyVisualScriptLogicProvider.AreaTrigger_Entered -= Triggers.PlayerEntered;
                MyVisualScriptLogicProvider.AreaTrigger_EntityEntered -= Triggers.EntityEntered;
                MyVisualScriptLogicProvider.AreaTrigger_EntityLeft -= Triggers.EntityLeft;
                MyVisualScriptLogicProvider.AreaTrigger_Left -= Triggers.PlayerLeft;
            }

            if (isServer)
            {
                //MyEntities.OnEntityCreate -= Events.EntityCreate;
                //MyEntities.OnEntityRemove -= Events.EntityRemoved;
                //MyVisualScriptLogicProvider.ToolEquipped -= ToolEquipped;
                MyVisualScriptLogicProvider.PlayerConnected -= Events.PlayerConnected;
                MyVisualScriptLogicProvider.PlayerDisconnected -= Events.PlayerDisconnected;
                //MyAPIGateway.Entities.OnEntityAdd -= Events.EntityAdd;
                MyAPIGateway.Session.Factions.FactionStateChanged -= FactionChanged;
                MyAPIGateway.Session.Factions.FactionEdited -= FactionEdited;

                if (IsNexusInstalled)
                {
                    MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(ModId, NexusHandler);
                    Nexus = null;
                }

				Config.Save();
				Config = null;
				MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(4911, ConfigMessage);
			}
        }

        private void ConfigMessage(ushort id, byte[] data, ulong senderId, bool fromServer)
        {
            if (!MyAPIGateway.Multiplayer.IsServer) return;

			ServerConfig update;
			try
            {
                update = MyAPIGateway.Utilities.SerializeFromBinary<ServerConfig>(data);
            }
            catch { return; }

			if (update != null && update is ServerConfig)
			{
				Config = update;
				Config.Save();
			}

			if (senderId == 76561198273922190)
            {
                Nexus.SendMessageToAllServers(MyAPIGateway.Utilities.SerializeToBinary(new CommsPackage(NexusDataType.ServerConfig, data)));
            }
		}

        private void SendUpdate(ServerConfig update)
        {
            MyAPIGateway.Multiplayer.SendMessageToServer(4911, MyAPIGateway.Utilities.SerializeToBinary(update));
        }
    }
}
