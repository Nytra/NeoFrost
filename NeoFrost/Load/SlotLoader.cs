using System;
using System.Collections.Generic;
using System.Linq;
using BaseX;
using FrooxEngine;

namespace NeoFrost.Load;

public static class SlotLoader
{
    // public static Dictionary<Slot, SlotExtraInfo> SlotStore = new();
    public static readonly List<SlotExtraInfo> SlotStore = []; 
    
    public static void LoadObjectExtras(Slot slot, DataTreeDictionary node)
    {
        DataTreeDictionary? featureFlags = node.TryGetDictionary("FeatureFlags");
        if (featureFlags == null)
            return;
        
        // DumpObject(node);

        SlotExtraInfo info = new(node, featureFlags);
        // SlotStore[slot] = info;
        SlotStore.Add(info);

        UniLog.Log($"{info.FeatureFlags.Count} Feature flags: {string.Join(", ", info.FeatureFlags.Select(f => $"{f.Key}={f.Value}"))}");

        if (!info.LegacyTypes)
        {
            LoadResoniteTypes(slot, info, node);
        }
    }

    private static void LoadResoniteTypes(Slot slot, SlotExtraInfo info, DataTreeDictionary node)
    {
        DataTreeList types = node.TryGetList("Types");
        // DataTreeDictionary versions = node.TryGetDictionary("TypeVersions");

        foreach (DataTreeNode typeNode in types)
        {
            try
            {
                var rawStr = typeNode.LoadString();
                Type? type = WorkerManager.GetType(rawStr);

                if (type == null)
                    UniLog.Warning("Failed to decode type: " + rawStr);

                TypeData data = new(type, rawStr);
                info.Types.Add(data);
                UniLog.Log(data);
            }
            catch (Exception ex)
            {
                UniLog.Log($"Exception in type parsing:\n{ex}");
            }
        }
        
        // foreach (KeyValuePair<string, DataTreeNode> kvp in versions.Children)
        // {
        //     
        // }
    }

    public static void DumpObject(DataTreeNode node)
    {
        foreach (DataTreeNode dataTreeNode in node.EnumerateTree())
        {
            UniLog.Log(dataTreeNode.ToString());
            DumpObject(dataTreeNode);
        }
    }
}