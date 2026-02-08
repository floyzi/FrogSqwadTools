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

        protected readonly List<GameObject> CurrentLobbies;
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

            Owner.gameObject.SetActive(false);
        }

        internal abstract void RefreshRequest(bool silent);
        protected abstract void RefreshListLogic(object upcoming);
        internal void RefreshList(object upcoming)
        {
            RefreshListLogic(upcoming);
            Plugin.Instance.LobbyManager.SetStats(CurrentLobbies.Count, 0);
        }
        internal abstract void CreateLobby(object source);
    }
}
