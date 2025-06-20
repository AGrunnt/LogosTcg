using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LogosTcg
{
#if UNITY_EDITOR


    public class DotweenSetupLauncher
    {
        [MenuItem("Tools/Run DOTween Setup")]
        public static void RunDotweenSetup()
        {
            EditorApplication.ExecuteMenuItem("Tools/Demigiant/DOTween Utility Panel");
        }
    }
#endif

    public class VisualCardsHandler : MonoBehaviour
    {

        public static VisualCardsHandler instance;

        private void Awake()
        {
            instance = this;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}