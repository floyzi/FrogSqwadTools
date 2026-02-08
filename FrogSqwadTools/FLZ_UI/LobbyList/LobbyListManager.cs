using FrogSqwad.SFX;
using FrogSqwad.UI;
using FrogSqwadTools.FLZ_UI.Core;
using FrogSqwadTools.FLZ_UI.LobbyList.Core;
using FrogSqwadTools.FLZ_UI.LobbyList.Tabs;
using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FrogSqwadTools.FLZ_UI.LobbyList
{
    internal class LobbyListManager : UIElement
    {
        internal static LobbyListManager Instance { get; private set; }

        internal GameObject RegionDropdownPrefab { get; }
        internal Dropdown RegionDropdown { get; private set; }
 
        LobbyListTab CurrentTab;
        readonly List<LobbyListTab> ListTabs;

        [UIReference("Stats")] Text StatsText { get; set; }
        [UIReference("ListTitle")] Text TitleText { get; set; }
        [UIReference("RegionInfo")] Text RegionInfoText { get; set; }
        [UIReference("KillList")] CustomButton HideListBtn { get; set; }
        [UIReference("RefreshList")] CustomButton RefreshListBtn { get; set; }
        [UIReference("PrevTab")] CustomButton PrevTabBtn { get; set; }
        [UIReference("NextTab")] CustomButton NextTabBtn { get; set; }
        [UIReference("JoinRandomBtn")] CustomButton JoinRandomBtn { get; set; }

        internal PhotonRegionsLookup KnownRegions;
        internal LobbyListManager(GameObject listPrefab, GameObject item, GameObject regionDropdown) : base(GameObject.Instantiate(listPrefab).GetComponent<Transform>())
        {
            Instance = this;
            RegionDropdownPrefab = regionDropdown;

            ToggleList(false);
            GameObject.DontDestroyOnLoad(ElementInstance);

            HideListBtn.onClick.AddListener(() =>
            {
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                ElementInstance.gameObject.SetActive(false);
            });

            RefreshListBtn.onClick.AddListener(() =>
            {
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                CurrentTab.RefreshRequest(false);
            });

            PrevTabBtn.onClick.AddListener(() =>
            {
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                var prev = (ListTabs.IndexOf(CurrentTab) - 1 + ListTabs.Count) % ListTabs.Count;
                SetTabAtIndex(prev);
            });

            NextTabBtn.onClick.AddListener(() =>
            {
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                var next = (ListTabs.IndexOf(CurrentTab) + 1) % ListTabs.Count;
                SetTabAtIndex(next);
            });

            JoinRandomBtn.onClick.AddListener(() =>
            {
                SFXSystem.Instance.PlayUI(SFXType.UIClick);
                CurrentTab.JoinRandom();
            });

            var allLists = ElementInstance.gameObject.transform.GetComponentsInChildren<ScrollRect>();
            ListTabs = [];

            ListTabs.Add(new GlobalListTab("Global", allLists.FirstOrDefault(x => x.name == "GlobalList"), RefreshListBtn, item));
            //ListTabs.Add(new CustomListTab("Custom", allLists.FirstOrDefault(x => x.name == "CustomList"), RefreshListBtn, item));

            SetTabAtIndex(0);

            RegionInfoText.text = "???";
            PhotonLobbyList.OnConnectEnd += new((success, region) =>
            {
                if (string.IsNullOrEmpty(region))
                    RegionInfoText.text = $"... i don't know :(";
                else
                    RegionInfoText.text = $"{region}: AVG ping ~{NetworkManager.Instance._allRegions.FirstOrDefault(x => x.RegionCode == region).RegionPing}ms";
            });
        }

        internal void ToggleList(bool state) => ElementInstance.gameObject.SetActive(state);

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
            StatsText.text = $"TOTAL: {lobbyCount} | NEW: {newLobbies}";
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
                if (x != 0) goWith = KnownRegions.Regions[x - 1].Code;

                _ = GetListTab<GlobalListTab>().LobbyList.BeginConnect(goWith);
            });
        }

        internal T GetListTab<T>() where T : LobbyListTab => ListTabs.FirstOrDefault(x => x.GetType() == typeof(T)) as T;
    }
}
