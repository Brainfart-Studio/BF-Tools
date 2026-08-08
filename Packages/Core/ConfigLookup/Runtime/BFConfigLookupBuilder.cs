using System;
using System.Collections.Generic;
using UnityEngine;
using BFTools.Core.Logger;

namespace BFTools.Core.ConfigLookup
{
    public static class BFConfigLookupBuilder
    {
        public static Dictionary<string, TEntry> Merge<TEntry>(
            IEnumerable<TEntry> entries,
            Func<TEntry, string> keySelector,
            string logTag,
            string ownerName,
            string keyLabel = "eventName",
            UnityEngine.Object context = null)
        {
            Dictionary<string, TEntry> lookup = new Dictionary<string, TEntry>();
            foreach (TEntry entry in entries)
            {
                string key = keySelector(entry);
                if (lookup.ContainsKey(key))
                {
                    BFLogger.Warning(logTag,
                        $"Duplicate {keyLabel} '{key}' across assigned configs on '{ownerName}'. Last one wins.",
                        context);
                }
                lookup[key] = entry;
            }

            BFLogger.Debug(logTag, $"Built lookup with {lookup.Count} entrie(s) on '{ownerName}'.", context);
            return lookup;
        }
    }
}