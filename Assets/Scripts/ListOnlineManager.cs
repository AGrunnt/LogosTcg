using System.Collections;
using System.Security.Cryptography;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LogosTcg
{
    public class ListOnlineManager : NetworkBehaviour
    {
        DeckSceneManager dsm;
        ListManager lm;
        LoadingCards lc;

        void Start()
        {
            dsm = DeckSceneManager.instance;
            lm = ListManager.instance;
            lc = LoadingCards.instance;
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddToOnlineListServerRpc(string key, int player, int listType)
        {
            AddToOnlineListClientRpc(key, player, listType);
        }

        [ClientRpc]
        public void AddToOnlineListClientRpc(string key, int player, int listType)
        {
            RemoveCardFromGridIfPresent(key);

            // 2) mark it assigned so loader never spawns it
            LoadingCards.instance.listAssigned.Add(key);

            Transform parent = listType == 0
                ? dsm.faithfulListTf[player]
                : listType == 1
                    ? dsm.locationListTf
                    : dsm.encounterListTf;

            var lineGO = Instantiate(lm.cardLinePrefab, parent);
            var line = lineGO.GetComponent<CardLine>();
            line.addressableKey = key;

            // 3) load or reuse the CardDef handle safely
            if (!LoadingCards.instance.loadedAssets.TryGetValue(key, out var handle))
            {
                // client never loaded this key, so start loading now
                handle = Addressables.LoadAssetAsync<CardDef>(key);
                LoadingCards.instance.loadedAssets[key] = handle;
            }

            // 4) when the handle completes (or is already done), apply the definition
            if (handle.IsDone && handle.Status == AsyncOperationStatus.Succeeded)
            {
                line.cardDef = handle.Result;
                line.Apply();
            }
            else
            {
                handle.Completed += op =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        line.cardDef = op.Result;
                        line.Apply();
                    }
                    else
                    {
                        Debug.LogError($"Failed to load CardDef for key {key}");
                    }
                };
            }
            // 4) update UI stats if it was a faithful
            if (listType == 0 && player == dsm.currPlayer)
                StartCoroutine(UpdateFaithfulDelayed());
        }

        IEnumerator UpdateFaithfulDelayed()
        {
            yield return new WaitForSeconds(1.0f);
            dsm.UpdateFaithfulStats();
        }

        public void RemoveCardFromGridIfPresent(string key)
        {
            // do we have a grid?spawned GameObject for this key?
            if (lc.gridItems.TryGetValue(key, out var go)
                && go.transform.IsChildOf(lc.cardGridTf))
            {
                // destroy the UI element
                Destroy(go);
                // remove from our lookup
                lc.gridItems.Remove(key);
                // release & forget the asset handle
                Addressables.Release(lc.loadedAssets[key]);
                lc.loadedAssets.Remove(key);
            }
        }


        

        [ServerRpc(RequireOwnership = false)]
        public void RemoveFromOnlineListServerRpc(string addressableKey, int listType, int playerIndex)
        {
            RemoveFromOnlineListClientRpc(addressableKey, listType, playerIndex);
        }

        [ClientRpc]
        void RemoveFromOnlineListClientRpc(string addressableKey, int listType, int playerIndex)
        {
            // un?assign
            lc.listAssigned.Remove(addressableKey);

            // respawn grid card
            var cd = lc.loadedAssets[addressableKey].Result;
            lc.AddCardToGrid(addressableKey, cd);

            RemoveFromListIfPresent(addressableKey, listType, playerIndex);

            // update faithful stats if needed
            if (listType == 0 && playerIndex == dsm.currPlayer)
                dsm.UpdateFaithfulStats();
        }

        void RemoveFromListIfPresent(string key, int listType, int playerIndex)
        {
            Transform parent =
                listType == 0 ? dsm.faithfulListTf[playerIndex]
              : listType == 1 ? dsm.locationListTf
              : dsm.encounterListTf;

            // look for any CardLine under the target parent with this key
            foreach (var line in parent.GetComponentsInChildren<CardLine>())
            {
                if (line.addressableKey == key)
                {
                    Destroy(line.gameObject);
                    break;
                }
            }

                // release & forget the asset handle
                //Addressables.Release(lc.loadedAssets[key]);
                //lc.loadedAssets.Remove(key);
            
        }
    }
}
