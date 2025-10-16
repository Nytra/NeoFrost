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
        if (typename == null || typename.Contains("ProtoFlux"))
        {
            __result = null;
            return false;
        }

        var fixedTypeName = GetFixedTypeStr(typename);

        bool isGeneric = IsGeneric(fixedTypeName);

        Type type = null;
        if (isGeneric)
        {
            var parts = fixedTypeName.Split('<');
            var therest = string.Join("<", parts.Skip(1));
            var genTypeDef = parts[0] + $"`{parts.Length - 1}";
            Type genType = TypeHelper.FindType(genTypeDef);
            if (genType != null)
            {
                var typeArgName = fixedTypeName.Split('<')[1];
                typeArgName = typeArgName.Remove(typeArgName.Length - 1); // remove last '>'
                Type typeArg = WorkerManager.GetType(typeArgName);
                if (typeArg != null)
                {
                    type = genType!.MakeGenericType(typeArg);
                }
                else
                {
                    UniLog.Warning($"Could not find generic type argument: {typeArgName}");
                }
            }
            else
            {
                UniLog.Warning($"Could not find generic type definition: {genTypeDef}");
            }
        }
        else
        {
            typename = fixedTypeName;
            return true;
        }

        __result = type;
        return false;
    }

    private static string GetFixedTypeStr(string typeStr)
    {
        if (typeStr.StartsWith("["))
            return string.Join("]", typeStr.Split(']').Skip(1));
        else
            return typeStr;
    }

    private static bool IsGeneric(string typeName)
    {
        return typeName.Contains("<");
    }
}