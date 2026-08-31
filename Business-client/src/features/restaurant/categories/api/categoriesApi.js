import { apiFetch } from "../../../../shared/api/apiClient.js";

const PATH = "/api/restaurant/categories";

export async function getCategories({ search = "", status = "all", page = 1, pageSize = 20 } = {}) {
  const query = new URLSearchParams({ status, page, pageSize });
  if (search.trim()) query.set("search", search.trim());
  return (await apiFetch(`${PATH}?${query}`)).data;
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
