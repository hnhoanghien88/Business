import { useCallback, useEffect, useMemo, useState } from "react";
import { Alert, Box, Button, Card, CardContent, CircularProgress, MenuItem, Stack, TextField, Typography } from "@mui/material";
import { confirmOrder, getOrderingMenu, getSessionOrders, previewOrder } from "./api/orderingApi.js";
import { clearCart, getCart, setCart as saveCart } from "./api/cartStore.js";

export function OrderingPage() {
  const sessionId = new URLSearchParams(window.location.search).get("sessionId") || "";
  const [menu, setMenu] = useState(null); const [history, setHistory] = useState(null);
  const [cart, setCart] = useState(() => getCart(sessionId)); const [search, setSearch] = useState("");
  const [categoryId, setCategoryId] = useState(""); const [promotionCode, setPromotionCode] = useState("");
  const [preview, setPreview] = useState(null); const [message, setMessage] = useState(""); const [busy, setBusy] = useState(false);
  const [requestId, setRequestId] = useState(() => crypto.randomUUID());
  const load = useCallback(async () => { if (!sessionId) return; try {
    const [m, h] = await Promise.all([getOrderingMenu(sessionId, { search, categoryId }), getSessionOrders(sessionId)]);
    setMenu(m); setHistory(h);
  } catch (error) { setMessage(error.message); } }, [sessionId, search, categoryId]);
  useEffect(() => { load(); }, [load]);
  useEffect(() => { if (!sessionId) return undefined; const timer = setInterval(load, 2500); return () => clearInterval(timer); }, [sessionId, load]);
  const update = (nextLines) => { setCart(nextLines); saveCart(sessionId, nextLines); setPreview(null); setRequestId(crypto.randomUUID()); };
  const add = (food, variant) => { if (!variant.isAvailable) return; const found = cart.find((x) => x.foodVariantId === variant.id);
    update(found ? cart.map((x) => x === found ? { ...x, quantity: x.quantity + 1 } : x)
      : [...cart, { foodVariantId: variant.id, foodName: food.name, variantName: variant.name, unitPrice: variant.currentPrice, quantity: 1, note: "" }]); };
  const lines = () => cart.map(({ foodVariantId, quantity, note }) => ({ foodVariantId, quantity, note: note || null }));
  const subtotal = useMemo(() => cart.reduce((sum, x) => sum + x.unitPrice * x.quantity, 0), [cart]);
  const calculate = async () => { setBusy(true); setMessage(""); try { setPreview(await previewOrder(sessionId, { items: lines(), promotionCode: promotionCode || null })); } catch (error) { setMessage(error.message); } finally { setBusy(false); } };
  const submit = async () => { setBusy(true); setMessage(""); try { const current = preview || await previewOrder(sessionId, { items: lines(), promotionCode: promotionCode || null }); setPreview(current);
    const result = await confirmOrder(sessionId, { requestId, observedTotal: current.totalAmount, items: lines(), promotionCode: promotionCode || null });
    clearCart(sessionId); setCart([]); setPreview(null); setPromotionCode(""); setRequestId(crypto.randomUUID()); setMessage(`Đã gửi ${result.orderNo} đến bếp (${result.kitchenNo}).`); await load();
  } catch (error) { setMessage(error.message); } finally { setBusy(false); } };
  if (!sessionId) return <Alert severity="warning">Thiếu sessionId để gọi món.</Alert>;
  if (!menu) return <CircularProgress aria-label="Đang tải menu" />;
  return <Stack spacing={2}>
    <Stack direction={{ xs: "column", md: "row" }} justifyContent="space-between"><Typography variant="h5">Gọi món · {menu.tableCode}</Typography><Typography>Tổng lượt bàn: {(history?.totalAmount || 0).toLocaleString("vi-VN")} ₫</Typography></Stack>
    {message && <Alert severity={message.startsWith("Đã gửi") ? "success" : "error"}>{message}</Alert>}
    <Stack direction={{ xs: "column", md: "row" }} spacing={2}><Box flex={2}>
      <Stack direction="row" spacing={1} mb={2}><TextField fullWidth label="Tìm món" value={search} onChange={(e) => setSearch(e.target.value)} /><TextField select label="Nhóm" value={categoryId} onChange={(e) => setCategoryId(e.target.value)} sx={{ minWidth: 160 }}><MenuItem value="">Tất cả</MenuItem>{menu.categories.map((x) => <MenuItem key={x.id} value={x.id}>{x.name}</MenuItem>)}</TextField></Stack>
      <Box className="ordering-menu-grid">{menu.foods.map((food) => <Card key={food.id}><CardContent><Typography fontWeight={700}>{food.name}</Typography>{food.variants.map((v) => <Button key={v.id} disabled={!v.isAvailable} onClick={() => add(food, v)}>{v.name} · {v.currentPrice.toLocaleString("vi-VN")} ₫ {!v.isAvailable && `(${v.soldOutReason || "Hết"})`}</Button>)}</CardContent></Card>)}</Box>
    </Box><Stack component="aside" flex={1} spacing={1}><Typography variant="h6">Giỏ món</Typography>{cart.map((x, i) => <Card key={x.foodVariantId}><CardContent><Typography>{x.foodName} · {x.variantName}</Typography><Stack direction="row" spacing={1}><TextField type="number" label="SL" value={x.quantity} inputProps={{ min: 1 }} onChange={(e) => update(cart.map((item, n) => n === i ? { ...item, quantity: Math.max(1, Number(e.target.value) || 1) } : item))} /><TextField label="Ghi chú" value={x.note} onChange={(e) => update(cart.map((item, n) => n === i ? { ...item, note: e.target.value } : item))} /></Stack><Button color="error" onClick={() => update(cart.filter((_, n) => n !== i))}>Xóa</Button></CardContent></Card>)}
      <TextField label="Mã khuyến mãi" value={promotionCode} onChange={(e) => { setPromotionCode(e.target.value); setPreview(null); }} /><Button variant="outlined" disabled={!cart.length || busy} onClick={calculate}>Kiểm tra giá</Button><Typography>Tạm tính: {subtotal.toLocaleString("vi-VN")} ₫</Typography>{preview && <><Typography>Giảm: {preview.discountAmount.toLocaleString("vi-VN")} ₫</Typography><Typography variant="h6">Thanh toán: {preview.totalAmount.toLocaleString("vi-VN")} ₫</Typography></>}<Button variant="contained" disabled={!cart.length || busy} onClick={submit}>{busy ? <CircularProgress size={20} /> : "Xác nhận & gửi bếp"}</Button><Typography variant="h6">Các lần gọi</Typography>{history?.orders.map((x) => <Typography key={x.id}>{x.orderNo} · {x.totalAmount.toLocaleString("vi-VN")} ₫</Typography>)}</Stack></Stack>
  </Stack>;
}
