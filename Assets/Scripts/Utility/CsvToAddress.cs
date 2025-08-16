// Assets/Editor/CardImport_BySetCode.cs
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace LogosTcg
{
    /// One-click pipeline:
    /// 1) CSV -> ScriptableObjects   2) Mark Addressable
    /// 3) Put into group resolved from set CODE (supports many->one)  4) Label ("Card", Types, set code, group)
    public static class CardImport_BySetCode
    {
        // Search anywhere under Assets/Sets so BaseSet / Season1 / Season2 etc. are covered
        private static readonly string[] SearchFolders = { "Assets/Sets" };

        // === EDIT THIS MAP ===
        // Lower-case keys recommended; mapping supports many codes pointing to one group.
        // Example:
        //   "bas" -> "BaseSet"
        //   "adv" -> "Holiday"
        //   "est" -> "Holiday"
        private static readonly Dictionary<string, string> SetCodeToGroupMap = new Dictionary<string, string>
        {
            { "bas", "BaseSet" },
            { "adv", "Holiday" },
            { "est", "Holiday" }
            // add more here...
        };

        [MenuItem("Tools/Import Cards (CSV ? Addressables + Labels, by Set Code)")]
        public static void ImportMakeAddressableAndLabel_BySetCode()
        {
            try
            {
                // 1) Your CSV -> ScriptableObjects
                DatabaseSetupBase.ImportSo();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 2) Addressables settings must exist
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null)
                {
                    Debug.LogError("Addressables not configured. Open 'Window > Asset Management > Addressables' to create settings.");
                    return;
                }

                // 3) Find all CardDef assets created
                var cardGuids = AssetDatabase.FindAssets("t:CardDef", SearchFolders);
                if (cardGuids == null || cardGuids.Length == 0)
                {
                    Debug.LogWarning("No CardDef assets found under Assets/Sets.");
                    return;
                }

                int newAddressables = 0, movedGroups = 0, relabeled = 0;

                foreach (var guid in cardGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                    if (obj == null)
                        continue;

                    // ---- read set CODE from the CardDef (supports Set / SetStr / SetName) ----
                    var setCode = ReadStringProp(obj, "Set") ??
                                  ReadStringProp(obj, "SetStr") ??
                                  ReadStringProp(obj, "SetName");

                    // If no explicit property, attempt to infer from folder path: Assets/Sets/<something>/...
                    if (string.IsNullOrWhiteSpace(setCode))
                        setCode = InferSetCodeFromPath(path);

                    // Normalize to lower for map lookup
                    var setCodeKey = (setCode ?? "").Trim().ToLowerInvariant();

                    // Resolve the Addressables group name using the mapping; fall back to the code itself if unknown
                    var targetGroupName = ResolveGroupName(setCodeKey);

                    // ---- ensure/create the target group ----
                    var group = settings.FindGroup(targetGroupName);
                    if (group == null)
                    {
                        group = settings.CreateGroup(
                            targetGroupName,
                            setAsDefaultGroup: false,
                            readOnly: false,
                            postEvent: true,
                            schemasToCopy: null,
                            types: new[]
                            {
                                typeof(BundledAssetGroupSchema),
                                typeof(ContentUpdateGroupSchema)
                            }
                        );

                        var bundle = group.GetSchema<BundledAssetGroupSchema>();
                        if (bundle != null) bundle.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
                    }

                    // ---- ensure Addressable entry and move to proper group ----
                    var entry = settings.FindAssetEntry(guid);
                    if (entry == null)
                    {
                        entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: true);
                        entry.address = System.IO.Path.GetFileNameWithoutExtension(path);
                        newAddressables++;
                    }
                    else if (entry.parentGroup != group)
                    {
                        settings.MoveEntry(entry, group);
                        movedGroups++;
                    }

                    // ---- Labels: "Card", Types, set code, group name ----
                    bool changed = false;

                    if (!entry.labels.Contains("Card"))
                    {
                        entry.SetLabel("Card", true, true);
                        changed = true;
                    }

                    // Add all card "Type" strings (supports List<string> Type / Types / Labels / Tags)
                    foreach (var t in ReadStringArray(obj, "Type", "Types", "Labels", "Tags").Where(s => !string.IsNullOrWhiteSpace(s)).Distinct())
                    {
                        if (!entry.labels.Contains(t))
                        {
                            entry.SetLabel(t, true, true);
                            changed = true;
                        }
                    }

                    // set code label (e.g., "bas")
                    if (!string.IsNullOrWhiteSpace(setCode) && !entry.labels.Contains(setCode))
                    {
                        entry.SetLabel(setCode, true, true);
                        changed = true;
                    }

                    // resolved group name label (e.g., "BaseSet" / "Holiday")
                    if (!entry.labels.Contains(targetGroupName))
                    {
                        entry.SetLabel(targetGroupName, true, true);
                        changed = true;
                    }

                    if (changed) relabeled++;
                }

                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Done. New addressables: {newAddressables}, moved groups: {movedGroups}, relabeled: {relabeled}.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Import/Addressables/Labels failed: {ex}");
            }
        }

        // ----------------- helpers -----------------
        private static string ResolveGroupName(string setCodeLower)
        {
            if (!string.IsNullOrEmpty(setCodeLower) && SetCodeToGroupMap.TryGetValue(setCodeLower, out var group))
                return group;

            // Fallbacks:
            // - If code looks like "baseset" or "season1", use a TitleCase-ish variant
            // - Otherwise just use the code itself (Addressables will create that group)
            if (string.IsNullOrWhiteSpace(setCodeLower)) return "Ungrouped";
            return GuessTitle(setCodeLower);
        }

        private static string GuessTitle(string s)
        {
            // very small beautifier: "baseset" -> "BaseSet", "season1" -> "Season1"
            if (string.IsNullOrWhiteSpace(s)) return s;
            var parts = s.Split(new[] { '_', '-', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p.Substring(1) : "")));
        }

        private static string InferSetCodeFromPath(string assetPath)
        {
            // Assets/Sets/<something>/ScriptableObjects/... -> "<something>"
            var parts = assetPath.Replace('\\', '/').Split('/');
            var idx = System.Array.IndexOf(parts, "Sets");
            if (idx >= 0 && idx + 1 < parts.Length)
                return parts[idx + 1];
            return null;
        }

        private static string ReadStringProp(Object obj, string name)
        {
            var so = new SerializedObject(obj);
            var sp = so.FindProperty(name);
            return (sp != null && sp.propertyType == SerializedPropertyType.String) ? sp.stringValue : null;
        }

        private static IEnumerable<string> ReadStringArray(Object obj, params string[] candidateNames)
        {
            var so = new SerializedObject(obj);
            foreach (var name in candidateNames)
            {
                var sp = so.FindProperty(name);
                if (sp == null) continue;

                if (sp.isArray && sp.propertyType != SerializedPropertyType.String)
                {
                    for (int i = 0; i < sp.arraySize; i++)
                    {
                        var elem = sp.GetArrayElementAtIndex(i);
                        if (elem.propertyType == SerializedPropertyType.String && !string.IsNullOrWhiteSpace(elem.stringValue))
                            yield return elem.stringValue;
                    }
                    yield break;
                }
                if (sp.propertyType == SerializedPropertyType.String && !string.IsNullOrWhiteSpace(sp.stringValue))
                {
                    yield return sp.stringValue;
                    yield break;
                }
            }
        }
    }
}
