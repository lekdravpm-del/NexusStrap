<h1 align="center">NexusStrap</h1>

<p align="center">
    The Roblox launcher that respects your RAM, your FPS, and your cursor.
</p>

<p align="center">
    Star this repo if you like it — it fuels the caffeine intake.
</p>

> [!CAUTION]
> Only download NexusStrap from the official repository or Releases page.
> Any other source is a stranger handing you candy in a van. Don't take it.

---

## Why NexusStrap?

Vanilla Roblox is heavy. NexusStrap is the crash diet:

- **Memory Limiter** — cap how much RAM Roblox can eat, plus a built-in RAM cleaner
- **Process Priority Control** — tell Roblox who's boss
- **Auto-close Crash Handler** — that memory-hogging background process, gone
- **Multi-instance support** — because one Roblox wasn't enough

---

## The Cursor Stuff (the good stuff)

- **Shift-lock cursor themes** — pick a theme, see a live preview, it just works
- **Custom Cursor Sets** — build your own packs, switch between them with one click
- **Export/Import** your cursor sets — share them with friends
- **Custom death sounds** — make dying in Roblox sound the way it feels

All custom images are auto-scaled to Roblox's native cursor sizes, so they
won't render 10 feet tall in-game.

---

## Looks

- **Femboy theme** (yes, it's pink. no, we don't apologize)
- **8-bit bootstrapper icon** for the retro gamer in you
- Custom fonts, custom icons, custom everything
- Spotlight guide that walks you through the app

---

## Quality of Life

- FastFlag editor with profiles, presets, undo/redo, and ban-flag warnings
- Game shortcuts for one-click joining
- Custom Discord RPC showing what you're actually doing
- Combined log viewer so you can see what broke
- Account manager with cookie login
- One-click uninstall that cleans up after itself

---

## Download

Grab the latest `NexusStrap.exe` from the **Releases** page.
No installer, no bloat — replace your exe, keep your settings.

## Building from Source

Requirements: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```bash
# Build only
dotnet build NexusStrap.slnx -c Release

# Publish single .exe
dotnet publish NexusStrap/NexusStrap.csproj -c Release -r win-x64 -p:SelfContained=false -p:PublishSingleFile=true -p:PublishReadyToRun=false

## Licensing

This project is **dual-licensed** under `GPL-3.0-or-later` and `Unlicensed`.
