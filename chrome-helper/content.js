console.info("[YT Panel Helper] loaded", location.href);

function getVideo() {
  return document.querySelector("video");
}

function clickFirst(selectors) {
  for (const selector of selectors) {
    const element = document.querySelector(selector);
    if (element) {
      element.click();
      return true;
    }
  }

  return false;
}

function getArtwork() {
  const media = navigator.mediaSession && navigator.mediaSession.metadata;
  if (media && media.artwork && media.artwork.length) {
    return media.artwork[media.artwork.length - 1].src || "";
  }

  return (
    document.querySelector("ytmusic-player-bar img.image")?.src ||
    document.querySelector(".thumbnail-image-wrapper img")?.src ||
    document.querySelector("ytd-video-owner-renderer img")?.src ||
    document.querySelector("img.yt-core-image")?.src ||
    document.querySelector("img#img")?.src ||
    ""
  );
}

function getTitle() {
  const media = navigator.mediaSession && navigator.mediaSession.metadata;
  return (
    media?.title ||
    document.querySelector("ytmusic-player-bar .title")?.textContent?.trim() ||
    document.querySelector("h1 yt-formatted-string")?.textContent?.trim() ||
    document.title.replace(" - YouTube Music", "").replace(" - YouTube", "")
  );
}

function getArtist() {
  const media = navigator.mediaSession && navigator.mediaSession.metadata;
  return (
    media?.artist ||
    document.querySelector("ytmusic-player-bar .byline")?.textContent?.trim() ||
    document.querySelector("#owner #text a")?.textContent?.trim() ||
    ""
  );
}

async function postState() {
  const video = getVideo();

  chrome.runtime.sendMessage(
    {
      kind: "postState",
      state: {
      connected: true,
      hasVideo: Boolean(video),
      title: getTitle(),
      artist: getArtist(),
      artwork: getArtwork(),
      volume: video ? Math.round(video.volume * 100) : null,
      muted: video ? video.muted : false,
      paused: video ? video.paused : true,
      url: location.href
      }
    },
    (response) => {
      if (!response?.ok) {
        console.warn("[YT Panel Helper] state post failed", response?.error);
      }
    }
  );
}

function runCommand(command) {
  const video = getVideo();

  if (command.type === "playPause" && video) {
    if (video.paused) {
      video.play();
    } else {
      video.pause();
    }
  }

  if (command.type === "volume" && video) {
    video.volume = Math.max(0, Math.min(1, video.volume + Number(command.delta || 0)));
    video.muted = false;
  }

  if (command.type === "mute" && video) {
    video.muted = !video.muted;
  }

  if (command.type === "previous") {
    clickFirst([
      "ytmusic-player-bar .previous-button",
      ".previous-button button",
      ".ytp-prev-button"
    ]);
  }

  if (command.type === "next") {
    clickFirst([
      "ytmusic-player-bar .next-button",
      ".next-button button",
      ".ytp-next-button"
    ]);
  }
}

async function pollCommand() {
  try {
    const response = await chrome.runtime.sendMessage({ kind: "getCommand" });
    const command = response?.command || { type: "none" };
    if (command.type && command.type !== "none") {
      runCommand(command);
      setTimeout(postState, 120);
    }
  } catch (error) {
    console.warn("[YT Panel Helper] command poll failed", error);
  }
}

setInterval(pollCommand, 250);
setInterval(postState, 1000);
setTimeout(postState, 500);
