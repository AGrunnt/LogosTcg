using UnityEngine;
using UnityEngine.SceneManagement;

namespace LogosTcg
{
    public class MenuManager : MonoBehaviour
    {
        public void LoadOnlineLobby()
        {
            SceneManager.LoadSceneAsync("OnlineLobby", LoadSceneMode.Single);
        }

        public void LoadOfflineLobby()
        {
            SceneManager.LoadSceneAsync("OfflineLobby", LoadSceneMode.Single);
        }
    }
}
