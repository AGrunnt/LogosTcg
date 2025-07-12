using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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
        public void MountServerRpc(string objName, string newParent)
        {

            MountClientRpc(objName, newParent);
        }

        [ClientRpc]
        public void MountClientRpc(string objName, string newParent)
        {
            Transform obj = GameObject.Find(objName).transform;
            Transform parentTf = GameObject.Find(newParent).transform;

            obj.SetParent(parentTf, worldPositionStays: true);

            if (parentTf.GetComponent<HorizontalLayoutGroup>() == null && parentTf.GetComponent<GridLayoutGroup>() == null)
                obj.DOLocalMove(Vector3.zero, .15f).SetEase(Ease.OutBack);
        }
    }
}
