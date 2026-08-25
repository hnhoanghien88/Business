import { useState } from "react";
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
} from "@mui/material";

export function ProductFormDialog({ open, product, onClose, onSubmit }) {
  const [code, setCode] = useState(product?.code ?? "");
  const [name, setName] = useState(product?.name ?? "");
  const [errors, setErrors] = useState({});
  const [error, setError] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const isEditing = Boolean(product);

  const handleSubmit = async (event) => {
    event.preventDefault();
    const nextErrors = {};
    if (!code.trim()) nextErrors.code = ["Code là bắt buộc."];
    if (!name.trim()) nextErrors.name = ["Tên sản phẩm là bắt buộc."];
    if (Object.keys(nextErrors).length) {
      setErrors(nextErrors);
      return;
    }

    setIsSaving(true);
    setError("");
    try {
      await onSubmit({ code: code.trim(), name: name.trim() });
      onClose();
    } catch (saveError) {
      setErrors(saveError.errors ?? {});
      setError(saveError.message);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Dialog open={open} onClose={isSaving ? undefined : onClose} fullWidth maxWidth="sm">
      <form onSubmit={handleSubmit}>
        <DialogTitle>{isEditing ? "Cập nhật sản phẩm" : "Thêm sản phẩm"}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {error && <Alert severity="error">{error}</Alert>}
            <TextField
              label="Code"
              value={code}
              onChange={(event) => setCode(event.target.value)}
              disabled={isEditing}
              error={Boolean(errors.code)}
              helperText={errors.code?.[0]}
              inputProps={{ maxLength: 50 }}
              autoFocus={!isEditing}
              required
            />
            <TextField
              label="Tên sản phẩm"
              value={name}
              onChange={(event) => setName(event.target.value)}
              error={Boolean(errors.name)}
              helperText={errors.name?.[0]}
              inputProps={{ maxLength: 255 }}
              autoFocus={isEditing}
              required
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isSaving}>Hủy</Button>
          <Button type="submit" variant="contained" disabled={isSaving}>
            {isSaving ? "Đang lưu..." : "Lưu"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
