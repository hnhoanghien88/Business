import { useEffect, useState } from "react";
import {
  Alert,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  Stack,
  TextField,
} from "@mui/material";

export function OpenTableDialog({ open, table, canOverride, onClose, onSubmit }) {
  const [form, setForm] = useState({
    guestCount: 1,
    note: "",
    overrideCapacity: false,
    overrideReason: "",
  });
  const [error, setError] = useState("");

  useEffect(() => {
    // Reset the draft when a different table opens.
    if (open) {
      // oxlint-disable-next-line react/set-state-in-effect
      setForm({
        guestCount: 1,
        note: "",
        overrideCapacity: false,
        overrideReason: "",
      });
      setError("");
    }
  }, [open, table]);

  const save = async () => {
    setError("");
    try {
      await onSubmit({
        ...form,
        guestCount: Number(form.guestCount),
      });
      onClose();
    } catch (failure) {
      setError(failure.message);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>Mở bàn {table?.name}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          {error && <Alert severity="error">{error}</Alert>}
          <TextField
            label="Số khách"
            type="number"
            required
            inputProps={{ min: 1 }}
            value={form.guestCount}
            onChange={(event) => setForm((value) => ({
              ...value,
              guestCount: event.target.value,
            }))}
          />
          <TextField
            label="Ghi chú"
            multiline
            value={form.note}
            onChange={(event) => setForm((value) => ({
              ...value,
              note: event.target.value,
            }))}
          />
          {canOverride && Number(form.guestCount) > (table?.capacity ?? 0) && (
            <>
              <FormControlLabel
                control={(
                  <Checkbox
                    checked={form.overrideCapacity}
                    onChange={(event) => setForm((value) => ({
                      ...value,
                      overrideCapacity: event.target.checked,
                    }))}
                  />
                )}
                label="Cho phép vượt sức chứa"
              />
              {form.overrideCapacity && (
                <TextField
                  label="Lý do override"
                  required
                  value={form.overrideReason}
                  onChange={(event) => setForm((value) => ({
                    ...value,
                    overrideReason: event.target.value,
                  }))}
                />
              )}
            </>
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Hủy</Button>
        <Button variant="contained" onClick={save}>Mở bàn</Button>
      </DialogActions>
    </Dialog>
  );
}
