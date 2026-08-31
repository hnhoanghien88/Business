import { Button, Dialog, DialogActions, DialogContent, DialogTitle, Typography } from "@mui/material";

export function CategoryStatusDialog({ category, onClose, onConfirm }) {
  return (
    <Dialog open={Boolean(category)} onClose={onClose}>
      <DialogTitle>Ngừng hoạt động nhóm món?</DialogTitle>
      <DialogContent>
        <Typography>
          {category?.descendantCount ?? 0} nhóm con và {category?.directFoodCount ?? 0} món trực tiếp
          sẽ không còn hiệu lực bán. Trạng thái riêng của chúng được giữ nguyên.
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Hủy</Button>
        <Button color="warning" variant="contained" onClick={onConfirm}>Xác nhận</Button>
      </DialogActions>
    </Dialog>
  );
}
