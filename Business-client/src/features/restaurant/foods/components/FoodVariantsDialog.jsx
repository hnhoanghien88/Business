import { useEffect, useState } from "react";
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  Stack,
  Switch,
  TextField,
  Typography,
} from "@mui/material";

const empty = {
  code: "",
  name: "",
  currentPrice: 0,
  isDefault: false,
  displayOrder: 0,
  isActive: true,
};

export function FoodVariantsDialog({
  food,
  permissions,
  onClose,
  onCreate,
  onUpdate,
  onPrice,
  onHistory,
  onAvailability,
}) {
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(empty);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);
  const [history, setHistory] = useState([]);

  useEffect(() => {
    // oxlint-disable-next-line react/set-state-in-effect
    setEditing(null);
    setForm(empty);
    setError("");
    setHistory([]);
  }, [food]);

  if (!food) return null;

  const change = (field) => (event) => {
    setForm((current) => ({
      ...current,
      [field]: ["isDefault", "isActive"].includes(field)
        ? event.target.checked
        : event.target.value,
    }));
  };

  const save = async () => {
    setSaving(true);
    setError("");
    try {
      if (editing) {
        await onUpdate(editing.code, {
          name: form.name,
          isDefault: form.isDefault,
          displayOrder: Number(form.displayOrder),
          isActive: form.isActive,
          version: editing.version,
        });
      } else {
        await onCreate({
          ...form,
          currentPrice: Number(form.currentPrice),
          displayOrder: Number(form.displayOrder),
        });
      }
      setEditing(null);
      setForm(empty);
    } catch (failure) {
      setError(failure.message);
    } finally {
      setSaving(false);
    }
  };

  const run = async (action) => {
    setSaving(true);
    setError("");
    try {
      await action();
    } catch (failure) {
      setError(failure.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open fullWidth maxWidth="md" onClose={saving ? undefined : onClose}>
      <DialogTitle>Biến thể — {food.name}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          {error && <Alert severity="error">{error}</Alert>}
          {food.variants.map((variant) => (
            <Stack
              key={variant.code}
              direction="row"
              spacing={1}
              sx={{ alignItems: "center" }}
            >
              <Typography sx={{ flex: 1 }}>
                {variant.name} ({variant.code}) — {variant.currentPrice.toLocaleString("vi-VN")} đ
                {variant.isDefault ? " · Mặc định" : ""}
                {!variant.isAvailable ? ` · Hết món: ${variant.soldOutReason || ""}` : ""}
              </Typography>
              {permissions.update && (
                <Button onClick={() => {
                  setEditing(variant);
                  setForm({ ...variant });
                }}>
                  Sửa
                </Button>
              )}
              {permissions.price && (
                <Button onClick={() => {
                  const value = window.prompt("Giá mới", String(variant.currentPrice));
                  if (value !== null) {
                    run(() => onPrice(variant.code, Number(value), variant.version));
                  }
                }}>
                  Đổi giá
                </Button>
              )}
              <Button onClick={() => run(async () => {
                setHistory(await onHistory(variant.code));
              })}>
                Lịch sử giá
              </Button>
              {permissions.availability && (
                <Button onClick={() => {
                  const available = !variant.isAvailable;
                  const reason = available ? null : window.prompt("Lý do hết món", "");
                  if (available || reason !== null) {
                    run(() => onAvailability(
                      variant.code,
                      available,
                      reason,
                      variant.version,
                    ));
                  }
                }}>
                  {variant.isAvailable ? "Hết món" : "Bán lại"}
                </Button>
              )}
            </Stack>
          ))}
          {history.length > 0 && (
            <Stack spacing={0.5}>
              <Typography variant="subtitle2">Lịch sử giá</Typography>
              {history.map((item) => (
                <Typography key={item.id} variant="body2">
                  {item.price.toLocaleString("vi-VN")} đ · {new Date(item.effectiveFrom).toLocaleString("vi-VN")}
                  {item.effectiveTo
                    ? ` → ${new Date(item.effectiveTo).toLocaleString("vi-VN")}`
                    : " → hiện tại"}
                </Typography>
              ))}
            </Stack>
          )}
          {permissions.update && (
            <Stack spacing={1}>
              <Typography variant="subtitle2">
                {editing ? "Cập nhật biến thể" : "Thêm biến thể"}
              </Typography>
              <TextField
                label="Mã"
                value={form.code}
                disabled={Boolean(editing)}
                onChange={change("code")}
              />
              <TextField label="Tên" value={form.name} onChange={change("name")} />
              {!editing && (
                <TextField
                  type="number"
                  label="Giá ban đầu"
                  value={form.currentPrice}
                  onChange={change("currentPrice")}
                />
              )}
              <TextField
                type="number"
                label="Thứ tự"
                value={form.displayOrder}
                onChange={change("displayOrder")}
              />
              <FormControlLabel
                control={<Switch checked={form.isDefault} onChange={change("isDefault")} />}
                label="Mặc định"
              />
              <FormControlLabel
                control={<Switch checked={form.isActive} onChange={change("isActive")} />}
                label="Hoạt động"
              />
              <Button variant="contained" onClick={save} disabled={saving}>
                {editing ? "Lưu biến thể" : "Thêm biến thể"}
              </Button>
            </Stack>
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={saving}>Đóng</Button>
      </DialogActions>
    </Dialog>
  );
}
