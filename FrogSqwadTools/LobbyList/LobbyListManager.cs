using FrogSqwad.SFX;
using FrogSqwadTools.LobbyList.Core;
using FrogSqwadTools.LobbyList.Tabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FrogSqwadTools.LobbyList
{
    internal class LobbyListManager
    {
        internal static LobbyListManager Instance { get; private set; }

        GameObject ListMenuPrefab { get; }
        internal GameObject RegionDropdownPrefab { get; }

        readonly GameObject CurrentListMenu;
        internal Dropdown RegionDropdown { get; private set; }
 
        LobbyListTab CurrentTab;
        readonly List<LobbyListTab> ListTabs;

        readonly Text StatsText;
        readonly Text TitleText;

        internal PhotonRegionsLookup KnownRegions;
        internal LobbyListManager(GameObject listPrefab, GameObject item, GameObject regionDropdown)
        {
            Instance = this;
            ListMenuPrefab = listPrefab;
            RegionDropdownPrefab = regionDropdown;

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
            CurrentTab.OnTabSet();
            TitleText.text = $"{CurrentTab.Name} LIST";
        }

        internal void SetStats(int lobbyCount, int newLobbies)
        {
            StatsText.text = $"Lobbies in list: {lobbyCount} | New lobbies: {newLobbies}";
        }

        internal void InitRegionDropdown(GameObject dropdownObj)
        {
            KnownRegions = Resources.FindObjectsOfTypeAll<PhotonRegionsLookup>().FirstOrDefault();

            RegionDropdown = dropdownObj.GetComponent<Dropdown>();

            RegionDropdown.ClearOptions();

            RegionDropdown.options.Add(new("Auto"));
            foreach (var region in KnownRegions.Regions)
            {
                RegionDropdown.options.Add(new(region.Code));
            }

            RegionDropdown.onValueChanged.AddListener(x =>
            {
                string goWith = null;
                if (x != 0)
                    goWith = KnownRegions.Regions[x - 1].Code;

                _ = GetListTab<GlobalListTab>().LobbyList.BeginConnect(goWith);
            });
        }

        T GetListTab<T>() where T : LobbyListTab => ListTabs.FirstOrDefault(x => x.GetType() == typeof(T)) as T;
    }
}
