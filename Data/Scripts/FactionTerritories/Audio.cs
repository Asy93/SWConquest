using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Entity;
using VRage.ModAPI;

namespace Faction_Territories
{
    public static class Audio
    {
        private static IMyEntity currentEmitter;
        private static MyEntity3DSoundEmitter emitter;
        public static int delay = 0;

        public static void PlayClip(string clip)
        {
            if (delay > 0) return;
            IMyEntity controlledEntity = MyAPIGateway.Session.LocalHumanPlayer?.Controller?.ControlledEntity?.Entity;
            if (controlledEntity == null) return;

            if (currentEmitter == null || currentEmitter != controlledEntity || emitter == null)
            {
                currentEmitter = controlledEntity;
                emitter = new MyEntity3DSoundEmitter(controlledEntity as MyEntity);
            }

            if (emitter == null) return;
            var soundPair = new MySoundPair(clip);
            emitter.PlaySound(soundPair);
            delay = 5;
            //MyVisualScriptLogicProvider.ShowNotification("No Tools Allowed Inside Claimed Territory...", 3000, "Red");
        }
    }
}
