import {
  Chip,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Tooltip,
} from "@mui/material";
import ChevronRightRoundedIcon from "@mui/icons-material/ChevronRightRounded";
import ExpandMoreRoundedIcon from "@mui/icons-material/ExpandMoreRounded";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";

export function CategoryTreeTable({ categories, expanded, onToggle, onEdit, canUpdate }) {
  const visible = categories.filter((category) =>
    category.ancestorPath.every((ancestor) => expanded.has(ancestor.id)));

  return (
    <Table aria-label="Cây nhóm món">
      <TableHead>
        <TableRow>
          <TableCell>Nhóm món</TableCell>
          <TableCell>Mã</TableCell>
          <TableCell>Thứ tự</TableCell>
          <TableCell>Trạng thái</TableCell>
          <TableCell align="right">Thao tác</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {visible.map((category) => (
          <TableRow key={category.id} hover>
            <TableCell>
              <span style={{ display: "inline-flex", alignItems: "center", paddingLeft: category.depth * 24 }}>
                {category.hasChildren ? (
                  <IconButton
                    size="small"
                    aria-label={expanded.has(category.id) ? "Thu gọn nhánh" : "Mở rộng nhánh"}
                    onClick={() => onToggle(category.id)}
                  >
                    {expanded.has(category.id) ? <ExpandMoreRoundedIcon /> : <ChevronRightRoundedIcon />}
                  </IconButton>
                ) : <span style={{ width: 34 }} />}
                {category.name}
              </span>
            </TableCell>
            <TableCell>{category.code}</TableCell>
            <TableCell>{category.displayOrder}</TableCell>
            <TableCell>
              <Chip
                size="small"
                color={category.isEffectivelyActive ? "success" : "default"}
                label={category.isEffectivelyActive ? "Hoạt động" : "Ngừng"}
              />
            </TableCell>
            <TableCell align="right">
              {canUpdate && (
                <Tooltip title="Sửa">
                  <IconButton size="small" onClick={() => onEdit(category)}>
                    <EditOutlinedIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              )}
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
