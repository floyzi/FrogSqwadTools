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
        readonly Text TitleText;
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

            StatsText = allTxts.FirstOrDefault(x => x.name == "Stats");
            TitleText = allTxts.FirstOrDefault(x => x.name == "ListTitle");

            var allLists = CurrentListMenu.transform.GetComponentsInChildren<ScrollRect>();
            ListTabs = [];

            ListTabs.Add(new GlobalListTab("Global", allLists.FirstOrDefault(x => x.name == "GlobalList"), refrBtn, item));
            ListTabs.Add(new CustomListTab("Custom", allLists.FirstOrDefault(x => x.name == "CustomList"), refrBtn, item));

            SetTabAtIndex(0);
            SetStats(0, 0);
        }

        internal void ToggleList(bool state) => CurrentListMenu.SetActive(state);

        internal void NextTab()
        {
            var next = (ListTabs.IndexOf(CurrentTab) + 1) % ListTabs.Count;
            SetTabAtIndex(next);
        }
        internal void PreviousTab()
        {
            var prev = (ListTabs.IndexOf(CurrentTab) - 1 + ListTabs.Count) % ListTabs.Count;
            SetTabAtIndex(prev);
        }

        void SetTabAtIndex(int indx)
        {
            CurrentTab?.Owner.gameObject.SetActive(false);
            CurrentTab = ListTabs[indx];
            CurrentTab.Owner.gameObject.SetActive(true);
            TitleText.text = $"{CurrentTab.Name} LIST";
        }

        internal void SetStats(int lobbyCount, int newLobbies)
        {
            StatsText.text = $"Lobbies in list: {lobbyCount} | New lobbies: {102}";
        }
    }
}
