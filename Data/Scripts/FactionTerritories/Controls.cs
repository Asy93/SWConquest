using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Utils;
using VRage.ModAPI;
using Sandbox.Game;
using VRageMath;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Components;

namespace Faction_Territories
{
    public static class Controls
    {
        public static bool _beaconControlsCreated;
        public static bool _jumpdriveControlsCreated;
        public static bool _jumpdriveActionsCreated;
        public static StringBuilder sb;
        public static List<IMyTerminalControl> AdminControls = new List<IMyTerminalControl>();
        public static List<IMyTerminalControl> PlayerControls = new List<IMyTerminalControl>();
        public static List<IMyTerminalControl> SiegeControls = new List<IMyTerminalControl>();

        public static void CreateBeaconControls(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (block as IMyBeacon == null || block.BlockDefinition.SubtypeName != "ClaimBlock") return;

            if (!_beaconControlsCreated)
            {
                CreateClaimControls(block);
            }

            controls.Clear();

            if (MyAPIGateway.Session.HasCreativeRights)
                controls.AddRange(AdminControls);

            controls.AddRange(PlayerControls);

            foreach (var control in controls)
            {
                if (control.Id.Contains("PlayerToggleSpeed"))
                {
                    var toggle = control as IMyTerminalControlCheckbox;
                    if (toggle == null) continue;
                    toggle.Title = MyStringId.GetOrCompute($"Add {Math.Round(ActionControls.GetProductionSpeed(block), 0)}% Production Speed:\n{(ActionControls.GetSpeedTokens(block))} Token(s)/{ActionControls.GetTimeToConsumeToken(block) / 60} minute(s)");
                    control.RedrawControl();
                }

                if (control.Id.Contains("PlayerToggleYield"))
                {
                    var toggle = control as IMyTerminalControlCheckbox;
                    if (toggle == null) continue;
                    toggle.Title = MyStringId.GetOrCompute($"Add {Math.Round(ActionControls.GetProductionYield(block), 0)}% Production Yield:\n{(ActionControls.GetYieldTokens(block))} Token(s)/{ActionControls.GetTimeToConsumeToken(block) / 60} minute(s)");
                    control.RedrawControl();
                }

                if (control.Id.Contains("PlayerToggleEnergy"))
                {
                    var toggle = control as IMyTerminalControlCheckbox;
                    if (toggle == null) continue;
                    toggle.Title = MyStringId.GetOrCompute($"Add {Math.Round(ActionControls.GetProductionEnergy(block), 0)}% Energy Efficiency:\n{(ActionControls.GetEnergyTokens(block))} Token(s)/{ActionControls.GetTimeToConsumeToken(block) / 60} minute(s)");
                    control.RedrawControl();
                }

                if (control.Id.Contains("Label"))
                {
                    var label = control as IMyTerminalControlLabel;
                    if (label == null) continue;

                    string info = label.Label.ToString();

                    if (info.Contains("Consume"))
                    {
                        label.Label = MyStringId.GetOrCompute($"Consume {ActionControls.GetDelayCost(block)} tokens to extend\nsiege time by {ActionControls.GetSiegeDelayTime(block)} hours");
                        label.UpdateVisual();
                        control.RedrawControl();
                        continue;
                    }

                    if (info.Contains("Territory Center On:"))
                    {
                        label.Label = MyStringId.GetOrCompute($"Territory Center On: {ActionControls.GetPlanetName(block)}");
                        label.UpdateVisual();
                        control.RedrawControl();
                        continue;
                    }
                }
            }
        }


        private static void CreateClaimControls(IMyTerminalBlock block)
        {
            // Admin Controls
            var adminControls = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("AdminControlsLabel");
            adminControls.Enabled = Block => true;
            adminControls.SupportsMultipleBlocks = false;
            adminControls.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            adminControls.Label = MyStringId.GetOrCompute("=== Admin Only Visible Controls ===");

            AdminControls.Add(adminControls);

            // Enable Switch
            var enableSwitch = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyBeacon>("EnableSwitch");
            enableSwitch.Enabled = Block => ActionControls.IsAdminValidItem(Block);
            enableSwitch.SupportsMultipleBlocks = false;
            enableSwitch.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            enableSwitch.Title = MyStringId.GetOrCompute("Enable Claim Switch");
            enableSwitch.OnText = MyStringId.GetOrCompute("On");
            enableSwitch.OffText = MyStringId.GetOrCompute("Off");
            enableSwitch.Getter = ActionControls.GetSwitchState;
            enableSwitch.Setter = ActionControls.SetSwitchState;

            AdminControls.Add(enableSwitch);

            // Reset Claim Button
            var reset = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("Reset");
            reset.Enabled = Block => ActionControls.IsAdmin(Block);
            reset.SupportsMultipleBlocks = false;
            reset.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            reset.Title = MyStringId.GetOrCompute("Reset Territory");
            reset.Tooltip = MyStringId.GetOrCompute("Resets the block data to unclaimed");
            reset.Action = Block => ActionControls.ResetTerritory(Block);

            AdminControls.Add(reset);

            // Territory Name
            var territoryName = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("TerritoryName");
            territoryName.Enabled = Block => ActionControls.IsAdmin(Block);
            territoryName.SupportsMultipleBlocks = false;
            territoryName.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            territoryName.Title = MyStringId.GetOrCompute("Territory Name");
            territoryName.Getter = Block => ActionControls.GetTerritoryName(Block);
            territoryName.Setter = (Block, Builder) => ActionControls.SetTerritoryName(Block, Builder);

            AdminControls.Add(territoryName);

			// Global Discord Id
			var globalDiscordId = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("GlobalDiscordId");
			globalDiscordId.Enabled = Block => ActionControls.IsAdmin(Block);
			globalDiscordId.SupportsMultipleBlocks = false;
			globalDiscordId.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
			globalDiscordId.Title = MyStringId.GetOrCompute("Discord Global Chat Channel Id");
			globalDiscordId.Getter = Block => ActionControls.GetGlobalDiscordId(Block);
			globalDiscordId.Setter = (Block, Builder) => ActionControls.SetGlobalDiscordId(Block, Builder);

			AdminControls.Add(globalDiscordId);

			// Consumption Item
			var consumptionItem = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("ConsumptionItem");
            consumptionItem.Enabled = Block => ActionControls.IsAdminAndEnabled(Block);
            consumptionItem.SupportsMultipleBlocks = false;
            consumptionItem.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            consumptionItem.Title = MyStringId.GetOrCompute("Consumption Item");
            consumptionItem.Getter = Block => ActionControls.GetConsumptionItem(Block);
            consumptionItem.Setter = (Block, Builder) => ActionControls.SetConsumptionItem(Block, Builder);

            AdminControls.Add(consumptionItem);

            // Consumption Item Valid Check
            var itemValid = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("ValidItem");
            itemValid.Enabled = Block => true;
            itemValid.SupportsMultipleBlocks = false;
            itemValid.Visible = Block => ActionControls.IsConsumptionItemValid(Block);
            itemValid.Label = MyStringId.GetOrCompute("* That Item is NOT Valid *");

            AdminControls.Add(itemValid);

            // Territory Maintain Cost
            var maintainCost = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("MaintainCost");
            maintainCost.Enabled = Block => ActionControls.IsAdminAndEnabled(Block);
            maintainCost.SupportsMultipleBlocks = false;
            maintainCost.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            maintainCost.Title = MyStringId.GetOrCompute("Territory Maintain Cost");
            maintainCost.Tooltip = MyStringId.GetOrCompute("Admin only slider, sets the cost to maintain a territory");
            maintainCost.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(10000d, 0));
            maintainCost.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetMaintainCost(Block), 0)} units");

            };
            maintainCost.Getter = Block => ActionControls.GetMaintainCost(Block);
            maintainCost.Setter = (Block, Value) => ActionControls.SetMaintainCost(Block, Value);

            AdminControls.Add(maintainCost);

            // NpcFactionAssignment
            /*var npc = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("NPC");
            npc.Enabled = Block => ActionControls.IsAdminAndEnabled(Block);
            npc.SupportsMultipleBlocks = false;
            npc.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            npc.Title = MyStringId.GetOrCompute("Claim Block Faction Assignment (Tag)");
            npc.Getter = Block => ActionControls.GetNPC(Block);
            npc.Setter = (Block, Builder) => ActionControls.SetNPC(Block, Builder);

            AdminControls.Add(npc);*/

            // SafeZone Radius Slider
            var safezone = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("SafeZoneRadius");
            safezone.Enabled = Block => ActionControls.IsAdminAndEnabled(Block);
            safezone.SupportsMultipleBlocks = false;
            safezone.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            safezone.Title = MyStringId.GetOrCompute("Safe Zone Radius");
            safezone.Tooltip = MyStringId.GetOrCompute("Admin only slider, sets the radius of the safe zone when territory is claimed");
            safezone.SetLimits((float)Math.Round(10d, 0), (float)Math.Round(5000d, 0));
            safezone.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetSafeZoneSlider(Block), 0)} m");

            };
            safezone.Getter = Block => ActionControls.GetSafeZoneSlider(Block);
            safezone.Setter = (Block, Value) => ActionControls.SetSafeZoneSlider(Block, Value);

            AdminControls.Add(safezone);

            // Claim Radius Slider
            var claimArea = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ClaimAreaRadius");
            claimArea.Enabled = Block => ActionControls.IsAdminAndEnabled(Block);
            claimArea.SupportsMultipleBlocks = false;
            claimArea.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            claimArea.Title = MyStringId.GetOrCompute("Territory Area Radius");
            claimArea.Tooltip = MyStringId.GetOrCompute("Admin only slider, adjusts the territory area radius");
            claimArea.SetLimits((float)Math.Round(10d, 0), (float)Math.Round(500000d, 0));
            claimArea.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetClaimAreaSlider(Block), 0)} m");

            };
            claimArea.Getter = Block => ActionControls.GetClaimAreaSlider(Block);
            claimArea.Setter = (Block, Value) => ActionControls.SetClaimAreaSlider(Block, Value);

            AdminControls.Add(claimArea);

            /*// Claim Radius Confirmation Button
            var claimSet = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("ClaimSet");
            claimSet.Enabled = Block => ActionControls.IsAdmin(Block);
            claimSet.SupportsMultipleBlocks = false;
            claimSet.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            claimSet.Title = MyStringId.GetOrCompute("Set Claim Radius");
            claimSet.Tooltip = MyStringId.GetOrCompute("Sets the claim area radius.");
            claimSet.Action = Block => ActionControls.SetAreaTrigger(Block);

            AdminControls.Add(claimSet);*/

            // Combo UI Controls
            var combo = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyBeacon>("ComboBox");
            combo.Enabled = Block => ActionControls.IsAdmin(Block);
            combo.SupportsMultipleBlocks = false;
            combo.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            combo.Title = MyStringId.GetOrCompute("Select Control UI");
            combo.ComboBoxContent = ActionControls.GetControlsContent;
            combo.Getter = Block => ActionControls.GetSelectedControl(Block);
            combo.Setter = ActionControls.SetSelectedControl;

            AdminControls.Add(combo);

            // =================== TERRITORY OPTIONS CONTROLS ========================

            // Territory Options Label
            var terriOptionsLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("TerritoryOptionLabel");
            terriOptionsLabel.Enabled = Block => true;
            terriOptionsLabel.SupportsMultipleBlocks = false;
            terriOptionsLabel.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            terriOptionsLabel.Label = MyStringId.GetOrCompute("--- Territory Option Configurations ---");

            AdminControls.Add(terriOptionsLabel);

            // Sep G
            var sepG = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SepG");
            sepG.Enabled = Block => true;
            sepG.SupportsMultipleBlocks = false;
            sepG.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);

            AdminControls.Add(sepG);

            // Center Territory To Planet
            var centerToggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("CenterToggle");
            centerToggle.Enabled = Block => ActionControls.IsAdminAndEnabled(Block);
            centerToggle.SupportsMultipleBlocks = false;
            centerToggle.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            centerToggle.Title = MyStringId.GetOrCompute("Center Territory To\nNearest Planet/Moon");
            centerToggle.Getter = Block => ActionControls.GetCenterToggle(Block);
            centerToggle.Setter = (Block, Builder) => ActionControls.SetCenterToggle(Block, Builder);

            AdminControls.Add(centerToggle);

            // Planet Label
            var planetLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("PlanetLabel");
            planetLabel.Enabled = Block => true;
            planetLabel.SupportsMultipleBlocks = false;
            planetLabel.Visible = Block => ActionControls.IsCenterToPlanetEnabled(Block);
            planetLabel.Label = MyStringId.GetOrCompute($"Territory Center On: {ActionControls.GetPlanetName(block)}");

            AdminControls.Add(planetLabel);

            // Allow Allies SafeZone Choice
            //var adminAllowAlliesSZ = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("AdminAllowAlliesSafeZone");
            //adminAllowAlliesSZ.Enabled = Block => ActionControls.IsAdmin(Block);
            //adminAllowAlliesSZ.SupportsMultipleBlocks = false;
            //adminAllowAlliesSZ.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            //adminAllowAlliesSZ.Title = MyStringId.GetOrCompute("Allow Claimers Safezone Allies");
            //adminAllowAlliesSZ.Getter = Block => ActionControls.GetAdminSafeZoneAllies(Block);
            //adminAllowAlliesSZ.Setter = (Block, Builder) => ActionControls.SetAdminSafeZoneAllies(Block, Builder);
            //
            //AdminControls.Add(adminAllowAlliesSZ);

            // Allow Allies In Territory Choice
            //var adminAllowAlliesTerritory = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("AdminAllowAlliesTerritory");
            //adminAllowAlliesTerritory.Enabled = Block => !ActionControls.IsAdminAllowAlliesEnabledSZ(Block);
            //adminAllowAlliesTerritory.SupportsMultipleBlocks = false;
            //adminAllowAlliesTerritory.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            //adminAllowAlliesTerritory.Title = MyStringId.GetOrCompute("Allow Claimers Territory Allies");
            //adminAllowAlliesTerritory.Getter = Block => ActionControls.GetAdminTerritoryAllies(Block);
            //adminAllowAlliesTerritory.Setter = (Block, Builder) => ActionControls.SetAdminTerritoryAllies(Block, Builder);
            //
            //AdminControls.Add(adminAllowAlliesTerritory);

            // Allow All Enemy Tools
            var allowTools = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("AdminAllowTools");
            allowTools.Enabled = Block => ActionControls.IsAdmin(Block);
            allowTools.SupportsMultipleBlocks = false;
            allowTools.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            allowTools.Title = MyStringId.GetOrCompute("Allow All Tools");
            allowTools.Getter = Block => ActionControls.GetAllowTools(Block);
            allowTools.Setter = (Block, Builder) => ActionControls.SetAllowTools(Block, Builder);

            AdminControls.Add(allowTools);

            // Allow Drilling
            var allowDrilling = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("AdminDrilling");
            allowDrilling.Enabled = Block => ActionControls.IsAllowToolsEnabled(Block);
            allowDrilling.SupportsMultipleBlocks = false;
            allowDrilling.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            allowDrilling.Title = MyStringId.GetOrCompute("Allow Drilling");
            allowDrilling.Getter = Block => ActionControls.GetAllowDrilling(Block);
            allowDrilling.Setter = (Block, Builder) => ActionControls.SetAllowDrilling(Block, Builder);

            AdminControls.Add(allowDrilling);

            // Allow Welding
            var allowWelding = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("AdminWelding");
            allowWelding.Enabled = Block => ActionControls.IsAllowToolsEnabled(Block);
            allowWelding.SupportsMultipleBlocks = false;
            allowWelding.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            allowWelding.Title = MyStringId.GetOrCompute("Allow Welding");
            allowWelding.Getter = Block => ActionControls.GetAllowWelding(Block);
            allowWelding.Setter = (Block, Builder) => ActionControls.SetAllowWelding(Block, Builder);

            AdminControls.Add(allowWelding);

            // Allow Grinding
            var allowGrinding = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("AdminGrinding");
            allowGrinding.Enabled = Block => ActionControls.IsAllowToolsEnabled(Block);
            allowGrinding.SupportsMultipleBlocks = false;
            allowGrinding.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            allowGrinding.Title = MyStringId.GetOrCompute("Allow Grinding");
            allowGrinding.Getter = Block => ActionControls.GetAllowGrinding(Block);
            allowGrinding.Setter = (Block, Builder) => ActionControls.SetAllowGrinding(Block, Builder);

            AdminControls.Add(allowGrinding);

            // ======================= CLAIMING CONTROLS =============================

            // Claiming Label
            var claimLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("ClaimLabel");
            claimLabel.Enabled = Block => true;
            claimLabel.SupportsMultipleBlocks = false;
            claimLabel.Visible = Block => ActionControls.IsClaimingControls(Block);
            claimLabel.Label = MyStringId.GetOrCompute("--- Claiming Control Configurations ---");

            AdminControls.Add(claimLabel);

            // Sep C
            var sepC = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SepC");
            sepC.Enabled = Block => true;
            sepC.SupportsMultipleBlocks = false;
            sepC.Visible = Block => ActionControls.IsClaimingControls(Block);

            AdminControls.Add(sepC);

            // ToClaim Slider
            var toClaimTime = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ToClaimTime");
            toClaimTime.Enabled = Block => ActionControls.IsAdmin(Block);
            toClaimTime.SupportsMultipleBlocks = false;
            toClaimTime.Visible = Block => ActionControls.IsClaimingControls(Block);
            toClaimTime.Title = MyStringId.GetOrCompute("Time To Claim");
            toClaimTime.Tooltip = MyStringId.GetOrCompute("Sets the time it takes to claim a territory");
            toClaimTime.SetLimits((float)Math.Round(1d, 0), (float)Math.Round(3600d, 0));
            toClaimTime.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetToClaimTime(Block), 0)} second(s)");

            };
            toClaimTime.Getter = Block => ActionControls.GetToClaimTime(Block);
            toClaimTime.Setter = (Block, Value) => ActionControls.SetToClaimTime(Block, Value);

            AdminControls.Add(toClaimTime);

            // Tokens ToClaim Slider
            var tokensToClaim = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("TokensToClaim");
            tokensToClaim.Enabled = Block => ActionControls.IsAdmin(Block);
            tokensToClaim.SupportsMultipleBlocks = false;
            tokensToClaim.Visible = Block => ActionControls.IsClaimingControls(Block);
            tokensToClaim.Title = MyStringId.GetOrCompute("Tokens To Claim");
            tokensToClaim.Tooltip = MyStringId.GetOrCompute("Sets the amount of tokens required to claim territory");
            tokensToClaim.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(10000d, 0));
            tokensToClaim.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetTokensToClaim(Block), 0)} token(s)");

            };
            tokensToClaim.Getter = Block => ActionControls.GetTokensToClaim(Block);
            tokensToClaim.Setter = (Block, Value) => ActionControls.SetTokensToClaim(Block, Value);

            AdminControls.Add(tokensToClaim);

            // Consume Token Timer Slider
            var consumeTokenTimer = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ConsumeTokenTimer");
            consumeTokenTimer.Enabled = Block => ActionControls.IsAdmin(Block);
            consumeTokenTimer.SupportsMultipleBlocks = false;
            consumeTokenTimer.Visible = Block => ActionControls.IsClaimingControls(Block);
            consumeTokenTimer.Title = MyStringId.GetOrCompute("Time To Consume\n    Token");
            consumeTokenTimer.Tooltip = MyStringId.GetOrCompute("Sets the amount of time before a token is consumed");
            consumeTokenTimer.SetLimits((float)Math.Round(1d, 0), (float)Math.Round(3600d, 0));
            consumeTokenTimer.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetTimeToConsumeToken(Block), 0)} second(s)");

            };
            consumeTokenTimer.Getter = Block => ActionControls.GetTimeToConsumeToken(Block);
            consumeTokenTimer.Setter = (Block, Value) => ActionControls.SetTimeToConsumeToken(Block, Value);

            AdminControls.Add(consumeTokenTimer);

            // Update Token Timer
            var updateTokenTimer = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("UpdateTokenTimer");
            updateTokenTimer.Enabled = Block => ActionControls.IsAdmin(Block);
            updateTokenTimer.SupportsMultipleBlocks = false;
            updateTokenTimer.Visible = Block => ActionControls.IsClaimingControls(Block);
            updateTokenTimer.Title = MyStringId.GetOrCompute("Reset Token Timer");
            updateTokenTimer.Tooltip = MyStringId.GetOrCompute("Resets elapsed time to the defined amount");
            updateTokenTimer.Action = Block => ActionControls.SetTokenTimer(Block);

            AdminControls.Add(updateTokenTimer);

            // ToClaim Distance Slider
            var claimDistance = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ClaimDistance");
            claimDistance.Enabled = Block => ActionControls.IsAdmin(Block);
            claimDistance.SupportsMultipleBlocks = false;
            claimDistance.Visible = Block => ActionControls.IsClaimingControls(Block);
            claimDistance.Title = MyStringId.GetOrCompute("To Claim Distance");
            claimDistance.Tooltip = MyStringId.GetOrCompute("Sets the distance required to claim in meters");
            claimDistance.SetLimits((float)Math.Round(10d, 0), (float)Math.Round(5000d, 0));
            claimDistance.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetDistanceToClaim(Block), 0)} m");

            };
            claimDistance.Getter = Block => ActionControls.GetDistanceToClaim(Block);
            claimDistance.Setter = (Block, Value) => ActionControls.SetDistanceToClaim(Block, Value);

            AdminControls.Add(claimDistance);

            // GPS Update Slider
            var gpsUpdate = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("GpsUpdate");
            gpsUpdate.Enabled = Block => ActionControls.IsAdmin(Block);
            gpsUpdate.SupportsMultipleBlocks = false;
            gpsUpdate.Visible = Block => ActionControls.IsClaimingControls(Block);
            gpsUpdate.Title = MyStringId.GetOrCompute("Gps Update");
            gpsUpdate.Tooltip = MyStringId.GetOrCompute("Sets how often to update enemey gps markers (in seconds)");
            gpsUpdate.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(60d, 0));
            gpsUpdate.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetGpsUpdate(Block), 0)} seconds(s)");

            };
            gpsUpdate.Getter = Block => ActionControls.GetGpsUpdate(Block);
            gpsUpdate.Setter = (Block, Value) => ActionControls.SetGpsUpdate(Block, Value);

            AdminControls.Add(gpsUpdate);

            // ====================== SIEGING CONTROLS ===============================

            // Sieging Label
            var siegeLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("SiegeLabel");
            siegeLabel.Enabled = Block => true;
            siegeLabel.SupportsMultipleBlocks = false;
            siegeLabel.Visible = Block => ActionControls.IsSiegeControls(Block);
            siegeLabel.Label = MyStringId.GetOrCompute("--- Sieging Control Configurations ---");

            AdminControls.Add(siegeLabel);

            // Sep D
            var sepD = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SepD");
            sepD.Enabled = Block => true;
            sepD.SupportsMultipleBlocks = false;
            sepD.Visible = Block => ActionControls.IsSiegeControls(Block);

            AdminControls.Add(sepD);

            // ToSiege Time Slider
            var toSiegeTime = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ToSiegeTime");
            toSiegeTime.Enabled = Block => ActionControls.IsAdmin(Block);
            toSiegeTime.SupportsMultipleBlocks = false;
            toSiegeTime.Visible = Block => ActionControls.IsSiegeControls(Block);
            toSiegeTime.Title = MyStringId.GetOrCompute("Time To Init Siege");
            toSiegeTime.Tooltip = MyStringId.GetOrCompute("Sets the time it takes to init a siege in seconds");
            toSiegeTime.SetLimits((float)Math.Round(1d, 0), (float)Math.Round(3600d, 0));
            toSiegeTime.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetToSiegeTime(Block), 0)} second(s)");

            };
            toSiegeTime.Getter = Block => ActionControls.GetToSiegeTime(Block);
            toSiegeTime.Setter = (Block, Value) => ActionControls.SetToSiegeTime(Block, Value);

            AdminControls.Add(toSiegeTime);

            // Tokens ToSiege Slider
            var tokensToSiege = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("TokensToSiege");
            tokensToSiege.Enabled = Block => ActionControls.IsAdmin(Block);
            tokensToSiege.SupportsMultipleBlocks = false;
            tokensToSiege.Visible = Block => ActionControls.IsSiegeControls(Block);
            tokensToSiege.Title = MyStringId.GetOrCompute("Tokens To Init Siege");
            tokensToSiege.Tooltip = MyStringId.GetOrCompute("Sets the amount of tokens required to init a siege");
            tokensToSiege.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(10000d, 0));
            tokensToSiege.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetTokensToSiege(Block), 0)} token(s)");

            };
            tokensToSiege.Getter = Block => ActionControls.GetTokensToSiege(Block);
            tokensToSiege.Setter = (Block, Value) => ActionControls.SetTokensToSiege(Block, Value);

            AdminControls.Add(tokensToSiege);

            // Final Siege Slider
            var finalSiege = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("FinalSiege");
            finalSiege.Enabled = Block => ActionControls.IsAdmin(Block);
            finalSiege.SupportsMultipleBlocks = false;
            finalSiege.Visible = Block => ActionControls.IsSiegeControls(Block);
            finalSiege.Title = MyStringId.GetOrCompute("Time To Final Siege");
            finalSiege.Tooltip = MyStringId.GetOrCompute("Sets the time it takes to final siege in seconds");
            finalSiege.SetLimits((float)Math.Round(1d, 0), (float)Math.Round(3600d, 0));
            finalSiege.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetToSiegeTimeFinal(Block), 0)} second(s)");

            };
            finalSiege.Getter = Block => ActionControls.GetToSiegeTimeFinal(Block);
            finalSiege.Setter = (Block, Value) => ActionControls.SetToSiegeTimeFinal(Block, Value);

            AdminControls.Add(finalSiege);

            // Final Siege Tokens
            var finalSiegeTokens = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("FinalSiegeTokens");
            finalSiegeTokens.Enabled = Block => ActionControls.IsAdmin(Block);
            finalSiegeTokens.SupportsMultipleBlocks = false;
            finalSiegeTokens.Visible = Block => ActionControls.IsSiegeControls(Block);
            finalSiegeTokens.Title = MyStringId.GetOrCompute("Tokens To Final Siege");
            finalSiegeTokens.Tooltip = MyStringId.GetOrCompute("Sets the amount of tokens required to final siege");
            finalSiegeTokens.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(10000d, 0));
            finalSiegeTokens.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetTokensToSiegeFinal(Block), 0)} token(s)");

            };
            finalSiegeTokens.Getter = Block => ActionControls.GetTokensToSiegeFinal(Block);
            finalSiegeTokens.Setter = (Block, Value) => ActionControls.SetTokensToSiegeFinal(Block, Value);

            AdminControls.Add(finalSiegeTokens);

            // ToSiege Distance Slider
            var siegeDistance = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("SiegeDistance");
            siegeDistance.Enabled = Block => ActionControls.IsAdmin(Block);
            siegeDistance.SupportsMultipleBlocks = false;
            siegeDistance.Visible = Block => ActionControls.IsSiegeControls(Block);
            siegeDistance.Title = MyStringId.GetOrCompute("To Siege Distance");
            siegeDistance.Tooltip = MyStringId.GetOrCompute("Sets the distance required to siege in meters");
            siegeDistance.SetLimits((float)Math.Round(10d, 0), (float)Math.Round(15000d, 0));
            siegeDistance.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetDistanceToSiege(Block), 0)} m");

            };
            siegeDistance.Getter = Block => ActionControls.GetDistanceToSiege(Block);
            siegeDistance.Setter = (Block, Value) => ActionControls.SetDistanceToSiege(Block, Value);

            AdminControls.Add(siegeDistance);

            // NotifactionFreq
            var notifactionFreq = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("NotificationFreq");
            notifactionFreq.Enabled = Block => ActionControls.IsAdmin(Block);
            notifactionFreq.SupportsMultipleBlocks = false;
            notifactionFreq.Visible = Block => ActionControls.IsSiegeControls(Block);
            notifactionFreq.Title = MyStringId.GetOrCompute("Siege Warning\nNotification Frequency");
            notifactionFreq.Tooltip = MyStringId.GetOrCompute("Sets how often chat/discord warns everyone that the territory is being sieged (in minutes)");
            notifactionFreq.SetLimits((float)Math.Round(5d, 0), (float)Math.Round(30d, 0));
            notifactionFreq.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetSiegeNotificationFreq(Block), 0)} minute(s)");

            };
            notifactionFreq.Getter = Block => ActionControls.GetSiegeNotificationFreq(Block);
            notifactionFreq.Setter = (Block, Value) => ActionControls.SetSiegeNotificationFreq(Block, Value);

            AdminControls.Add(notifactionFreq);

            // Territory Deactivation Time Slider
            var deactivationTime = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("DeactivationTime");
            deactivationTime.Enabled = Block => ActionControls.IsAdmin(Block);
            deactivationTime.SupportsMultipleBlocks = false;
            deactivationTime.Visible = Block => ActionControls.IsSiegeControls(Block);
            deactivationTime.Title = MyStringId.GetOrCompute("Siege Countdown\nTimer");
            deactivationTime.Tooltip = MyStringId.GetOrCompute("Sets the time it takes after a successful siege before final siege can start (in minutes)");
            deactivationTime.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(1440d, 0));
            deactivationTime.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetDeactivationTime(Block), 0)} minute(s)");

            };
            deactivationTime.Getter = Block => ActionControls.GetDeactivationTime(Block);
            deactivationTime.Setter = (Block, Value) => ActionControls.SetDeactivationTime(Block, Value);

            AdminControls.Add(deactivationTime);

            // TimeFrame to Final Siege
            var siegeGap = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("SiegeGap");
            siegeGap.Enabled = Block => ActionControls.IsAdmin(Block);
            siegeGap.SupportsMultipleBlocks = false;
            siegeGap.Visible = Block => ActionControls.IsSiegeControls(Block);
            siegeGap.Title = MyStringId.GetOrCompute("Siege Gap");
            siegeGap.Tooltip = MyStringId.GetOrCompute("Timeframe given to start final siege (minutes)");
            siegeGap.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(60d, 0));
            siegeGap.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetSiegeGapTime(Block), 0)} minute(s)");

            };
            siegeGap.Getter = Block => ActionControls.GetSiegeGapTime(Block);
            siegeGap.Setter = (Block, Value) => ActionControls.SetSiegeGapTime(Block, Value);

            AdminControls.Add(siegeGap);

            // Siege Failure Cooldown
            var siegeCooldown = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("SiegeCooldown");
            siegeCooldown.Enabled = Block => ActionControls.IsAdmin(Block);
            siegeCooldown.SupportsMultipleBlocks = false;
            siegeCooldown.Visible = Block => ActionControls.IsSiegeControls(Block);
            siegeCooldown.Title = MyStringId.GetOrCompute("Siege Fail Cooldown");
            siegeCooldown.Tooltip = MyStringId.GetOrCompute("Sets the time to cooldown after a failed final siege before it can be sieged again (minutes)");
            siegeCooldown.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(7200d, 0));
            siegeCooldown.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetSiegeCooldownTime(Block), 0)} minute(s)");

            };
            siegeCooldown.Getter = Block => ActionControls.GetSiegeCooldownTime(Block);
            siegeCooldown.Setter = (Block, Value) => ActionControls.SetSiegeCooldownTime(Block, Value);

            AdminControls.Add(siegeCooldown);

            // Cooldown
            var cooldown = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("Cooldown");
            cooldown.Enabled = Block => ActionControls.IsAdmin(Block);
            cooldown.SupportsMultipleBlocks = false;
            cooldown.Visible = Block => ActionControls.IsSiegeControls(Block);
            cooldown.Title = MyStringId.GetOrCompute("Cooldown Timer");
            cooldown.Tooltip = MyStringId.GetOrCompute("Sets the time to cooldown after a successful final siege before territory can be claimed again (minutes)");
            cooldown.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(1440d, 0));
            cooldown.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetCooldownTime(Block), 0)} minute(s)");

            };
            cooldown.Getter = Block => ActionControls.GetCooldownTime(Block);
            cooldown.Setter = (Block, Value) => ActionControls.SetCooldownTime(Block, Value);

            AdminControls.Add(cooldown);

            // Tokens To Delay Siege
            var tokenDelay = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("TokenDelay");
            tokenDelay.Enabled = Block => ActionControls.IsAdmin(Block);
            tokenDelay.SupportsMultipleBlocks = false;
            tokenDelay.Visible = Block => ActionControls.IsSiegeControls(Block);
            tokenDelay.Title = MyStringId.GetOrCompute("Tokens To Siege Delay");
            tokenDelay.Tooltip = MyStringId.GetOrCompute("Sets the cost of tokens to delay siege time");
            tokenDelay.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(10000d, 0));
            tokenDelay.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetTokenSiegeDelay(Block), 0)} token(s)");

            };
            tokenDelay.Getter = Block => ActionControls.GetTokenSiegeDelay(Block);
            tokenDelay.Setter = (Block, Value) => ActionControls.SetTokenSiegeDelay(Block, Value);

            AdminControls.Add(tokenDelay);

            // # Times Siege Can Be Delayed
            var siegeDelayedCount = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("SiegeDelayCount");
            siegeDelayedCount.Enabled = Block => ActionControls.IsAdmin(Block);
            siegeDelayedCount.SupportsMultipleBlocks = false;
            siegeDelayedCount.Visible = Block => ActionControls.IsSiegeControls(Block);
            siegeDelayedCount.Title = MyStringId.GetOrCompute("Siege Delay Count");
            siegeDelayedCount.Tooltip = MyStringId.GetOrCompute("Sets how many times a siege can be delayed");
            siegeDelayedCount.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(10d, 0));
            siegeDelayedCount.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetSiegeDelayCount(Block), 0)}");

            };
            siegeDelayedCount.Getter = Block => ActionControls.GetSiegeDelayCount(Block);
            siegeDelayedCount.Setter = (Block, Value) => ActionControls.SetSiegeDelayCount(Block, Value);

            AdminControls.Add(siegeDelayedCount);

            // Siege Delay Time
            var siegeDelayedTime = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("SiegeDelayTime");
            siegeDelayedTime.Enabled = Block => ActionControls.IsAdmin(Block);
            siegeDelayedTime.SupportsMultipleBlocks = false;
            siegeDelayedTime.Visible = Block => ActionControls.IsSiegeControls(Block);
            siegeDelayedTime.Title = MyStringId.GetOrCompute("Siege Delay Time");
            siegeDelayedTime.Tooltip = MyStringId.GetOrCompute("Sets how long (in hours) to delay a siege");
            siegeDelayedTime.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(24d, 0));
            siegeDelayedTime.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{ActionControls.GetSiegeDelayTime(Block)} hour(s)");

            };
            siegeDelayedTime.Getter = Block => ActionControls.GetSiegeDelayTime(Block);
            siegeDelayedTime.Setter = (Block, Value) => ActionControls.SetSiegeDelayTime(Block, Value);

            AdminControls.Add(siegeDelayedTime);

            // ===================== MISC CONTROLS ==============================

            // Misc Label
            var miscLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("MiscLabel");
            miscLabel.Enabled = Block => true;
            miscLabel.SupportsMultipleBlocks = false;
            miscLabel.Visible = Block => ActionControls.IsMiscControls(Block);
            miscLabel.Label = MyStringId.GetOrCompute("--- Misc ---");

            AdminControls.Add(miscLabel);

            // Faction List
            var factionList = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyBeacon>("FactionList");
            factionList.Enabled = Block => ActionControls.IsAdmin(Block);
            factionList.SupportsMultipleBlocks = false;
            factionList.Visible = Block => ActionControls.IsMiscControls(Block);
            factionList.Title = MyStringId.GetOrCompute("Select Faction To Assign Territory");
            factionList.ListContent = ActionControls.GetFactionList;
            factionList.VisibleRowsCount = 10;
            factionList.ItemSelected = ActionControls.SetSelectedFaction;
            factionList.Multiselect = false;

            AdminControls.Add(factionList);

            // Set Territory To Faction
            var setTerritory = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("SetTerritory");
            setTerritory.Enabled = Block => ActionControls.IsFactionSelected(Block);
            setTerritory.SupportsMultipleBlocks = false;
            setTerritory.Visible = Block => ActionControls.IsMiscControls(Block);
            setTerritory.Title = MyStringId.GetOrCompute("Set Faction To Territory");
            setTerritory.Tooltip = MyStringId.GetOrCompute("Manually sets a faction to a territory");
            setTerritory.Action = Block => ActionControls.SetManualTerritory(Block);

            AdminControls.Add(setTerritory);

            // ===================== PERKS CONTROLS ============================

            // Perks Label
            var perksLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("PerksLabel");
            perksLabel.Enabled = Block => true;
            perksLabel.SupportsMultipleBlocks = false;
            perksLabel.Visible = Block => ActionControls.IsPerkControls(Block);
            perksLabel.Label = MyStringId.GetOrCompute("--- Perks ---");

            AdminControls.Add(perksLabel);

            // Sep E
            var sepE = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SepE");
            sepE.Enabled = Block => true;
            sepE.SupportsMultipleBlocks = false;
            sepE.Visible = Block => ActionControls.IsPerkControls(Block);

            AdminControls.Add(sepE);

            // Combo Perk Types
            var comboPerkType = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyBeacon>("ComboPerkType");
            comboPerkType.Enabled = Block => ActionControls.IsAdmin(Block);
            comboPerkType.SupportsMultipleBlocks = false;
            comboPerkType.Visible = Block => ActionControls.IsPerkControls(Block);
            comboPerkType.Title = MyStringId.GetOrCompute("Select Perk Type");
            comboPerkType.ComboBoxContent = ActionControls.GetPerkTypeContent;
            comboPerkType.Getter = Block => ActionControls.GetPerkType(Block);
            comboPerkType.Setter = ActionControls.SetPerkType;

            AdminControls.Add(comboPerkType);

            // Production Toggle
            var productionToggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("ProductionToggle");
            productionToggle.Enabled = Block => ActionControls.IsAdmin(Block);
            productionToggle.SupportsMultipleBlocks = false;
            productionToggle.Visible = Block => ActionControls.IsPerkType(Block, PerkTypeList.Production);
            productionToggle.Title = MyStringId.GetOrCompute("Enable Production Additive Perk");
            productionToggle.Getter = Block => ActionControls.GetProductionEnabled(Block);
            productionToggle.Setter = (Block, Builder) => ActionControls.SetProductionEnabled(Block, Builder);

            AdminControls.Add(productionToggle);

            // Production Speed
            var productionSpeed = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ProductionSpeed");
            productionSpeed.Enabled = Block => ActionControls.IsAdminProductionEnabled(Block);
            productionSpeed.SupportsMultipleBlocks = false;
            productionSpeed.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            productionSpeed.Title = MyStringId.GetOrCompute("Production Speed\nAdditive");
            productionSpeed.Tooltip = MyStringId.GetOrCompute("Adds additional production speed to refineries and assembliers (inside territory) to claimed faction");
            productionSpeed.SetLimits((float)Math.Round(0d, 2), (float)Math.Round(500d, 2));
            productionSpeed.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetProductionSpeed(Block), 0)} %");

            };
            productionSpeed.Getter = Block => ActionControls.GetProductionSpeed(Block);
            productionSpeed.Setter = (Block, Value) => ActionControls.SetProductionSpeed(Block, Value);

            AdminControls.Add(productionSpeed);

            // Production Yield
            var productionYield = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ProductionYield");
            productionYield.Enabled = Block => ActionControls.IsAdminProductionEnabled(Block);
            productionYield.SupportsMultipleBlocks = false;
            productionYield.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            productionYield.Title = MyStringId.GetOrCompute("Production Yield\nAdditive");
            productionYield.Tooltip = MyStringId.GetOrCompute("Adds additional production yield to refineries (inside territory) to claimed faction");
            productionYield.SetLimits((float)Math.Round(0d, 2), (float)Math.Round(500d, 2));
            productionYield.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetProductionYield(Block), 0)} %");

            };
            productionYield.Getter = Block => ActionControls.GetProductionYield(Block);
            productionYield.Setter = (Block, Value) => ActionControls.SetProductionYield(Block, Value);

            AdminControls.Add(productionYield);

            // Production Energy
            var productionEnergy = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ProductionEnergy");
            productionEnergy.Enabled = Block => ActionControls.IsAdminProductionEnabled(Block);
            productionEnergy.SupportsMultipleBlocks = false;
            productionEnergy.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            productionEnergy.Title = MyStringId.GetOrCompute("Production Energy Efficiency\nAdditive");
            productionEnergy.Tooltip = MyStringId.GetOrCompute("Adds additional production energy efficiency to refineries and assembliers (inside territory) to claimed faction");
            productionEnergy.SetLimits((float)Math.Round(0d, 2), (float)Math.Round(500d, 2));
            productionEnergy.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetProductionEnergy(Block), 0)} %");

            };
            productionEnergy.Getter = Block => ActionControls.GetProductionEnergy(Block);
            productionEnergy.Setter = (Block, Value) => ActionControls.SetProductionEnergy(Block, Value);

            AdminControls.Add(productionEnergy);

            // Apply Production Values
            var setProduction = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("SetProduction");
            setProduction.Enabled = Block => ActionControls.IsAdminProductionEnabled(Block);
            setProduction.SupportsMultipleBlocks = false;
            setProduction.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            setProduction.Title = MyStringId.GetOrCompute("Update Production Values");
            setProduction.Tooltip = MyStringId.GetOrCompute("Sets the values to all production blocks for claim faction (inside territory)");
            setProduction.Action = Block => ActionControls.SetProduction(Block);

            AdminControls.Add(setProduction);

            // Allow Standalone Production
            var standaloneToggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("StandAloneToggle");
            standaloneToggle.Enabled = Block => ActionControls.CheckPlayerControls(Block);
            standaloneToggle.SupportsMultipleBlocks = false;
            standaloneToggle.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            standaloneToggle.Title = MyStringId.GetOrCompute("Enable StandAlone");
            standaloneToggle.Tooltip = MyStringId.GetOrCompute("Enables the production perk to the values defined with no cost (no player interaction to AdminControls)");
            standaloneToggle.Getter = Block => ActionControls.GetStandAloneEnabled(Block);
            standaloneToggle.Setter = (Block, Builder) => ActionControls.SetStandAloneEnabled(Block, Builder);

            AdminControls.Add(standaloneToggle);

            // Allow Player Control Speed
            var playerSpeed = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerControlSpeed");
            playerSpeed.Enabled = Block => ActionControls.CheckStandAloneEnabled(Block);
            playerSpeed.SupportsMultipleBlocks = false;
            playerSpeed.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            playerSpeed.Title = MyStringId.GetOrCompute("Enable Player Speed Control");
            playerSpeed.Tooltip = MyStringId.GetOrCompute("Allows the player choose if they want to add the production speed perk");
            playerSpeed.Getter = Block => ActionControls.GetPlayerControlSpeed(Block);
            playerSpeed.Setter = (Block, Builder) => ActionControls.SetPlayerControlSpeed(Block, Builder);

            AdminControls.Add(playerSpeed);

            // Speed Control Tokens
            var speedTokens = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("SpeedTokens");
            speedTokens.Enabled = Block => ActionControls.CheckStandAloneEnabled(Block);
            speedTokens.SupportsMultipleBlocks = false;
            speedTokens.Visible = Block => ActionControls.GetPlayerControlSpeed(Block);
            speedTokens.Title = MyStringId.GetOrCompute("Speed Token Cost");
            speedTokens.Getter = Block => ActionControls.GetSpeedTokens(Block);
            speedTokens.Setter = (Block, Builder) => ActionControls.SetSpeedTokens(Block, Builder);

            AdminControls.Add(speedTokens);

            // Allow Player Control Yield
            var playerYield = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerControlYield");
            playerYield.Enabled = Block => ActionControls.CheckStandAloneEnabled(Block);
            playerYield.SupportsMultipleBlocks = false;
            playerYield.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            playerYield.Title = MyStringId.GetOrCompute("Enable Player Yield Control   ");
            playerYield.Tooltip = MyStringId.GetOrCompute("Allows the player choose if they want to add the production yield perk");
            playerYield.Getter = Block => ActionControls.GetPlayerControlYield(Block);
            playerYield.Setter = (Block, Builder) => ActionControls.SetPlayerControlYield(Block, Builder);

            AdminControls.Add(playerYield);

            // Yield Control Tokens
            var yieldTokens = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("YieldTokens");
            yieldTokens.Enabled = Block => ActionControls.CheckStandAloneEnabled(Block);
            yieldTokens.SupportsMultipleBlocks = false;
            yieldTokens.Visible = Block => ActionControls.GetPlayerControlYield(Block);
            yieldTokens.Title = MyStringId.GetOrCompute("Yield Token Cost");
            yieldTokens.Getter = Block => ActionControls.GetYieldTokens(Block);
            yieldTokens.Setter = (Block, Builder) => ActionControls.SetYieldTokens(Block, Builder);

            AdminControls.Add(yieldTokens);

            // Allow Player Control Energy
            var playerEnergy = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerControlEnergy");
            playerEnergy.Enabled = Block => ActionControls.CheckStandAloneEnabled(Block);
            playerEnergy.SupportsMultipleBlocks = false;
            playerEnergy.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            playerEnergy.Title = MyStringId.GetOrCompute("Enable Player Energy Control");
            playerEnergy.Tooltip = MyStringId.GetOrCompute("Allows the player choose if they want to add the production energy efficiency perk");
            playerEnergy.Getter = Block => ActionControls.GetPlayerControlEnergy(Block);
            playerEnergy.Setter = (Block, Builder) => ActionControls.SetPlayerControlEnergy(Block, Builder);

            AdminControls.Add(playerEnergy);

            // Energy Control Tokens
            var energyTokens = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("EnergyTokens");
            energyTokens.Enabled = Block => ActionControls.CheckStandAloneEnabled(Block);
            energyTokens.SupportsMultipleBlocks = false;
            energyTokens.Visible = Block => ActionControls.GetPlayerControlEnergy(Block);
            energyTokens.Title = MyStringId.GetOrCompute("Energy Token Cost");
            energyTokens.Getter = Block => ActionControls.GetEnergyTokens(Block);
            energyTokens.Setter = (Block, Builder) => ActionControls.SetEnergyTokens(Block, Builder);

            AdminControls.Add(energyTokens);

            // Sep A
            var sepA = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SepA");
            sepA.Enabled = Block => true;
            sepA.SupportsMultipleBlocks = false;
            sepA.Visible = Block => ActionControls.IsClaimAndAdmin(Block);

            AdminControls.Add(sepA);

            // Sep B
            var sepB = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SepB");
            sepB.Enabled = Block => true;
            sepB.SupportsMultipleBlocks = false;
            sepB.Visible = Block => ActionControls.IsClaimAndAdmin(Block);

            AdminControls.Add(sepB);

            // =========================== NON-ADMIN CONTROLS ==============================

            // Player Controls
            var playerControls = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("PlayerControlsLabel");
            playerControls.Enabled = Block => true;
            playerControls.SupportsMultipleBlocks = false;
            playerControls.Visible = Block => ActionControls.IsClaimBlock(Block);
            playerControls.Label = MyStringId.GetOrCompute("=== Player Controls ===");

            PlayerControls.Add(playerControls);

			// Discord Id
			var allianceSelect = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyBeacon>("DiscordId");
			allianceSelect.Enabled = Block => ActionControls.IsAdmin(Block);
			allianceSelect.SupportsMultipleBlocks = false;
			allianceSelect.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
			allianceSelect.Title = MyStringId.GetOrCompute("Select your alliance");
            allianceSelect.Tooltip = MyStringId.GetOrCompute("Determines which alliance will get notifications about this territory in their alliance discord channel.");
            allianceSelect.ComboBoxContent = l =>
            {
				ClaimBlockSettings settings;
				if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;
				for (int i = 0; i < Session.Instance.AllianceNames.Count; i++)
                {
                    MyTerminalControlComboBoxItem item = new MyTerminalControlComboBoxItem 
                    {
                        Key = i,
                        Value = MyStringId.GetOrCompute(Session.Instance.AllianceNames[i])
                    };
                    l.Add(item);
                }
            };
            allianceSelect.Getter = ActionControls.GetAllianceChannel;
            allianceSelect.Setter = ActionControls.SetAllianceChannel;

			PlayerControls.Add(allianceSelect);

			// Delay Siege Label
			var delaySiegeLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("DelaySiegeLabel");
            delaySiegeLabel.Enabled = Block => true;
            delaySiegeLabel.SupportsMultipleBlocks = false;
            delaySiegeLabel.Visible = Block => ActionControls.IsClaimBlock(Block);
            delaySiegeLabel.Label = MyStringId.GetOrCompute($"Consume {ActionControls.GetDelayCost(block)} tokens to extend\nsiege time by {ActionControls.GetSiegeDelayTime(block)} hours.\n*Enabled only when siege time\nhas more than\n1hr left.*");

            PlayerControls.Add(delaySiegeLabel);

            // Delay Siege Button
            var delaySiege = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("DelaySiege");
            delaySiege.Enabled = Block => ActionControls.CheckIsSieged(Block);
            delaySiege.SupportsMultipleBlocks = false;
            delaySiege.Visible = Block => ActionControls.IsClaimBlock(Block);
            delaySiege.Title = MyStringId.GetOrCompute("Delay Siege Time");
            delaySiege.Tooltip = MyStringId.GetOrCompute("Delays the siege time for the cost of tokens, only active if the siege timer is > 1hr");
            delaySiege.Action = Block => ActionControls.DelaySiege(Block);

            PlayerControls.Add(delaySiege);

            // Safezone faction whitelist
            var szAllowedFactionList = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyBeacon>("SZAllowedFactionList");
            szAllowedFactionList.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            szAllowedFactionList.SupportsMultipleBlocks = false;
            szAllowedFactionList.Visible = Block => ActionControls.IsClaimBlock(Block);
            szAllowedFactionList.Title = MyStringId.GetOrCompute("Safezone Faction Whitelist");
            szAllowedFactionList.Tooltip = MyStringId.GetOrCompute("Factions permitted inside the safe zone.");
            szAllowedFactionList.ListContent = ActionControls.FactionList;
            szAllowedFactionList.VisibleRowsCount = 10;
            szAllowedFactionList.ItemSelected = ActionControls.FactionSZSelectionChanged;
            szAllowedFactionList.Multiselect = false;

            PlayerControls.Add(szAllowedFactionList);

            // Alerted factions for radar
            var alertedFactionList = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyBeacon>("AlertedFactionList");
            alertedFactionList.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            alertedFactionList.SupportsMultipleBlocks = false;
            alertedFactionList.Visible = Block => ActionControls.IsClaimBlock(Block);
            alertedFactionList.Title = MyStringId.GetOrCompute("Send Territory Notifications To:");
            alertedFactionList.Tooltip = MyStringId.GetOrCompute("Factions that will recieve in-game notiifications for this territory.");
            alertedFactionList.ListContent = ActionControls.FactionRadarList;
            alertedFactionList.VisibleRowsCount = 10;
            alertedFactionList.ItemSelected = ActionControls.FactionRadarSelectionChanged;
            alertedFactionList.Multiselect = false;

            PlayerControls.Add(alertedFactionList);

            // Consider neutrals as enemies for radar
            var neutralsAreEnemies = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("neutralsAreEnemies");
            neutralsAreEnemies.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            neutralsAreEnemies.SupportsMultipleBlocks = false;
            neutralsAreEnemies.Visible = Block => ActionControls.IsClaimBlock(Block);
            neutralsAreEnemies.Title = MyStringId.GetOrCompute($"Neutrals show on radar");
            neutralsAreEnemies.Tooltip = MyStringId.GetOrCompute("Factions with neutral relations are detected and pinged by the territory radar.");
            neutralsAreEnemies.Getter = Block => ActionControls.GetNeutralEnemies(Block);
            neutralsAreEnemies.Setter = (Block, setting) => ActionControls.SetNeutralEnemies(Block, setting);

            PlayerControls.Add(neutralsAreEnemies);

            // Visible SZ bubble
            var visibleSzBubble = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("visibleSzBubble");
            visibleSzBubble.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            visibleSzBubble.SupportsMultipleBlocks = false;
            visibleSzBubble.Visible = Block => ActionControls.IsClaimBlock(Block);
            visibleSzBubble.Title = MyStringId.GetOrCompute($"Visible Safezone");
            visibleSzBubble.Tooltip = MyStringId.GetOrCompute("Enables customizing safezone texture and color.");
            visibleSzBubble.Getter = Block => ActionControls.GetVisibleSZ(Block);
            visibleSzBubble.Setter = (Block, Builder) => ActionControls.SetVisibleSZ(Block, Builder);

            PlayerControls.Add(visibleSzBubble);

            // Safezone texture selection
            IMyTerminalControlCombobox safezoneTextureCombobox = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyBeacon>("SafezoneTexture");
            safezoneTextureCombobox.Enabled = Block => ActionControls.IsClaimedAndFactionForSZ(Block);
            safezoneTextureCombobox.SupportsMultipleBlocks = false;
            safezoneTextureCombobox.Visible = Block => ActionControls.IsClaimBlock(Block);
            safezoneTextureCombobox.Title = MyStringId.GetOrCompute("Safezone Texture");
            safezoneTextureCombobox.ComboBoxContent = ActionControls.SafezoneTextures;
            safezoneTextureCombobox.Getter = Block => ActionControls.GetSafezoneTexture(Block);
            safezoneTextureCombobox.Setter = ActionControls.SetSafezoneTexture;

            PlayerControls.Add(safezoneTextureCombobox);

            // Safezone color selection
            IMyTerminalControlColor safezoneColor = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlColor, IMyBeacon>("SafezoneColor");
            safezoneColor.Enabled = Block => ActionControls.IsClaimedAndFactionForSZ(Block);
            safezoneColor.SupportsMultipleBlocks = false;
            safezoneColor.Visible = Block => ActionControls.IsClaimBlock(Block);
            safezoneColor.Title = MyStringId.GetOrCompute("Safezone Color");
            safezoneColor.Getter = Block => ActionControls.GetSafezoneColor(Block);
            safezoneColor.Setter = (Block, newColor) => ActionControls.SetSafezoneColor(Block, newColor);

            PlayerControls.Add(safezoneColor);

            // Player Allow SafeZone Allies
            //var playerAllowAlliesSZ = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerAllowAlliesSZ");
            //playerAllowAlliesSZ.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            //playerAllowAlliesSZ.SupportsMultipleBlocks = false;
            //playerAllowAlliesSZ.Visible = Block => ActionControls.IsAdminAllowAlliesEnabledSZ(Block);
            //playerAllowAlliesSZ.Title = MyStringId.GetOrCompute($"Allow SafeZone Allies");
            //playerAllowAlliesSZ.Tooltip = MyStringId.GetOrCompute("If enabled, will allow your allies inside the safe zone");
            //playerAllowAlliesSZ.Getter = Block => ActionControls.GetPlayerAllowAlliesSZ(Block);
            //playerAllowAlliesSZ.Setter = (Block, Builder) => ActionControls.SetPlayerAllowAlliesSZ(Block, Builder);
            //
            //PlayerControls.Add(playerAllowAlliesSZ);

            // Player Allow Territory Allies
            //var playerAllowAlliesTerritory = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerAllowAlliesTerritory");
            //playerAllowAlliesTerritory.Enabled = Block => ActionControls.IsPlayerSafezoneAllowed(Block);
            //playerAllowAlliesTerritory.SupportsMultipleBlocks = false;
            //playerAllowAlliesTerritory.Visible = Block => ActionControls.IsAdminAllowAlliesEnabledTerritory(Block);
            //playerAllowAlliesTerritory.Title = MyStringId.GetOrCompute($"Allow Territory Allies");
            //playerAllowAlliesTerritory.Tooltip = MyStringId.GetOrCompute("If enabled, will allow your allies inside the territory to use tools/not be alerted");
            //playerAllowAlliesTerritory.Getter = Block => ActionControls.GetPlayerAllowAlliesTerritory(Block);
            //playerAllowAlliesTerritory.Setter = (Block, Builder) => ActionControls.SetPlayerAllowAlliesTerritory(Block, Builder);
            //
            //PlayerControls.Add(playerAllowAlliesTerritory);

            // SafeZone Switch
            var szSwitch = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyBeacon>("SafeZoneSwitch");
            szSwitch.Enabled = Block => ActionControls.IsOffAndSieging(Block);
            szSwitch.SupportsMultipleBlocks = false;
            szSwitch.Visible = Block => ActionControls.IsClaimBlock(Block);
            szSwitch.Title = MyStringId.GetOrCompute("SafeZone Switch");
            szSwitch.Tooltip = MyStringId.GetOrCompute("If disabled, tokens will still be consumed!");
            szSwitch.OnText = MyStringId.GetOrCompute("On");
            szSwitch.OffText = MyStringId.GetOrCompute("Off");
            szSwitch.Getter = ActionControls.IsSafeZoneValid;
            szSwitch.Setter = ActionControls.SetSafeZoneState;

            PlayerControls.Add(szSwitch);


            // Claimed Territory Name
            /*var territoryName = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("TerritoryName");
            territoryName.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            territoryName.SupportsMultipleBlocks = false;
            territoryName.Visible = Block => ActionControls.IsClaimBlock(Block);
            territoryName.Title = MyStringId.GetOrCompute("Custom Territory Name");
            territoryName.Getter = Block => ActionControls.GetClaimedName(Block);
            territoryName.Setter = (Block, Builder) => ActionControls.SetClaimedName(Block, Builder);

            PlayerControls.Add(territoryName);*/

            // Set Custom Territory Name Button
            /*var setName = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("SetTerritoryName");
            setName.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            setName.SupportsMultipleBlocks = false;
            setName.Visible = Block => ActionControls.IsClaimBlock(Block);
            setName.Title = MyStringId.GetOrCompute("Set Custom Name");
            setName.Tooltip = MyStringId.GetOrCompute("Sets the custom territory name");
            setName.Action = Block => ActionControls.TriggerCustomName(Block);

            PlayerControls.Add(setName);*/



            // Combo Perk Types Player
            var comboPerkTypePlayer = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyBeacon>("ComboPerkTypePlayer");
            comboPerkTypePlayer.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            comboPerkTypePlayer.SupportsMultipleBlocks = false;
            comboPerkTypePlayer.Visible = Block => ActionControls.IsClaimBlock(Block);
            comboPerkTypePlayer.Title = MyStringId.GetOrCompute("Select Allowed Perk Type To Enable");
            comboPerkTypePlayer.Tooltip = MyStringId.GetOrCompute("Only available to the faction that claims this territory");
            comboPerkTypePlayer.ComboBoxContent = ActionControls.GetPerkTypeContentPlayer;
            comboPerkTypePlayer.Getter = Block => ActionControls.GetPerkTypePlayer(Block);
            comboPerkTypePlayer.Setter = ActionControls.SetPerkTypePlayer;

            PlayerControls.Add(comboPerkTypePlayer);

            // Player Controls Perks
            var playerControlsPerks = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("PlayerControlsPerksLabel");
            playerControlsPerks.Enabled = Block => true;
            playerControlsPerks.SupportsMultipleBlocks = false;
            playerControlsPerks.Visible = Block => ActionControls.IsPlayerPerkType(Block, PlayerPerks.Production);
            playerControlsPerks.Label = MyStringId.GetOrCompute("--- Player Controllable Perks ---");

            PlayerControls.Add(playerControlsPerks);

            // Sep F
            var sepF = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SepF");
            sepF.Enabled = Block => true;
            sepF.SupportsMultipleBlocks = false;
            sepF.Visible = Block => ActionControls.IsClaimBlock(Block);

            PlayerControls.Add(sepF);

            // Enable Player Speed Perk
            var playerToggleSpeed = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerToggleSpeed");
            playerToggleSpeed.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            playerToggleSpeed.SupportsMultipleBlocks = false;
            playerToggleSpeed.Visible = Block => ActionControls.IsPlayerProductionTypeAllowed(Block, "Speed");
            playerToggleSpeed.Title = MyStringId.GetOrCompute($"Add {Math.Round(ActionControls.GetProductionSpeed(block), 0)}% Production Speed:\n{(ActionControls.GetSpeedTokens(block))} Token(s)/{ActionControls.GetTimeToConsumeToken(block) / 60} minute(s)");
            playerToggleSpeed.Tooltip = MyStringId.GetOrCompute("If enabled, will add speed perk to all production inside the territory of the claimed faction and will add to token cost");
            playerToggleSpeed.Getter = Block => ActionControls.GetPlayerToggleSpeed(Block);
            playerToggleSpeed.Setter = (Block, Builder) => ActionControls.SetPlayerToggleSpeed(Block, Builder);

            PlayerControls.Add(playerToggleSpeed);

            // Enable Player Yield Perk
            var playerToggleYield = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerToggleYield");
            playerToggleYield.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            playerToggleYield.SupportsMultipleBlocks = false;
            playerToggleYield.Visible = Block => ActionControls.IsPlayerProductionTypeAllowed(Block, "Yield");
            playerToggleYield.Title = MyStringId.GetOrCompute($"Add {Math.Round(ActionControls.GetProductionYield(block), 0)}% Production Yield:\n{(ActionControls.GetYieldTokens(block))} Token(s)/{ActionControls.GetTimeToConsumeToken(block) / 60} minute(s)");
            playerToggleYield.Tooltip = MyStringId.GetOrCompute("If enabled, will add yield perk to all production inside the territory of the claimed faction and will add to token cost");
            playerToggleYield.Getter = Block => ActionControls.GetPlayerToggleYield(Block);
            playerToggleYield.Setter = (Block, Builder) => ActionControls.SetPlayerToggleYield(Block, Builder);

            PlayerControls.Add(playerToggleYield);

            // Enable Player Energy Perk
            var playerToggleEnergy = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerToggleEnergy");
            playerToggleEnergy.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            playerToggleEnergy.SupportsMultipleBlocks = false;
            playerToggleEnergy.Visible = Block => ActionControls.IsPlayerProductionTypeAllowed(Block, "Energy");
            playerToggleEnergy.Title = MyStringId.GetOrCompute($"Add {Math.Round(ActionControls.GetProductionEnergy(block), 0)}% Energy Efficiency:\n{(ActionControls.GetEnergyTokens(block))} Token(s)/{ActionControls.GetTimeToConsumeToken(block) / 60} minute(s)");
            playerToggleEnergy.Tooltip = MyStringId.GetOrCompute("If enabled, will add energy perk to all production inside the territory of the claimed faction and will add to token cost");
            playerToggleEnergy.Getter = Block => ActionControls.GetPlayerToggleEnergy(Block);
            playerToggleEnergy.Setter = (Block, Builder) => ActionControls.SetPlayerToggleEnergy(Block, Builder);

            PlayerControls.Add(playerToggleEnergy);

            _beaconControlsCreated = true;
        }

        public static void CreateJumpdriveControls(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (block as IMyJumpDrive == null || (block as IMyJumpDrive).BlockDefinition.SubtypeName != "SiegeBlock") return;
            if (!_jumpdriveControlsCreated)
            {
                CreateSiegeControls(block);
            }

            controls.Clear();
            controls.AddRange(SiegeControls);
        }

        public static void SiegeBlockCustomInfo(IMyTerminalBlock block, StringBuilder sb)
        {
            if (!ActionControls.IsSiegeBlock(block)) return;
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.CubeGrid.BigOwners.FirstOrDefault());
            if (faction == null) return;

            foreach (var item in Session.Instance.claimBlocks.Values)
            {
                sb.Clear();
                if (!item.Enabled) continue;
                if (Vector3D.Distance(item.BlockPos, block.GetPosition()) < item.ClaimRadius)
                {
                    sb.Append($"\n----[Territory Info]----");
                    sb.Append($"\n[Territory Name]: {item.TerritoryName}");
                    sb.Append($"\n[Token]:\n{item.ConsumptionItem}");

                    if (item.IsCooling)
                    {
                        sb.Append($"\n[Cooldown Time Left]: {TimeSpan.FromSeconds(item.Timer)}");
                        break;
                    }

                    if (item.IsSiegeCooling && item.ClaimedFaction != faction.Tag)
                    {
                        sb.Append($"\n[Siege Cooldown Left]: {TimeSpan.FromSeconds(item.SiegeTimer)}");
                        break;
                    }

                    if (!item.IsClaimed)
                    {
                        sb.Append($"\n[Cost To Claim]: {item.TokensToClaim} tokens");
                        sb.Append($"\n[Total Time To Claim]: {TimeSpan.FromSeconds(item.ToClaimTimer)}");
                        sb.Append($"\n[To Claim Distance]: {item.DistanceToClaim}m");
                        break;
                    }

                    if (!item.IsSieged && faction.Tag != item.ClaimedFaction)
                    {
                        sb.Append($"\n[Cost To Init Siege]: {item.TokensToSiege} tokens");
                        sb.Append($"\n[Total Time To Init Siege]: {TimeSpan.FromSeconds(item.ToSiegeTimer)}");
                        sb.Append($"\n[To Init Siege Distance]: {item.DistanceToSiege}m");
                        break;
                    }

                    if (item.IsSieged && !item.ReadyToSiege && item.SiegedBy == faction.Tag && !item.IsSiegingFinal)
                    {
                        sb.Append($"\n[Ready To Final Siege In]:\n{TimeSpan.FromSeconds(item.SiegeTimer)}");
                        break;
                    }

                    if (item.ReadyToSiege && item.SiegedBy == faction.Tag && !item.IsSiegingFinal)
                    {
                        sb.Append($"\n[Time Left To Start Final Siege]:\n{TimeSpan.FromSeconds(item.SiegeTimer)}");
                        sb.Append($"\n[Cost To Final Siege]: {item.TokensSiegeFinal} tokens");
                        sb.Append($"\n[Total Time To Final Siege]: {TimeSpan.FromSeconds(item.SiegeFinalTimer)}");
                        sb.Append($"\n[To Final Siege Distance]: {item.DistanceToSiege}m");
                        break;
                    }

                    break;
				}
			}
        }

        private static void CreateSiegeControls(IMyTerminalBlock block)
        {
            _jumpdriveControlsCreated = true;

            // Jumpdrive Control Seperate A
            var sepA = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyJumpDrive>("SepAJD");
            sepA.Enabled = Block => true;
            sepA.SupportsMultipleBlocks = false;
            sepA.Visible = Block => ActionControls.IsSiegeBlock(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(sepA);
            SiegeControls.Add(sepA);

            // Not in claim range error
            var claimrangeError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("ClaimRangeError");
            claimrangeError.Enabled = Block => true;
            claimrangeError.SupportsMultipleBlocks = false;
            claimrangeError.Visible = Block => ActionControls.IsInClaimOrSiegeRange(Block);
            claimrangeError.Label = MyStringId.GetOrCompute("Not Within Range of Claim Block");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(claimrangeError);
            SiegeControls.Add(claimrangeError);

            // Not in faction error
            var factionError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("FactionError");
            factionError.Enabled = Block => true;
            factionError.SupportsMultipleBlocks = false;
            factionError.Visible = Block => ActionControls.CheckForFaction(Block);
            factionError.Label = MyStringId.GetOrCompute("Must be in a faction");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(factionError);
            SiegeControls.Add(factionError);

            // Player not in range error
            var playerError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("PlayerError");
            playerError.Enabled = Block => true;
            playerError.SupportsMultipleBlocks = false;
            playerError.Visible = Block => ActionControls.CheckForPlayer(Block);
            playerError.Label = MyStringId.GetOrCompute("Player not within range of claim block");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(playerError);
            SiegeControls.Add(playerError);

            // Static Grid error
            var staticError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("StaticError");
            staticError.Enabled = Block => true;
            staticError.SupportsMultipleBlocks = false;
            staticError.Visible = Block => ActionControls.IsGridStatic(Block);
            staticError.Label = MyStringId.GetOrCompute("Grid must be a ship");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(staticError);
            SiegeControls.Add(staticError);

            // Underground error
            var undergroundError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("UndergroundError");
            undergroundError.Enabled = Block => true;
            undergroundError.SupportsMultipleBlocks = false;
            undergroundError.Visible = Block => ActionControls.IsGridUnderground(Block);
            undergroundError.Label = MyStringId.GetOrCompute("Grid must be above ground");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(undergroundError);
            SiegeControls.Add(undergroundError);

            // IsClaiming error
            var claimingError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("ClaimingError");
            claimingError.Enabled = Block => true;
            claimingError.SupportsMultipleBlocks = false;
            claimingError.Visible = Block => ActionControls.IsClaiming(Block);
            claimingError.Label = MyStringId.GetOrCompute("Territory is currently being claimed");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(claimingError);
            SiegeControls.Add(claimingError);

            // IsSieging error
            var siegingError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("SiegingError");
            siegingError.Enabled = Block => true;
            siegingError.SupportsMultipleBlocks = false;
            siegingError.Visible = Block => ActionControls.IsSieging(Block);
            siegingError.Label = MyStringId.GetOrCompute("Territory is currently being sieged");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(siegingError);
            SiegeControls.Add(siegingError);

            // Already Sieged Error
            var siegedError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("SiegedError");
            siegedError.Enabled = Block => true;
            siegedError.SupportsMultipleBlocks = false;
            siegedError.Visible = Block => ActionControls.IsSieged(Block);
            siegedError.Label = MyStringId.GetOrCompute("Territory has already been sieged");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(siegedError);
            SiegeControls.Add(siegedError);

            // Ready to final siege error
            var finalSiegeError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("FinalSiegedError");
            finalSiegeError.Enabled = Block => true;
            finalSiegeError.SupportsMultipleBlocks = false;
            finalSiegeError.Visible = Block => ActionControls.ReadyToFinalSiege(Block);
            finalSiegeError.Label = MyStringId.GetOrCompute("Territory not ready to final siege");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(finalSiegeError);
            SiegeControls.Add(finalSiegeError);

            // Enemy neaby error
            var enemyError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("EnemyError");
            enemyError.Enabled = Block => true;
            enemyError.SupportsMultipleBlocks = false;
            enemyError.Visible = Block => ActionControls.IsEnemyNearby(Block);
            enemyError.Label = MyStringId.GetOrCompute("Enemy grid is nearby");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(enemyError);
            SiegeControls.Add(enemyError);

            // Tokens error
            var tokenError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("TokenError");
            tokenError.Enabled = Block => true;
            tokenError.SupportsMultipleBlocks = false;
            tokenError.Visible = Block => ActionControls.CheckForClaimTokens(Block);
            tokenError.Label = MyStringId.GetOrCompute("Not enough tokens in inventory");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(tokenError);
            SiegeControls.Add(tokenError);

            // Not enough energy error
            var energyError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("EnergyError");
            energyError.Enabled = Block => true;
            energyError.SupportsMultipleBlocks = false;
            energyError.Visible = Block => ActionControls.CheckForEnergy(Block);
            energyError.Label = MyStringId.GetOrCompute("Jumpdrive is not fully charged");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(energyError);
            SiegeControls.Add(energyError);

            // Cooling Error
            var isCoolingError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("CoolingError");
            isCoolingError.Enabled = Block => true;
            isCoolingError.SupportsMultipleBlocks = false;
            isCoolingError.Visible = Block => ActionControls.IsCooling(Block);
            isCoolingError.Label = MyStringId.GetOrCompute("Wait for cooldown to claim");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(isCoolingError);
            SiegeControls.Add(isCoolingError);

            // Siege Cooling Error
            var siegeCoolingError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("SiegeCoolingError");
            siegeCoolingError.Enabled = Block => true;
            siegeCoolingError.SupportsMultipleBlocks = false;
            siegeCoolingError.Visible = Block => ActionControls.IsSiegeCooling(Block);
            siegeCoolingError.Label = MyStringId.GetOrCompute("Wait for siege cooldown to siege");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(siegeCoolingError);
            SiegeControls.Add(siegeCoolingError);

            // InVoxel Error
            /*var inVoxelError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("InvoxelError");
            inVoxelError.Enabled = Block => true;
            inVoxelError.SupportsMultipleBlocks = false;
            inVoxelError.Visible = Block => ActionControls.IsGridInvoxel(Block);
            inVoxelError.Label = MyStringId.GetOrCompute("Grid cannot be anchored to voxels");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(inVoxelError);
            SiegeControls.Add(inVoxelError);*/

            // Claim Button
            var claimButton = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyJumpDrive>("ClaimButton");
            claimButton.Enabled = Block => ActionControls.AllowClaimEnable(Block);
            claimButton.SupportsMultipleBlocks = false;
            claimButton.Visible = Block => ActionControls.IsNearClaim(Block);
            claimButton.Title = MyStringId.GetOrCompute("Claim Territory");
            //claimButton.Tooltip = MyStringId.GetOrCompute("Sets the claim area radius.");
            claimButton.Action = Block => ActionControls.InitClaim(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(claimButton);
            SiegeControls.Add(claimButton);

            // Siege Button
            var siegeButton = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyJumpDrive>("SiegeButton");
            siegeButton.Enabled = Block => ActionControls.AllowSiegeEnable(Block);
            siegeButton.SupportsMultipleBlocks = false;
            siegeButton.Visible = Block => ActionControls.IsNearClaimed(Block);
            siegeButton.Title = MyStringId.GetOrCompute("Init Siege");
            //claimButton.Tooltip = MyStringId.GetOrCompute("Sets the claim area radius.");
            siegeButton.Action = Block => ActionControls.InitSiege(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(siegeButton);
			SiegeControls.Add(siegeButton);

			// Final Siege Button
			var finalSiegeButton = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyJumpDrive>("FinalSiegeButton");
            finalSiegeButton.Enabled = Block => ActionControls.AllowFinalSiegeEnable(Block);
            finalSiegeButton.SupportsMultipleBlocks = false;
            finalSiegeButton.Visible = Block => ActionControls.IsNearClaimed(Block);
            finalSiegeButton.Title = MyStringId.GetOrCompute("Final Siege");
            //claimButton.Tooltip = MyStringId.GetOrCompute("Sets the claim area radius.");
            finalSiegeButton.Action = Block => ActionControls.InitSiege(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(finalSiegeButton);
			SiegeControls.Add(finalSiegeButton);
		}

        public static void CreateJumpdriveActions(IMyTerminalBlock block, List<IMyTerminalAction> actions)
        {
            if (!ActionControls.IsSiegeBlock(block)) return;
            actions.Clear();
        }
    }
}
