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
        internal GlobalListTab(string name, ScrollRect owner, Button refrBtn, GameObject lobby) : base(name, owner, refrBtn, lobby)
        {
            LobbyList = new();
            _ = LobbyList.Init(this).ContinueWith(x =>
            {
                ConnectionFailedTxt.gameObject.SetActive(!x.Result);
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

        protected override void RefreshListLogic(object upcoming)
        {
            foreach (var item in CurrentLobbies)
                GameObject.Destroy(item.gameObject);

            CurrentLobbies.Clear();

            var elems = (List<SessionInfo>)upcoming;

            NewCodes.Clear();

            NoLobbiesTxt.gameObject.SetActive(elems == null || elems.Count == 0);

            foreach (var item in elems.OrderBy(x => OldCodes.Contains(x.Name)))
                CreateLobby(item);
        }

        internal override void RefreshRequest(bool silent)
        {
        }

        protected override void OnTabSetLogic()
        {
        }
    }
}
