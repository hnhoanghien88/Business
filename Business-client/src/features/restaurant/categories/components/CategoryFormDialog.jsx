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

const empty = { parentId: "", code: "", name: "", description: "", displayOrder: 0, isActive: true };

export function CategoryFormDialog({ open, category, categories, onClose, onSubmit }) {
  const [form, setForm] = useState(empty);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    // Reset local form state when the selected remote record changes.
    // oxlint-disable-next-line react/set-state-in-effect
    setForm(category ? { ...category, parentId: category.parentId ?? "" } : empty);
    setError("");
  }, [category, open]);

  const change = (field) => (event) => setForm((value) => ({
    ...value,
    [field]: field === "isActive" ? event.target.checked : event.target.value,
  }));

  const submit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setError("");
    try {
      await onSubmit({
        ...form,
        parentId: form.parentId === "" ? null : Number(form.parentId),
        displayOrder: Number(form.displayOrder),
      });
      onClose();
    } catch (failure) {
      setError(failure.status === 409 ? `${failure.message} Hãy tải lại và thử lại.` : failure.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={open} onClose={saving ? undefined : onClose} fullWidth maxWidth="sm">
      <Stack component="form" onSubmit={submit}>
        <DialogTitle>{category ? "Cập nhật nhóm món" : "Thêm nhóm món"}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            {error && <Alert severity="error">{error}</Alert>}
            <TextField label="Mã" required value={form.code} disabled={Boolean(category)} onChange={change("code")} inputProps={{ maxLength: 50 }} />
            <TextField label="Tên" required value={form.name} onChange={change("name")} inputProps={{ maxLength: 150 }} />
            <TextField select label="Nhóm cha" value={form.parentId} onChange={change("parentId")}>
              <MenuItem value="">Không có (nhóm gốc)</MenuItem>
              {categories.filter((item) =>
                item.id !== category?.id
                && item.isActive
                && !item.ancestorPath.some((ancestor) => ancestor.id === category?.id)).map((item) => (
                <MenuItem key={item.id} value={item.id}>{item.name}</MenuItem>
              ))}
            </TextField>
            <TextField label="Mô tả" multiline minRows={2} value={form.description ?? ""} onChange={change("description")} inputProps={{ maxLength: 500 }} />
            <TextField label="Thứ tự" type="number" value={form.displayOrder} onChange={change("displayOrder")} />
            <FormControlLabel control={<Switch checked={form.isActive} onChange={change("isActive")} />} label="Hoạt động" />
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
