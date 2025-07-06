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

    public class VisualHandler : MonoBehaviour
    {

        public static VisualHandler instance;

        private void Awake()
        {
            instance = this;
        }

    }
}