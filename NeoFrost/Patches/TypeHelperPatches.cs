using BaseX;
using HarmonyLib;
using System.Collections.Generic;

namespace NeoFrost.Patches;

[HarmonyPatch(typeof(TypeHelper))]
public class TypeHelperPatches
{
    private static Dictionary<string, string> TypeRemappings = new();
    static TypeHelperPatches()
    {
        // Add DevTool backwards compatibility to make equipping it easier
        TypeRemappings.Add("FrooxEngine.DevTool", "FrooxEngine.DevToolTip");
    }
    [HarmonyPatch(typeof(TypeHelper), nameof(TypeHelper.FindType))]
    [HarmonyPrefix]
    public static bool FindTypePrefix(ref string typename)
    {
        if (TypeRemappings.TryGetValue(typename, out var newTypeName))
        {
            typename = newTypeName;
        }
        return true;
    }
}