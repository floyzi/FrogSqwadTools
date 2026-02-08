using FrogSqwad.SFX;
using FrogSqwad.UI;
using FrogSqwadTools.LobbyList.Core;
using FS_LobbyList_Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static FS_LobbyList_Protocol.LobbyUpdateRequest;
using static LobbyManager;
using static UnityEngine.UI.GridLayoutGroup;

namespace FrogSqwadTools.LobbyList.Tabs
{
    [Obsolete("Replaced by global list")]
    internal class CustomListTab : LobbyListTab
    {
        ClientWebSocket ListSocket;
        CancellationTokenSource TokenSource;
        readonly Dictionary<Guid, TaskCompletionSource<Message>> PendingResponses = [];
        readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);
        LobbyInfo CurrentlyIn;
        int LobbyDay;
        int Lives;
        bool IsOwnedLobbyVisible;

        public CustomListTab(ScrollRect owner, Button refrBtn, GameObject lobby) : base(owner, refrBtn, lobby)
        {
            Task.Run(async () =>
            {
                ListSocket = new();
                TokenSource = new();

                ConnectionFailedTxt.gameObject.SetActive(true);

                ListSocket.Options.SetRequestHeader("app-ver", Application.version);
                ListSocket.Options.SetRequestHeader("mod-ver", Plugin.BuildDetails.Version);

#if !LOCALHOST
                await ListSocket.ConnectAsync(new("ws://85.192.49.206:10090/ws"), TokenSource.Token);
#else
                await ListSocket.ConnectAsync(new("ws://127.0.0.1:10090/ws"), TokenSource.Token);
#endif

                if (ListSocket.State == WebSocketState.Open)
                {
                    Plugin.Logger.LogInfo("Connected to lobby list");
                    ConnectionFailedTxt.gameObject.SetActive(false);
                    _ = Task.Run(Receive);

                    RefreshRequest(true);
                }
                else
                {
                    Plugin.Logger.LogInfo("Failed to connect to lobby list");
                }
            });

            EventSystem.Instance.Register<LobbyUpdateEvent>(OnLobbyInfoReceived);
        }

        async Task Receive()
        {
            var buffer = new byte[1024 * 256];

            while (!TokenSource.IsCancellationRequested && ListSocket.State == WebSocketState.Open)
            {
                var result = await ListSocket.ReceiveAsync(new ArraySegment<byte>(buffer), TokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                try
                {
                    var msg = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    Plugin.Logger.LogInfo($"<-- {msg.Type}\n{JsonConvert.SerializeObject(msg)}");

                    if (PendingResponses.TryGetValue(msg.RequestID, out var tcs))
                    {
                        tcs.TrySetResult(msg);
                        PendingResponses.Remove(msg.RequestID);
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError($"Received corrupted message!\n{ex}");
                }
            }

            ConnectionFailedTxt.gameObject.SetActive(true);
        }

        void OnLobbyInfoReceived(LobbyUpdateEvent evt)
        {
            LobbyDay = evt.CurrentDay;
            Lives = evt.Lives;

            UpdateLobbyInfo();
        }

        internal override void RefreshRequest(bool silent)
        {
            RefreshBtn.interactable = false;

            LoadingTxt.gameObject.SetActive(true);

            _ = Send(new(Message.MessageType.RefreshRequest, Message.OperationType.Request, null), new(res =>
            {
                RefreshBtn.interactable = true;

                if (res.Type != Message.MessageType.LobbyList) return;

                var listArray = (JArray)res.Payload;
                RefreshList(listArray.ToObject<List<LobbyInfo>>());
                LoadingTxt.gameObject.SetActive(false);
                if (!silent) SFXSystem.Instance.PlayUI(SFXType.UIConfirm);
            }), new(() =>
            {
                RefreshBtn.interactable = true;
            }));
        }

        internal void ToggleLobbyState(CustomButton owner)
        {
            var netMan = NetworkManager.Instance;
            if (!netMan.Runner.IsServer) return;

            IsOwnedLobbyVisible = !IsOwnedLobbyVisible;

            CurrentlyIn = new()
            {
                Day = LobbyDay,
                Version = Application.version,
                LobbyState = GameManager.Instance.CurrentState != GameManager.State.Level ? LobbyInfo.State.Filling : LobbyInfo.State.Playing,
                Players = netMan.Runner.ActivePlayers.Count(),
                Name = $"{SteamClient.Name}'s Lobby",
                Code = netMan.SessionNameWithRegion,
            };

            _ = Send(new(Message.MessageType.LobbyOperationRequest, Message.OperationType.Request, new LobbyUpdateRequest
            {
                State = IsOwnedLobbyVisible ? LobbyState.ShowInList : LobbyState.HideFromList,
                Lobby = CurrentlyIn
            }), new(x =>
            {
                if (x.Type != Message.MessageType.LobbyOperationResult) return;

                if (IsOwnedLobbyVisible)
                    owner.GetComponentInChildren<TextMeshProUGUI>().SetText("Hide My Lobby In List");
                else
                    owner.GetComponentInChildren<TextMeshProUGUI>().SetText("Show My Lobby In List");
            }), new(() =>
            {
                IsOwnedLobbyVisible = false;
            }));
        }

        internal void CloseLobbyIfNeeded()
        {
            IsOwnedLobbyVisible = false;

            if (CurrentlyIn == null) return;

            CurrentlyIn = null;
            _ = Send(new(Message.MessageType.LobbyOperationRequest, Message.OperationType.Request, new LobbyUpdateRequest
            {
                State = LobbyState.HideFromList,
                Lobby = null
            }));
        }

        internal void UpdateLobbyInfo()
        {
            if (NetworkManager.Instance == null || NetworkManager.Instance.Runner == null || !NetworkManager.Instance.Runner.IsServer) return;
            if (CurrentlyIn == null) return;

            CurrentlyIn.Day = LobbyDay;
            CurrentlyIn.Players = NetworkManager.Instance.Runner.ActivePlayers.Count();
            CurrentlyIn.LobbyState = GameManager.Instance.CurrentState != GameManager.State.Level ? LobbyInfo.State.Filling : LobbyInfo.State.Playing;

            _ = Send(new(Message.MessageType.LobbyOperationRequest, Message.OperationType.Request, new LobbyUpdateRequest
            {
                State = LobbyState.UpdateDetails,
                Lobby = CurrentlyIn
            }));
        }

        async Task Send(Message msg, Action<Message> onceCompleted = null, Action onceFailed = null)
        {
            try
            {
                var tcs = new TaskCompletionSource<Message>(TaskCreationOptions.RunContinuationsAsynchronously);
                PendingResponses[msg.RequestID] = tcs;

                Plugin.Logger.LogInfo($"--> {msg.Type}\n{JsonConvert.SerializeObject(msg)}");

                var sendTask = ListSocket.SendAsync(new ArraySegment<byte>(msg.Serialize()), WebSocketMessageType.Text, true, CancellationToken.None);
                var res = await Task.WhenAny(tcs.Task, Task.Delay(Timeout));

                PendingResponses.Remove(msg.RequestID);

                if (res == tcs.Task)
                {
                    onceCompleted?.Invoke(await tcs.Task);
                }
                else
                {
                    onceFailed?.Invoke();
                    Plugin.Logger.LogWarning($"Reached timeout for {msg.RequestID}");
                }
            }
            catch
            {
                onceFailed?.Invoke();
            }
        }

        internal override void CreateLobby(object source)
        {
            var lobby = (LobbyInfo)source;

            var newLobby = GameObject.Instantiate(LobbyPrefab, Owner.content);

            newLobby.name = lobby.Name;
            newLobby.GetComponentInChildren<Text>().text = $"{lobby.Name} - {lobby.Code} - {lobby.LobbyState} | v{lobby.Version} - {lobby.Players}/8 | Day: {lobby.Day}";
            newLobby.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                var mmm = Resources.FindObjectsOfTypeAll<MainMenuManager>().FirstOrDefault();
                mmm?.OnLobbyCodeEntered(lobby.Code);
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                Plugin.Instance.LobbyManager.ToggleList(false);
            });

            CurrentLobbies.Add(newLobby);
        }

        internal override void RefreshList(object upcoming)
        {
            foreach (var item in CurrentLobbies)
                GameObject.Destroy(item.gameObject);

            CurrentLobbies.Clear();

            var elems = (List<LobbyInfo>)upcoming;

            NoLobbiesTxt.gameObject.SetActive(elems == null || elems.Count == 0);

            foreach (var item in elems)
                CreateLobby(item);
        }
    }
}
