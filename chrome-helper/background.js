const bridgeBaseUrl = "http://127.0.0.1:47821";

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (message.kind === "postState") {
    fetch(`${bridgeBaseUrl}/state`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(message.state || {})
    })
      .then(() => sendResponse({ ok: true }))
      .catch((error) => sendResponse({ ok: false, error: String(error) }));

    return true;
  }

  if (message.kind === "getCommand") {
    fetch(`${bridgeBaseUrl}/command`, { cache: "no-store" })
      .then((response) => response.json())
      .then((command) => sendResponse({ ok: true, command }))
      .catch((error) => sendResponse({ ok: false, error: String(error) }));

    return true;
  }

  return false;
});
