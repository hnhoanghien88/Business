const carts = new Map();
export const getCart = (id) => carts.get(String(id)) || [];
export const setCart = (id, lines) => { const copy = lines.map((line) => ({ ...line })); carts.set(String(id), copy); return copy; };
export const clearCart = (id) => carts.delete(String(id));
