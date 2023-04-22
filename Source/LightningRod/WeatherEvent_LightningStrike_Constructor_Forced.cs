using HarmonyLib;
using RimWorld;
using Verse;

namespace SSLightningRod;

[HarmonyPatch(typeof(WeatherEvent_LightningStrike), MethodType.Constructor, typeof(Map), typeof(IntVec3))]
public static class WeatherEvent_LightningStrike_Constructor_Forced
{
    public static void Postfix(ref IntVec3 ___strikeLoc, Map map)
    {
        ___strikeLoc = Main.SetNewStrikePoint(___strikeLoc, map);
    }
}