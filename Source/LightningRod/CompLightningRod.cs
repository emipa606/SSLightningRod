﻿using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace SSLightningRod
{
    internal class CompLightningRod : CompPowerTrader
    {
        private float LightningRodCooldown;
        public bool notOverwhelmed = true;
        public int ToggleMode = 1;

        public int StrikesHitBase
        {
            get
            {
                if (Properties.strikesHitBase)
                {
                    return 0;
                }

                return 1;
            }
        }

        public float CooldownSpeed => Properties.cooldownSpeed;
        public float ChargeCapacity => Properties.chargeCapacity;
        public float FakeZIndex => Properties.fakeZIndex;
        public int Powersavechance => Properties.oneInXChanceHitPowerSave;
        public float PowerGainDischarge => Properties.powerGainDischarge;
        public float CooldownPercentPowerSave => Properties.cooldownPercentPowerSave;
        private CompProperties_LightningRod Properties => (CompProperties_LightningRod) props;

        public float PowerOutputFromMode()
        {
            float num;
            switch (ToggleMode)
            {
                case 1:
                    num = 0;
                    break;
                case 2:
                    num = Properties.basePowerConsumption * -1;
                    break;
                case 3:
                    num = Properties.basePowerConsumption * -3;
                    break;
                default:
                    num = 0;
                    ToggleMode = 1;
                    break;
            }

            return num;
        }

        public override void CompTick()
        {
            base.CompTick();
            var num2 = ToggleMode == 1 ? CooldownSpeed * CooldownPercentPowerSave / 100 : CooldownSpeed;
            var num = ToggleMode == 3 ? CooldownSpeed * 4 : num2;
            LightningRodCooldown = LightningRodCooldown <= 0f ? 0f : LightningRodCooldown - num;
            powerOutputInt = LightningRodCooldown > 0f
                ? PowerOutputFromMode() + PowerGainDischarge
                : PowerOutputFromMode();
            notOverwhelmed = LightningRodCooldown < ChargeCapacity;
        }

        public void Hit()
        {
            LightningRodCooldown += 500f;
            if (LightningRodCooldown > ChargeCapacity)
            {
                notOverwhelmed = false;
            }
        }

        public override string CompInspectStringExtra()
        {
            var str = new StringBuilder();
            var powerConsumptionAsValue = powerOutputInt * -1;
            var str1 = powerConsumptionAsValue >= 0
                ? "Power consumption: " + powerConsumptionAsValue + " W"
                : "Power output: " + powerOutputInt + " W";
            str.AppendLine(str1);
            var str2 = LightningRodCooldown <= 0 ? "Standby." : notOverwhelmed ? "Discharging." : "Overwhelmed.";
            str.AppendLine(str2);
            if (!PowerOn)
            {
                str.AppendLine("PowerNotConnected".Translate());
            }
            else
            {
                var text = (PowerNet.CurrentEnergyGainRate() / WattsToWattDaysPerTick).ToString("F0");
                var text2 = PowerNet.CurrentStoredEnergy().ToString("F0");
                str.AppendLine("PowerConnectedRateStored".Translate(text, text2));
            }

            str.Append("Cooldown: " + Math.Round(LightningRodCooldown) + "/" + ChargeCapacity);
            return str.ToString();
        }

        public string ModeDescs()
        {
            string returnstr;
            switch (ToggleMode)
            {
                case 1:
                    returnstr =
                        "Power saving mode(Click to change modes): Does not consume power when idle, outputs a lot of power when struck, but only has a " +
                        Math.Round((decimal) 100 / Powersavechance, 2) +
                        "% chance to attract a lightning bolt and cools down " + CooldownPercentPowerSave + "% slower";
                    break;
                case 2:
                    returnstr =
                        "Normal mode(Click to change modes): Consumes a bit of power when idle, outputs enough power to sustain itself in a storm and has a 100% chance to attract a lightning bolt. Can attract lightning at a " +
                        Math.Round((decimal) 100 / Powersavechance, 2) + "% chance when overwhelmed.";
                    break;
                case 3:
                    returnstr =
                        "Fast cooldown mode(Click to change modes): Cools down 4x faster than normal but consumes triple the amount of power. Has a 100% chance to attract a lightning bolt. Can attract lightning at a " +
                        Math.Round((decimal) 100 / Powersavechance, 2) + "% chance when overwhelmed.";
                    break;
                default:
                    ToggleMode = 1;
                    returnstr =
                        "Power saving mode(Click to change modes): Does not consume power when idle, outputs a lot of power when struck, but only has a " +
                        Math.Round((decimal) 100 / Powersavechance, 2) +
                        "% chance to attract a lightning bolt and cools down " + CooldownPercentPowerSave + "% slower";
                    break;
            }

            return returnstr;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ToggleMode, "ToggleMode", 1);
            Scribe_Values.Look(ref LightningRodCooldown, "Cooldown");
            Scribe_Values.Look(ref notOverwhelmed, "notOverwhelmed", true);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var c in base.CompGetGizmosExtra())
            {
                yield return c;
            }

            if (parent.Faction == Faction.OfPlayer)
            {
                yield return new Command_ChangeMode
                {
                    hotKey = KeyBindingDefOf.Misc8,
                    icon = ContentFinder<Texture2D>.Get("RotRight_Green"),
                    defaultLabel = "Change Mode",
                    defaultDesc = ModeDescs(),
                    Mode = () => ToggleMode,
                    toggleAction = () =>
                    {
                        if (LightningRodCooldown > 0)
                        {
                            Messages.Message("Cannot change mode now, rod still discharging.",
                                MessageTypeDefOf.RejectInput);
                        }
                        else
                        {
                            ToggleMode += 1;
                        }
                    }
                };
            }
        }
    }
}