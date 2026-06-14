<h1 align="center">
<a href="https://github.com/RepoOwner/RepoName/releases/latest/download/TowerDPSDisplay.dll">
    <img align="left" alt="Icon" height="90" src="Icon.png">
    <img align="right" alt="Download" height="75" src="https://raw.githubusercontent.com/gurrenm3/BTD-Mod-Helper/master/BloonsTD6%20Mod%20Helper/Resources/DownloadBtn.png">
</a>
Tower DPS Display
</h1>

A [BTD Mod Helper](https://github.com/gurrenm3/BTD-Mod-Helper) mod that adds a **live DPS**
and **average DPS** readout to the tower selection menu, right next to the vanilla Damage
("pops") and Cash Generated counters. Money-making towers also get a cash/second line.

- **Live DPS** — damage gained over the last *N* seconds (a sliding window you can set).
- **Average DPS** — total damage divided by how long the tower has existed.
- **$/s** — the same two rates for generated cash (only shown for towers that earn money).

Towers are tracked from the moment they're placed, so the average reflects the tower's whole
life, not just the time since you last clicked it. The rates use the game's scaled clock, so
pausing freezes them and fast-forward reports the genuinely higher real-time output.

## Settings

Everything is configurable from **Mods → Tower DPS Display → Mod Settings**: toggle live DPS,
average DPS, and the cash line independently; set the live-DPS window length; and nudge the
readout's X/Y position if it doesn't sit perfectly on your resolution.

## Building

This is a standard Mod Helper source mod. Open the solution and build (Debug or Release); the
`btd6.targets` import wires up the game references and copies the finished `TowerDPSDisplay.dll`
into your `BloonsTD6\Mods` folder. If BTD6 isn't at the default Steam path, set the game
directory in `btd6.targets`. There's also a GitHub Actions workflow that builds on push and
publishes a release when you push a tag (using `LATEST.md` as the body).

## If a game update breaks it

The two fragile spots are isolated and commented:

- `TowerDpsTracker.ReadDamage` / `ReadCash` — the only two game-internal fields read
  (`Tower.damageDealt` and `Tower.cashEarned`). Fix here if a future build renames them;
  confirm current names with UnityExplorer or an Il2Cpp dump.
- `DpsReadout.TryCreate` — the `ModHelperText.Create(...)` call, if your Mod Helper version
  uses a different overload.

Because Mod Helper wraps its hooks in resilient patching and the per-frame work is inside
try/catch, a mismatch degrades gracefully (the readout just won't show) rather than crashing.

[![Requires BTD6 Mod Helper](https://raw.githubusercontent.com/gurrenm3/BTD-Mod-Helper/master/banner.png)](https://github.com/gurrenm3/BTD-Mod-Helper#readme)
