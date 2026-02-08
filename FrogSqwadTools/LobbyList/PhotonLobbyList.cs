using FrogSqwadTools.LobbyList.Tabs;
using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrogSqwadTools.LobbyList
{
    internal class PhotonLobbyList : INetworkRunnerCallbacks
    {
        internal static event Action OnConnectBegin;
        internal static event Action<bool, string> OnConnectEnd;

        NetworkRunner Runner;
        GlobalListTab Owner;
        PhotonAppSettings Settings;
        bool _canConnect = true;
        internal async Task Init(GlobalListTab owner)
        {
            Owner = owner;
        
            while (SceneManager.GetActiveScene().name != "Main Menu") //am i retarded?
                await Task.Delay(100);

            Settings = Resources.FindObjectsOfTypeAll<PhotonAppSettings>().FirstOrDefault();

            await BeginConnect();
        }

        internal async Task BeginConnect(string region = null)
        {
            if (!_canConnect) return;

            var hasRegion = !string.IsNullOrEmpty(region);
            Plugin.Logger.LogInfo(string.Format("Connecting to photon lobby on region \"{0}\"...", hasRegion ? region : "auto"));
            OnConnectBegin?.Invoke();

            try
            {
                _canConnect = false;
                LobbyListManager.Instance.RegionDropdown.interactable = false;

                if (Runner != null)
                {
                    await TerminateConnection();
                    await Task.Yield();
                }

                var lbr = new GameObject("LobbyRunner");
                lbr.transform.SetParent(Owner.Owner.transform);
                Runner = lbr.AddComponent<NetworkRunner>();
                Runner.AddCallbacks(this);

                var regs = await NetworkRunner.GetAvailableRegions();
                NetworkManager.Instance._allRegions = regs;

                if (!hasRegion)
                {
                    var bestReg = regs.Where(r => r.RegionPing > 0).OrderBy(r => r.RegionPing).FirstOrDefault();
                    Plugin.Logger.LogInfo($"Best region to connect is \"{bestReg.RegionCode}\" with {bestReg.RegionPing}ms ping");
                    region = bestReg.RegionCode;
                }

                Settings.AppSettings.FixedRegion = region;

                var auth = await NetworkManager.Instance.GetAuthenticationAsync();
                var res = await Runner.JoinSessionLobby(SessionLobby.ClientServer, authentication: auth);

                if (res.Ok)
                    Plugin.Logger.LogInfo("Connected to photon lobby");
                else
                    ErrorManager.Instance.ShowDialog($"Failed to connect to lobby list (tried to connect to: {region})");

                OnConnectEnd?.Invoke(res.Ok, region);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogInfo($"Connect attempt failed\n{e}");
                OnConnectEnd?.Invoke(false, null);
            }
            finally
            {
                _canConnect = true;
                LobbyListManager.Instance.RegionDropdown.interactable = true;
            }
        }

        internal async Task TerminateConnection()
        {
            await Runner.Shutdown();
            GameObject.Destroy(Runner);
            Runner = null;
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) => Owner.UpdateList(sessionList);

        public void OnConnectedToServer(NetworkRunner runner)
        {
            
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
            
        }
    }
}
