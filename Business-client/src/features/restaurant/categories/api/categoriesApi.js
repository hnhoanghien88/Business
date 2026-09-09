import { apiFetch } from "../../../../shared/api/apiClient.js";

const PATH = "/api/restaurant/categories";
const pendingCategoryRequests = new Map();

export function getCategories({ search = "", status = "all", page = 1, pageSize = 20 } = {}) {
  const query = new URLSearchParams({ status, page, pageSize });
  if (search.trim()) query.set("search", search.trim());
  const requestPath = `${PATH}?${query}`;

  if (pendingCategoryRequests.has(requestPath)) {
    return pendingCategoryRequests.get(requestPath);
  }

  const request = apiFetch(requestPath)
    .then((response) => response.data)
    .finally(() => pendingCategoryRequests.delete(requestPath));
  pendingCategoryRequests.set(requestPath, request);
  return request;
}

export async function createCategory(category) {
  return (await apiFetch(PATH, {
    method: "POST",
    body: JSON.stringify(category),
  })).data;
}

export async function updateCategory(code, category) {
  return (await apiFetch(`${PATH}/${encodeURIComponent(code)}`, {
    method: "PUT",
    body: JSON.stringify(category),
  })).data;
}
