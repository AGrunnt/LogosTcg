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

        private void Awake()
        {
            Instance = this;
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
    }
}
