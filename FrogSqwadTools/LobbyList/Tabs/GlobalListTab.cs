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
        readonly PhotonLobbyList LobbyList;
        PhotonRegionsLookup KnownRegions;
        internal GlobalListTab(ScrollRect owner, Button refrBtn, GameObject lobby) : base(owner, refrBtn, lobby)
        {
            LobbyList = new();
            _ = LobbyList.Init(this).ContinueWith(x =>
            {
                KnownRegions = Resources.FindObjectsOfTypeAll<PhotonRegionsLookup>().FirstOrDefault();
            });
        }

        internal override void CreateLobby(object source)
        {
            var lobby = (SessionInfo)source;
            if (lobby == null) return;

            var newLobby = GameObject.Instantiate(LobbyPrefab, Owner.content);

            newLobby.name = lobby.Name;

            var realCode = lobby.Name + KnownRegions.GetSessionCodeCharForRegion(lobby.Region, NetworkManager.Instance._allRegions);

            newLobby.GetComponentInChildren<Text>().text = $"{realCode} - {lobby.Region} | {lobby.PlayerCount}/{lobby.MaxPlayers}";
            newLobby.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                var mmm = Resources.FindObjectsOfTypeAll<MainMenuManager>().FirstOrDefault();
                mmm?.OnLobbyCodeEntered(realCode);
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

            var elems = (List<SessionInfo>)upcoming;

            NoLobbiesTxt.gameObject.SetActive(elems == null || elems.Count == 0);

            foreach (var item in elems)
                CreateLobby(item);
        }

        internal override void RefreshRequest(bool silent)
        {
            throw new NotImplementedException();
        }
    }
}
