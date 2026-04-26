using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace yt_panel;

public partial class Form1 : Form, IMessageFilter
{
    private const int WmNclButtonDown = 0xA1;
    private const int WmMouseWheel = 0x20A;
    private const int HtCaption = 0x2;
    private const int ChromeDebugPort = 47822;

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(3) };

    private readonly System.Windows.Forms.Timer refreshTimer = new();

    private bool isPinned = true;
    private bool isSyncingVolumeSlider;
    private int lastNonZeroVolume = 50;
    private string? lastArtworkUrl;

    public Form1()
    {
        InitializeComponent();
        ApplyAppIcon();
        BuildInterface();
        Application.AddMessageFilter(this);

        refreshTimer.Interval = 5000;
        refreshTimer.Tick += async (_, _) => await RefreshNowPlayingAsync();
        refreshTimer.Start();
    }

    private void ApplyAppIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "settings-sliders_3917103.ico");
        if (File.Exists(iconPath))
        {
            Icon = new Icon(iconPath);
            ShowIcon = true;
        }
    }

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    private void BuildInterface()
    {
        header.MouseDown += DragWindow;
        statusLabel.MouseDown += DragWindow;

        albumArtBox.Cursor = Cursors.Hand;
        albumArtBox.Click += (_, _) => OpenControlledChrome("https://music.youtube.com/");
        ConfigureButton(previousButton, "<<", 140, 34, async (_, _) => await ClickYoutubeButtonAsync("previous"));
        ConfigureButton(playPauseButton, "Play / Pause", 140, 34, async (_, _) => await TogglePlayPauseAsync());
        ConfigureButton(nextButton, ">>", 140, 34, async (_, _) => await ClickYoutubeButtonAsync("next"));
        ConfigureVolumeSlider();
        ConfigureButton(topMostButton, "Pin", 130, 32, (_, _) => TogglePinned());
        ConfigureButton(closeButton, "X", 36, 30, (_, _) => Close());
    }

    private void ConfigureVolumeSlider()
    {
        volumeSlider.ValueChanged += async (_, _) =>
        {
            if (isSyncingVolumeSlider)
            {
                return;
            }

            await SetYoutubeVolumeAsync(volumeSlider.Value);
        };
        volumeSlider.MouseUp += async (_, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                await ToggleYoutubeMuteWithRestoreAsync();
            }
        };
        volumeSlider.MouseWheel += (_, e) =>
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                return;
            }

            volumeSlider.Value += e.Delta > 0 ? 1 : -1;
        };
    }

    private void ConfigureButton(Button button, string text, int width, int height, EventHandler onClick)
    {
        button.Click += onClick;
    }

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg != WmMouseWheel ||
            !ContainsFocus ||
            (ModifierKeys & Keys.Control) != Keys.Control ||
            !ClientRectangle.Contains(PointToClient(Cursor.Position)))
        {
            return false;
        }

        var delta = unchecked((short)((long)m.WParam >> 16));
        AdjustOpacity(delta);
        return true;
    }

    private void AdjustOpacity(int wheelDelta)
    {
        if (wheelDelta == 0)
        {
            return;
        }

        var delta = wheelDelta > 0 ? 0.05 : -0.05;
        Opacity = Math.Max(0.1, Math.Min(1.0, Opacity + delta));
        statusLabel.Text = $"Opacity {Math.Round(Opacity * 100)}%";
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Application.RemoveMessageFilter(this);
        base.OnFormClosed(e);
    }

    private async Task ToggleYoutubeMuteWithRestoreAsync()
    {
        var restoreVolume = Math.Max(1, Math.Min(100, lastNonZeroVolume));
        var script = $$"""
            (() => {
                const player = document.querySelector('#movie_player');
                const video = document.querySelector('video');
                if (!player && !video) return { ok:false };

                const getVolume = () => player && typeof player.getVolume === 'function'
                    ? Math.round(player.getVolume())
                    : video ? Math.round(video.volume * 100) : 0;
                const isMuted = () => player && typeof player.isMuted === 'function'
                    ? player.isMuted()
                    : !!video?.muted;
                const setVolume = (value) => {
                    const volume = Math.max(1, Math.min(100, Math.round(value)));
                    if (player && typeof player.setVolume === 'function') {
                        player.setVolume(volume);
                    } else if (video) {
                        video.volume = volume / 100;
                    }
                    return volume;
                };
                const syncVisibleSlider = (volume) => {
                    const slider = document.querySelector('ytmusic-player-bar #volume-slider')
                        || document.querySelector('ytmusic-player-bar tp-yt-paper-slider');
                    if (!slider) return;

                    slider.value = volume;
                    slider.immediateValue = volume;
                    slider.oldValue = volume;
                    slider.setAttribute('value', String(volume));
                    slider.setAttribute('aria-valuenow', String(volume));
                    slider.setAttribute('aria-valuetext', String(volume));

                    if (typeof slider.set === 'function') {
                        slider.set('value', volume);
                        slider.set('immediateValue', volume);
                    }
                    if (typeof slider._setImmediateValue === 'function') {
                        slider._setImmediateValue(volume);
                    }
                    if (typeof slider._update === 'function') {
                        slider._update();
                    }
                };

                let volume = getVolume();
                const previousVolume = volume > 0 ? volume : {{restoreVolume}};

                if (isMuted() || volume === 0) {
                    volume = setVolume({{restoreVolume}});
                    if (player && typeof player.unMute === 'function') {
                        player.unMute();
                    }
                    if (video) {
                        video.muted = false;
                    }
                    syncVisibleSlider(volume);
                    requestAnimationFrame(() => syncVisibleSlider(volume));
                    return { ok:true, muted:false, volume, previousVolume };
                }

                if (player && typeof player.mute === 'function') {
                    player.mute();
                } else if (video) {
                    video.muted = true;
                }

                return { ok:true, muted:true, volume, previousVolume };
            })()
            """;

        var result = await EvaluateOnYoutubeTabAsync(script);
        if (result is null || !GetBool(result.Value, "ok"))
        {
            statusLabel.Text = "Open app Chrome first";
            return;
        }

        if (GetInt(result.Value, "previousVolume") is { } previousVolume && previousVolume > 0)
        {
            lastNonZeroVolume = previousVolume;
        }

        await RefreshNowPlayingAsync();
    }

    private void OpenControlledChrome(string url)
    {
        var chromePath = FindChromePath();
        if (chromePath is null)
        {
            MessageBox.Show("Chrome not found.", "YT Panel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Directory.CreateDirectory(GetProfilePath());

        Process.Start(new ProcessStartInfo
        {
            FileName = chromePath,
            Arguments = $"--remote-debugging-port={ChromeDebugPort} --user-data-dir=\"{GetProfilePath()}\" --new-window \"{url}\"",
            UseShellExecute = false
        });

        statusLabel.Text = "Connecting...";
        _ = Task.Run(async () =>
        {
            await Task.Delay(1200);
            BeginInvoke(async () => await RefreshNowPlayingAsync());
        });
    }

    private async Task SetYoutubeVolumeAsync(int targetVolume)
    {
        var targetVolumeText = Math.Max(0, Math.Min(100, targetVolume)).ToString(CultureInfo.InvariantCulture);
        var script = $$"""
            (() => {
                const player = document.querySelector('#movie_player');
                const video = document.querySelector('video');
                if (!player && !video) return { ok:false };

                const current = player && typeof player.getVolume === 'function'
                    ? player.getVolume()
                    : Math.round(video.volume * 100);
                const volume = {{targetVolumeText}};

                if (player && typeof player.setVolume === 'function') {
                    player.setVolume(volume);
                    if (typeof player.unMute === 'function') {
                        player.unMute();
                    }
                } else {
                    video.volume = volume / 100;
                    video.muted = false;
                }

                const syncVisibleSlider = () => {
                    const slider = document.querySelector('ytmusic-player-bar #volume-slider')
                        || document.querySelector('ytmusic-player-bar tp-yt-paper-slider');
                    if (!slider) return;

                    slider.value = volume;
                    slider.immediateValue = volume;
                    slider.oldValue = volume;
                    slider.setAttribute('value', String(volume));
                    slider.setAttribute('aria-valuenow', String(volume));
                    slider.setAttribute('aria-valuetext', String(volume));

                    if (typeof slider.set === 'function') {
                        slider.set('value', volume);
                        slider.set('immediateValue', volume);
                    }
                    if (typeof slider._setImmediateValue === 'function') {
                        slider._setImmediateValue(volume);
                    }
                    if (typeof slider._update === 'function') {
                        slider._update();
                    }
                };

                syncVisibleSlider();
                requestAnimationFrame(syncVisibleSlider);

                const now = Date.now();
                const forever = now + 315360000000;
                const storedVolume = JSON.stringify({
                    data: String(volume),
                    creation: now,
                    expiration: forever
                });
                const storedMuted = JSON.stringify({
                    data: "false",
                    creation: now,
                    expiration: forever
                });

                localStorage.setItem('yt-player-volume', storedVolume);
                localStorage.setItem('yt-player-muted', storedMuted);
                sessionStorage.setItem('yt-player-volume', storedVolume);
                sessionStorage.setItem('yt-player-muted', storedMuted);
                return {
                    ok:true,
                    volume,
                    muted: player && typeof player.isMuted === 'function'
                        ? player.isMuted()
                        : !!video?.muted
                };
            })()
            """;

        var result = await EvaluateOnYoutubeTabAsync(script);
        if (result is null || !GetBool(result.Value, "ok"))
        {
            statusLabel.Text = "Open Music first";
            return;
        }

        if (GetInt(result.Value, "volume") is { } volume)
        {
            if (volume > 0)
            {
                lastNonZeroVolume = volume;
            }

            statusLabel.Text = $"Volume {volume}%";
        }

        await RefreshNowPlayingAsync();
    }

    private async Task ToggleYoutubeMuteAsync()
    {
        const string buttonScript = """
            (() => {
                const selectors = [
                    'ytmusic-player-bar .volume',
                    'ytmusic-player-bar tp-yt-paper-icon-button[title]',
                    '.ytp-mute-button',
                    'button[aria-label*="Mute"]',
                    'button[aria-label*="Unmute"]',
                    'button[aria-label*="음소거"]'
                ];
                const button = selectors.map(s => document.querySelector(s)).find(Boolean);
                if (!button) return { ok:false };
                const rect = button.getBoundingClientRect();
                return {
                    ok: true,
                    x: Math.round(rect.left + rect.width / 2),
                    y: Math.round(rect.top + rect.height / 2)
                };
            })()
            """;

        var button = await EvaluateOnYoutubeTabAsync(buttonScript);
        if (button is not null &&
            GetBool(button.Value, "ok") &&
            GetInt(button.Value, "x") is { } x &&
            GetInt(button.Value, "y") is { } y &&
            await DispatchMouseClickAsync(x, y))
        {
            await Task.Delay(150);
            await RefreshNowPlayingAsync();
            return;
        }

        const string fallbackScript = """
            (() => {
                const player = document.querySelector('#movie_player');
                if (player && typeof player.isMuted === 'function') {
                    if (player.isMuted()) {
                        if (typeof player.unMute === 'function') player.unMute();
                    } else if (typeof player.mute === 'function') {
                        player.mute();
                    }
                    return {
                        ok:true,
                        volume: typeof player.getVolume === 'function' ? player.getVolume() : null,
                        muted: player.isMuted()
                    };
                }

                const video = document.querySelector('video');
                if (!video) return { ok:false };
                video.muted = !video.muted;
                return { ok:true, volume:Math.round(video.volume * 100), muted:video.muted };
            })()
            """;

        if (await EvaluateOnYoutubeTabAsync(fallbackScript) is null)
        {
            statusLabel.Text = "Open app Chrome first";
        }

        await RefreshNowPlayingAsync();
    }

    private async Task<double?> GetYoutubeVolumeAsync()
    {
        const string script = """
            (() => {
                const player = document.querySelector('#movie_player');
                if (player && typeof player.getVolume === 'function') {
                    return { ok:true, volume:player.getVolume() / 100 };
                }

                const video = document.querySelector('video');
                if (!video) return { ok:false };
                return { ok:true, volume:video.volume };
            })()
            """;

        var result = await EvaluateOnYoutubeTabAsync(script);
        if (result is null ||
            !result.Value.TryGetProperty("volume", out var volume) ||
            volume.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return volume.GetDouble();
    }

    private async Task<bool> ClickYoutubeVolumeAsync(double volume)
    {
        var volumeText = Math.Max(0, Math.Min(1, volume)).ToString("0.00", CultureInfo.InvariantCulture);
        var script = $$"""
            (() => {
                const desired = {{volumeText}};
                const hoverTargets = [
                    '.ytp-mute-button',
                    'ytmusic-player-bar .volume',
                    'ytmusic-player-bar tp-yt-paper-icon-button[title]'
                ];
                const hover = hoverTargets.map(s => document.querySelector(s)).find(Boolean);
                if (hover) {
                    hover.dispatchEvent(new MouseEvent('mouseover', { bubbles:true, view:window }));
                    hover.dispatchEvent(new MouseEvent('mouseenter', { bubbles:true, view:window }));
                }

                const sliderSelectors = [
                    'ytmusic-player-bar #volume-slider',
                    'ytmusic-player-bar tp-yt-paper-slider',
                    'ytmusic-player-bar paper-slider',
                    '.ytp-volume-slider',
                    '.ytp-volume-panel'
                ];
                const slider = sliderSelectors.map(s => document.querySelector(s)).find(Boolean);
                if (!slider) return { ok:false };

                const rect = slider.getBoundingClientRect();
                if (!rect.width || !rect.height) return { ok:false };

                return {
                    ok: true,
                    x: Math.round(rect.left + Math.max(4, Math.min(rect.width - 4, rect.width * desired))),
                    y: Math.round(rect.top + rect.height / 2)
                };
            })()
            """;

        var target = await EvaluateOnYoutubeTabAsync(script);
        if (target is null ||
            !GetBool(target.Value, "ok") ||
            GetInt(target.Value, "x") is not { } x ||
            GetInt(target.Value, "y") is not { } y)
        {
            return false;
        }

        return await DispatchMouseClickAsync(x, y);
    }

    private async Task TogglePlayPauseAsync()
    {
        const string buttonScript = """
            (() => {
                const selectors = [
                    'ytmusic-player-bar #play-pause-button',
                    'ytmusic-player-bar .play-pause-button',
                    'ytmusic-player-bar tp-yt-paper-icon-button',
                    'ytmusic-player-bar yt-icon-button',
                    'tp-yt-paper-icon-button.play-pause-button',
                    'yt-icon-button.play-pause-button',
                    'button[aria-label="Play"]',
                    'button[aria-label="Pause"]',
                    'button[aria-label="재생"]',
                    'button[aria-label="일시중지"]'
                ];
                const button = selectors.map(s => document.querySelector(s)).find(Boolean);
                if (button) {
                    const rect = button.getBoundingClientRect();
                    return {
                        ok: true,
                        source: 'button',
                        x: Math.round(rect.left + rect.width / 2),
                        y: Math.round(rect.top + rect.height / 2)
                    };
                }

                return { ok:false };
            })()
            """;

        var button = await EvaluateOnYoutubeTabAsync(buttonScript);
        if (button is not null && GetBool(button.Value, "ok"))
        {
            var x = GetInt(button.Value, "x");
            var y = GetInt(button.Value, "y");
            if (x is not null && y is not null && await DispatchMouseClickAsync(x.Value, y.Value))
            {
                await Task.Delay(300);
                await RefreshNowPlayingAsync();
                return;
            }
        }

        if (await DispatchKeyAsync("KeyK", "k"))
        {
            await Task.Delay(250);
            await RefreshNowPlayingAsync();
            return;
        }

        const string videoScript = """
            (() => {
                const video = document.querySelector('video');
                if (!video) return { ok:false };
                video.paused ? video.play() : video.pause();
                return { ok:true, source:'video' };
            })()
            """;

        if (await EvaluateOnYoutubeTabAsync(videoScript) is null)
        {
            statusLabel.Text = "Open app Chrome first";
        }

        await Task.Delay(300);
        await RefreshNowPlayingAsync();
    }

    private async Task ClickYoutubeButtonAsync(string direction)
    {
        var selectors = direction == "next"
            ? "['ytmusic-player-bar .next-button', 'ytmusic-player-bar #right-controls .next-button', '.next-button button', 'button[aria-label=\"Next\"]', 'button[aria-label=\"다음\"]']"
            : "['ytmusic-player-bar .previous-button', 'ytmusic-player-bar #left-controls .previous-button', '.previous-button button', 'button[aria-label=\"Previous\"]', 'button[aria-label=\"이전\"]']";

        var buttonScript = $$"""
            (() => {
                const button = {{selectors}}.map(s => document.querySelector(s)).find(Boolean);
                if (!button) return { ok:false };
                const rect = button.getBoundingClientRect();
                if (!rect.width || !rect.height) return { ok:false };
                return {
                    ok: true,
                    x: Math.round(rect.left + rect.width / 2),
                    y: Math.round(rect.top + rect.height / 2)
                };
            })()
            """;

        var button = await EvaluateOnYoutubeTabAsync(buttonScript);
        if (button is not null &&
            GetBool(button.Value, "ok") &&
            GetInt(button.Value, "x") is { } x &&
            GetInt(button.Value, "y") is { } y &&
            await DispatchMouseClickAsync(x, y))
        {
            await Task.Delay(250);
            await RefreshNowPlayingAsync();
            return;
        }

        var fallbackScript = $$"""
            (() => {
                const button = {{selectors}}.map(s => document.querySelector(s)).find(Boolean);
                if (!button) return { ok:false };
                button.click();
                return { ok:true };
            })()
            """;

        if (await EvaluateOnYoutubeTabAsync(fallbackScript) is null)
        {
            statusLabel.Text = "Open app Chrome first";
        }

        await Task.Delay(250);
        await RefreshNowPlayingAsync();
    }

    private async Task RefreshNowPlayingAsync()
    {
        const string script = """
            (() => {
                const player = document.querySelector('#movie_player');
                const video = document.querySelector('video');
                const media = navigator.mediaSession && navigator.mediaSession.metadata;
                const artwork = media?.artwork?.length
                    ? media.artwork[media.artwork.length - 1].src
                    : (document.querySelector('ytmusic-player-bar img.image')?.src
                       || document.querySelector('img.yt-core-image')?.src
                       || document.querySelector('img#img')?.src
                       || '');

                return {
                    ok: !!video || !!media,
                    hasVideo: !!video,
                    title: media?.title || document.querySelector('ytmusic-player-bar .title')?.textContent?.trim() || document.title.replace(' - YouTube Music', '').replace(' - YouTube', ''),
                    artist: media?.artist || document.querySelector('ytmusic-player-bar .byline')?.textContent?.trim() || '',
                    artwork,
                    volume: player && typeof player.getVolume === 'function'
                        ? player.getVolume()
                        : video ? Math.round(video.volume * 100) : null,
                    muted: player && typeof player.isMuted === 'function'
                        ? player.isMuted()
                        : video ? video.muted : false,
                    paused: video ? video.paused : true
                };
            })()
            """;

        var info = await EvaluateOnYoutubeTabAsync(script);
        if (info is null)
        {
            statusLabel.Text = "Not attached";
            return;
        }

        var title = GetString(info.Value, "title");
        var artist = GetString(info.Value, "artist");
        var artwork = GetString(info.Value, "artwork");
        var hasVideo = GetBool(info.Value, "hasVideo");
        var muted = GetBool(info.Value, "muted");
        var paused = GetBool(info.Value, "paused");
        var volume = GetInt(info.Value, "volume");

        if (volume is > 0 && !muted)
        {
            lastNonZeroVolume = volume.Value;
        }

        playPauseButton.Text = paused ? "\u25B6" : "II";
        trackLabel.Text = string.IsNullOrWhiteSpace(title)
            ? "Attached - choose a song"
            : string.IsNullOrWhiteSpace(artist) ? title : $"{title} - {artist}";
        statusLabel.Text = hasVideo && volume is not null
            ? $"Volume {volume}%{(muted ? " muted" : "")}"
            : "Attached";
        if (volume is not null && !volumeSlider.Capture)
        {
            isSyncingVolumeSlider = true;
            volumeSlider.Value = Math.Max(volumeSlider.Minimum, Math.Min(volumeSlider.Maximum, volume.Value));
            isSyncingVolumeSlider = false;
        }

        if (!string.IsNullOrWhiteSpace(artwork) && artwork != lastArtworkUrl)
        {
            await LoadArtworkAsync(artwork);
        }
    }

    private async Task<JsonElement?> EvaluateOnYoutubeTabAsync(string expression)
    {
        try
        {
            var tab = await FindYoutubeTabAsync();
            if (tab is null)
            {
                return null;
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri(tab.WebSocketUrl), CancellationToken.None);

            var command = JsonSerializer.Serialize(new
            {
                id = 1,
                method = "Runtime.evaluate",
                @params = new
                {
                    expression,
                    awaitPromise = true,
                    returnByValue = true
                }
            });

            await socket.SendAsync(Encoding.UTF8.GetBytes(command), WebSocketMessageType.Text, true, CancellationToken.None);

            var buffer = new byte[64 * 1024];
            using var stream = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                stream.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            using var document = JsonDocument.Parse(stream.ToArray());
            if (!document.RootElement.TryGetProperty("result", out var resultElement) ||
                !resultElement.TryGetProperty("result", out var runtimeResult) ||
                !runtimeResult.TryGetProperty("value", out var value))
            {
                return null;
            }

            return value.Clone();
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> DispatchMouseClickAsync(int x, int y)
    {
        try
        {
            var tab = await FindYoutubeTabAsync();
            if (tab is null)
            {
                return false;
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri(tab.WebSocketUrl), CancellationToken.None);

            await SendCdpCommandAsync(socket, 1, "Input.dispatchMouseEvent", new
            {
                type = "mouseMoved",
                x,
                y,
                button = "none"
            });
            await SendCdpCommandAsync(socket, 2, "Input.dispatchMouseEvent", new
            {
                type = "mousePressed",
                x,
                y,
                button = "left",
                clickCount = 1
            });
            await SendCdpCommandAsync(socket, 3, "Input.dispatchMouseEvent", new
            {
                type = "mouseReleased",
                x,
                y,
                button = "left",
                clickCount = 1
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> DispatchMouseDragAsync(int startX, int startY, int endX, int endY)
    {
        try
        {
            var tab = await FindYoutubeTabAsync();
            if (tab is null)
            {
                return false;
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri(tab.WebSocketUrl), CancellationToken.None);

            await SendCdpCommandAsync(socket, 1, "Input.dispatchMouseEvent", new
            {
                type = "mouseMoved",
                x = startX,
                y = startY,
                button = "none"
            });
            await SendCdpCommandAsync(socket, 2, "Input.dispatchMouseEvent", new
            {
                type = "mousePressed",
                x = startX,
                y = startY,
                button = "left",
                clickCount = 1
            });

            for (var step = 1; step <= 5; step++)
            {
                var x = startX + ((endX - startX) * step / 5);
                var y = startY + ((endY - startY) * step / 5);
                await SendCdpCommandAsync(socket, 2 + step, "Input.dispatchMouseEvent", new
                {
                    type = "mouseMoved",
                    x,
                    y,
                    button = "left",
                    buttons = 1
                });
            }

            await SendCdpCommandAsync(socket, 8, "Input.dispatchMouseEvent", new
            {
                type = "mouseReleased",
                x = endX,
                y = endY,
                button = "left",
                clickCount = 1
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> DispatchKeyAsync(string code, string key, bool shift = false)
    {
        try
        {
            var tab = await FindYoutubeTabAsync();
            if (tab is null)
            {
                return false;
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri(tab.WebSocketUrl), CancellationToken.None);

            var modifiers = shift ? 8 : 0;
            await SendCdpCommandAsync(socket, 1, "Input.dispatchKeyEvent", new
            {
                type = "keyDown",
                code,
                key,
                windowsVirtualKeyCode = char.ToUpperInvariant(key[0]),
                nativeVirtualKeyCode = char.ToUpperInvariant(key[0]),
                modifiers
            });
            await SendCdpCommandAsync(socket, 2, "Input.dispatchKeyEvent", new
            {
                type = "keyUp",
                code,
                key,
                windowsVirtualKeyCode = char.ToUpperInvariant(key[0]),
                nativeVirtualKeyCode = char.ToUpperInvariant(key[0]),
                modifiers
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task SendCdpCommandAsync(ClientWebSocket socket, int id, string method, object parameters)
    {
        var command = JsonSerializer.Serialize(new
        {
            id,
            method,
            @params = parameters
        });

        await socket.SendAsync(Encoding.UTF8.GetBytes(command), WebSocketMessageType.Text, true, CancellationToken.None);

        var buffer = new byte[8192];
        WebSocketReceiveResult result;
        do
        {
            result = await socket.ReceiveAsync(buffer, CancellationToken.None);
        }
        while (!result.EndOfMessage);
    }

    private async Task<ChromeTab?> FindYoutubeTabAsync()
    {
        var json = await Http.GetStringAsync($"http://127.0.0.1:{ChromeDebugPort}/json");
        using var document = JsonDocument.Parse(json);

        var candidates = new List<(ChromeTab Tab, string Url, int Score)>();
        foreach (var tab in document.RootElement.EnumerateArray())
        {
            var url = GetString(tab, "url");
            if (!url.Contains("music.youtube.com", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var socket = GetString(tab, "webSocketDebuggerUrl");
            if (!string.IsNullOrWhiteSpace(socket))
            {
                var score = url.Contains("/watch", StringComparison.OrdinalIgnoreCase) ? 30 : 0;

                candidates.Add((new ChromeTab(socket), url, score));
            }
        }

        return candidates
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Url)
            .FirstOrDefault()
            .Tab;
    }

    private async Task LoadArtworkAsync(string url)
    {
        try
        {
            var bytes = await Http.GetByteArrayAsync(url);
            using var stream = new MemoryStream(bytes);
            using var image = Image.FromStream(stream);
            var bitmap = new Bitmap(image);
            var previous = albumArtBox.Image;
            albumArtBox.Image = bitmap;
            previous?.Dispose();
            lastArtworkUrl = url;
        }
        catch
        {
            lastArtworkUrl = null;
        }
    }

    private static string? FindChromePath()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "Application", "chrome.exe")
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static string GetProfilePath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "YT Panel",
            "ChromeProfile");
    }

    private static string GetString(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    private static int? GetInt(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : null;
    }

    private static bool GetBool(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.True;
    }

    private void TogglePinned()
    {
        isPinned = !isPinned;
        TopMost = isPinned;
        topMostButton.Text = isPinned ? "Pin" : "Unpin";
    }

    private void DragWindow(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        ReleaseCapture();
        SendMessage(Handle, WmNclButtonDown, HtCaption, 0);
    }

    private sealed record ChromeTab(string WebSocketUrl);
}
