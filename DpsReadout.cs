using System;
using System.Collections.Generic;
using System.Globalization;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppTMPro;
using UnityEngine;
using Mod = TowerDPSDisplay.TowerDPSDisplay;
using TSM = Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu.TowerSelectionMenu;

namespace TowerDPSDisplay;

public static class DpsReadout
{
    private static ModHelperText? _text;
    private static IntPtr _attachedMenu = IntPtr.Zero;
    private static Tower? _selected;

    public static void SetSelected(Tower? tower) => _selected = tower;

    public static void Reset()
    {
        _selected = null;
        _text = null;
        _attachedMenu = IntPtr.Zero;
    }

    public static void Refresh()
    {
        TSM menu;
        try
        {
            menu = TSM.instance;
        }
        catch
        {
            return;
        }

        if (menu == null || _selected == null)
        {
            Hide();
            return;
        }

        if (_text == null || _attachedMenu != menu.Pointer)
        {
            if (!TryCreate(menu)) return;
        }

        if (!TowerDpsTracker.TryGetStats(_selected, out var stats))
        {
            Hide();
            return;
        }

        var body = BuildText(stats);
        if (string.IsNullOrEmpty(body))
        {
            Hide();
            return;
        }

        try
        {
            ApplyStyle();
            _text!.SetText(body);
            _text.gameObject.SetActive(true);
        }
        catch
        {
            Reset();
        }
    }
    private static void ApplyStyle()
    {
        if (_text == null) return;

        _text.RectTransform.anchoredPosition =
            new Vector2((float)Mod.OffsetX, (float)Mod.OffsetY);

        var tmp = _text.Text;
        tmp.fontSize = Mod.FontSize;
        tmp.characterSpacing = (float)Mod.LetterSpacing;
        tmp.color = new Color32((byte)(int)Mod.ColorR, (byte)(int)Mod.ColorG, (byte)(int)Mod.ColorB, 255);
    }

    private static bool TryCreate(TSM menu)
    {
        try
        {
            var info = new Info("TowerDpsReadout",
                (float)Mod.OffsetX,
                (float)Mod.OffsetY,
                760, 170);

            _text = ModHelperText.Create(info, "", 42f, TextAlignmentOptions.Center);
            menu.gameObject.AddModHelperComponent(_text);

            _text.Text.richText = true;

            _attachedMenu = menu.Pointer;
            return true;
        }
        catch (Exception e)
        {
            ModHelper.Warning<Mod>($"Couldn't create the DPS readout: {e.Message}");
            _text = null;
            _attachedMenu = IntPtr.Zero;
            return false;
        }
    }
    private static string Space(double em) =>
        $"<space={em.ToString("0.###", CultureInfo.InvariantCulture)}em>";

    private static string BuildText(TowerDpsTracker.Stats stats)
    {
        var labelGap = Space(Mod.LabelGap);
        var groupGap = Space(Mod.GroupGap);
        var lines = new List<string>();

        if (Mod.ShowLiveDps || Mod.ShowAverageDps)
        {
            var parts = new List<string>();
            if (Mod.ShowLiveDps)
                parts.Add($"DPS{labelGap}{TowerDpsTracker.Format(stats.LiveDps)}");
            if (Mod.ShowAverageDps)
                parts.Add($"avg{labelGap}{TowerDpsTracker.Format(stats.AverageDps)}");
            lines.Add(string.Join(groupGap, parts));
        }

        return string.Join("\n", lines);
    }

    private static void Hide()
    {
        if (_text == null) return;
        try { _text.gameObject.SetActive(false); }
        catch { Reset(); }
    }
}