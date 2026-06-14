using System;
using System.Collections.Generic;
using Il2CppAssets.Scripts.Simulation.Towers;
using UnityEngine;
using Mod = TowerDPSDisplay.TowerDPSDisplay;

namespace TowerDPSDisplay;

/// <summary>
/// Tracks total damage and cash for every tower over time and turns that into
/// live + average rates. Timing uses <see cref="Time.time"/> (the game's scaled
/// clock), so a paused game freezes the numbers and a fast-forwarded game
/// reports the genuinely higher real-time output.
/// </summary>
public static class TowerDpsTracker
{
    // ---------------------------------------------------------------------
    //  THE ONLY TWO GAME-INTERNAL FIELDS THIS MOD READS.
    //  These are the runtime counters behind the menu's "Damage" (formerly
    //  "Pops") and "Cash Generated" displays. If a future BTD6 build renames
    //  them, this is the single place to fix it. Confirm current names with
    //  UnityExplorer (inspect a live Tower) or an Il2Cpp dump.
    // ---------------------------------------------------------------------
    private static double ReadDamage(Tower t) => t.damageDealt;
    private static double ReadCash(Tower t) => t.cashEarned;

    private readonly struct Sample
    {
        public readonly float Time;
        public readonly double Damage;
        public readonly double Cash;

        public Sample(float time, double damage, double cash)
        {
            Time = time;
            Damage = damage;
            Cash = cash;
        }
    }

    private sealed class Record
    {
        public Tower? Tower;
        public float BirthTime;
        public double BirthDamage;
        public double BirthCash;
        public readonly Queue<Sample> Window = new();
        public Sample Latest;
        public bool HasLatest;
    }

    // Keyed by the Il2Cpp object pointer, stable for a tower's lifetime.
    private static readonly Dictionary<IntPtr, Record> Records = new();

    public static void Register(Tower? tower)
    {
        if (tower == null) return;

        var key = tower.Pointer;
        if (Records.ContainsKey(key)) return;

        double dmg, cash;
        try
        {
            dmg = ReadDamage(tower);
            cash = ReadCash(tower);
        }
        catch
        {
            return; // not readable yet; we'll catch it on Tick
        }

        Records[key] = new Record
        {
            Tower = tower,
            BirthTime = Time.time,
            BirthDamage = dmg,
            BirthCash = cash
        };
    }

    public static void Unregister(Tower? tower)
    {
        if (tower == null) return;
        Records.Remove(tower.Pointer);
    }

    public static void Clear() => Records.Clear();

    /// <summary>Sample every tracked tower for this frame and trim old samples.</summary>
    public static void Tick()
    {
        if (Records.Count == 0) return;

        var now = Time.time;
        var window = Mathf.Max(0.25f, (float) Mod.LiveWindowSeconds);

        List<IntPtr>? dead = null;

        foreach (var kvp in Records)
        {
            var rec = kvp.Value;
            var tower = rec.Tower;

            double dmg, cash;
            try
            {
                if (tower == null) throw new Exception("destroyed");
                dmg = ReadDamage(tower);
                cash = ReadCash(tower);
            }
            catch
            {
                (dead ??= new List<IntPtr>()).Add(kvp.Key);
                continue;
            }

            var sample = new Sample(now, dmg, cash);
            rec.Latest = sample;
            rec.HasLatest = true;
            rec.Window.Enqueue(sample);

            // Keep at least two samples so a rate can always be computed.
            while (rec.Window.Count > 2 && now - rec.Window.Peek().Time > window)
                rec.Window.Dequeue();
        }

        if (dead != null)
            foreach (var key in dead)
                Records.Remove(key);
    }

    public static bool TryGetStats(Tower? tower, out Stats stats)
    {
        stats = default;
        if (tower == null) return false;
        if (!Records.TryGetValue(tower.Pointer, out var rec) || !rec.HasLatest) return false;

        var now = rec.Latest.Time;

        // ---- Live rates: change across the sliding window ----
        double liveDps = 0, liveCash = 0;
        if (rec.Window.Count >= 2)
        {
            var oldest = rec.Window.Peek(); // front of the queue = oldest sample
            var dt = rec.Latest.Time - oldest.Time;
            if (dt > 0.0001f)
            {
                liveDps = (rec.Latest.Damage - oldest.Damage) / dt;
                liveCash = (rec.Latest.Cash - oldest.Cash) / dt;
            }
        }

        // ---- Average rates: total over the tower's whole tracked life ----
        double avgDps = 0, avgCash = 0;
        var life = now - rec.BirthTime;
        if (life > 0.25f)
        {
            avgDps = (rec.Latest.Damage - rec.BirthDamage) / life;
            avgCash = (rec.Latest.Cash - rec.BirthCash) / life;
        }

        stats = new Stats
        {
            LiveDps = Math.Max(0, liveDps),
            AverageDps = Math.Max(0, avgDps),
            LiveCash = Math.Max(0, liveCash),
            AverageCash = Math.Max(0, avgCash)
        };
        return true;
    }

    public struct Stats
    {
        public double LiveDps;
        public double AverageDps;
        public double LiveCash;
        public double AverageCash;
    }

    /// <summary>Format a number BTD6-style: 742, 12.3K, 4.56M, 7.8B...</summary>
    public static string Format(double v)
    {
        if (double.IsNaN(v) || double.IsInfinity(v)) return "0";
        var sign = v < 0 ? "-" : "";
        v = Math.Abs(v);

        if (v < 1_000) return sign + v.ToString("0");
        if (v < 1_000_000) return sign + (v / 1_000d).ToString("0.0") + "K";
        if (v < 1_000_000_000) return sign + (v / 1_000_000d).ToString("0.00") + "M";
        if (v < 1_000_000_000_000) return sign + (v / 1_000_000_000d).ToString("0.00") + "B";
        return sign + (v / 1_000_000_000_000d).ToString("0.00") + "T";
    }
}
