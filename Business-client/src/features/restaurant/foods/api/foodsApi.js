import { apiFetch } from "../../../../shared/api/apiClient.js";

const PATH = "/api/restaurant/foods";
const pendingListRequests = new Map();

export function getFoods({
  search = "",
  categoryId = "",
  status = "all",
  availability = "all",
  page = 1,
  pageSize = 10,
} = {}) {
  const query = new URLSearchParams({ status, availability, page, pageSize });
  if (search.trim()) query.set("search", search.trim());
  if (categoryId) query.set("categoryId", categoryId);
  const requestPath = `${PATH}?${query}`;
  if (pendingListRequests.has(requestPath)) {
    return pendingListRequests.get(requestPath);
  }
  const request = apiFetch(requestPath)
    .then((response) => response.data)
    .finally(() => pendingListRequests.delete(requestPath));
  pendingListRequests.set(requestPath, request);
  return request;
}

export async function getFood(code) {
  return (await apiFetch(`${PATH}/${encodeURIComponent(code)}`)).data;
}

export async function createFood(food) {
  return (await apiFetch(PATH, {
    method: "POST",
    body: JSON.stringify(food),
  })).data;
}

export async function updateFood(code, food) {
  return (await apiFetch(`${PATH}/${encodeURIComponent(code)}`, {
    method: "PUT",
    body: JSON.stringify(food),
  })).data;
}

export async function deactivateFood(code, version) {
  const query = version
    ? `?version=${encodeURIComponent(version)}`
    : "";
  await apiFetch(`${PATH}/${encodeURIComponent(code)}${query}`, {
    method: "DELETE",
  });
}

export async function createVariant(foodCode, variant) {
  return (await apiFetch(
    `${PATH}/${encodeURIComponent(foodCode)}/variants`,
    {
      method: "POST",
      body: JSON.stringify(variant),
    },
  )).data;
}

export async function updateVariant(foodCode, variantCode, variant) {
  return (await apiFetch(
    `${PATH}/${encodeURIComponent(foodCode)}/variants/${encodeURIComponent(variantCode)}`,
    {
      method: "PUT",
      body: JSON.stringify(variant),
    },
  )).data;
}

export async function changePrice(foodCode, variantCode, price, version) {
  return (await apiFetch(
    `${PATH}/${encodeURIComponent(foodCode)}/variants/${encodeURIComponent(variantCode)}/price`,
    {
      method: "PUT",
      body: JSON.stringify({ price, version }),
    },
  )).data;
}

export async function getPriceHistory(foodCode, variantCode) {
  return (await apiFetch(
    `${PATH}/${encodeURIComponent(foodCode)}/variants/${encodeURIComponent(variantCode)}/prices`,
  )).data;
}

export async function changeAvailability(
  foodCode,
  variantCode,
  isAvailable,
  reason,
  version,
) {
  return (await apiFetch(
    `${PATH}/${encodeURIComponent(foodCode)}/variants/${encodeURIComponent(variantCode)}/availability`,
    {
      method: "PUT",
      body: JSON.stringify({ isAvailable, reason, version }),
    },
  )).data;
}
