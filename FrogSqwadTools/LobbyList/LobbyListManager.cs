using FrogSqwad.SFX;
using FrogSqwadTools.LobbyList.Core;
using FrogSqwadTools.LobbyList.Tabs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FrogSqwadTools.LobbyList
{
    internal class LobbyListManager
    {
        GameObject ListMenuPrefab { get; }
        readonly GameObject CurrentListMenu;
 
        LobbyListTab CurrentTab;
        readonly List<LobbyListTab> ListTabs;

        readonly Text StatsText;

        internal LobbyListManager(GameObject listPrefab, GameObject item)
        {
            ListMenuPrefab = listPrefab;

            CurrentListMenu = GameObject.Instantiate(ListMenuPrefab);
            ToggleList(false);
            GameObject.DontDestroyOnLoad(CurrentListMenu);

            var allBtns = CurrentListMenu.transform.GetComponentsInChildren<Button>();
            allBtns.FirstOrDefault(x => x.name == "KillList").onClick.AddListener(() =>
            {
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                CurrentListMenu.SetActive(false);
            });

            var refrBtn = allBtns.FirstOrDefault(x => x.name == "RefreshList");
            refrBtn.onClick.AddListener(() =>
            {
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                CurrentTab.RefreshRequest(false);
            });

            allBtns.FirstOrDefault(x => x.name == "PrevTab").onClick.AddListener(() =>
            {
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                PreviousTab();
            });

            allBtns.FirstOrDefault(x => x.name == "NextTab").onClick.AddListener(() =>
            {
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                NextTab();
            });

            var allTxts = CurrentListMenu.transform.GetComponentsInChildren<Text>();



            var allLists = CurrentListMenu.transform.GetComponentsInChildren<ScrollRect>();
            ListTabs = [];

            ListTabs.Add(new GlobalListTab(allLists.FirstOrDefault(x => x.name == "GlobalList"), refrBtn, item));

            CurrentTab = ListTabs.First();
            SetStats(0);
        }

        internal void ToggleList(bool state) => CurrentListMenu.SetActive(state);

        internal void NextTab()
        {
            var next = (ListTabs.IndexOf(CurrentTab) + 1) % ListTabs.Count;
            CurrentTab.Owner.gameObject.SetActive(false);
            CurrentTab = ListTabs[next];
            CurrentTab.Owner.gameObject.SetActive(true);
        }
        internal void PreviousTab()
        {
            var prev = (ListTabs.IndexOf(CurrentTab) - 1 + ListTabs.Count) % ListTabs.Count;
            CurrentTab.Owner.gameObject.SetActive(false);
            CurrentTab = ListTabs[prev];
            CurrentTab.Owner.gameObject.SetActive(true);
        }

        internal void SetStats(int lobbyCount)
        {

        }
    }
}
