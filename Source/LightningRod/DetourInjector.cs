using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace SSDetours
{
    [StaticConstructorOnStartup]
    public static class DetourInjector
    {
        static DetourInjector()
        {
            LongEventHandler.QueueLongEvent(Inject, "Running patches", false, null);
        }

        public static void Inject()
        {
            var Info = "WeatherEvent_LightningStrike.FireEvent";
            try
            {
                ((Action)(() =>
                {
                    var hinstance = new Harmony("com.spdskatr.lightningrod.detours");
                    Log.Message("SS Lightning Rod Detours: Using Harmony to Prefix and Transpiler patch " + Info);
                    hinstance.PatchAll(Assembly.GetExecutingAssembly());
                }))();
            }
            catch (TypeLoadException) //These lines shouldn't be activated in normal circumstances
            {
                Log.Error(
                    "SS Lightning Rod Detours: Tried to use Harmony to patch method, but Harmony was not found. Mod will not work if this error comes up.");
            }
        }
    }
}