import { refreshSession } from "./identityClient.js";

const CHANNEL_NAME = "business-auth-session";
const REFRESH_LOCK_NAME = "business-refresh-token";
const SESSION_WAIT_MS = 200;
const EXPIRY_BUFFER_MS = 30_000;
const LOGOUT_MARKER = "business-explicitly-logged-out";

const channel = typeof BroadcastChannel === "undefined"
  ? null
  : new BroadcastChannel(CHANNEL_NAME);

let currentSession = null;
let restorePromise = null;
let restorationBlocked = readLogoutMarker();
const listeners = new Set();
const pendingRequests = new Map();

channel?.addEventListener("message", ({ data }) => {
  if (!data?.type) return;

  if (data.type === "SESSION_REQUEST" && isUsable(currentSession)) {
    channel.postMessage({
      type: "SESSION_RESPONSE",
      requestId: data.requestId,
      session: currentSession,
    });
    return;
  }

  if (data.type === "SESSION_RESPONSE") {
    pendingRequests.get(data.requestId)?.(data.session);
    return;
  }

  if (data.type === "SESSION_UPDATED" && isUsable(data.session)) {
    setRestorationBlocked(false);
    updateLocalSession(data.session);
    return;
  }

  if (data.type === "SESSION_CLEARED") {
    setRestorationBlocked(true);
    updateLocalSession(null);
  }
});

export function getSession() {
  return isUsable(currentSession) ? currentSession : null;
}

export function publishSession(session) {
  setRestorationBlocked(false);
  updateLocalSession(session);
  channel?.postMessage({ type: "SESSION_UPDATED", session });
}

export function clearSession() {
  setRestorationBlocked(true);
  updateLocalSession(null);
  channel?.postMessage({ type: "SESSION_CLEARED" });
}

export function subscribeToSession(listener) {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

export function restoreSession() {
  if (restorationBlocked) return Promise.resolve(null);
  if (isUsable(currentSession)) return Promise.resolve(currentSession);
  if (restorePromise) return restorePromise;

  restorePromise = restoreFromAnotherTab()
    .then((session) => session || refreshWithCrossTabLock())
    .finally(() => {
      restorePromise = null;
    });

  return restorePromise;
}

async function refreshWithCrossTabLock() {
  if (!navigator.locks?.request) return refreshAndPublish();

  return navigator.locks.request(REFRESH_LOCK_NAME, async () => {
    const sharedSession = await restoreFromAnotherTab();
    return sharedSession || refreshAndPublish();
  });
}

async function refreshAndPublish() {
  const session = await refreshSession();
  publishSession(session);
  return session;
}

function restoreFromAnotherTab() {
  if (!channel) return Promise.resolve(null);

  const requestId = crypto.randomUUID();
  return new Promise((resolve) => {
    const timeoutId = window.setTimeout(() => {
      pendingRequests.delete(requestId);
      resolve(null);
    }, SESSION_WAIT_MS);

    pendingRequests.set(requestId, (session) => {
      window.clearTimeout(timeoutId);
      pendingRequests.delete(requestId);
      if (isUsable(session)) {
        updateLocalSession(session);
        resolve(session);
      } else {
        resolve(null);
      }
    });

    channel.postMessage({ type: "SESSION_REQUEST", requestId });
  });
}

function updateLocalSession(session) {
  currentSession = session;
  listeners.forEach((listener) => listener(session));
}

function isUsable(session) {
  if (!session?.accessToken || !session?.accessTokenExpiresAtUtc) return false;
  return Date.parse(session.accessTokenExpiresAtUtc) > Date.now() + EXPIRY_BUFFER_MS;
}

function setRestorationBlocked(isBlocked) {
  restorationBlocked = isBlocked;
  try {
    if (isBlocked) localStorage.setItem(LOGOUT_MARKER, "true");
    else localStorage.removeItem(LOGOUT_MARKER);
  } catch {
    // Memory and BroadcastChannel coordination remain available.
  }
}

function readLogoutMarker() {
  try {
    return localStorage.getItem(LOGOUT_MARKER) === "true";
  } catch {
    return false;
  }
}
