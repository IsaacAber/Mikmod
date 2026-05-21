# Mikmod

A [MelonLoader](https://melonwiki.xyz/) mod for **Mikmak2** by DIC NetworkTechnologies.

## Features

### Mod Menu (`Left Shift`)
An in-game draggable window that shows mod controls relevant to the current scene.

**Any scene (default tab)**
- **FreeSpeech** — type and send arbitrary chat messages to every player in the room, bypassing the game's normal chat restrictions. Messages are encoded as coordinate pairs and decoded by other Mikmod users.
- Configurable **message timeout** (how long the chat bubble stays visible, in seconds).
- Toggle **SmartFox Server (SFS) raw packet logging** to the MelonLoader console.

**Arcade scene**
- **MikDash debug mode** — spoof `IsDebugBuild()` so the game treats the session as a debug build, enabling developer-only code paths in MikDash.

**Spike Game scene (`g_spike`)**
- **Invincibility** — disables spike damage.
- **Freeze Player** — halts all player movement.
- **Spawn items in front of player** — overrides random item placement.
- **Infinite item spawns** — forces the collectible spawner to run every frame.
  - Configurable **spawn iterations** (1–100 items per cycle).
  - Configurable **per-type ratios** (Points / Shield / Life / Laser).

### Debug Overlay (`F3`)
A heads-up display drawn in the top corners of the screen showing:
- Active scene name and path
- Live GameObject and Canvas counts (refreshed every 2 s)
- Main camera position and rotation
- FPS, frame time, delta time
- Resolution and quality level
- Game version
- MelonLoader version and all loaded mods
- Device name, GPU, CPU, and RAM

### Settings (persisted via MelonLoader Preferences)
| Key | Default | Description |
|---|---|---|
| `DisableVsync` | `false` | Forces `vSyncCount = 0` every frame |
| `UnlimitedFps` | `false` | Removes the frame rate cap (`targetFrameRate = -1`) |
| `MessageLifetime` | `5` | Seconds before a FreeSpeech chat bubble auto-hides (0 = never) |

Settings are saved to `UserData/MelonPreferences.cfg` and persist across sessions.

### Other internals
- **ExceptionHandler** — catches unhandled AppDomain, Task, and Unity exceptions and logs them through MelonLogger so they don't silently swallow errors.
- **SFSPatches** — Harmony patches on `SmartFoxClient` that log connect/disconnect events and (optionally) every raw packet.

---

## Requirements

- [MelonLoader](https://melonwiki.xyz/#/?id=requirements) **0.6+** installed on the game
- .NET Framework **4.8**
- Visual Studio 2022

---

## Building

1. Open `Mikmod.sln` in Visual Studio.
2. Add references to the game's managed assemblies and MelonLoader's libraries. These are typically found in:
   ```
   %LocalAppData%\Programs\Mikmak2\Game\MelonLoader\Managed\
   %LocalAppData%\Programs\Mikmak2\Game\MelonLoader\
   ```
3. Build the solution (`Ctrl+Shift+B`). The output `Mikmod.dll` will be placed in `bin\Debug\` or `bin\Release\`.

---

## Installation

1. **Install the base game** and launch it at least once until you reach the login screen, then close it.
2. **Install MelonLoader** on the game using the [MelonLoader installer](https://github.com/LavaGang/MelonLoader/releases). Point it at:
   ```
   %LocalAppData%\Programs\Mikmak2\Game\Mikmak2.exe
   ```
   Run the game once to let MelonLoader generate its folder structure, then close it.
3. **Copy `Mikmod.dll`** into the game's `Mods/` folder:
   ```
   %LocalAppData%\Programs\Mikmak2\Game\Mods\Mikmod.dll
   ```
4. Launch the game. MelonLoader will load the mod automatically on startup.

---

## Controls

| Key | Action |
|---|---|
| `Left Shift` | Toggle the mod menu |
| `F3` | Toggle the debug overlay |

---

## Contributing

Scene-specific hacks live in their own files (`SpikesHack.cs`, `MikDashHack.cs`, etc.). To add hacks for a new scene:

1. Create a new `.cs` file with your Harmony patches.
2. Add a `case "your_scene_name":` block in `ModMenu.cs → DrawWindow()` to expose controls.
3. Open a pull request on [GitHub](https://github.com/IsaacAber/Mikmod).
