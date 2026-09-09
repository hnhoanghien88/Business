import { apiFetch } from "../../../../shared/api/apiClient.js";

const PATH = "/api/restaurant/table-operations";
const pendingSnapshots = new Map();

export function getOperationalTables({ search = "", areaId = "", status = "all" } = {}) {
  const query = new URLSearchParams();
  if (search.trim()) query.set("search", search.trim());
  if (areaId !== "") query.set("areaId", areaId);
  if (status && status !== "all") query.set("status", status);
  const requestPath = `${PATH}/tables?${query}`;
  if (pendingSnapshots.has(requestPath)) {
    return pendingSnapshots.get(requestPath);
  }
  const request = apiFetch(requestPath)
    .then((response) => response.data)
    .finally(() => pendingSnapshots.delete(requestPath));
  pendingSnapshots.set(requestPath, request);
  return request;
}

export async function getSession(sessionId) {
  return (await apiFetch(`${PATH}/sessions/${sessionId}`)).data;
}

export async function openTable(code, request) {
  return (await apiFetch(`${PATH}/tables/${encodeURIComponent(code)}/open`, {
    method: "POST",
    body: JSON.stringify(request),
  })).data;
}

export async function closeSession(sessionId, request) {
  return (await apiFetch(`${PATH}/sessions/${sessionId}/close`, {
    method: "POST",
    body: JSON.stringify(request),
  })).data;
}

export async function markTableClean(code) {
  return (await apiFetch(`${PATH}/tables/${encodeURIComponent(code)}/mark-clean`, {
    method: "POST",
  })).data;
}
