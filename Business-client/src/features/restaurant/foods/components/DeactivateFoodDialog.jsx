import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
} from "@mui/material";

export function DeactivateFoodDialog({
  food,
  error,
  processing,
  onClose,
  onConfirm,
}) {
  return (
    <Dialog open={Boolean(food)} onClose={processing ? undefined : onClose}>
      <DialogTitle>Ngừng sử dụng món</DialogTitle>
      <DialogContent>
        {error && <Alert severity="error">{error}</Alert>}
        <Typography>
          Ngừng sử dụng <strong>{food?.name}</strong> ({food?.code}) và toàn bộ
          biến thể của món? Dữ liệu lịch sử vẫn được giữ nguyên.
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={processing}>Hủy</Button>
        <Button
          color="error"
          variant="contained"
          onClick={onConfirm}
          disabled={processing}
        >
          Ngừng sử dụng
        </Button>
      </DialogActions>
    </Dialog>
  );
}
