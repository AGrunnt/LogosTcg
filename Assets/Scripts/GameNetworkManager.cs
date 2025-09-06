using DG.Tweening;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Multiplay.Authoring.Core.Assets;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace LogosTcg
{
    public class GameNetworkManager : NetworkBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public static GameNetworkManager Instance;
        GameManager gm;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            gm = GameManager.Instance;
        }

        [ServerRpc(RequireOwnership = false)]
        public void MountByNameServerRpc(string objName, string newParent)
        {

            MountByNameClientRpc(objName, newParent);
        }

        [ClientRpc]
        public void MountByNameClientRpc(string objName, string newParentName)
        {

            Transform obj = GameObject
                .FindGameObjectsWithTag("Card")
                .FirstOrDefault(go => go.name == objName).transform;

            
            Transform parentTf = new[] { "Hand", "Slot" }
                .SelectMany(tag => GameObject.FindGameObjectsWithTag(tag))
                .FirstOrDefault(go => go.name == newParentName).transform;
            

            SlotScript oldParentSlot = obj.GetComponentInParent<SlotScript>();

            obj.SetParent(parentTf, worldPositionStays: true);

            oldParentSlot.SetLastCardSettings();
            parentTf.GetComponent<SlotScript>().SetLastCardSettings();

            if (parentTf.GetComponent<LayoutGroup>() == null)
                obj.DOLocalMove(Vector3.zero, .15f).SetEase(Ease.OutBack);


            if (oldParentSlot.slotType == "LocSlot")
                oldParentSlot.GetComponent<GridSlotActions>().shiftLeft();

            parentTf.GetComponent<SlotScript>().OnCardDropped?.Invoke(obj);
        }







        [ServerRpc(RequireOwnership = false)]
        public void CoinDropServerRpc(string cardFaith, string cardOth, int val)
        {
            CoinDropClientRpc(cardFaith, cardOth, val);
        }

        [ClientRpc]
        public void CoinDropClientRpc(string cardFaith, string cardOth, int val)
        {
            OfflineCoinDrop(cardFaith, cardOth, val);
        }

        public void OfflineCoinDrop(string cardFaithStr, string cardOthStr, int val)
        {
            Card cardFaith = GameObject
                .FindGameObjectsWithTag("Card")
                .FirstOrDefault(go => go.name == cardFaithStr).GetComponent<Card>();

            Card cardOth = GameObject
                .FindGameObjectsWithTag("Card")
                .FirstOrDefault(go => go.name == cardOthStr).GetComponent<Card>();


            int overkill = cardOth.SetValue(val + gm.coinModifier);
            cardFaith.SetValue(val - overkill);
            cardFaith.GetComponent<CoinStack>().ReVisible();
        }
    }
}
