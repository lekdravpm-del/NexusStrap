<p align="center">
    <img src="https://img.shields.io/github/v/release/lekdravpm-del/NexusStrap?label=Latest%20Release" alt="Latest Release">
    <img src="https://img.shields.io/github/downloads/lekdravpm-del/NexusStrap/total" alt="Total Downloads">
    <img src="https://img.shields.io/github/stars/lekdravpm-del/NexusStrap" alt="Stars">
</p>

<p align="center">
    <b>Leave a star if you like the project! ⭐️</b>
</p>

# NexusStrap

NexusStrap is a fork of Bloxstrap focused on performance, memory efficiency, and cursor customization.

> [!IMPORTANT]
> NexusStrap supports Windows 10 and above only.

## Installation

1. Download the latest version 👉 [Releases](https://github.com/lekdravpm-del/NexusStrap/releases/latest)
2. Run the Exe and finish the setup
3. Launch NexusStrap
4. Enjoy a lighter, cleaner Roblox

## Features

- **Memory Limiter** — cap Roblox's RAM usage and clean up leftover memory
- **Shift-lock cursor themes** with live preview
- **Custom Cursor Sets** — build, switch, export, and import packs
- **Custom death sounds**
- **Auto-close Crash Handler** — frees up memory
- **Process priority control**
- **Multi-instance launching**
- **Femboy theme** and other custom themes
- **8-bit bootstrapper icon**
- FastFlag editor with profiles and ban-flag warnings
- Custom Discord RPC, game shortcuts, account manager, and more

## Frequently Asked Questions (FAQ)

**Can it get you banned?**

No. NexusStrap doesn't modify Roblox gameplay files — only the launcher and client appearance, like any other bootstrapper.

**Is it a virus?**

No. It's open source. Read the code or build it yourself.

## How to Fork

NexusStrap is built using **C# and .NET 10**.

1. Go to: https://github.com/lekdravpm-del/NexusStrap
2. Click **Fork** (top right)
3. This creates your own copy under your GitHub account

## Building from Source

```bash
# Build
dotnet build NexusStrap.slnx -c Release

# Publish single .exe
dotnet publish NexusStrap/NexusStrap.csproj -c Release -r win-x64 -p:SelfContained=false -p:PublishSingleFile=true -p:PublishReadyToRun=false
