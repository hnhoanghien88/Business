import { apiFetch } from "../../../../shared/api/apiClient.js";
const pendingMenus = new Map();
export function getOrderingMenu(sessionId, conditions = {}) {
  const search = conditions.search?.trim().toLowerCase() || "";
  const categoryId = conditions.categoryId || "";
  const key = `${sessionId}|${search}|${categoryId}`;
  if (pendingMenus.has(key)) return pendingMenus.get(key);
  const params = new URLSearchParams();
  if (search) params.set("search", search);
  if (categoryId) params.set("categoryId", categoryId);
  const request = apiFetch(`/api/restaurant/ordering/sessions/${sessionId}/menu?${params}`)
    .then((payload) => payload.data).finally(() => pendingMenus.delete(key));
  pendingMenus.set(key, request);
  return request;
}
export const getSessionOrders = (id) => apiFetch(`/api/restaurant/ordering/sessions/${id}/orders`).then((p) => p.data);
export const previewOrder = (id, body) => apiFetch(`/api/restaurant/ordering/sessions/${id}/promotion-preview`, { method: "POST", body: JSON.stringify(body) }).then((p) => p.data);
export const confirmOrder = (id, body) => apiFetch(`/api/restaurant/ordering/sessions/${id}/confirm`, { method: "POST", body: JSON.stringify(body) }).then((p) => p.data);
