using Fusion;
using Fusion.Sockets;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace FrogSqwadDebug
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
            Plugin.AdvVer = new(__instance);
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
    }
}
