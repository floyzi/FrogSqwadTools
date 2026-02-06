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

        [HarmonyPatch(typeof(PauseMenu), nameof(PauseMenu.Start)), HarmonyPostfix]
        static void Start(PauseMenu __instance)
        {
            __instance._showLobbyCodeButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Show & Copy Lobby Code");
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
            lbBtn.GetComponent<CustomButton>().onClick.AddListener(Plugin.Instance.LobbyManager.ShowList);
        }
    }
}
