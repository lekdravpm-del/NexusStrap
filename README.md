<p align="center">
    <img src="https://img.shields.io/github/v/release/lekdravpm-del/NexusStrap?label=release&color=ff69b4" alt="Release">
    <img src="https://img.shields.io/github/downloads/lekdravpm-del/NexusStrap/total?color=8b5cf6" alt="Downloads">
    <img src="https://img.shields.io/github/stars/lekdravpm-del/NexusStrap?color=22c55e" alt="Stars">
    <img src="https://img.shields.io/badge/built%20by-one%20guy-3b82f6" alt="Built by one guy">
</p>

<h1 align="center">
    NexusStrap
</h1>

<p align="center">
    A Roblox bootstrapper built by <b>one developer</b> in their spare time,
    focused on making Roblox run lighter, look cooler, and behave.
</p>

<p align="center">
    <a href="https://discord.gg/hvhEDxFek"><b>Join the Discord</b></a> — come say hi, report bugs, or just vibe.
</p>

<p align="center">
    Leave a star if you like it. It's the only payment this project accepts. ⭐
</p>

---

> [!WARNING]
> **Only download from this repo's Releases page.** Binaries found anywhere else
> are not ours — don't trust them, don't run them. One dev means one source of truth.

> [!TIP]
> This is a one-man project. Bugs get fixed, features get added, but coffee is required.
> If something's broken, open an Issue or hit the Discord — I actually read those.

---

## What is this?

NexusStrap is a fork of Bloxstrap with one job: **make Roblox feel better on your PC.**

### Performance

- **Memory Limiter** — cap how much RAM Roblox is allowed to touch
- **RAM Cleaner** — sweep up the leftovers when Roblox is done eating
- **Auto-close Crash Handler** — kill the invisible memory hog before it hogs
- **Process priority control** — decide who's boss on your machine
- **Multi-instance launching** — several games, no mutex war
- and yes it has the feature to clean on version-XXXXXX so save up space

> [!NOTE]
> The Memory Limiter alone usually cuts Roblox RAM usage by a noticeable chunk
> on longer sessions. That's the whole point.

### The Cursor Stuff

- **Shift-lock cursor themes** with live preview — see it before you commit
- **Custom Cursor Sets** — build whole packs, switch with one click
- **Export / Import** sets — share your pack with friends
- **Custom death sounds** — replace that noise with something better
- Every image is **auto-scaled to Roblox's real cursor size**, so your PNG won't
  render 10 feet tall in-game

### Looks

- **Femboy theme** — yes, it's pink. No, we don't apologize.
- **8-bit bootstrapper icon** for the retro crowd
- Custom fonts, icons, and bootstrapper appearance
- **Spotlight guide** that teaches you the app while you use it

### Everything Else

- FastFlag editor with profiles, presets, undo/redo, and ban-flag warnings
- Custom Discord RPC showing what you're actually doing
- Game shortcuts for one-click joining
- Account manager, log viewer, settings import/export
- One-click uninstall that cleans up after itself

> [!IMPORTANT]
> Your settings survive exe swaps. Replace the exe, launch, done —
> no reinstall, no setup, no drama.

---

## Install

1. Grab `NexusStrap.exe` from **[Releases](https://github.com/lekdravpm-del/NexusStrap/releases/latest)**
2. Run it, finish the setup
3. Launch and enjoy

> [!TIP]
> Windows 10 or 11 required. Windows 7 is retired — let it rest.

## Build It Yourself

Needs [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
# Build
dotnet build NexusStrap.slnx -c Release

# Publish single .exe
dotnet publish NexusStrap/NexusStrap.csproj -c Release -r win-x64 -p:SelfContained=false -p:PublishSingleFile=true -p:PublishReadyToRun=false
```

Output: `NexusStrap/bin/Release/net10.0-windows/win-x64/publish/NexusStrap.exe`

> [!WARNING]
> Building from source? Great. But note: a single dev maintains this,
> so keep the main branch clean — or at least apologize before breaking it.

---

## FAQ

**Can it get you banned?**

No. NexusStrap changes the launcher and appearance — not gameplay.

**Is it a virus?**

No, it's open source. Read the code, build it yourself, or don't

**Will you add my feature?**

Maybe! Open an Issue or ask on the Discord. Good ideas get built.

## Credits

Born from [Bloxstrap](https://github.com/pizzaboxer/bloxstrap) by pizzaboxer.
Raised solo. Forks welcome.

© NexusStrap
