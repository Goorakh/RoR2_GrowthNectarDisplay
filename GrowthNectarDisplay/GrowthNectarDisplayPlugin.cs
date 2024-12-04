using BepInEx;
using GrowthNectarDisplay.BuffIconIndicator;
using GrowthNectarDisplay.ModCompatibility;
using System.Diagnostics;

namespace GrowthNectarDisplay
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class GrowthNectarDisplayPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "GrowthNectarDisplay";
        public const string PluginVersion = "1.0.0";

        static GrowthNectarDisplayPlugin _instance;
        internal static GrowthNectarDisplayPlugin Instance => _instance;

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            SingletonHelper.Assign(ref _instance, this);

            Log.Init(Logger);

            Assets.Load();
            GrowthNectarBuffIndicatorController.StaticInit(Config);

            if (RiskOfOptionsCompat.Enabled)
            {
                RiskOfOptionsCompat.AddOptions();
            }

            stopwatch.Stop();
            Log.Message_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalMilliseconds:F0}ms");
        }

        void OnDestroy()
        {
            SingletonHelper.Unassign(ref _instance, this);
        }
    }
}
