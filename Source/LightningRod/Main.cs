using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace SSLightningRod;

[StaticConstructorOnStartup]
public static class Main
{
    static Main()
    {
        new Harmony("com.spdskatr.lightningrod.detours").PatchAll(Assembly.GetExecutingAssembly());
    }

    public static List<Building> ColonistsHaveLightningRodActive(Map map, bool getAll = false)
    {
        var activeRods = new List<Building>();
        foreach (var building in map.listerBuildings.allBuildingsColonist)
        {
            var comp = building.TryGetComp<CompLightningRod>();
            if (comp == null)
            {
                continue;
            }

            if (getAll || comp.IsBasic && Rand.Bool ||
                comp is { NotOverwhelmed: true, PowerOn: true } && comp.ToggleMode != 1 ||
                comp is { NotOverwhelmed: true, PowerOn: true, ToggleMode: 1 } &&
                Rand.Range(1, comp.Powersavechance) == 1)
            {
                activeRods.Add(building);
                continue;
            }

            if (comp is { NotOverwhelmed: false, PowerOn: true } && comp.ToggleMode != 1 &&
                Rand.Range(1, comp.Powersavechance) == 1)
            {
                activeRods.Add(building);
            }
        }

        return activeRods;
    }

    public static IntVec3 SetNewStrikePoint(IntVec3 intVec3, Map map)
    {
        var activeRods = ColonistsHaveLightningRodActive(map);
        if (!activeRods.Any())
        {
            return intVec3;
        }

        var currentRod = activeRods.RandomElement();
        var rand = new Random();
        var list = GenAdj.CellsOccupiedBy(currentRod).ToList();
        var strikesHitBase = currentRod.TryGetComp<CompLightningRod>().StrikesHitBase;
        var num1 = currentRod.TryGetComp<CompLightningRod>().FakeZIndex;
        var num2 = rand.Next((int)num1 - strikesHitBase);
        var strikePoint = list[0];
        strikePoint.z += num2 + strikesHitBase;
        return strikePoint;
    }
}