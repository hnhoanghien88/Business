import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
} from "@mui/material";

export function DeleteProductDialog({ product, error, isDeleting, onClose, onConfirm }) {
  return (
    <Dialog open={Boolean(product)} onClose={isDeleting ? undefined : onClose}>
      <DialogTitle>Xóa sản phẩm</DialogTitle>
      <DialogContent>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        <DialogContentText>
          Bạn có chắc muốn xóa sản phẩm <strong>{product?.name}</strong> ({product?.code})?
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={isDeleting}>Hủy</Button>
        <Button color="error" variant="contained" onClick={onConfirm} disabled={isDeleting}>
          {isDeleting ? "Đang xóa..." : "Xóa"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
