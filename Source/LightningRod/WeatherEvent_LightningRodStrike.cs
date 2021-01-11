using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SSLightningRod
{
    [HarmonyPatch(typeof(WeatherEvent_LightningStrike))]
    [HarmonyPatch("FireEvent")]
    [StaticConstructorOnStartup]
	public static class WeatherEvent_LightningRodStrike
    {
        static bool _state_ = false;
        public static bool ColonistsHaveLightningRodActive(out List<Building> activeRods, Map map)
        {
            var result = false;
            activeRods = new List<Building>();
            for (var i = 0; i < map.listerBuildings.allBuildingsColonist.Count; i++)
            {
                CompLightningRod comp = map.listerBuildings.allBuildingsColonist[i].TryGetComp<CompLightningRod>();
                if (comp != null && comp.notOverwhelmed && comp.PowerOn && comp.ToggleMode != 1)
                {
                    activeRods.Add(map.listerBuildings.allBuildingsColonist[i]);
                    result = true;
                }
            }
            if (!result)
            {
                for (var i = 0; i < map.listerBuildings.allBuildingsColonist.Count; i++)
                {
                    CompLightningRod comp = map.listerBuildings.allBuildingsColonist[i].TryGetComp<CompLightningRod>();
                    var random = new System.Random(i + Find.TickManager.TicksAbs);
                    if (comp != null && comp.notOverwhelmed && comp.PowerOn && comp.ToggleMode == 1)
                    {
                        var h = random.Next(4);
                        if (h == 0)
                        {
                            activeRods.Add(map.listerBuildings.allBuildingsColonist[i]);
                            result = true;
                        }
                    }
                    else if (comp != null && !comp.notOverwhelmed && comp.PowerOn && comp.ToggleMode != 1)
                    {
                        var h = random.Next(comp.Powersavechance);
                        if (h == 0)
                        {
                            activeRods.Add(map.listerBuildings.allBuildingsColonist[i]);
                            result = true;
                        }
                    }
                }
            }
            _state_ = result;
            return result;
        }

        [HarmonyPriority(Priority.Last)]
        public static void Prefix(WeatherEvent_LightningStrike __instance)
        {
            var flash = new WeatherEvent_LightningFlash(Traverse.Create(__instance).Field("map").GetValue<Map>());
            Map map1 = Traverse.Create(__instance).Field("map").GetValue<Map>();
            var activeRodsDetected = ColonistsHaveLightningRodActive(out List<Building> activeRods, map1);
            if (activeRodsDetected)
            {
                var rand = new System.Random();
                var num = rand.Next(activeRods.Count);
                Building target = activeRods[num];
                var list = GenAdj.CellsOccupiedBy(target).ToList();
                var strikesHitBase = target.TryGetComp<CompLightningRod>().StrikesHitBase;
                var num1 = target.TryGetComp<CompLightningRod>().FakeZIndex;
                var num2 = rand.Next((int)num1 - strikesHitBase);
                IntVec3 intvec = list[0];
                intvec.z += num2 + strikesHitBase;
                Traverse.Create(__instance).Field("strikeLoc").SetValue(intvec);
                target.TryGetComp<CompLightningRod>().Hit();
            }
            else
            {
                flash.FireEvent();
            }
        }

        /// <summary>
        /// To stop rods blowing themselves up.
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var intermediate = Transpilers.MethodReplacer(instructions, typeof(GenExplosion).GetMethod("DoExplosion", BindingFlags.Public | BindingFlags.Static), typeof(WeatherEvent_LightningRodStrike).GetMethod(nameof(DoExplosionLogic)));
            return Transpilers.MethodReplacer(intermediate, typeof(MoteMaker).GetMethod("ThrowSmoke"), typeof(WeatherEvent_LightningRodStrike).GetMethod(nameof(DoSmokeLogic)));
        }
        public static void DoExplosionLogic (IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, int damAmount = -1, float armorPenetration = -1f, SoundDef explosionSound = null, ThingDef weapon = null, ThingDef projectile = null, Thing intendedTarget = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1, float chanceToStartFire = 0f, bool damageFalloff = false, float? direction = null, List<Thing> ignoredThings = null)
        {
            if (!_state_)
            {
                GenExplosion.DoExplosion(center, map, radius, damType, instigator, damAmount, armorPenetration, explosionSound, weapon, projectile, intendedTarget, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, chanceToStartFire, damageFalloff, direction, ignoredThings);
            }
        }
        public static void DoSmokeLogic (Vector3 loc, Map map, float size)
        {
            if (!_state_)
            {
                MoteMaker.ThrowSmoke(loc, map, size);
            }
        }
	}
}
