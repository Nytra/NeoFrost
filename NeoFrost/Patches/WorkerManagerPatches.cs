using BaseX;
using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoFrost.Patches;

#nullable disable

[HarmonyPatch(typeof(WorkerManager))]
public static class WorkerManagerPatches
{
    [HarmonyPatch(typeof(WorkerManager), nameof(WorkerManager.GetType))]
    [HarmonyPrefix]
    public static bool GetTypePrefix(ref string typename, ref Type __result)
    {
        if (typename == null)
        {
            __result = null;
            return false;
        }

        Type type = null;
        if (typename.StartsWith("["))
        {
            if (typename.Contains("ProtoFlux"))
            {
                __result = null;
                return false;
            }
            typename = string.Join("]", typename.Split(']').Skip(1));
            if (typename.Contains('<'))
            {
                var parts = typename.Split('<');
                var therest = string.Join("<", parts.Skip(1));
                var genTypeDefName = parts[0] + $"`{parts.Length - 1}";
                Type genTypeDef = TypeHelper.FindType(genTypeDefName);
                if (genTypeDef != null)
                {
                    var typeArgName = typename.Split('<')[1];
                    typeArgName = typeArgName.Remove(typeArgName.Length - 1); // remove last '>'
                    Type typeArg = WorkerManager.GetType(typeArgName);
                    if (typeArg != null)
                    {
                        type = genTypeDef!.MakeGenericType(typeArg);
                    }
                    else
                    {
                        UniLog.Warning($"Could not find generic type argument: {typeArgName}");
                    }
                }
                else
                {
                    UniLog.Warning($"Could not find generic type definition: {genTypeDefName}");
                }
                __result = type;
                return false;
            }
        }
        return true;
    }
}