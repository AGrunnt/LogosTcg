using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LogosTcg
{
    public class OfflineLobbyManager : MonoBehaviour
    {
        public TextMeshProUGUI roundNumUi;

        public void LoadPlayBoard()
        {
            StaticData.roundNums = int.Parse(roundNumUi.text);

            SceneManager.LoadSceneAsync("PlayBoard", LoadSceneMode.Single);
        }
    }
}
