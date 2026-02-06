using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using FrogSqwadTools.LobbyList;
using HarmonyLib;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using static FrogSqwadTools.AdvancedVersion;

namespace FrogSqwadTools
{
    [BepInPlugin("flz.fs.tools", "Frog Sqwad Tools", Version)]
    public class Plugin : BaseUnityPlugin
    {
        internal const string Version = "0.1.0";

        internal static Plugin Instance { get; private set; }
        internal static new ManualLogSource Logger;
        internal Harmony Harmony;
        internal AdvancedVersion AdvVer;
        static AssetBundle LobbyBundle;
        internal LobbyListManager LobbyManager;

        private void Awake()
        {
            Instance = this;
            Logger = base.Logger;
            Harmony = new("flz.fs.tools.harmony");
            Harmony.PatchAll(typeof(HarmonyPatches));

            LobbyBundle = AssetBundle.LoadFromFile(Path.Combine(Paths.PluginPath, "frog_sqwad_lobbylist"));
            LobbyManager = new(LobbyBundle.LoadAsset<GameObject>("LobbyListPrefab"), LobbyBundle.LoadAsset<GameObject>("LobbyListLobby"));

            Logger.LogInfo($"Plugin is loaded!");
        }

        void Update()
        {
            AdvVer?.SetFPS();
            AdvVer?.UpdateStyleDisplay(
                NetworkManager.Instance != null && NetworkManager.Instance.Runner != null && NetworkManager.Instance.Runner.SessionInfo != null ? 
                Style.InGame : 
                Style.Default);
        }

        void FixedUpdate()
        {
            AdvVer?.UpdateDisplay();
        }
    }
}
