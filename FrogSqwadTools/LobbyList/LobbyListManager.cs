using FrogSqwad.SFX;
using FrogSqwad.UI;
using FS_LobbyList_Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace FrogSqwadTools.LobbyList
{
    internal class LobbyListManager
    {
        GameObject ListMenuPrefab { get; }
        GameObject LobbyPrefab { get; }

        readonly GameObject CurrentListMenu;
        readonly List<GameObject> CurrentLobbies;
        readonly Text NoLobbiesTxt;
        readonly Text ConnectionFailedTxt;
        readonly Button RefreshListBtn;

        ClientWebSocket ListSocket;
        CancellationTokenSource TokenSource;
        readonly Dictionary<Guid, TaskCompletionSource<Message>> PendingResponses = [];
        readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        internal LobbyListManager(GameObject listPrefab, GameObject item)
        {
            ListMenuPrefab = listPrefab;
            LobbyPrefab = item;
            CurrentLobbies = [];

            CurrentListMenu = GameObject.Instantiate(ListMenuPrefab);
            ToggleList(false);
            GameObject.DontDestroyOnLoad(CurrentListMenu);

            var allBtns = CurrentListMenu.transform.GetComponentsInChildren<Button>();
            allBtns.FirstOrDefault(x => x.name == "KillList").onClick.AddListener(() =>
            {
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                CurrentListMenu.SetActive(false);
            });

            RefreshListBtn = allBtns.FirstOrDefault(x => x.name == "RefreshList");
            RefreshListBtn.onClick.AddListener(() =>
            {
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                RefreshList(false);
            });

            var allTxts = CurrentListMenu.transform.GetComponentsInChildren<Text>(true);
            NoLobbiesTxt = allTxts.FirstOrDefault(x => x.name == "NoLobbiesTxt");
            ConnectionFailedTxt = allTxts.FirstOrDefault(x => x.name == "ConnectFailedTxt");

            Task.Run(async () =>
            {
                ListSocket = new();
                TokenSource = new();
                ConnectionFailedTxt.gameObject.SetActive(true);

                await ListSocket.ConnectAsync(new("ws://127.0.0.1:10002/ws"), TokenSource.Token);

                if (ListSocket.State == WebSocketState.Open)
                {
                    Plugin.Logger.LogInfo("Connected to lobby list");
                    ConnectionFailedTxt.gameObject.SetActive(false);
                    _ = Task.Run(Receive);

                    RefreshList(true);
                }
                else
                {
                    Plugin.Logger.LogInfo("Failed to connect to lobby list");
                }
            });
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

        internal void ToggleList(bool state) => CurrentListMenu.SetActive(state);

        internal void CreateLobby(LobbyInfo lobby)
        {
            var newLobby = GameObject.Instantiate(LobbyPrefab, CurrentListMenu.GetComponentInChildren<ScrollRect>().content);

            newLobby.name = lobby.Name;
            newLobby.GetComponentInChildren<Text>().text = $"{lobby.Name} | {lobby.Code} | {lobby.Version} | {lobby.LobbyState} | {lobby.Version} / 102 | {lobby.Day}";
            newLobby.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                var mmm = Resources.FindObjectsOfTypeAll<MainMenuManager>().FirstOrDefault();
                mmm?.OnLobbyCodeEntered(lobby.Code);
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                ToggleList(false);
            });

            CurrentLobbies.Add(newLobby);
        }

        internal void RefreshList(List<LobbyInfo> upcoming)
        {
            foreach (var item in CurrentLobbies)
                GameObject.Destroy(item.gameObject);

            CurrentLobbies.Clear();
            NoLobbiesTxt.gameObject.SetActive(upcoming == null || upcoming.Count == 0);

            foreach (var item in upcoming)
                CreateLobby(item);
        }

        internal async Task Send(Message msg, Action<Message> onceCompleted = null, Action onceFailed = null)
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

        void RefreshList(bool silent)
        {
            RefreshListBtn.interactable = false;

            _ = Send(new(Message.MessageType.RefreshRequest, Message.OperationType.Request, null), new(res =>
            {
                RefreshListBtn.interactable = true;

                if (res.Type != Message.MessageType.LobbyList) return;

                var listArray = (JArray)res.Payload;
                RefreshList(listArray.ToObject<List<LobbyInfo>>());
                if (!silent) SFXSystem.Instance.PlayUI(SFXType.UIConfirm);
            }), new(() =>
            {
                RefreshListBtn.interactable = true;
            }));
        }
    }
}
