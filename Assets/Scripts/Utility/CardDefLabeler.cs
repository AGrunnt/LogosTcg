// Assets/Editor/CardDefLabeler.cs
using LogosTcg;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace LogosTcg
{
    public static class CardDefLabeler
    {
        private const string SearchFolder = "Assets/Sets/BaseSet/ScriptableObjects";

        [MenuItem("Tools/Apply CardDef Type & Group Labels")]
        public static void ApplyLabels()
        {
            // 1) Find all CardDef assets in the folder
            var guids = AssetDatabase.FindAssets("t:CardDef", new[] { SearchFolder });
            if (guids.Length == 0)
            {
                Debug.LogWarning($"No CardDef assets found under {SearchFolder}");
                return;
            }

            // 2) Get the Addressables settings
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Could not find AddressableAssetSettings. Make sure you’ve set up Addressables.");
                return;
            }

            // 3) For each CardDef, add its types + its group name as addressable labels
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var card = AssetDatabase.LoadAssetAtPath<CardDef>(assetPath);
                if (card == null)
                    continue;

                var entry = settings.FindAssetEntry(guid);
                if (entry == null)
                {
                    Debug.LogWarning($"'{assetPath}' is not marked as addressable. Skipping.");
                    continue;
                }

                // a) Add each distinct type label
                foreach (var typeLabel in card.Type.Distinct())
                {
                    if (!entry.labels.Contains(typeLabel))
                    {
                        entry.SetLabel(typeLabel, true, true);
                        Debug.Log($"Added type label '{typeLabel}' to {assetPath}");
                    }
                    else
                    {
                        Debug.Log($"{assetPath} already has type label '{typeLabel}'");
                    }
                }

                // b) Add the Addressables group name as a label
                var groupName = entry.parentGroup.Name;
                if (!entry.labels.Contains(groupName))
                {
                    entry.SetLabel(groupName, true, true);
                    Debug.Log($"Added group label '{groupName}' to {assetPath}");
                }
                else
                {
                    Debug.Log($"{assetPath} already has group label '{groupName}'");
                }
            }

            // 4) Save any changes
            settings.SetDirty(
                AddressableAssetSettings.ModificationEvent.EntryModified,
                null,
                true);
            AssetDatabase.SaveAssets();
            Debug.Log("Done: Applied CardDef Type & Group labels.");
        }
    }
}
