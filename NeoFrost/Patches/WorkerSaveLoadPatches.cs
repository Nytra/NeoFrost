using System;
using System.Linq;
using BaseX;
using HarmonyLib;
using NeoFrost.Extensions;
using NeoFrost.Load;

namespace NeoFrost.Patches;

#nullable disable

[HarmonyPatch("WorkerSaveLoad", "ExtractWorker")]
public static class WorkerSaveLoadPatches
{
    private static readonly Type WorkerDataType =
        AccessTools.TypeByName("WorkerSaveLoad").GetNestedType("WorkerData", AccessTools.all);
    
    [HarmonyPrefix]
    public static bool Prefix(DataTreeNode node, ref object __result)
    {
        DataTreeDictionary dict = (DataTreeDictionary)node;
        var data = (DataTreeDictionary)dict["Data"];
        foreach (var thing in data.EnumerateTree())
        {
            if (thing is DataTreeValue dtval)
            {
                if (dtval.IsURL)
                {
                    // change resdb to neosdb so assets from resonite can load with the mod
                    var str = dtval.Value as string;
                    dtval.UpdateValue(new Uri(str.Substring(1).Replace("resdb", "neosdb")));
                }
            }
            else if (thing is DataTreeDictionary dtdict)
            {
                // Change all assets to use DirectLoad which helps if certain variants are missing from the cloud
                RecurseDirectLoad(dtdict);
            }
        }
        DataTreeValue dataVal = (DataTreeValue)dict["Type"];
        if (dataVal.Value is long val)
        {
            SlotExtraInfo info = SlotLoader.SlotStore.First(s => s.Node == node || s.Node.Contains(node));
            object workerData = Activator.CreateInstance(WorkerDataType, info.Types[(int)val].TypeName, dict["Data"]);
            __result = workerData;
            return false;
        }
        return true;
    }

    private static void RecurseDirectLoad(DataTreeDictionary dict)
    {
        foreach (var child in dict.Children)
        {
            if (child.Key == "DirectLoad" && child.Value is DataTreeDictionary dtdict)
            {
                var dtval = (DataTreeValue)dtdict["Data"];
                dtval.UpdateValue(true);
            }
        }
    }
}