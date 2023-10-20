using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.Gui;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Faction_Territories
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_JumpDrive), false, "SiegeBlock")]
	public class SiegeBlockLogic : MyGameLogicComponent
	{
		public IMyJumpDrive jd => Entity as IMyJumpDrive;
		private bool IsServer => MyAPIGateway.Session.IsServer;
		private bool IsDedicated => MyAPIGateway.Utilities.IsDedicated;

		public override void Init(MyObjectBuilder_EntityBase objectBuilder)
		{
			base.Init(objectBuilder);
			NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
		}

		public override void UpdateOnceBeforeFrame()
		{
			if (ActionControls.IsSiegeBlock(jd))
				jd.AppendingCustomInfo += Controls.SiegeBlockCustomInfo;
		}

		public override void OnRemovedFromScene()
		{
			if (ActionControls.IsSiegeBlock(jd))
				jd.AppendingCustomInfo -= Controls.SiegeBlockCustomInfo;
		}
	}
}
