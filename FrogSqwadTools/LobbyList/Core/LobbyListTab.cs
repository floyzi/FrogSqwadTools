using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace FrogSqwadTools.LobbyList.Core
{
    internal abstract class LobbyListTab
    {
        internal string Name { get; private set; }
        internal ScrollRect Owner { get; private set; }
        internal Button RefreshBtn { get; private set; }
        internal GameObject LobbyPrefab { get; private set; }

        protected readonly HashSet<GameObject> CurrentLobbies;
        protected readonly HashSet<string> OldCodes;
        protected readonly HashSet<string> NewCodes;

        protected readonly Text NoLobbiesTxt;
        protected readonly Text ConnectionFailedTxt;
        protected readonly Text LoadingTxt;

        internal LobbyListTab(string name, ScrollRect owner, Button refreshBtn, GameObject lobbyPrefab)
        {
            Name = name;
            Owner = owner;
            RefreshBtn = refreshBtn;
            LobbyPrefab = lobbyPrefab;

            var allTxts = Owner.transform.GetComponentsInChildren<Text>(true);

            NoLobbiesTxt = allTxts.FirstOrDefault(x => x.name == "NoLobbiesTxt");
            ConnectionFailedTxt = allTxts.FirstOrDefault(x => x.name == "ConnectFailedTxt");
            LoadingTxt = allTxts.FirstOrDefault(x => x.name == "LoadingTxt");

            ConnectionFailedTxt.gameObject.SetActive(true);

            CurrentLobbies = [];
            OldCodes = [];
            NewCodes = [];

            Owner.gameObject.SetActive(false);
        }

        internal abstract void RefreshRequest(bool silent);
        protected abstract void RefreshListLogic(object upcoming);
        internal void RefreshList(object upcoming)
        {
            RefreshListLogic(upcoming);
            LobbyListManager.Instance.SetStats(CurrentLobbies.Count, NewCodes.Count);
        }
        internal abstract void CreateLobby(object source);
        protected abstract void OnTabSetLogic();
        internal void OnTabSet()
        {
            OnTabSetLogic();
            LobbyListManager.Instance.SetStats(CurrentLobbies.Count, NewCodes.Count);
        }
    }
}
