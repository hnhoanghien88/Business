import { useCallback, useEffect, useState } from "react";
import {
  Alert, Box, Button, CircularProgress, IconButton, InputAdornment, Paper, Stack,
  Table, TableBody, TableCell, TableContainer, TableHead, TablePagination, TableRow,
  TextField, Tooltip, Typography,
} from "@mui/material";
import AddRoundedIcon from "@mui/icons-material/AddRounded";
import DeleteOutlineRoundedIcon from "@mui/icons-material/DeleteOutlineRounded";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import RefreshRoundedIcon from "@mui/icons-material/RefreshRounded";
import SearchRoundedIcon from "@mui/icons-material/SearchRounded";
import { createProduct, deleteProduct, getProducts, updateProduct } from "./api/productsApi.js";
import { DeleteProductDialog } from "./components/DeleteProductDialog.jsx";
import { ProductFormDialog } from "./components/ProductFormDialog.jsx";

const permissions = {
  create: "Product.Create",
  update: "Product.Update",
  delete: "Product.Delete",
};

export function ProductPage({ grantedPermissions = [] }) {
  const [products, setProducts] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [editingProduct, setEditingProduct] = useState(undefined);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [deletingProduct, setDeletingProduct] = useState(null);
  const [deleteError, setDeleteError] = useState("");
  const [isDeleting, setIsDeleting] = useState(false);
  const permissionSet = new Set(grantedPermissions);

  const loadProducts = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      const result = await getProducts({ search, page: page + 1, pageSize });
      setProducts(result.items);
      setTotalCount(result.totalCount);
    } catch (loadError) {
      setError(loadError.status === 403
        ? "Bạn không có quyền xem danh sách sản phẩm."
        : loadError.message);
    } finally {
      setIsLoading(false);
    }
  }, [page, pageSize, search]);

  useEffect(() => {
    // Loading remote data is the synchronization performed by this effect.
    // oxlint-disable-next-line react/set-state-in-effect
    loadProducts();
  }, [loadProducts]);

  const handleSearch = (event) => {
    event.preventDefault();
    setPage(0);
    setSearch(searchInput.trim());
  };

  const handleSave = async (product) => {
    if (editingProduct) await updateProduct(editingProduct.code, product);
    else await createProduct(product);
    await loadProducts();
  };

  const handleDelete = async () => {
    setIsDeleting(true);
    setDeleteError("");
    try {
      await deleteProduct(deletingProduct.code);
      setDeletingProduct(null);
      if (products.length === 1 && page > 0) setPage((value) => value - 1);
      else await loadProducts();
    } catch (deleteFailure) {
      setDeleteError(deleteFailure.message);
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <Box className="product-page">
      <Stack className="product-heading" direction="row" spacing={2}>
        <Box>
          <Typography variant="h4" component="h1">Sản phẩm</Typography>
          <Typography color="text.secondary">Quản lý danh mục sản phẩm nhà hàng</Typography>
        </Box>
        {permissionSet.has(permissions.create) && (
          <Button variant="contained" startIcon={<AddRoundedIcon />} onClick={() => {
            setEditingProduct(undefined);
            setIsFormOpen(true);
          }}>
            Thêm sản phẩm
          </Button>
        )}
      </Stack>

      <Paper className="product-card" elevation={0}>
        <Stack component="form" className="product-toolbar" direction="row" onSubmit={handleSearch}>
          <TextField
            size="small"
            placeholder="Tìm theo code hoặc tên..."
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
            slotProps={{ input: { startAdornment: (
              <InputAdornment position="start"><SearchRoundedIcon fontSize="small" /></InputAdornment>
            ) } }}
          />
          <Button type="submit" variant="outlined">Tìm kiếm</Button>
          <Tooltip title="Tải lại">
            <IconButton onClick={loadProducts} disabled={isLoading}><RefreshRoundedIcon /></IconButton>
          </Tooltip>
        </Stack>

        {error && <Alert severity="error" sx={{ m: 2 }}>{error}</Alert>}
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell width="35%">Code</TableCell>
                <TableCell>Tên sản phẩm</TableCell>
                <TableCell align="right" width={130}>Thao tác</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {isLoading ? (
                <TableRow><TableCell colSpan={3} align="center" className="product-empty"><CircularProgress size={28} /></TableCell></TableRow>
              ) : products.length === 0 ? (
                <TableRow><TableCell colSpan={3} align="center" className="product-empty">Chưa có sản phẩm phù hợp.</TableCell></TableRow>
              ) : products.map((product) => (
                <TableRow key={product.code} hover>
                  <TableCell><Typography className="product-code">{product.code}</Typography></TableCell>
                  <TableCell>{product.name}</TableCell>
                  <TableCell align="right">
                    {permissionSet.has(permissions.update) && (
                      <Tooltip title="Sửa"><IconButton size="small" onClick={() => {
                        setEditingProduct(product);
                        setIsFormOpen(true);
                      }}><EditOutlinedIcon fontSize="small" /></IconButton></Tooltip>
                    )}
                    {permissionSet.has(permissions.delete) && (
                      <Tooltip title="Xóa"><IconButton size="small" color="error" onClick={() => setDeletingProduct(product)}><DeleteOutlineRoundedIcon fontSize="small" /></IconButton></Tooltip>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>

        <TablePagination
          component="div" count={totalCount} page={page} rowsPerPage={pageSize}
          rowsPerPageOptions={[5, 10, 20, 50]}
          onPageChange={(_, nextPage) => setPage(nextPage)}
          onRowsPerPageChange={(event) => {
            setPageSize(Number(event.target.value));
            setPage(0);
          }}
          labelRowsPerPage="Số dòng:"
        />
      </Paper>

      {isFormOpen && (
        <ProductFormDialog
          open product={editingProduct}
          onClose={() => setIsFormOpen(false)} onSubmit={handleSave}
        />
      )}
      <DeleteProductDialog
        product={deletingProduct} error={deleteError} isDeleting={isDeleting}
        onClose={() => {
          setDeletingProduct(null);
          setDeleteError("");
        }}
        onConfirm={handleDelete}
      />
    </Box>
  );
}
