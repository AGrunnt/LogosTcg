using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LogosTcg
{
    public class DatabaseSetupBase : MonoBehaviour
    {
        // general card info
        public List<CsvRow> dataCdTable = new List<CsvRow>();
        public List<string> dataCdHeaders = new List<string>();
        // abilities sheet
        public List<CsvRow> dataAbTable = new List<CsvRow>();
        public List<string> dataAbHeaders = new List<string>();

        [MenuItem("Tools/Import CSV and Create Cards")]
        public static void ImportSo()
        {
            // your two CSVs
            const string csvAbAssetPath = "Assets/Sets/BaseSet/CardDb - AbilitySht.csv";
            const string csvCdAssetPath = "Assets/Sets/BaseSet/CardDb - CardSht.csv";

            var csvAbAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(csvAbAssetPath);
            var csvCdAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(csvCdAssetPath);
            if (csvAbAsset == null || csvCdAsset == null)
            {
                Debug.LogError("One of your CSVs wasn't found. Check the paths!");
                return;
            }

            // parse both CSVs
            var linesAb = csvAbAsset.text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            var linesCd = csvCdAsset.text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();

            if (linesAb.Length < 2 || linesCd.Length < 2)
            {
                Debug.LogWarning("At least one of the CSVs only has a header or is empty.");
                return;
            }

            // headers
            var headersAb = linesAb[0].Split(',').Select(h => h.Trim()).ToList();
            var headersCd = linesCd[0].Split(',').Select(h => h.Trim()).ToList();

            // rows
            var tableAb = linesAb
                .Skip(1)
                .Select(l => new CsvRow(l.Split(',').Select(c => c.Trim()).ToArray()))
                .ToList();
            var tableCd = linesCd
                .Skip(1)
                .Select(l => new CsvRow(l.Split(',').Select(c => c.Trim()).ToArray()))
                .ToList();

            // set up importer instance
            var go = new GameObject("CSV Importer");
            var importer = go.AddComponent<DatabaseSetupBase>();

            importer.dataAbHeaders = headersAb;
            importer.dataAbTable = tableAb;
            importer.dataCdHeaders = headersCd;
            importer.dataCdTable = tableCd;

            importer.ImportCardsFromDataTable();

            // clean up
            GameObject.DestroyImmediate(go);
        }

#if UNITY_EDITOR
        public void ImportCardsFromDataTable()
        {
            const string artFolder = "Assets/Sets/BaseSet/Images";
            foreach (var row in dataCdTable)
                CreateNewCard(row, artFolder);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void CreateNewCard(CsvRow cdRow, string artFolder)
        {
            // 1) Instantiate the SO
            var card = ScriptableObject.CreateInstance<CardDef>();

            // 2) Map basic CSV columns ? CardDef fields
            card.Id = cdRow[dataCdHeaders.IndexOf("CardId")];
            card.Title = cdRow[dataCdHeaders.IndexOf("Name")];
            card.SetStr = cdRow[dataCdHeaders.IndexOf("Set")];
            card.Rarity = cdRow[dataCdHeaders.IndexOf("Rarity")];
            // initialize Type list
            card.Type = cdRow[dataCdHeaders.IndexOf("Type")].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(s => s.Trim())
                  .ToList();
            card.Tag = cdRow[dataCdHeaders.IndexOf("Tag")].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(s => s.Trim())
                  .ToList();
            int vIdx = dataCdHeaders.IndexOf("Value");
            if (vIdx >= 0 && int.TryParse(cdRow[vIdx], out var val))
                card.Value = val;

            card.AbilityText = cdRow[dataCdHeaders.IndexOf("AbilityText")];
            card.Verse = cdRow[dataCdHeaders.IndexOf("Verse")];
            card.VerseText = cdRow[dataCdHeaders.IndexOf("VerseText")];

            // 3) Find & assign the artwork Sprite
            {
                string id = card.Id;
                string[] guids = AssetDatabase.FindAssets("001" + id, new[] { artFolder });
                if (guids.Length > 0)
                {
                    string imgPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imgPath);
                    if (sprite != null)
                        card.Artwork = sprite;
                    else
                        Debug.LogWarning($"Found asset for {id} at {imgPath}, but it isn’t a Sprite.");
                }
                else
                {
                    Debug.LogWarning($"No image found for card {id} in folder {artFolder}");
                }
            }

            // 4) Gather all matching abilities by CardId
            {
                int abIdIndex = dataAbHeaders.IndexOf("CardId");
                if (abIdIndex >= 0)
                {
                    var matches = dataAbTable
                        .Where(r => r[abIdIndex].Equals(card.Id, StringComparison.OrdinalIgnoreCase));

                    foreach (var abRow in matches)
                    {
                        var ability = new Ability
                        {
                            AbilityType = new List<string>(),
                            Target = new List<string>(),
                            Tag = new List<string>(),
                        };

                        int idx;
                        if ((idx = dataAbHeaders.IndexOf("AbilityType")) >= 0)
                            ability.AbilityType = abRow[idx]
                                .Split('|')
                                .Select(s => s.Trim())
                                .ToList();

                        if ((idx = dataAbHeaders.IndexOf("Target")) >= 0)
                            ability.Target = abRow[idx]
                                .Split('|')
                                .Select(s => s.Trim())
                                .ToList();

                        if ((idx = dataAbHeaders.IndexOf("Tag")) >= 0)
                            ability.Tag = abRow[idx]
                                .Split('|')
                                .Select(s => s.Trim())
                                .ToList();

                        card.Abilities.Add(ability);
                    }
                }
            }

            // 5) Save the ScriptableObject asset
            string assetPath = $"Assets/Sets/BaseSet/ScriptableObjects/001{card.Id}{card.Title}.asset";
            AssetDatabase.CreateAsset(card, assetPath);
        }
#endif


        public class CsvRow
        {
            public List<string> Columns { get; set; }
            public CsvRow(string[] values) => Columns = new List<string>(values);
            public string this[int i] => Columns[i];
        }
    }
}





