using FrogSqwad.UI;
using HarmonyLib;
using TMPro;
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

            if (NetworkManager.Instance.Runner.IsServer)
            {
                var lobbyToggleBtn = GameObject.Instantiate(__instance._showLobbyCodeButton.gameObject, __instance._showLobbyCodeButton.transform.GetParent());
                lobbyToggleBtn.GetComponentInChildren<TextMeshProUGUI>().SetText("Show My Lobby In List");
                lobbyToggleBtn.transform.SetSiblingIndex(lobbyToggleBtn.transform.GetSiblingIndex() -  1);
                var btn = lobbyToggleBtn.GetComponent<CustomButton>();
                btn.onClick.AddListener(() =>
                {
                    Plugin.Instance.LobbyManager.ToggleLobbyState(btn);
                });

                __instance._showLobbyCodeButton.transform.GetParent().transform.localPosition += new Vector3(0, 80, 0); 
            }
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
            lbBtn.GetComponent<CustomButton>().onClick.AddListener(() => Plugin.Instance.LobbyManager.ToggleList(true));
        }


        [HarmonyPatch(typeof(LevelLoader), nameof(LevelLoader.LoadMainMenu)), HarmonyPostfix]
        static void LoadMainMenu(LevelLoader __instance)
        {
            Plugin.Instance.LobbyManager.CloseLobbyIfNeeded();
        }

        [HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.OnPlayerJoined)), HarmonyPostfix]
        static void OnPlayerJoined(NetworkManager __instance)
        {
            Plugin.Instance.LobbyManager.UpdateLobbyInfo();
        }

        [HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.OnPlayerLeft)), HarmonyPostfix]
        static void OnPlayerLeft(NetworkManager __instance)
        {
            Plugin.Instance.LobbyManager.UpdateLobbyInfo();
        }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.ChangeState)), HarmonyPostfix]
        static void ChangeState(GameManager __instance)
        {
            Plugin.Instance.LobbyManager.UpdateLobbyInfo();
        }
    }
}
