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

        GameObject CurrentListMenu;
        readonly List<GameObject> CurrentLobbies;

        ClientWebSocket ListSocket;
        CancellationTokenSource TokenSource;

        internal LobbyListManager(GameObject listPrefab, GameObject item)
        {
            ListMenuPrefab = listPrefab;
            LobbyPrefab = item;
            CurrentLobbies = [];

            CurrentListMenu = GameObject.Instantiate(ListMenuPrefab);
            CurrentListMenu.gameObject.SetActive(false);
            GameObject.DontDestroyOnLoad(CurrentListMenu);

            var allBtns = CurrentListMenu.transform.GetComponentsInChildren<Button>();
            allBtns.FirstOrDefault(x => x.name == "KillList").onClick.AddListener(() =>
            {
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                CurrentListMenu.SetActive(false);
            });

            allBtns.FirstOrDefault(x => x.name == "RefreshList").onClick.AddListener(() =>
            {
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                _ = Send(new(Message.MessageType.RefreshRequest, null));
            });

            Task.Run(async () =>
            {
                ListSocket = new();
                TokenSource = new();

                await ListSocket.ConnectAsync(new("ws://127.0.0.1:10002/ws"), TokenSource.Token);

                if (ListSocket.State == WebSocketState.Open)
                {
                    Plugin.Logger.LogInfo("Connected to lobby list");
                    _ = Task.Run(Receive);
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
                    Plugin.Logger.LogInfo($"--> {msg.Type}\n{JsonConvert.SerializeObject(msg)}");
                    HandleMessage(msg);
                }
                catch
                {

                }
            }
        }

        void HandleMessage(Message msg)
        {
            switch (msg.Type)
            {
                case Message.MessageType.LobbyList:
                    var listArray = (JArray)msg.Payload;
                    RefreshList(listArray.ToObject<List<LobbyInfo>>());
                    break;
            }
        }

        internal void ShowList()
        {
            CurrentListMenu.SetActive(true);
        }

        internal void CreateLobby(LobbyInfo lobby)
        {
            var newLobby = GameObject.Instantiate(LobbyPrefab, CurrentListMenu.GetComponentInChildren<ScrollRect>().content);

            newLobby.name = lobby.Name;
            newLobby.GetComponentInChildren<Text>().text = $"{lobby.Name} | {lobby.Code} | {lobby.Version} | {lobby.LobbyState} | {lobby.Version} / 102 | {lobby.Day}";
            newLobby.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                Plugin.Logger.LogInfo("102");
            });

            CurrentLobbies.Add(newLobby);
        }

        internal void RefreshList(List<LobbyInfo> upcoming)
        {
            foreach (var item in CurrentLobbies)
                GameObject.Destroy(item.gameObject);

            foreach (var item in upcoming)
                CreateLobby(item);
        }

        internal async Task Send(Message msg)
        {
            await ListSocket.SendAsync(new ArraySegment<byte>(msg.Serialize()), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
