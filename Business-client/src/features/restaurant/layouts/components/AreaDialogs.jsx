import { useEffect, useState } from "react";
import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, TextField } from "@mui/material";

export function AreaDialog({ open, area, onClose, onSubmit }) {
  const [form, setForm] = useState({ code: "", name: "", description: "", displayOrder: 0, isActive: true });
  const [error, setError] = useState("");
  useEffect(() => {
    // Reset local draft when the selected entity changes.
    // oxlint-disable-next-line react/set-state-in-effect
    if (open) setForm(area ? { ...area, confirmImpact: false } : { code: "", name: "", description: "", displayOrder: 0, isActive: true });
  }, [open, area]);
  const set = (key) => (event) => setForm((value) => ({ ...value, [key]: event.target.value }));
  const save = async () => {
    setError("");
    try {
      const payload = { ...form, displayOrder: Number(form.displayOrder) };
      if (area?.isActive && !payload.isActive && area.tableCount > 0) payload.confirmImpact = window.confirm(`Khu vực có ${area.tableCount} bàn. Tiếp tục ngừng hoạt động?`);
      if (area?.isActive && !payload.isActive && area.tableCount > 0 && !payload.confirmImpact) return;
      await onSubmit(payload);
      onClose();
    } catch (failure) { setError(failure.message); }
  };
  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>{area ? "Sửa khu vực" : "Thêm khu vực"}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          {error && <Alert severity="error">{error}</Alert>}
          <TextField label="Mã" value={form.code} onChange={set("code")} disabled={Boolean(area)} required />
          <TextField label="Tên" value={form.name} onChange={set("name")} required />
          <TextField label="Mô tả" value={form.description ?? ""} onChange={set("description")} multiline />
          <TextField label="Thứ tự" type="number" value={form.displayOrder} onChange={set("displayOrder")} />
          {area && <Button onClick={() => setForm((value) => ({ ...value, isActive: !value.isActive }))}>{form.isActive ? "Ngừng hoạt động" : "Kích hoạt lại"}</Button>}
        </Stack>
      </DialogContent>
      <DialogActions><Button onClick={onClose}>Hủy</Button><Button variant="contained" onClick={save}>Lưu</Button></DialogActions>
    </Dialog>
  );
}
