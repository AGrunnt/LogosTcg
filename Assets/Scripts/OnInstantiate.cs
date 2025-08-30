using LogoTcg;
using UnityEngine;
using UnityEngine.UI;

namespace LogosTcg
{
    public class OnInstantiate : MonoBehaviour
    {
        public void OnInstActions(GameObject go)
        {
            if(go.GetComponent<Gobject>() != null)
            {
                go.AddComponent<CardTogAttachment>();
            } else if(go.GetComponent<GobjectVisual>() != null)
            {
                Destroy(go.GetComponent<Canvas>());
                Image[] imgs = go.GetComponentsInChildren<Image>(includeInactive: true);
                foreach(var img in imgs)
                {
                    img.raycastTarget = false;
                }
            }
        }
    }
}
