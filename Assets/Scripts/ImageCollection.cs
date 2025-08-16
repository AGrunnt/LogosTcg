using LogoTcg;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace LogosTcg
{
    public class ImageCollection : MonoBehaviour
    {
        public static ImageCollection instance;

        void Awake() => instance = this;

        public Sprite rarityCommon;
        public Sprite rarityUncommon;
        public Sprite rarityRare;
        public Sprite boxBckLocLeaf;
        public Sprite boxBckLocMnt;
        public Sprite boxBckLocWave;
        public Sprite dimBckLocLeaf;
        public Sprite dimBckLocMnt;
        public Sprite dimBckLocWave;
        public Sprite IconLeaf;
        public Sprite IconMnt;
        public Sprite IconWave;
        public Sprite IconDove;
        public Sprite IconSnake;
        public Sprite IconTrap;
        public Sprite setBase;
    }
}
