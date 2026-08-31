const IDENTITY_BASE_URL = "/identity";
const APPLICATION_CODE = import.meta.env.VITE_APPLICATION_CODE?.trim() || "restaurant";

let pendingRefresh = null;

export async function login(credentials) {
  const response = await fetch(`${IDENTITY_BASE_URL}/login`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ ...credentials, applicationCode: APPLICATION_CODE }),
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    if (response.status === 401) {
      throw new Error("Code hoặc mật khẩu không đúng, hoặc bạn chưa có quyền vào Restaurant.");
    }
    throw new Error(problem?.detail || "Không thể đăng nhập vào Identity.");
  }

  return enrichSession(await response.json());
}

export function refreshSession() {
  if (!pendingRefresh) {
    pendingRefresh = requestRefresh().finally(() => {
      pendingRefresh = null;
    });
  }

  return pendingRefresh;
}

async function requestRefresh() {
  const query = new URLSearchParams({ applicationCode: APPLICATION_CODE });
  const response = await fetch(`${IDENTITY_BASE_URL}/refresh?${query}`, {
    method: "POST",
    credentials: "include",
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new Error(problem?.detail || "Phiên đăng nhập đã hết hạn.");
  }

  return enrichSession(await response.json());
}

async function enrichSession(session) {
  const authorization = await getAuthorization(session.accessToken);
  return { ...session, authorization };
}

async function getAuthorization(accessToken) {
  const query = new URLSearchParams({ applicationCode: APPLICATION_CODE });
  const response = await fetch(`${IDENTITY_BASE_URL}/authorization?${query}`, {
    credentials: "include",
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new Error(problem?.detail || "Không thể tải quyền và menu.");
  }

  return response.json();
}

export async function logout() {
  const query = new URLSearchParams({ applicationCode: APPLICATION_CODE });
  const response = await fetch(`${IDENTITY_BASE_URL}/logout?${query}`, {
    method: "POST",
    credentials: "include",
  });

  if (!response.ok && response.status !== 401) {
    const problem = await response.json().catch(() => null);
    throw new Error(problem?.detail || "Không thể đăng xuất khỏi Identity.");
  }
}
