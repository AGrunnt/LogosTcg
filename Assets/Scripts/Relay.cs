using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;

namespace LogosTcg
{
    public class Relay : MonoBehaviour
    {
        NetworkManager networkManager;


        public TMP_InputField joinCodeEntry;
        public TMP_InputField gameCodeDisp;



        public async void HostGame()
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            var serverData = AllocationUtils.ToRelayServerData(allocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);
            var newJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            gameCodeDisp.text = NetworkManager.Singleton.StartHost() ? newJoinCode : null;
            
        }

        public async void JoinGame()
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            string joinStr = joinCodeEntry.text.Substring(0, 6);
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinStr);
            var serverData = AllocationUtils.ToRelayServerData(joinAllocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);

            NetworkManager.Singleton.StartClient();
        }
    }
}
