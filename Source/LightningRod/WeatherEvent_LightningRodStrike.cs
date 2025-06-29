using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SSLightningRod;

[HarmonyPatch(typeof(WeatherEvent_LightningStrike), nameof(WeatherEvent_LightningStrike.DoStrike))]
public static class WeatherEvent_LightningRodStrike
{
    [HarmonyPriority(Priority.Last)]
    public static bool Prefix(Map map, ref IntVec3 strikeLoc, ref Mesh boltMesh)
    {
        var vec3 = strikeLoc;
        var activeRods = Main.ColonistsHaveLightningRodActive(map, true);
        var currentRod = activeRods.OrderBy(building => building.Position.DistanceTo(vec3)).FirstOrDefault();

        if (currentRod == null)
        {
            return true;
        }

        if (currentRod.Position.DistanceTo(vec3) > 5f)
        {
            return true;
        }

        currentRod.TryGetComp<CompLightningRod>().Hit();
        SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(map);

        if (!strikeLoc.IsValid)
        {
            strikeLoc = CellFinderLoose.RandomCellWith(sq => sq.Standable(map) && !map.roofGrid.Roofed(sq), map);
        }

        var info = SoundInfo.InMap(new TargetInfo(strikeLoc, map));
        SoundDefOf.Thunder_OnMap.PlayOneShot(info);

        boltMesh = LightningBoltMeshPool.RandomBoltMesh;
        if (strikeLoc.Fogged(map))
        {
            return false;
        }

        var loc = strikeLoc.ToVector3Shifted();

        for (var i = 0; i < 4; i++)
        {
            FleckMaker.ThrowMicroSparks(loc, map);
            FleckMaker.ThrowLightningGlow(loc, map, 1.5f);
        }

        return false;
    }
}