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
        internal ScrollRect Owner { get; private set; }
        internal Button RefreshBtn { get; private set; }
        internal GameObject LobbyPrefab { get; private set; }

        protected readonly List<GameObject> CurrentLobbies;
        protected readonly Text NoLobbiesTxt;
        protected readonly Text ConnectionFailedTxt;
        protected readonly Text LoadingTxt;

        internal LobbyListTab(ScrollRect owner, Button refreshBtn, GameObject lobbyPrefab)
        {
            Owner = owner;
            RefreshBtn = refreshBtn;
            LobbyPrefab = lobbyPrefab;

            var allTxts = Owner.transform.GetComponentsInChildren<Text>(true);
            NoLobbiesTxt = allTxts.FirstOrDefault(x => x.name == "NoLobbiesTxt");
            ConnectionFailedTxt = allTxts.FirstOrDefault(x => x.name == "ConnectFailedTxt");
            LoadingTxt = allTxts.FirstOrDefault(x => x.name == "LoadingTxt");

            NoLobbiesTxt.gameObject.SetActive(true);
            CurrentLobbies = [];
        }

        internal abstract void RefreshRequest(bool silent);
        internal abstract void RefreshList(object upcoming);
        internal abstract void CreateLobby(object source);
    }
}
