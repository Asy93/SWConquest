using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Faction_Territories
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_JumpDrive), false, "LargeJumpDrive")]
    public class JumpDriveLogic : MyGameLogicComponent
    {
        public IMyJumpDrive jd;
        private bool isServer;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            isServer = MyAPIGateway.Session.IsServer;
            jd = Entity as IMyJumpDrive;
            if (jd == null) return;
            if (jd?.CubeGrid?.Physics == null) return;

            Events.JumpDriveSetup(jd);
        }

        public override void OnRemovedFromScene()
        {
            //Unregister any handlers here
            if (jd.Physics == null) return;
            if (jd?.CubeGrid?.Physics == null) return;
            jd.AppendingCustomInfo -= Events.UpdateCustomInfo;

        }
    }
}
