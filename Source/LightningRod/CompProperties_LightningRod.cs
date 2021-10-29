using RimWorld;

namespace SSLightningRod
{
    public class CompProperties_LightningRod : CompProperties_Power
    {
        public float chargeCapacity = 2000.00f;
        public float cooldownPercentPowerSave = 50;
        public float cooldownSpeed = 1f;
        public float fakeZIndex = 4f;
        public bool isBasic = false;
        public int oneInXChanceHitPowerSave = 4;
        public float powerGainDischarge = 2000;
        public bool strikesHitBase = false;

        public CompProperties_LightningRod()
        {
            compClass = typeof(CompLightningRod);
        }
    }
}