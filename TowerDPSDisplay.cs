global using BTD_Mod_Helper.Extensions;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.ModOptions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Simulation.Towers;
using MelonLoader;
using TowerDPSDisplay;

[assembly: MelonInfo(typeof(TowerDPSDisplay.TowerDPSDisplay), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6-Epic")]

namespace TowerDPSDisplay;

public class TowerDPSDisplay : BloonsTD6Mod
{
    private static readonly ModSettingCategory DisplayCat = new("Display") { collapsed = false, order = 1 };
    private static readonly ModSettingCategory PositionCat = new("Position") { collapsed = false, order = 2 };
    private static readonly ModSettingCategory StyleCat = new("Style") { collapsed = false, order = 3 };


    public static readonly ModSettingBool ShowLiveDps = new(true)
    {
        displayName = "Show live DPS",
        category = DisplayCat
    };
    public static readonly ModSettingBool ShowAverageDps = new(true)
    {
        displayName = "Show average DPS",
        category = DisplayCat
    };
    public static readonly ModSettingDouble LiveWindowSeconds = new(1.0)
    {
        displayName = "Live DPS window (seconds)",
        category = DisplayCat,
        min = 0.25,
        max = 10.0,
        slider = true
    };
    public static readonly ModSettingDouble OffsetX = new(0.0)
    {
        displayName = "Readout X offset",
        category = PositionCat,
        min = -2000,
        max = 2000,
        slider = true
    };
    public static readonly ModSettingDouble OffsetY = new(900.0)
    {
        displayName = "Readout Y offset",
        category = PositionCat,
        min = -2000,
        max = 2000,
        slider = true
    };
    public static readonly ModSettingInt FontSize = new(42)
    {
        displayName = "Font size",
        category = StyleCat,
        min = 12,
        max = 120,
        slider = true
    };
    public static readonly ModSettingDouble LetterSpacing = new(-10.0)
    {
        displayName = "Letter spacing (lower = tighter)",
        category = StyleCat,
        min = -30,
        max = 10,
        slider = true
    };
    public static readonly ModSettingDouble LabelGap = new(0.55)
    {
        displayName = "Gap: label to number (em)",
        category = StyleCat,
        min = 0,
        max = 2,
        slider = true
    };
    public static readonly ModSettingDouble GroupGap = new(1.1)
    {
        displayName = "Gap: between DPS and avg (em)",
        category = StyleCat,
        min = 0,
        max = 3,
        slider = true
    };
    public static readonly ModSettingInt ColorR = new(255)
    {
        displayName = "Text colour: Red",
        category = StyleCat,
        min = 0,
        max = 255,
        slider = true
    };
    public static readonly ModSettingInt ColorG = new(255)
    {
        displayName = "Text colour: Green",
        category = StyleCat,
        min = 0,
        max = 255,
        slider = true
    };
    public static readonly ModSettingInt ColorB = new(255)
    {
        displayName = "Text colour: Blue",
        category = StyleCat,
        min = 0,
        max = 255,
        slider = true
    };
    public override void OnApplicationStart()
    {
        ModHelper.Msg<TowerDPSDisplay>("Tower DPS Display loaded!");
    }
    public override void OnTowerCreated(Tower tower, Entity target, Model modelToUse) =>
        TowerDpsTracker.Register(tower);

    public override void OnTowerDestroyed(Tower tower) =>
        TowerDpsTracker.Unregister(tower);

    public override void OnTowerSelected(Tower tower)
    {
        TowerDpsTracker.Register(tower);
        DpsReadout.SetSelected(tower);
    }

    public override void OnTowerDeselected(Tower tower) =>
        DpsReadout.SetSelected(null);

    public override void OnMatchEnd()
    {
        TowerDpsTracker.Clear();
        DpsReadout.Reset();
    }

    public override void OnRestart()
    {
        TowerDpsTracker.Clear();
        DpsReadout.Reset();
    }

    public override void OnUpdate()
    {
        try
        {
            TowerDpsTracker.Tick();
            DpsReadout.Refresh();
        }
        catch
        {
        }
    }
}