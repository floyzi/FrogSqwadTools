using FrogSqwad.SFX;
using FrogSqwadTools.LobbyList.Core;
using Fusion;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

namespace FrogSqwadTools.LobbyList.Tabs
{
    internal class GlobalListTab : LobbyListTab
    {
        internal readonly PhotonLobbyList LobbyList;
        List<SessionInfo> _pendingLobbies;
        bool _hasPending;
        bool _hasSeenLobbiesBefore;
        internal GlobalListTab(string name, ScrollRect owner, Button refrBtn, GameObject lobby) : base(name, owner, refrBtn, lobby)
        {
            LobbyList = new();
            _pendingLobbies = [];

            _ = LobbyList.Init(this);

            PhotonLobbyList.OnConnectBegin += new(() =>
            {
                ConnectionFailedTxt.gameObject.SetActive(false);
                NoLobbiesTxt.gameObject.SetActive(false);
                LoadingTxt.gameObject.SetActive(true);
            });

            PhotonLobbyList.OnConnectEnd += new((success, region) =>
            {
                _hasSeenLobbiesBefore = false;
                _pendingLobbies?.Clear();

                foreach (var item in CurrentLobbies)
                    GameObject.Destroy(item.gameObject);

                CurrentLobbies.Clear();

                NewCodes.Clear();

                LoadingTxt.gameObject.SetActive(false);
                NoLobbiesTxt.gameObject.SetActive(false);
                ConnectionFailedTxt.gameObject.SetActive(!success);

                if (!string.IsNullOrEmpty(region))
                {
                    var newReg = LobbyListManager.Instance.RegionDropdown.options.Find(x => string.Equals(x.text, region, StringComparison.InvariantCultureIgnoreCase));
                    if (newReg != null)
                        LobbyListManager.Instance.RegionDropdown.value = LobbyListManager.Instance.RegionDropdown.options.IndexOf(newReg);
                }
            });
        }

        internal override void CreateLobby(object source)
        {
            var lobby = (SessionInfo)source;
            if (lobby == null) return;

            var newLobby = GameObject.Instantiate(LobbyPrefab, Owner.content);

            newLobby.name = lobby.Name;

            var realCode = lobby.Name + LobbyListManager.Instance.KnownRegions.GetSessionCodeCharForRegion(lobby.Region, NetworkManager.Instance._allRegions);

            CurrentCodes.Add(realCode);

            bool isNew = OldCodes.Add(lobby.Name);
            if (isNew)
                NewCodes.Add(lobby.Name);

            var texts = newLobby.GetComponentsInChildren<Text>(true);

            texts.FirstOrDefault(x => x.name == "TextContent").text = $"{realCode} - {lobby.Region} | {lobby.PlayerCount}/{lobby.MaxPlayers}";
            texts.FirstOrDefault(x => x.name == "NewMarker").gameObject.SetActive(isNew);
            newLobby.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                var mmm = Resources.FindObjectsOfTypeAll<MainMenuManager>().FirstOrDefault();
                mmm?.OnLobbyCodeEntered(realCode);
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                LobbyListManager.Instance.ToggleList(false);
            });

            CurrentLobbies.Add(newLobby);
        }

        internal override void UpdateList(object upcoming)
        {
            _pendingLobbies = (List<SessionInfo>)upcoming;
            _hasPending = true;

            if (!_hasSeenLobbiesBefore)
            {
                _hasSeenLobbiesBefore = true;
                RefreshRequest(true);
            }
        }

        void Refresh(List<SessionInfo> upcoming)
        {
            foreach (var item in CurrentLobbies)
                GameObject.Destroy(item.gameObject);

            CurrentLobbies.Clear();
            CurrentCodes.Clear();
            NewCodes.Clear();

            NoLobbiesTxt.gameObject.SetActive(upcoming == null || upcoming.Count == 0);

            foreach (var item in upcoming.OrderBy(x => OldCodes.Contains(x.Name)))
                CreateLobby(item);
        }

        protected override void RefreshRequestLogic(bool silent)
        {
            if (!_hasPending)
                return;

            Refresh(_pendingLobbies);
            _hasPending = false;
            _pendingLobbies.Clear();
        }

        protected override void OnTabSetLogic()
        {
        }
    }
}
