import { useCallback, useEffect, useState } from "react";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  MenuItem,
  Paper,
  Stack,
  TableContainer,
  TablePagination,
  TextField,
  Typography,
} from "@mui/material";
import AddRoundedIcon from "@mui/icons-material/AddRounded";
import { createCategory, getCategories, updateCategory } from "./api/categoriesApi.js";
import { CategoryFormDialog } from "./components/CategoryFormDialog.jsx";
import { CategoryStatusDialog } from "./components/CategoryStatusDialog.jsx";
import { CategoryTreeTable } from "./components/CategoryTreeTable.jsx";

export function CategoryPage({ grantedPermissions = [] }) {
  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("all");
  const [expanded, setExpanded] = useState(new Set());
  const [editing, setEditing] = useState(undefined);
  const [formOpen, setFormOpen] = useState(false);
  const [deactivating, setDeactivating] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const permissionSet = new Set(grantedPermissions);

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const result = await getCategories({ search, status, page: page + 1, pageSize });
      setItems(result.items);
      setTotal(result.totalCount);
      if (result.items.length === 0 && page > 0) setPage((value) => value - 1);
    } catch (failure) {
      setError(failure.message);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, status]);

  useEffect(() => {
    // Loading remote data is the synchronization performed by this effect.
    // oxlint-disable-next-line react/set-state-in-effect
    load();
  }, [load]);

  const save = async (form) => {
    if (!form.isActive && editing?.isActive) {
      setDeactivating({ ...editing, pending: form });
      return;
    }
    if (editing) await updateCategory(editing.code, form);
    else await createCategory(form);
    await load();
  };

  const confirmDeactivate = async () => {
    await updateCategory(deactivating.code, deactivating.pending);
    setDeactivating(null);
    setFormOpen(false);
    await load();
  };

  return (
    <Box className="category-page">
      <Stack className="category-heading" direction="row" spacing={2}>
        <Box>
          <Typography variant="h4" component="h1">Nhóm món</Typography>
          <Typography color="text.secondary">Quản lý cây nhóm món nhà hàng</Typography>
        </Box>
        {permissionSet.has("Categories.Create") && (
          <Button variant="contained" startIcon={<AddRoundedIcon />} onClick={() => {
            setEditing(undefined);
            setFormOpen(true);
          }}>
            Thêm nhóm món
          </Button>
        )}
      </Stack>
      <Paper className="category-card" elevation={0}>
        <Stack component="form" className="category-toolbar" direction="row" onSubmit={(event) => {
          event.preventDefault();
          setPage(0);
          setSearch(searchInput.trim());
        }}>
          <TextField size="small" label="Tìm mã hoặc tên" value={searchInput} onChange={(event) => setSearchInput(event.target.value)} />
          <TextField select size="small" label="Trạng thái" value={status} onChange={(event) => {
            setStatus(event.target.value);
            setPage(0);
          }}>
            <MenuItem value="all">Tất cả</MenuItem>
            <MenuItem value="active">Đang hoạt động</MenuItem>
            <MenuItem value="inactive">Đã ngừng</MenuItem>
            <MenuItem value="effective-active">Có hiệu lực bán</MenuItem>
            <MenuItem value="effective-inactive">Không hiệu lực bán</MenuItem>
          </TextField>
          <Button type="submit" variant="outlined">Tìm kiếm</Button>
          <Button onClick={load}>Tải lại</Button>
        </Stack>
        {error && <Alert severity="error" action={<Button onClick={load}>Thử lại</Button>}>{error}</Alert>}
        {loading ? (
          <Box className="category-loading"><CircularProgress /></Box>
        ) : items.length === 0 ? (
          <Box className="category-loading">Không có nhóm món phù hợp.</Box>
        ) : (
          <TableContainer>
            <CategoryTreeTable
              categories={items}
              expanded={expanded}
              onToggle={(id) => setExpanded((current) => {
                const next = new Set(current);
                if (next.has(id)) next.delete(id);
                else next.add(id);
                return next;
              })}
              onEdit={(category) => {
                setEditing(category);
                setFormOpen(true);
              }}
              canUpdate={permissionSet.has("Categories.Update")}
            />
          </TableContainer>
        )}
        <TablePagination
          component="div"
          count={total}
          page={page}
          rowsPerPage={pageSize}
          rowsPerPageOptions={[10, 20, 50, 100]}
          onPageChange={(_, value) => setPage(value)}
          onRowsPerPageChange={(event) => {
            setPageSize(Number(event.target.value));
            setPage(0);
          }}
        />
      </Paper>
      <CategoryFormDialog
        open={formOpen}
        category={editing}
        categories={items}
        onClose={() => setFormOpen(false)}
        onSubmit={save}
      />
      <CategoryStatusDialog
        category={deactivating}
        onClose={() => setDeactivating(null)}
        onConfirm={confirmDeactivate}
      />
    </Box>
  );
}
