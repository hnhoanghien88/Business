const IDENTITY_BASE_URL = "/identity";
const APPLICATION_CODE = import.meta.env.VITE_APPLICATION_CODE || "Business";

export async function login(credentials) {
  const response = await fetch(`${IDENTITY_BASE_URL}/login`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(credentials),
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error("Code hoặc mật khẩu không đúng.");
    }
    const problem = await response.json().catch(() => null);
    throw new Error(problem?.detail || "Không thể đăng nhập vào Identity.");
  }

  return enrichSession(await response.json());
}

export async function refreshSession() {
  const response = await fetch(`${IDENTITY_BASE_URL}/refresh`, {
    method: "POST",
    credentials: "include",
  });

  if (!response.ok) {
    throw new Error("Phiên đăng nhập đã hết hạn.");
  }

  return enrichSession(await response.json());
}

async function enrichSession(session) {
  const application = await requireBusinessApplication(session.accessToken);
  const [authorization, menus] = await Promise.all([
    getAuthorization(session.accessToken),
    getBusinessMenus(session.accessToken, application.id),
  ]);
  return {
    ...session,
    application,
    authorization: { ...authorization, menus },
  };
}

async function getAuthorization(accessToken) {
  const query = new URLSearchParams({ applicationCode: APPLICATION_CODE });
  const response = await fetch(`${IDENTITY_BASE_URL}/authorization?${query}`, {
    credentials: "include",
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new Error(problem?.detail || "Không thể tải quyền authorization.");
  }

  return response.json();
}

async function requireBusinessApplication(accessToken) {
  const response = await fetch(`${IDENTITY_BASE_URL}/api/applications/search`, {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify({
      filter: { code: { values: [APPLICATION_CODE] }, isActive: true },
      sorts: [{ column: 0, direction: 0 }],
      page: 1,
      pageSize: 1,
    }),
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new Error(problem?.detail || "Không thể kiểm tra application Business trên Identity.");
  }

  const payload = await response.json();
  const application = payload?.data?.items?.find(
    (item) => item.code === APPLICATION_CODE && item.isActive,
  );
  if (!application) {
    throw new Error("Application Business không tồn tại hoặc chưa được kích hoạt.");
  }
  return application;
}

async function getBusinessMenus(accessToken, applicationId) {
  const response = await fetch(
    `${IDENTITY_BASE_URL}/api/menus?applicationId=${applicationId}`,
    {
      credentials: "include",
      headers: { Authorization: `Bearer ${accessToken}` },
    },
  );

  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new Error(problem?.detail || "Không thể tải menu của application Business.");
  }

  const payload = await response.json();
  return payload?.data ?? [];
}

export async function logout(accessToken) {
  const response = await fetch(`${IDENTITY_BASE_URL}/logout`, {
    method: "POST",
    credentials: "include",
    headers: { Authorization: `Bearer ${accessToken}` },
  });
  if (!response.ok && response.status !== 401) {
    throw new Error("Không thể đăng xuất khỏi Identity.");
  }
}
