using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using System;

namespace LogosTcg
{
    public class FilterLabels : MonoBehaviour
    {
        public TextMeshProUGUI primLabs;
        public TextMeshProUGUI secLabs;

        public List<string> primLabels = new();
        public List<string> secLabels = new();

        public static FilterLabels instance;
        CardLoader cl;
        GridManager gm;


        void Awake() => instance = this;


        // ?? Consumers (GridManager) subscribe to this
        public event Action FiltersChanged;


        /// <summary>Call this from your UI toggle handlers after you change any filter state.</summary>
        public void RaiseFiltersChanged()
        {
            FiltersChanged?.Invoke();
        }

        void Start()
        {
            cl = CardLoader.instance;
            gm = GridManager.instance;
        }

        public async void togglePrimLabel(string label)
        {
            if (primLabels.Contains(label)) primLabels.Remove(label);
            else primLabels.Add(label);
            //await gm.RefreshGridAsync();
            await gm.RefreshGridAsync();
            UpdateLabelDisplay();
        }

        public async void toggleSecLabel(string label)
        {
            if (secLabels.Contains(label)) secLabels.Remove(label);
            else secLabels.Add(label);
            await gm.RefreshGridAsync();
            UpdateLabelDisplay();
        }

        void UpdateLabelDisplay()
        {
            primLabs.text = primLabels.Count > 0 ? string.Join("\n", primLabels) : "<none>";
            secLabs.text = secLabels.Count > 0 ? string.Join("\n", secLabels) : "<none>";
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        public async Task<IList<IResourceLocation>> GetFilteredLocationsAsync()
        {
            IList<IResourceLocation> primLocs = null, secLocs = null;
            bool hasPrim = primLabels?.Count > 0;
            bool hasSec = secLabels?.Count > 0;

            if (!hasPrim && !hasSec)
            {
                var allHandle = Addressables.LoadResourceLocationsAsync(new[] { "Card" }, Addressables.MergeMode.Union);
                await allHandle.Task;
                var allLocations = allHandle.Result;
                Addressables.Release(allHandle);
                return allLocations;
            }

            if (hasPrim)
            {
                var primHandle = Addressables.LoadResourceLocationsAsync(primLabels.ToArray(), Addressables.MergeMode.Union);
                await primHandle.Task;
                primLocs = primHandle.Result;
                Addressables.Release(primHandle);
            }

            if (hasSec)
            {
                var secHandle = Addressables.LoadResourceLocationsAsync(secLabels.ToArray(), Addressables.MergeMode.Union);
                await secHandle.Task;
                secLocs = secHandle.Result;
                Addressables.Release(secHandle);
            }

            return (hasPrim && hasSec) ? primLocs.Intersect(secLocs, new ResourceLocationComparer()).ToList() : primLocs ?? secLocs;
        }
    }
}
