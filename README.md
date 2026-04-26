# YT Panel

<img width="400" height="300" alt="image" src="https://github.com/user-attachments/assets/768b136d-397c-4332-9071-972cd5fec6ff" />



YT Panel is a small Windows desktop controller for YouTube Music. It opens a dedicated Chrome profile, attaches to YouTube Music through the Chrome DevTools Protocol, and gives you a compact always-on-top panel for playback and volume control.

## Features

- Compact YouTube Music control panel
- Album artwork, track title, and volume display
- Previous, play/pause, and next controls
- YouTube Music volume slider
- Right-click the slider to mute or unmute
- Mouse wheel on the focused slider changes volume by 1%
- `Ctrl + Mouse Wheel` changes panel opacity
- `Pin` toggles always-on-top mode
- Uses a separate Chrome profile so your app session can stay signed in

## Controls

| Action | Result |
| --- | --- |
| Click album art | Open YouTube Music |
| `<<` | Previous track |
| `>` / `II` | Play / pause |
| `>>` | Next track |
| Drag slider | Change YouTube Music volume |
| Focus slider + mouse wheel | Change volume by 1% |
| Right-click slider | Mute / unmute |
| `Ctrl + Mouse Wheel` | Change panel opacity |
| `Pin` | Toggle always-on-top |

## Requirements

- Windows
- Google Chrome
- .NET 10 SDK

## Run

```powershell
dotnet run
```

## Build

```powershell
dotnet build
```

## How It Works

The app launches Chrome with a dedicated remote debugging port:

```text
--remote-debugging-port=47822
```

It then finds the active YouTube Music tab, reads media session information, and dispatches playback or volume commands to that tab. The Chrome profile is stored separately from your normal Chrome profile, so signing in once keeps the YouTube Music session available for later app launches.
