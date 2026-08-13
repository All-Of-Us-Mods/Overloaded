global using static Reactor.Utilities.Logger<Reactor.ReactorPlugin>;
using System.Linq;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;

namespace CrowdedMod;

[BepInAutoPlugin("dev.allofus.overloaded", "Overloaded")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnHost)]
[BepInDependency("gg.reactor.debugger", BepInDependency.DependencyFlags.SoftDependency)]
public partial class CrowdedModPlugin : BasePlugin
{
    public const int MaxPlayers = 127; // could be 254, we will stick to 127 for safety
    public const int MaxImpostors = MaxPlayers / 2;

    private Harmony Harmony { get; } = new(Id);

    public override void Load()
    {
        ReactorCredits.Register<CrowdedModPlugin>(ReactorCredits.AlwaysShow);

        Harmony.PatchAll();

        RemoveVanillaServer();
        Info("Finished loading Overloaded!");
    }

    public static void RemoveVanillaServer()
    {
        var sm = ServerManager.Instance;
        var curRegions = sm.AvailableRegions;
        sm.AvailableRegions = curRegions.Where(region => !IsVanillaServer(region)).ToArray();

        var defaultRegion = ServerManager.DefaultRegions;
        ServerManager.DefaultRegions = defaultRegion.Where(region => !IsVanillaServer(region)).ToArray();

        if (IsVanillaServer(sm.CurrentRegion))
        {
            var region = defaultRegion.FirstOrDefault();
            sm.SetRegion(region);
        }

        Info("Finished removing Vanilla Servers!");
    }

    private static bool IsVanillaServer(IRegionInfo? regionInfo)
        => regionInfo is { TranslateName: 
            StringNames.ServerAS or
            StringNames.ServerEU or
            StringNames.ServerNA
        };
}