#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using LogosTcg; // your CardDef/Ability namespace
using System.IO;

public static class CsvToAddressablesOneClick
{
    // --- Customize these as needed ---
    private const string CsvAbilitiesPath = "Assets/Cards/CardDb - AbilitySht.csv";
    private const string CsvCardsPath = "Assets/Cards/CardDb - CardSht.csv";

    // Where to save created ScriptableObject assets:
    private const string OutputSoFolder = "Assets/Cards/SOs";

    // Map CSV "Set" abbreviations (case-insensitive) -> Addressables Group names.
    // Add as many as you need; multiple abbreviations can point to one group.
    private static readonly Dictionary<string, string> SetToGroup = new(StringComparer.OrdinalIgnoreCase)
    {
        { "bas", "BaseSet" },
        { "s1",  "Season1" },
        { "s2",  "Season2" },
        { "adv", "Holiday" },
        { "est", "Holiday" }
        // add more here...
    };

    [MenuItem("Tools/One-Click: CSV ? Scriptables ? Addressables & Labels")]
    public static void RunAll()
    {
        // 1) Load CSVs
        var csvAb = AssetDatabase.LoadAssetAtPath<TextAsset>(CsvAbilitiesPath);
        var csvCd = AssetDatabase.LoadAssetAtPath<TextAsset>(CsvCardsPath);
        if (csvAb == null || csvCd == null)
        {
            Debug.LogError($"Could not find CSVs. Check paths:\n{CsvAbilitiesPath}\n{CsvCardsPath}");
            return;
        }

        var abLines = SplitCsv(csvAb.text);
        var cdLines = SplitCsv(csvCd.text);

        if (abLines.Length < 2 || cdLines.Length < 2)
        {
            Debug.LogWarning("One or both CSVs have no data rows.");
            return;
        }

        var abHeaders = abLines[0].Split(',').Select(h => h.Trim()).ToList();
        var cdHeaders = cdLines[0].Split(',').Select(h => h.Trim()).ToList();

        var abRows = abLines.Skip(1).Select(l => l.Split(',').Select(c => c.Trim()).ToArray()).ToList();
        var cdRows = cdLines.Skip(1).Select(l => l.Split(',').Select(c => c.Trim()).ToArray()).ToList();

        // 2) Prepare Addressables
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressables are not set up (AddressableAssetSettingsDefaultObject.Settings is null).");
            return;
        }

        EnsureFolder(OutputSoFolder);

        var createdOrUpdated = new List<string>();

        // 3) Create/Update ScriptableObjects, then make them Addressable + label them
        foreach (var row in cdRows)
        {
            var card = CreateOrUpdateCardDef(row, cdHeaders, abRows, abHeaders, out var assetPath);

            // mark/move to Addressables group
            var groupName = ResolveGroupName(card.SetStr);
            var group = settings.FindGroup(groupName) ?? settings.CreateGroup(groupName, false, false, true, null);

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.CreateOrMoveEntry(guid, group);

            // Labels: "Card", group name, and each Type value (distinct)
            EnsureLabel(settings, "Card");
            entry.SetLabel("Card", true, true);

            EnsureLabel(settings, groupName);
            entry.SetLabel(groupName, true, true);

            if (card.Type != null)
            {
                foreach (var typeLabel in card.Type.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(typeLabel)) continue;
                    EnsureLabel(settings, typeLabel);
                    entry.SetLabel(typeLabel, true, true);
                }
            }

            createdOrUpdated.Add(assetPath);
        }

        // 4) Save
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Done. Created/updated {createdOrUpdated.Count} CardDef assets and Addressables.\n" +
                  string.Join("\n", createdOrUpdated.Select(p => $" - {p}")));
    }

    // ---------------- helpers ----------------

    private static string[] SplitCsv(string text) =>
        text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();

    private static void EnsureFolder(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            var parts = folderPath.Split('/');
            var path = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{path}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(path, parts[i]);
                path = next;
            }
        }
    }

    private static string ResolveGroupName(string setStr)
    {
        if (string.IsNullOrWhiteSpace(setStr)) return "Ungrouped";
        if (SetToGroup.TryGetValue(setStr.Trim(), out var group)) return group;
        // Fallback: title-case the set string or use a default
        return setStr.Trim();
    }

    private static void EnsureLabel(AddressableAssetSettings settings, string label)
    {
        if (string.IsNullOrWhiteSpace(label)) return;
        var labels = settings.GetLabels();
        if (!labels.Contains(label))
        {
            settings.AddLabel(label);
        }
    }

    /// <summary>
    /// Creates or updates a CardDef asset from CSV rows, then returns the instance and its asset path.
    /// </summary>
    private static CardDef CreateOrUpdateCardDef(
        string[] cdRow,
        List<string> cdHeaders,
        List<string[]> abRows,
        List<string> abHeaders,
        out string assetPath)
    {
        // build safe file name
        string setStr = Get(cdHeaders, cdRow, "Set");
        string id = Get(cdHeaders, cdRow, "CardId");
        string title = Get(cdHeaders, cdRow, "Name");

        string fileName = $"{Sanitize(setStr)}{Sanitize(id)} - {Sanitize(title)}.asset";
        assetPath = $"{OutputSoFolder}/{fileName}"; //001001 - Adam.png

        var existing = AssetDatabase.LoadAssetAtPath<CardDef>(assetPath);
        CardDef card = existing != null ? existing : ScriptableObject.CreateInstance<CardDef>();

        // --- basic fields ---
        card.Id = id;
        card.Title = title;
        card.SetStr = setStr;
        card.Rarity = Get(cdHeaders, cdRow, "Rarity");
        card.AbilityText = Get(cdHeaders, cdRow, "AbilityText");
        card.Verse = Get(cdHeaders, cdRow, "Verse");
        card.VerseText = Get(cdHeaders, cdRow, "VerseText");

        // lists
        card.Type = SplitList(Get(cdHeaders, cdRow, "Type"));
        card.Tag = SplitList(Get(cdHeaders, cdRow, "Tag"));

        // value
        if (int.TryParse(Get(cdHeaders, cdRow, "Value"), out var val)) card.Value = val;

        // artwork (optional – adjust naming if needed)
        TryAssignArtwork(card);

        // abilities by CardId
        RebuildAbilities(card, abRows, abHeaders);

        if (existing == null)
            AssetDatabase.CreateAsset(card, assetPath);
        else
            EditorUtility.SetDirty(card);

        return card;
    }

    private static string Get(List<string> headers, string[] row, string name)
    {
        int idx = headers.IndexOf(name);
        return (idx >= 0 && idx < row.Length) ? row[idx] : string.Empty;
    }

    private static List<string> SplitList(string raw)
        => string.IsNullOrWhiteSpace(raw)
            ? new List<string>()
            : raw.Split('|').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();

    private static string Sanitize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "NA";
        foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            s = s.Replace(c, '_');
        return s;
    }

    private static void TryAssignArtwork(CardDef card)
    {
        // Example: your originals searched "Assets/Sets/BaseSet/Images" for name starting with "001" + id.
        // Tweak this to your actual art naming convention/location if needed.
        const string artFolder = "Assets/Cards/Images";
        var guids = AssetDatabase.FindAssets(card.SetStr + card.Id, new[] { artFolder });
        if (guids.Length == 0) return;

        string imgPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imgPath);
        if (sprite != null) card.Artwork = sprite;
    }

    private static void RebuildAbilities(CardDef card, List<string[]> abRows, List<string> abHeaders)
    {
        card.Abilities.Clear();

        int abIdIndex = abHeaders.IndexOf("CardId");
        if (abIdIndex < 0) return;

        foreach (var abRow in abRows.Where(r => string.Equals(r[abIdIndex], card.Id, StringComparison.OrdinalIgnoreCase)))
        {
            var ability = new Ability
            {
                AbilityType = SplitList(Get(abHeaders, abRow, "AbilityType")),
                Target = SplitList(Get(abHeaders, abRow, "Target")),
                Tag = SplitList(Get(abHeaders, abRow, "Tag"))
            };
            card.Abilities.Add(ability);
        }
    }

    private const string TargetFolder = "Assets/Cards/Images";
    private const string NewPrefix = "bas";
    private const bool IncludeSubfolders = false; // set true to include subfolders

    [MenuItem("Tools/Rename PNGs in Cards/Images (first 3 ? 'bas')")]
    public static void RenamePngs()
    {
        if (!AssetDatabase.IsValidFolder(TargetFolder))
        {
            Debug.LogError($"Folder not found: {TargetFolder}");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { TargetFolder });

        int renamed = 0, skipped = 0, errors = 0;

        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                {
                    skipped++;
                    continue;
                }

                // Only files directly in TargetFolder (exclude subfolders unless toggled)
                if (!IncludeSubfolders)
                {
                    string dir = Path.GetDirectoryName(path)?.Replace('\\', '/');
                    if (!string.Equals(dir, TargetFolder, System.StringComparison.Ordinal))
                    {
                        skipped++;
                        continue;
                    }
                }

                string nameNoExt = Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrEmpty(nameNoExt) || nameNoExt.Length < 3)
                {
                    skipped++;
                    continue;
                }

                if (nameNoExt.StartsWith(NewPrefix))
                {
                    skipped++;
                    continue;
                }

                string newNameNoExt = NewPrefix + nameNoExt.Substring(3);

                EditorUtility.DisplayProgressBar(
                    "Renaming PNGs in Assets/Cards/Images",
                    $"{nameNoExt}.png ? {newNameNoExt}.png",
                    (float)i / Mathf.Max(1, guids.Length - 1)
                );

                string err = AssetDatabase.RenameAsset(path, newNameNoExt);
                if (!string.IsNullOrEmpty(err))
                {
                    errors++;
                    Debug.LogError($"Failed to rename '{path}': {err}");
                }
                else
                {
                    renamed++;
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"Done. Renamed: {renamed}, Skipped: {skipped}, Errors: {errors}");
    }
}
#endif
