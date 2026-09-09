import { useEffect, useState } from "react";
import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Stack, TextField } from "@mui/material";

export function TableDialog({ open, table, areas, onClose, onSubmit }) {
  const [form, setForm] = useState({ areaId: "", code: "", name: "", capacity: 2 });
  const [error, setError] = useState("");
  useEffect(() => {
    // Reset local draft when the selected entity changes.
    // oxlint-disable-next-line react/set-state-in-effect
    if (open) setForm(table ?? { areaId: areas[0]?.id ?? "", code: "", name: "", capacity: 2 });
  }, [open, table, areas]);
  const set = (key) => (event) => setForm((value) => ({ ...value, [key]: event.target.value }));
  const save = async () => {
    setError("");
    try { await onSubmit({ ...form, areaId: Number(form.areaId), capacity: Number(form.capacity) }); onClose(); }
    catch (failure) { setError(failure.message); }
  };
  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>{table ? "Sửa bàn" : "Thêm bàn"}</DialogTitle>
      <DialogContent><Stack spacing={2} sx={{ pt: 1 }}>
        {error && <Alert severity="error">{error}</Alert>}
        <TextField select label="Khu vực" value={form.areaId} onChange={set("areaId")} required>{areas.filter((area) => area.isActive).map((area) => <MenuItem key={area.id} value={area.id}>{area.name}</MenuItem>)}</TextField>
        <TextField label="Mã" value={form.code} onChange={set("code")} disabled={Boolean(table)} required />
        <TextField label="Tên" value={form.name} onChange={set("name")} required />
        <TextField label="Sức chứa" type="number" inputProps={{ min: 1, max: 1000 }} value={form.capacity} onChange={set("capacity")} required />
      </Stack></DialogContent>
      <DialogActions><Button onClick={onClose}>Hủy</Button><Button variant="contained" onClick={save}>Lưu</Button></DialogActions>
    </Dialog>
  );
}
