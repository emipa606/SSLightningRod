using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace SSLightningRod;

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
    public bool IsBasic => Properties.isBasic;
    private CompProperties_LightningRod Properties => (CompProperties_LightningRod)props;


    public float PowerOutputFromMode()
    {
        float num;
        switch (ToggleMode)
        {
            case 1:
                num = 0;
                break;
            case 2:
                num = Properties.PowerConsumption * -1;
                break;
            case 3:
                num = Properties.PowerConsumption * -3;
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
        if (IsBasic)
        {
            return;
        }

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
        if (IsBasic)
        {
            parent.HitPoints -= parent.MaxHitPoints / 100;
            return;
        }

        if (LightningRodCooldown > ChargeCapacity)
        {
            notOverwhelmed = false;
        }
    }

    public override string CompInspectStringExtra()
    {
        if (IsBasic)
        {
            return null;
        }

        var str = new StringBuilder();
        var powerConsumptionAsValue = powerOutputInt * -1;
        var str1 = powerConsumptionAsValue >= 0
            ? "SSLR.PowerConsumption".Translate(powerConsumptionAsValue)
            : "SSLR.PowerOutput".Translate(powerOutputInt);
        str.AppendLine(str1);
        var str2 = LightningRodCooldown <= 0 ? "SSLR.Standby".Translate() :
            notOverwhelmed ? "SSLR.Discharging".Translate() : "SSLR.Overwhelmed".Translate();
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

        str.Append("SSLR.Cooldown".Translate(Math.Round(LightningRodCooldown), ChargeCapacity));
        return str.ToString();
    }

    public string ModeDescs()
    {
        string returnstr;
        switch (ToggleMode)
        {
            case 1:
                returnstr = "SSLR.PowerSaveMode".Translate(Math.Round((decimal)100 / Powersavechance, 2).ToString(),
                    CooldownPercentPowerSave);
                break;
            case 2:
                returnstr = "SSLR.NormalMode".Translate(Math.Round((decimal)100 / Powersavechance, 2).ToString());
                break;
            case 3:
                returnstr = "SSLR.FastMode".Translate(Math.Round((decimal)100 / Powersavechance, 2).ToString());
                break;
            default:
                ToggleMode = 1;
                returnstr = "SSLR.PowerSaveMode".Translate(Math.Round((decimal)100 / Powersavechance, 2).ToString(),
                    CooldownPercentPowerSave);
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

        if (IsBasic)
        {
            yield break;
        }

        if (parent.Faction == Faction.OfPlayer)
        {
            yield return new Command_ChangeMode
            {
                hotKey = KeyBindingDefOf.Misc8,
                icon = ContentFinder<Texture2D>.Get("RotRight_Green"),
                defaultLabel = "SSLR.ChangeMode".Translate(),
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