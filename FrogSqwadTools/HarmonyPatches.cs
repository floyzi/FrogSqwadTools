using FrogSqwad.UI;
using FrogSqwadTools.FLZ_UI.LobbyList;
using FrogSqwadTools.FLZ_UI.LobbyList.Tabs;
using Fusion;
using HarmonyLib;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace FrogSqwadTools
{
    internal class HarmonyPatches
    {
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OnQuitPressed)), HarmonyPrefix]
        static bool OnQuitPressed(MainMenuManager __instance)
        {
            Application.Quit();
            return false;
        }

        [HarmonyPatch(typeof(VersionNumberHUDManager), nameof(VersionNumberHUDManager.Start)), HarmonyPostfix]
        static void Start(VersionNumberHUDManager __instance)
        {
            Plugin.Instance.AdvVer = new(__instance);
        }

        [HarmonyPatch(typeof(PauseMenu), nameof(PauseMenu.OpenMenu)), HarmonyPostfix]
        static void OpenMenu(PauseMenu __instance)
        {
            if (__instance._showLobbyCodeButton.name == "Init") return; //what am i even doing man

            __instance._showLobbyCodeButton.name = "Init";
            __instance._showLobbyCodeButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Show & Copy Lobby Code");

            //no point in restoring this atp ... no there IS a point

            //if (NetworkManager.Instance.Runner.IsServer)
            //{
            //    var lobbyToggleBtn = GameObject.Instantiate(__instance._showLobbyCodeButton.gameObject, __instance._showLobbyCodeButton.transform.GetParent());
            //    lobbyToggleBtn.GetComponentInChildren<TextMeshProUGUI>().SetText("Show My Lobby In List");
            //    lobbyToggleBtn.transform.SetSiblingIndex(lobbyToggleBtn.transform.GetSiblingIndex() -  1);
            //    var btn = lobbyToggleBtn.GetComponent<CustomButton>();
            //    btn.onClick.AddListener(() =>
            //    {
            //        Plugin.Instance.LobbyManager.ToggleLobbyState(btn);
            //    });

            //    __instance._showLobbyCodeButton.transform.GetParent().transform.localPosition += new Vector3(0, 80, 0); 
            //}
        }

        [HarmonyPatch(typeof(PauseMenu), nameof(PauseMenu.OnShowLobbyCodePressed)), HarmonyPostfix]
        static void OnShowLobbyCodePressed(PauseMenu __instance)
        {
            GUIUtility.systemCopyBuffer = NetworkManager.Instance.SessionNameWithRegion;
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPostfix]
        static void Start(MainMenuManager __instance)
        {
            var lbBtn = GameObject.Instantiate(__instance._hostButton.gameObject, __instance._hostButton.transform.GetParent());
            lbBtn.transform.SetSiblingIndex(2);
            lbBtn.GetComponentInChildren<TextMeshProUGUI>().SetText("Lobby List");
            lbBtn.GetComponent<CustomButton>().onClick.AddListener(() => LobbyListManager.Instance.ToggleList(true));

            var menuContent = lbBtn.transform.GetParent().transform.GetParent();
            var lbInp = menuContent.transform.Find("Lobby code input")?.transform;
            var regionDropdown = GameObject.Instantiate(LobbyListManager.Instance.RegionDropdownPrefab, lbInp.transform.position, Quaternion.identity, menuContent);

            regionDropdown.transform.localPosition += new Vector3(-140, 0, 0);
            lbInp.localPosition += new Vector3(0, -95, 0);

            LobbyListManager.Instance.InitRegionDropdown(regionDropdown);

            _ = LobbyListManager.Instance.GetListTab<GlobalListTab>().LobbyList.BeginConnect();
        }

        [HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.StartHost)), HarmonyPostfix]
        static void StartHost(NetworkManager __instance)
        {
            _ = LobbyListManager.Instance.GetListTab<GlobalListTab>().LobbyList.TerminateConnection();
        }

        [HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.StartJoin)), HarmonyPostfix]
        static void StartJoin(NetworkManager __instance)
        {
            _ = LobbyListManager.Instance.GetListTab<GlobalListTab>().LobbyList.TerminateConnection();
        }

        //[HarmonyPatch(typeof(LevelLoader), nameof(LevelLoader.LoadMainMenu)), HarmonyPostfix]
        //static void LoadMainMenu(LevelLoader __instance)
        //{
        //    Plugin.Instance.LobbyManager.CloseLobbyIfNeeded();
        //}

        //[HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.OnPlayerJoined)), HarmonyPostfix]
        //static void OnPlayerJoined(NetworkManager __instance)
        //{
        //    Plugin.Instance.LobbyManager.UpdateLobbyInfo();
        //}

        //[HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.OnPlayerLeft)), HarmonyPostfix]
        //static void OnPlayerLeft(NetworkManager __instance)
        //{
        //    Plugin.Instance.LobbyManager.UpdateLobbyInfo();
        //}

        //[HarmonyPatch(typeof(GameManager), nameof(GameManager.ChangeState)), HarmonyPostfix]
        //static void ChangeState(GameManager __instance)
        //{
        //    Plugin.Instance.LobbyManager.UpdateLobbyInfo();
        //}
    }
}
