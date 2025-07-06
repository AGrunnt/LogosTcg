using UnityEngine;
using UnityEngine.SceneManagement;

namespace LogosTcg
{
    public class OfflineLobbyManager : MonoBehaviour
    {
        public void LoadPlayBoard()
        {
            SceneManager.LoadSceneAsync("PlayBoard", LoadSceneMode.Single);
        }
    }
}
