import { apiFetch } from "../../../../shared/api/apiClient.js";

const PATH = "/api/restaurant/layouts";
const pending = new Map();

function list(resource, conditions) {
  const normalized = Object.fromEntries(Object.entries(conditions)
    .filter(([, value]) => value !== "" && value !== null && value !== undefined)
    .map(([key, value]) => [key, typeof value === "string" ? value.trim() : value]));
  const query = new URLSearchParams(normalized);
  const key = `${PATH}/${resource}?${query}`;
  if (pending.has(key)) return pending.get(key);
  const request = apiFetch(key)
    .then((response) => response.data)
    .finally(() => pending.delete(key));
  pending.set(key, request);
  return request;
}

export const getAreas = (conditions = {}) => list("areas", conditions);
export const getTables = (conditions = {}) => list("tables", conditions);
export async function createArea(value) { return (await apiFetch(`${PATH}/areas`, { method: "POST", body: JSON.stringify(value) })).data; }
export async function updateArea(code, value) { return (await apiFetch(`${PATH}/areas/${encodeURIComponent(code)}`, { method: "PUT", body: JSON.stringify(value) })).data; }
export async function createTable(value) { return (await apiFetch(`${PATH}/tables`, { method: "POST", body: JSON.stringify(value) })).data; }
export async function updateTable(code, value) { return (await apiFetch(`${PATH}/tables/${encodeURIComponent(code)}`, { method: "PUT", body: JSON.stringify(value) })).data; }
export async function setTableActivation(code, value) { return (await apiFetch(`${PATH}/tables/${encodeURIComponent(code)}/activation`, { method: "PUT", body: JSON.stringify(value) })).data; }
