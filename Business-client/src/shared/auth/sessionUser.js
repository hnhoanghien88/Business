const fallbackUser = { code: "User", displayName: "User" };

export function getSessionUser(session) {
  try {
    const payload = session?.accessToken?.split(".")[1];
    if (!payload) return fallbackUser;

    const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
    const padded = normalized.padEnd(
      normalized.length + ((4 - (normalized.length % 4)) % 4),
      "=",
    );
    const claims = JSON.parse(atob(padded));
    const code = claims.code || claims.email || "User";
    const displayName = claims.display_name || claims.name || code;
    return { code, displayName };
  } catch {
    return fallbackUser;
  }
}
