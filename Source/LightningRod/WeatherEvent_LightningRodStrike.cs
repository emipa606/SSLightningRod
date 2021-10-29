using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Random = System.Random;

namespace SSLightningRod
{
    [HarmonyPatch(typeof(WeatherEvent_LightningStrike))]
    [HarmonyPatch("FireEvent")]
    [StaticConstructorOnStartup]
    public static class WeatherEvent_LightningRodStrike
    {
        private static bool _state_;

        public static bool ColonistsHaveLightningRodActive(out List<Building> activeRods, Map map)
        {
            activeRods = new List<Building>();
            foreach (var building in map.listerBuildings.allBuildingsColonist)
            {
                var comp = building.TryGetComp<CompLightningRod>();
                if (comp == null)
                {
                    continue;
                }

                if (comp.IsBasic && Rand.Bool)
                {
                    activeRods.Add(building);
                    continue;
                }

                if (comp is { notOverwhelmed: true, PowerOn: true } && comp.ToggleMode != 1)
                {
                    activeRods.Add(building);
                    continue;
                }

                if (comp is { notOverwhelmed: true, PowerOn: true, ToggleMode: 1 } &&
                    Rand.Range(1, comp.Powersavechance) == 1)
                {
                    activeRods.Add(building);
                    continue;
                }

                if (comp is { notOverwhelmed: false, PowerOn: true } && comp.ToggleMode != 1 &&
                    Rand.Range(1, comp.Powersavechance) == 1)
                {
                    activeRods.Add(building);
                }
            }

            _state_ = activeRods.Any();
            return _state_;
        }

        [HarmonyPriority(Priority.Last)]
        public static void Prefix(Map ___map, ref IntVec3 ___strikeLoc)
        {
            //var flash = new WeatherEvent_LightningFlash(Traverse.Create(__instance).Field("map").GetValue<Map>());
            //var map1 = Traverse.Create(__instance).Field("map").GetValue<Map>();
            var activeRodsDetected = ColonistsHaveLightningRodActive(out var activeRods, ___map);
            if (!activeRodsDetected)
            {
                return;
            }

            var rand = new Random();
            var target = activeRods.RandomElement();
            var list = GenAdj.CellsOccupiedBy(target).ToList();
            var strikesHitBase = target.TryGetComp<CompLightningRod>().StrikesHitBase;
            var num1 = target.TryGetComp<CompLightningRod>().FakeZIndex;
            var num2 = rand.Next((int)num1 - strikesHitBase);
            var intvec = list[0];
            intvec.z += num2 + strikesHitBase;
            ___strikeLoc = intvec;
            //Traverse.Create(__instance).Field("strikeLoc").SetValue(intvec);
            target.TryGetComp<CompLightningRod>().Hit();
            //else
            //{
            //    flash.FireEvent();
            //}
        }

        /// <summary>
        ///     To stop rods blowing themselves up.
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var intermediate = instructions.MethodReplacer(
                typeof(GenExplosion).GetMethod("DoExplosion", BindingFlags.Public | BindingFlags.Static),
                typeof(WeatherEvent_LightningRodStrike).GetMethod(nameof(DoExplosionLogic)));
            return intermediate.MethodReplacer(typeof(FleckMaker).GetMethod("ThrowSmoke"),
                typeof(WeatherEvent_LightningRodStrike).GetMethod(nameof(DoSmokeLogic)));
        }

        public static void DoExplosionLogic(IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator,
            int damAmount = -1, float armorPenetration = -1f, SoundDef explosionSound = null, ThingDef weapon = null,
            ThingDef projectile = null, Thing intendedTarget = null, ThingDef postExplosionSpawnThingDef = null,
            float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1,
            bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null,
            float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1, float chanceToStartFire = 0f,
            bool damageFalloff = false, float? direction = null, List<Thing> ignoredThings = null)
        {
            if (!_state_)
            {
                GenExplosion.DoExplosion(center, map, radius, damType, instigator, damAmount, armorPenetration,
                    explosionSound, weapon, projectile, intendedTarget, postExplosionSpawnThingDef,
                    postExplosionSpawnChance, postExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors,
                    preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, chanceToStartFire,
                    damageFalloff, direction, ignoredThings);
            }
        }

        public static void DoSmokeLogic(Vector3 loc, Map map, float size)
        {
            if (!_state_)
            {
                FleckMaker.ThrowSmoke(loc, map, size);
            }
        }
    }
}