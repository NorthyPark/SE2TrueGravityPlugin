using System;
using System.Reflection;
using HarmonyLib;
using Keen.Game2.Simulation.GameSystems.Physicss;
using Keen.VRage.Library.Diagnostics;

namespace ClientPlugin.Patches;

// SE2 ships with "GravityMultiplier": 2 in its PhysicsSessionConfiguration definition
// (GameData/Vanilla/Content/System/Configurations/PhysicsSessionConfiguration.def).
// That multiplier is applied on top of every gravity source (planets and gravity generators) for all
// rigid bodies except characters, so ships, floating objects, debris and so on fall at twice the
// displayed acceleration. The HUD and the gravity generator settings still show the unmultiplied value,
// which is why "1G" is really 2G.
//
// This plugin replaces that multiplier with the value from the plugin settings (default 1).
//
// PhysicsSessionConfiguration.GravityMultiplier is a plain auto-property, so the JIT may inline it into
// callers. To not depend on a single hook, the value is overridden in three places and each one is
// applied separately (a failure of one does not stop the others):
//   1. Init(builder):   postfix that overwrites the value right after the definition was loaded
//   2. setter:          prefix that replaces the incoming value
//   3. getter:          postfix that replaces the returned value
public static class PhysicsSessionConfigurationPatch
{
    private const string BuilderTypeName = "Keen.Game2.Simulation.GameSystems.Physicss.PhysicsSessionConfigurationObjectBuilder";

    private static MethodInfo setter;
    private static bool loggedOverride;

    public static void Apply(Harmony harmony)
    {
        var target = typeof(PhysicsSessionConfiguration);
        const string property = nameof(PhysicsSessionConfiguration.GravityMultiplier);

        setter = AccessTools.PropertySetter(target, property);

        TryPatch("Init postfix", () =>
        {
            var builderType = AccessTools.TypeByName(BuilderTypeName);
            var init = builderType == null ? null : AccessTools.Method(target, "Init", new[] { builderType });
            if (init == null)
                throw new MissingMethodException("PhysicsSessionConfiguration.Init(builder) not found");
            harmony.Patch(init, postfix: new HarmonyMethod(typeof(PhysicsSessionConfigurationPatch), nameof(InitPostfix)));
        });

        TryPatch("setter prefix", () =>
        {
            if (setter == null)
                throw new MissingMethodException("GravityMultiplier setter not found");
            harmony.Patch(setter, prefix: new HarmonyMethod(typeof(PhysicsSessionConfigurationPatch), nameof(SetterPrefix)));
        });

        TryPatch("getter postfix", () =>
        {
            var getter = AccessTools.PropertyGetter(target, property);
            if (getter == null)
                throw new MissingMethodException("GravityMultiplier getter not found");
            harmony.Patch(getter, postfix: new HarmonyMethod(typeof(PhysicsSessionConfigurationPatch), nameof(GetterPostfix)));
        });
    }

    private static void TryPatch(string what, Action patch)
    {
        try
        {
            patch();
            Log.Default.WriteLine($"[{Plugin.Name}] Patched GravityMultiplier ({what})");
        }
        catch (Exception e)
        {
            Log.Default.WriteLine(LogSeverity.Error, $"[{Plugin.Name}] Could not patch GravityMultiplier ({what}): {e.Message}");
        }
    }

    // Called by Harmony: after the definition was loaded from the game data.
    public static void InitPostfix(PhysicsSessionConfiguration __instance)
    {
        try
        {
            var config = Config.Current;
            if (!config.Enabled || setter == null)
                return;

            setter.Invoke(__instance, new object[] { config.GravityMultiplier });

            if (!loggedOverride)
            {
                loggedOverride = true;
                Log.Default.WriteLine($"[{Plugin.Name}] GravityMultiplier overridden with {config.GravityMultiplier} (the game default is 2)");
            }
        }
        catch (Exception e)
        {
            Log.Default.WriteLine(LogSeverity.Error, $"[{Plugin.Name}] InitPostfix failed: {e}");
        }
    }

    // Called by Harmony: instead of the value the game wants to store.
    public static void SetterPrefix(ref float value)
    {
        var config = Config.Current;
        if (config.Enabled)
            value = config.GravityMultiplier;
    }

    // Called by Harmony: instead of the value the game reads.
    public static void GetterPostfix(ref float __result)
    {
        var config = Config.Current;
        if (config.Enabled)
            __result = config.GravityMultiplier;
    }
}
