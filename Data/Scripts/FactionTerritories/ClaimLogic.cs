using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using Faction_Territories.Network;
using VSL = Sandbox.Game.MyVisualScriptLogicProvider;

namespace Faction_Territories
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon), false, "ClaimBlock")]
    public class ClaimLogic : MyGameLogicComponent
    {
        public IMyBeacon beacon;
        private bool isServer => MyAPIGateway.Session.IsServer;
		private Vector3 BlockColor = new Vector3(0, 0, 0);
        private MyStringHash texture;

        public ClaimBlockSettings Settings;


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME; // | MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
			//if (!isServer) return;
            beacon = Entity as IMyBeacon;
            if (beacon == null || beacon.CubeGrid == null)
            {
				NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
				return;
            }

            if (beacon.CubeGrid.Physics == null)
            {
                beacon.CubeGrid.Close();
                return;
            }

			if (!Events.BeaconSetup(beacon))
            {
				NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                return;
            }

			BlockColor = beacon.SlimBlock.ColorMaskHSV;
            texture = beacon.SlimBlock.SkinSubtypeId;

            Session.Instance.claimBlocks.TryGetValue(beacon.EntityId, out Settings);
		}

        //public override void UpdateBeforeSimulation100()
        //{
        //    if (!isServer) return;
        //    if (beacon.SlimBlock.ColorMaskHSV != BlockColor)
        //    {
        //        BlockColor = beacon.SlimBlock.ColorMaskHSV;
        //
        //        ClaimBlockSettings settings;
        //        if (!Session.Instance.claimBlocks.TryGetValue(beacon.EntityId, out settings)) return;
        //        IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
        //        Utils.SetScreen(beacon, blockFaction, true);
        //
        //        settings.BlockEmissive = settings.BlockEmissive;
        //    }
        //
        //    if (beacon.SlimBlock.SkinSubtypeId != texture)
        //    {
        //        texture = beacon.SlimBlock.SkinSubtypeId;
        //
        //        ClaimBlockSettings settings;
        //        if (!Session.Instance.claimBlocks.TryGetValue(beacon.EntityId, out settings)) return;
        //        IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
        //        Utils.SetScreen(beacon, blockFaction, true);
        //
        //        settings.BlockEmissive = settings.BlockEmissive;
        //    }
        //}

        private void AddInventory()
        {
            MyDefinitionId token = new MyDefinitionId(typeof(MyObjectBuilder_Component), "ZoneChip");
            MyInventory inventory = new MyInventory(0.5f, new Vector3(1, 1, 1), MyInventoryFlags.CanReceive | MyInventoryFlags.CanSend);
            MyInventoryConstraint constraint = new MyInventoryConstraint("ClaimBlock");
            constraint.Add(token);
            inventory.Constraint = constraint;
            beacon.Components.Add<MyInventoryBase>(inventory);

            var inv = (MyInventory)beacon.GetInventory(0);
            if(inventory == null)
            {
                return;
            }

            inv.SetFlags(MyInventoryFlags.CanReceive | MyInventoryFlags.CanSend);
        }

        public override void OnRemovedFromScene()
        {
            if (beacon == null || beacon.CubeGrid == null || beacon.CubeGrid.Physics == null) return;
            //Unregister any handlers here
            beacon.AppendingCustomInfo -= Events.UpdateCustomInfo;
        }

		public override void MarkForClose()
		{
			base.MarkForClose();
			if (beacon == null || beacon.CubeGrid == null || beacon.CubeGrid.Physics == null) return;


			if (isServer)
            {
                beacon.OwnershipChanged -= Events.CheckOwnership;
                beacon.CubeGrid.OnIsStaticChanged -= Events.CheckGridStatic;
        
                Triggers.RemoveTriggerData(beacon.EntityId);
                Comms.SendRemoveBlockToOthers(beacon.EntityId);
				Session.Instance.claimBlocks.Remove(beacon.EntityId);
			}
            //else
            //{
            //    ClaimBlockSettings settings;
            //    if (!Session.Instance.claimBlocks.TryGetValue(beacon.EntityId, out settings)) return;
            //
            //    settings.Block = null;
            //}
        }
	}
}
