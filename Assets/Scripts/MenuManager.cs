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
            StaticData.seedNum = Random.Range(-2000000000, 2000000000);
            SceneManager.LoadSceneAsync("OfflineLobby", LoadSceneMode.Single);
        }
    }
}
