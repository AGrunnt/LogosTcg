using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LogosTcg
{
    public class OnlineLobbyManager : MonoBehaviour
    {
        public void LoadPlayBoard()
        {
            NetworkManager.Singleton.SceneManager.LoadScene("PlayBoard", LoadSceneMode.Single);
        }
    }
}
