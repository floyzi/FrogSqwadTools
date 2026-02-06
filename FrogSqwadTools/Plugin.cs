using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using FrogSqwadTools.LobbyList;
using HarmonyLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using static FrogSqwadTools.AdvancedVersion;

namespace FrogSqwadTools
{
    [BepInPlugin("flz.fs.tools", "Frog Sqwad Tools", Version)]
    public class Plugin : BaseUnityPlugin
    {
        internal const string Version = "0.1.0";
        internal const string ExpectedProtocol = "0.0.0.1";

        internal readonly struct BuildInfo
        {
            public readonly string Version;
            public readonly string Config;
            public readonly string Commit;
            public readonly string CommitShort;
            public readonly DateTime BuildDate;

            internal BuildInfo(string version, string commit, string build_date, string conf)
            {
                Version = version;
                Commit = commit;
                Config = conf;
                CommitShort = GetCommit(6);

                if (long.TryParse(build_date, out long unixTimestamp))
                    BuildDate = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
            }

            public override readonly string ToString() => $"v{Version}";
            internal readonly string GetCommit(int l) => Commit.Length > l ? Commit[..l] : Commit;
        }

        internal static Plugin Instance { get; private set; }
        internal static new ManualLogSource Logger;
        internal Harmony Harmony;
        internal AdvancedVersion AdvVer;
        internal LobbyListManager LobbyManager;
        internal static BuildInfo BuildDetails;
        internal static string ModDir = Path.Combine(Paths.PluginPath, "FrogSqwadTools");

        private void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            var assembly = Assembly.GetExecutingAssembly();

            var version = FileVersionInfo.GetVersionInfo(assembly.Location);
            var commit = version.ProductVersion.Split('+');
            var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToList();
            var cfg = assembly.GetCustomAttributes<AssemblyConfigurationAttribute>().ToList()[0].Configuration;

            var buildDate = metadata.FirstOrDefault(x => x.Key == "BuildDate")?.Value;

            BuildDetails = new BuildInfo(Version, commit.Length > 1 ? commit[1] : "...", buildDate, cfg);

            var protocolPath = Path.Combine(ModDir, "FS_LobbyList_Protocol.dll");
            if (!File.Exists(protocolPath))
            {
                Logger.LogError("Unable to find protocol file at path\n" + protocolPath);
                Application.Quit();
                return;
            }

            var verInfo = FileVersionInfo.GetVersionInfo(protocolPath);
            if (verInfo.ProductVersion != ExpectedProtocol)
                Logger.LogWarning("Incorrect protocol version\n" + verInfo.ProductVersion);

            Harmony = new("flz.fs.tools.harmony");
            Harmony.PatchAll(typeof(HarmonyPatches));

            var bundlePath = Path.Combine(ModDir, "frog_sqwad_lobbylist");
            if (!File.Exists(bundlePath))
            {
                Logger.LogError("Unable to find bundle file at path\n" + bundlePath);
                Application.Quit();
                return;
            }

            var menusBundle = AssetBundle.LoadFromFile(bundlePath);
            LobbyManager = new(menusBundle.LoadAsset<GameObject>("LobbyListPrefab"), menusBundle.LoadAsset<GameObject>("LobbyListLobby"));

            menusBundle.Unload(false);

            Logger.LogInfo($"");
            Logger.LogInfo($"---");
            Logger.LogInfo($"Frog Sqwad Tools (REAL Tools) v{BuildDetails.Version}");
            Logger.LogInfo($"Build Date: {BuildDetails.BuildDate} | Commit: #{BuildDetails.GetCommit(12)}");
            Logger.LogInfo($"---");
            Logger.LogInfo($"");
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
