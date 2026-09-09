import { useState } from "react";
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  Stack,
  TextField,
  Typography,
} from "@mui/material";

export function SessionDialog({
  open,
  session,
  canClose,
  canOverride,
  canOrder,
  canPay,
  onClose,
  onCloseSession,
  onNavigate,
}) {
  const [reason, setReason] = useState("");
  const [error, setError] = useState("");
  if (!session) return null;
  const blocked = session.closeBlockers.length > 0;

  const close = async (overrideObligations) => {
    setError("");
    try {
      await onCloseSession({
        overrideObligations,
        overrideReason: overrideObligations ? reason : null,
      });
      onClose();
    } catch (failure) {
      setError(failure.message);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="md">
      <DialogTitle>Phiên bàn {session.tableName}</DialogTitle>
      <DialogContent>
        <Stack spacing={2}>
          {error && <Alert severity="error">{error}</Alert>}
          <Typography>{session.guestCount} khách · mở lúc {new Date(session.openedDate).toLocaleString()}</Typography>
          <Typography>Tổng: {money(session.totalAmount)} · Đã thu: {money(session.paidAmount)} · Còn lại: {money(session.remainingAmount)}</Typography>
          <Divider />
          {session.orders.length === 0
            ? <Typography color="text.secondary">Chưa có đơn hàng.</Typography>
            : session.orders.map((order) => (
              <Typography key={order.id}>{order.orderNo} · {order.status} · {money(order.totalAmount)}</Typography>
            ))}
          {blocked && (
            <Alert severity="warning">{session.closeBlockers.join(" ")}</Alert>
          )}
          {blocked && canOverride && (
            <TextField
              label="Lý do đóng ngoại lệ"
              value={reason}
              onChange={(event) => setReason(event.target.value)}
              required
            />
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        {canOrder && <Button onClick={() => onNavigate(`/restaurant/ordering?sessionId=${session.id}`)}>Gọi món</Button>}
        {canPay && <Button onClick={() => onNavigate(`/restaurant/payments?sessionId=${session.id}`)}>Thanh toán</Button>}
        {canClose && !blocked && <Button color="warning" onClick={() => close(false)}>Đóng phiên</Button>}
        {canClose && blocked && canOverride && <Button color="warning" disabled={!reason.trim()} onClick={() => close(true)}>Đóng ngoại lệ</Button>}
        <Button onClick={onClose}>Đóng</Button>
      </DialogActions>
    </Dialog>
  );
}

function money(value) {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
  }).format(value);
}
