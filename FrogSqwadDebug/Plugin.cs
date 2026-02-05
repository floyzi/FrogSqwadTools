using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;

namespace FrogSqwadDebug
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static Harmony Harmony;
        internal static AdvancedVersion AdvVer;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Harmony = new("flz.fs.thing");
            Harmony.PatchAll(typeof(HarmonyPatches));

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        void Update()
        {
            AdvVer?.SetFPS();
            AdvVer?.UpdateStyleDisplay(NetworkManager.Instance != null && NetworkManager.Instance.Runner != null && NetworkManager.Instance.Runner.SessionInfo != null ? AdvancedVersion.Style.InGame : AdvancedVersion.Style.Default);
        }

        void FixedUpdate()
        {
            AdvVer?.UpdateDisplay();
        }
    }
}
