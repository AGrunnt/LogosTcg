using UnityEngine;
using UnityEngine.SceneManagement;

namespace LogosTcg
{
    public class OnlineLobbyManager : MonoBehaviour
    {
        public void LoadPlayBoard()
        {
            SceneManager.LoadSceneAsync("PlayBoard", LoadSceneMode.Single);
        }
    }
}
