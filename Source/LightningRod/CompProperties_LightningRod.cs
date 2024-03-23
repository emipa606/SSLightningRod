using RimWorld;

namespace SSLightningRod;

public class CompProperties_LightningRod : CompProperties_Power
{
    public readonly float chargeCapacity = 2000.00f;
    public readonly float cooldownPercentPowerSave = 50;
    public readonly float cooldownSpeed = 1f;
    public readonly float fakeZIndex = 4f;
    public readonly bool isBasic = false;
    public readonly int oneInXChanceHitPowerSave = 4;
    public readonly float powerGainDischarge = 2000;
    public readonly bool strikesHitBase = false;

    public CompProperties_LightningRod()
    {
        compClass = typeof(CompLightningRod);
    }
}