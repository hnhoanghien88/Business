import { useCallback, useEffect, useState } from "react";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  MenuItem,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TextField,
  Typography,
} from "@mui/material";
import { getCategories } from "../categories/api/categoriesApi.js";
import {
  changeAvailability,
  changePrice,
  createFood,
  createVariant,
  deactivateFood,
  getFood,
  getFoods,
  getPriceHistory,
  updateFood,
  updateVariant,
} from "./api/foodsApi.js";
import { DeactivateFoodDialog } from "./components/DeactivateFoodDialog.jsx";
import { FoodFormDialog } from "./components/FoodFormDialog.jsx";
import { FoodVariantsDialog } from "./components/FoodVariantsDialog.jsx";

export function FoodsPage({ grantedPermissions = [] }) {
  const permissionSet = new Set(grantedPermissions);
  const permissions = {
    create: permissionSet.has("Foods.Create"),
    update: permissionSet.has("Foods.Update"),
    delete: permissionSet.has("Foods.Delete"),
    price: permissionSet.has("Foods.Update"),
    availability: permissionSet.has("Foods.Update"),
  };
  const [foods, setFoods] = useState([]);
  const [categories, setCategories] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [status, setStatus] = useState("all");
  const [availability, setAvailability] = useState("all");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [editing, setEditing] = useState(undefined);
  const [formOpen, setFormOpen] = useState(false);
  const [deactivating, setDeactivating] = useState(null);
  const [dialogError, setDialogError] = useState("");
  const [processing, setProcessing] = useState(false);
  const [detail, setDetail] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const result = await getFoods({
        search,
        categoryId,
        status,
        availability,
        page: page + 1,
        pageSize,
      });
      setFoods(result.items);
      setTotal(result.totalCount);
      if (result.items.length === 0 && page > 0) setPage((value) => value - 1);
    } catch (failure) {
      setError(failure.message);
    } finally {
      setLoading(false);
    }
  }, [availability, categoryId, page, pageSize, search, status]);

  useEffect(() => {
    // oxlint-disable-next-line react/set-state-in-effect
    load();
  }, [load]);

  useEffect(() => {
    getCategories({ status: "active", pageSize: 100 })
      .then((result) => setCategories(result.items))
      .catch(() => setCategories([]));
  }, []);

  const refreshDetail = async (code = detail?.code) => {
    if (code) setDetail(await getFood(code));
    await load();
  };

  const save = async (form) => {
    if (editing) await updateFood(editing.code, form);
    else await createFood(form);
    await load();
  };

  const confirmDeactivate = async () => {
    setProcessing(true);
    setDialogError("");
    try {
      await deactivateFood(deactivating.code, deactivating.version);
      setDeactivating(null);
      await load();
    } catch (failure) {
      setDialogError(failure.message);
    } finally {
      setProcessing(false);
    }
  };

  return (
    <Box className="food-page">
      <Stack className="food-heading" direction="row" spacing={2}>
        <Box>
          <Typography variant="h4" component="h1">Món ăn</Typography>
          <Typography color="text.secondary">
            Quản lý món, biến thể, giá và tình trạng phục vụ
          </Typography>
        </Box>
        {permissions.create && (
          <Button variant="contained" onClick={() => {
            setEditing(undefined);
            setFormOpen(true);
          }}>
            Thêm món
          </Button>
        )}
      </Stack>

      <Paper className="food-card" elevation={0}>
        <Stack
          component="form"
          className="food-toolbar"
          direction="row"
          onSubmit={(event) => {
            event.preventDefault();
            setPage(0);
            setSearch(searchInput.trim());
          }}
        >
          <TextField
            size="small"
            label="Tìm mã hoặc tên"
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
          />
          <TextField
            select
            size="small"
            label="Nhóm"
            value={categoryId}
            onChange={(event) => {
              setCategoryId(event.target.value);
              setPage(0);
            }}
          >
            <MenuItem value="">Tất cả</MenuItem>
            {categories.map((category) => (
              <MenuItem key={category.id} value={category.id}>
                {category.name}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            select
            size="small"
            label="Trạng thái"
            value={status}
            onChange={(event) => {
              setStatus(event.target.value);
              setPage(0);
            }}
          >
            <MenuItem value="all">Tất cả</MenuItem>
            <MenuItem value="active">Hoạt động</MenuItem>
            <MenuItem value="inactive">Ngừng</MenuItem>
          </TextField>
          <TextField
            select
            size="small"
            label="Khả dụng"
            value={availability}
            onChange={(event) => {
              setAvailability(event.target.value);
              setPage(0);
            }}
          >
            <MenuItem value="all">Tất cả</MenuItem>
            <MenuItem value="available">Còn món</MenuItem>
            <MenuItem value="unavailable">Hết món</MenuItem>
          </TextField>
          <Button type="submit" variant="outlined">Tìm</Button>
          <Button onClick={load}>Tải lại</Button>
        </Stack>
        {error && <Alert severity="error">{error}</Alert>}
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Mã</TableCell>
                <TableCell>Tên món</TableCell>
                <TableCell>Nhóm</TableCell>
                <TableCell>Khoảng giá</TableCell>
                <TableCell>Trạng thái</TableCell>
                <TableCell align="right">Thao tác</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={6} align="center" className="food-empty">
                    <CircularProgress size={28} />
                  </TableCell>
                </TableRow>
              ) : foods.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} align="center" className="food-empty">
                    Chưa có món phù hợp.
                  </TableCell>
                </TableRow>
              ) : foods.map((food) => (
                <TableRow key={food.code} hover>
                  <TableCell><Typography className="food-code">{food.code}</Typography></TableCell>
                  <TableCell>{food.name}</TableCell>
                  <TableCell>{food.categoryName}</TableCell>
                  <TableCell>
                    {food.minimumPrice == null
                      ? "Chưa có"
                      : `${food.minimumPrice.toLocaleString("vi-VN")}–${food.maximumPrice.toLocaleString("vi-VN")} đ`}
                  </TableCell>
                  <TableCell>{food.isActive ? "Hoạt động" : "Ngừng"}</TableCell>
                  <TableCell align="right">
                    <Button onClick={async () => setDetail(await getFood(food.code))}>
                      Biến thể
                    </Button>
                    {permissions.update && (
                      <Button onClick={async () => {
                        setEditing(await getFood(food.code));
                        setFormOpen(true);
                      }}>
                        Sửa
                      </Button>
                    )}
                    {permissions.delete && food.isActive && (
                      <Button color="error" onClick={() => setDeactivating(food)}>
                        Ngừng
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
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

      <FoodFormDialog
        open={formOpen}
        food={editing}
        categories={categories}
        onClose={() => setFormOpen(false)}
        onSubmit={save}
      />
      <DeactivateFoodDialog
        food={deactivating}
        error={dialogError}
        processing={processing}
        onClose={() => setDeactivating(null)}
        onConfirm={confirmDeactivate}
      />
      <FoodVariantsDialog
        food={detail}
        permissions={permissions}
        onClose={() => setDetail(null)}
        onCreate={async (variant) => {
          await createVariant(detail.code, variant);
          await refreshDetail();
        }}
        onUpdate={async (variantCode, variant) => {
          await updateVariant(detail.code, variantCode, variant);
          await refreshDetail();
        }}
        onPrice={async (variantCode, price, version) => {
          await changePrice(detail.code, variantCode, price, version);
          await refreshDetail();
        }}
        onHistory={(variantCode) => getPriceHistory(detail.code, variantCode)}
        onAvailability={async (variantCode, available, reason, version) => {
          await changeAvailability(detail.code, variantCode, available, reason, version);
          await refreshDetail();
        }}
      />
    </Box>
  );
}
