import { useEffect, useState } from "react";
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  MenuItem,
  Stack,
  Switch,
  TextField,
} from "@mui/material";

const empty = {
  categoryId: "",
  code: "",
  name: "",
  description: "",
  imageUrl: "",
  displayOrder: 0,
  isActive: true,
};

export function FoodFormDialog({
  open,
  food,
  categories,
  onClose,
  onSubmit,
}) {
  const [form, setForm] = useState(empty);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    // oxlint-disable-next-line react/set-state-in-effect
    setForm(food ? { ...food } : empty);
    setError("");
  }, [food, open]);

  const change = (field) => (event) => {
    setForm((current) => ({
      ...current,
      [field]: field === "isActive" ? event.target.checked : event.target.value,
    }));
  };

  const submit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setError("");
    try {
      await onSubmit({
        ...form,
        categoryId: Number(form.categoryId),
        displayOrder: Number(form.displayOrder),
      });
      onClose();
    } catch (failure) {
      setError(failure.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={open} onClose={saving ? undefined : onClose} fullWidth maxWidth="sm">
      <Stack component="form" onSubmit={submit}>
        <DialogTitle>{food ? "Cập nhật món ăn" : "Thêm món ăn"}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            {error && <Alert severity="error">{error}</Alert>}
            <TextField
              select
              required
              label="Nhóm món"
              value={form.categoryId}
              onChange={change("categoryId")}
            >
              {categories.filter((item) => item.isActive).map((item) => (
                <MenuItem key={item.id} value={item.id}>
                  {item.name}
                </MenuItem>
              ))}
            </TextField>
            <TextField
              required
              label="Mã"
              value={form.code}
              disabled={Boolean(food)}
              onChange={change("code")}
              slotProps={{ htmlInput: { maxLength: 50 } }}
            />
            <TextField
              required
              label="Tên món"
              value={form.name}
              onChange={change("name")}
              slotProps={{ htmlInput: { maxLength: 200 } }}
            />
            <TextField
              multiline
              minRows={2}
              label="Mô tả"
              value={form.description ?? ""}
              onChange={change("description")}
            />
            <TextField
              label="URL ảnh"
              value={form.imageUrl ?? ""}
              onChange={change("imageUrl")}
              slotProps={{ htmlInput: { maxLength: 1000 } }}
            />
            <TextField
              type="number"
              label="Thứ tự"
              value={form.displayOrder}
              onChange={change("displayOrder")}
            />
            <FormControlLabel
              control={(
                <Switch
                  checked={form.isActive}
                  onChange={change("isActive")}
                />
              )}
              label="Hoạt động"
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={saving}>Hủy</Button>
          <Button type="submit" variant="contained" disabled={saving}>Lưu</Button>
        </DialogActions>
      </Stack>
    </Dialog>
  );
}
