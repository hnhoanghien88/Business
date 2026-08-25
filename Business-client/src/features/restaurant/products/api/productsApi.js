import { apiFetch } from "../../../../shared/api/apiClient.js";

const PRODUCTS_PATH = "/api/restaurant/products";

export async function getProducts({ search = "", page = 1, pageSize = 10 } = {}) {
  const query = new URLSearchParams({ page, pageSize });
  if (search.trim()) query.set("search", search.trim());
  const response = await apiFetch(`${PRODUCTS_PATH}?${query}`);
  return response.data;
}

export async function createProduct(product) {
  const response = await apiFetch(PRODUCTS_PATH, {
    method: "POST",
    body: JSON.stringify(product),
  });
  return response.data;
}

export async function updateProduct(code, product) {
  const response = await apiFetch(`${PRODUCTS_PATH}/${encodeURIComponent(code)}`, {
    method: "PUT",
    body: JSON.stringify({ name: product.name }),
  });
  return response.data;
}

export async function deleteProduct(code) {
  await apiFetch(`${PRODUCTS_PATH}/${encodeURIComponent(code)}`, {
    method: "DELETE",
  });
}
