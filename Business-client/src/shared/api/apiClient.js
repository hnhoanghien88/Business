import { refreshSession } from "../../services/identity/identityClient.js";
import {
  clearSession,
  getSession,
  publishSession,
} from "../../services/identity/session.js";

const API_BASE_URL = "/backend";

export async function apiFetch(path, options = {}) {
  return send(path, options, true);
}

async function send(path, options, retry) {
  const session = getSession();
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      ...(options.body ? { "Content-Type": "application/json" } : {}),
      ...(session?.accessToken
        ? { Authorization: `Bearer ${session.accessToken}` }
        : {}),
      ...options.headers,
    },
  });

  if (response.status === 401 && retry) {
    try {
      publishSession(await refreshSession());
      return send(path, options, false);
    } catch {
      clearSession();
      throw new ApiError("Phiên đăng nhập đã hết hạn.", 401);
    }
  }

  const payload = response.status === 204
    ? null
    : await response.json().catch(() => null);

  if (!response.ok) {
    throw new ApiError(
      payload?.detail || payload?.title || "Không thể hoàn tất yêu cầu.",
      response.status,
      payload?.errors,
    );
  }

  return payload;
}

export class ApiError extends Error {
  constructor(message, status, errors = {}) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.errors = errors;
  }
}
