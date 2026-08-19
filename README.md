<p align="center">
    <b>NEXUSSTRAP</b>
</p>

<p align="center">
    Roblox, but lighter. And with better cursors.
</p>

<p align="center">
    <img src="https://img.shields.io/github/v/release/lekdravpm-del/NexusStrap?label=release" alt="Release">
    <img src="https://img.shields.io/github/downloads/lekdravpm-del/NexusStrap/total" alt="Downloads">
    <img src="https://img.shields.io/github/stars/lekdravpm-del/NexusStrap" alt="Stars">
</p>

---

**The only rule:** download the exe from the Releases page on this repo.
If you got it from somewhere else, you downloaded a stranger's homework.

---

## What does it do?

Roblox eats RAM like it's free. NexusStrap puts it on a diet:

- **Memory Limiter** — hard cap on how much RAM Roblox can touch, plus a RAM cleaner
- **Auto-close Crash Handler** — that invisible process eating your memory, gone
- **Process priority** — make Roblox behave
- **Multi-instance** — run several games at once without the mutex war

## The cursor stuff

- Shift-lock cursor **themes** with live preview — pick it, see it, launch
- **Custom Cursor Sets** — save whole packs, switch with one click, export/import to share
- **Custom death sounds** — replace the death noise with whatever you want
- Images are auto-scaled to Roblox's real cursor size, so nothing renders absurdly huge

## The looks

- **Femboy theme** — it's pink, yes you read that right
- **8-bit bootstrapper icon**
- Custom fonts, custom icons, custom bootstrapper appearance
- Spotlight guide that teaches you the app as you go

## Everything else

- FastFlag editor: profiles, presets, undo/redo, ban-flag warnings
- Custom Discord RPC, game shortcuts, account manager
- Log viewer, settings import/export, one-click uninstall

---

## Install

1. Grab `NexusStrap.exe` from **[Releases](https://github.com/lekdravpm-del/NexusStrap/releases/latest)**
2. Run it, finish setup
3. Launch. Done.

No installer wizard torture. Swap the exe, keep your settings.

## Build it yourself

Needs [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
dotnet build NexusStrap.slnx -c Release
dotnet publish NexusStrap/NexusStrap.csproj -c Release -r win-x64 -p:SelfContained=false -p:PublishSingleFile=true -p:PublishReadyToRun=false
