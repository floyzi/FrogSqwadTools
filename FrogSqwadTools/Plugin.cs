using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using static FrogSqwadTools.AdvancedVersion;

namespace FrogSqwadTools
{
    [BepInPlugin("flz.fs.tools", "Frog Sqwad Tools", "0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static Harmony Harmony;
        internal static AdvancedVersion AdvVer;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Harmony = new("flz.fs.tools.harmony");
            Harmony.PatchAll(typeof(HarmonyPatches));

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
