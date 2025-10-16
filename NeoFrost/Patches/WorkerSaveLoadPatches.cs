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
        bool isResoniteWorker = false;
        DataTreeDictionary dict = (DataTreeDictionary)node;
        DataTreeValue dataVal = (DataTreeValue)dict["Type"];
        var data = (DataTreeDictionary)dict["Data"];
        if (dataVal.Value is long val)
        {
            isResoniteWorker = true;
            SlotExtraInfo info = SlotLoader.SlotStore.First(s => s.Node == node || s.Node.Contains(node));
            object workerData = Activator.CreateInstance(WorkerDataType, info.Types[(int)val].TypeName, dict["Data"]);
            __result = workerData;
        }
        foreach (var thing in data.EnumerateTree())
        {
            if (isResoniteWorker && thing is DataTreeValue dtval)
            {
                if (dtval.IsURL)
                {
                    // change resdb to neosdb so assets from resonite can load with the mod
                    var str = dtval.Value as string;
                    dtval.UpdateValue(new Uri(str.Substring(1).Replace("resdb", "neosdb")));
                }
            }
            if (thing is DataTreeDictionary dtdict)
            {
                // Change all assets to use DirectLoad which helps if certain variants are missing from the cloud
                ForceDirectLoad(dtdict);
            }
        }
        return !isResoniteWorker;
    }

    private static void ForceDirectLoad(DataTreeDictionary dict)
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