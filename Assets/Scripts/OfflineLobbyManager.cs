using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LogosTcg
{
    public class OfflineLobbyManager : MonoBehaviour
    {
        public TextMeshProUGUI playerNumUi;

        public void LoadPlayBoard()
        {
            StaticData.playerNums = int.Parse(playerNumUi.text);

            SceneManager.LoadSceneAsync("PlayBoard", LoadSceneMode.Single);
        }
    }
}
