import { useCallback, useEffect, useState } from "react";
import { Alert, Box, Button, Chip, CircularProgress, MenuItem, Paper, Stack, Tab, Table, TableBody, TableCell, TableContainer, TableHead, TablePagination, TableRow, Tabs, TextField, Typography } from "@mui/material";
import AddRoundedIcon from "@mui/icons-material/AddRounded";
import { createArea, createTable, getAreas, getTables, setTableActivation, updateArea, updateTable } from "./api/layoutsApi.js";
import { AreaDialog } from "./components/AreaDialogs.jsx";
import { TableDialog } from "./components/TableDialogs.jsx";

export function LayoutsPage({ grantedPermissions = [] }) {
  const [tab, setTab] = useState("areas");
  const [areas, setAreas] = useState([]);
  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("all");
  const [editing, setEditing] = useState(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const permissions = new Set(grantedPermissions);

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const areaResult = await getAreas({ page: 1, pageSize: 100, sort: "displayOrder" });
      setAreas(areaResult.items);
      const result = tab === "areas"
        ? await getAreas({ search, isActive: status === "all" ? "" : status === "active", page: page + 1, pageSize, sort: "displayOrder" })
        : await getTables({ search, status, page: page + 1, pageSize, sort: "area" });
      setItems(result.items);
      setTotal(result.totalCount);
    } catch (failure) { setError(failure.message); }
    finally { setLoading(false); }
  }, [tab, search, status, page, pageSize]);

  useEffect(() => {
    // Loading remote data is the synchronization performed by this effect.
    // oxlint-disable-next-line react/set-state-in-effect
    load();
  }, [load]);
  const save = async (value) => {
    if (tab === "areas") {
      if (editing) await updateArea(editing.code, value); else await createArea(value);
    } else if (editing) await updateTable(editing.code, value); else await createTable(value);
    await load();
  };
  const toggleTable = async (table) => {
    try { await setTableActivation(table.code, { isActive: !table.isActive, version: table.version }); await load(); }
    catch (failure) { setError(failure.message); }
  };

  return (
    <Box className="layout-page">
      <Stack className="layout-heading" direction="row" spacing={2}>
        <Box><Typography variant="h4" component="h1">Thiết lập khu vực & bàn</Typography><Typography color="text.secondary">Quản lý sơ đồ phục vụ tại nhà hàng</Typography></Box>
        {permissions.has("Layouts.Create") && <Button variant="contained" startIcon={<AddRoundedIcon />} onClick={() => { setEditing(null); setDialogOpen(true); }}>Thêm {tab === "areas" ? "khu vực" : "bàn"}</Button>}
      </Stack>
      <Paper className="layout-card" elevation={0}>
        <Tabs value={tab} onChange={(_, value) => { setTab(value); setPage(0); setStatus("all"); }}><Tab value="areas" label="Khu vực" /><Tab value="tables" label="Bàn" /></Tabs>
        <Stack component="form" className="layout-toolbar" direction="row" onSubmit={(event) => { event.preventDefault(); setPage(0); setSearch(searchInput.trim()); }}>
          <TextField size="small" label="Tìm mã hoặc tên" value={searchInput} onChange={(event) => setSearchInput(event.target.value)} />
          <TextField select size="small" label="Trạng thái" value={status} onChange={(event) => { setStatus(event.target.value); setPage(0); }}><MenuItem value="all">Tất cả</MenuItem><MenuItem value="active">Đang hoạt động</MenuItem><MenuItem value="inactive">Đã ngừng</MenuItem>{tab === "tables" && ["Available", "Occupied", "Cleaning", "Disabled"].map((value) => <MenuItem key={value} value={value}>{value}</MenuItem>)}</TextField>
          <Button type="submit" variant="outlined">Tìm kiếm</Button><Button onClick={load}>Tải lại</Button>
        </Stack>
        {error && <Alert severity="error" action={<Button onClick={load}>Thử lại</Button>}>{error}</Alert>}
        {loading ? <Box className="layout-loading"><CircularProgress /></Box> : <TableContainer><Table size="small"><TableHead><TableRow><TableCell>Mã</TableCell><TableCell>Tên</TableCell>{tab === "areas" ? <><TableCell>Thứ tự</TableCell><TableCell>Số bàn</TableCell></> : <><TableCell>Khu vực</TableCell><TableCell>Sức chứa</TableCell></>}<TableCell>Trạng thái</TableCell><TableCell align="right">Thao tác</TableCell></TableRow></TableHead><TableBody>{items.map((item) => <TableRow key={item.id} hover><TableCell>{item.code}</TableCell><TableCell>{item.name}</TableCell>{tab === "areas" ? <><TableCell>{item.displayOrder}</TableCell><TableCell>{item.tableCount}</TableCell></> : <><TableCell>{item.areaName}</TableCell><TableCell>{item.capacity}</TableCell></>}<TableCell><Chip size="small" color={item.isActive ? "success" : "default"} label={tab === "tables" ? item.status : item.isActive ? "Hoạt động" : "Đã ngừng"} /></TableCell><TableCell align="right">{permissions.has("Layouts.Update") && <><Button size="small" onClick={() => { setEditing(item); setDialogOpen(true); }}>Sửa</Button>{tab === "tables" && <Button size="small" color={item.isActive ? "warning" : "success"} disabled={item.isActive && !item.canDisable} onClick={() => toggleTable(item)}>{item.isActive ? "Disable" : "Activate"}</Button>}</>}</TableCell></TableRow>)}</TableBody></Table></TableContainer>}
        <TablePagination component="div" count={total} page={page} rowsPerPage={pageSize} rowsPerPageOptions={[10, 20, 50, 100]} onPageChange={(_, value) => setPage(value)} onRowsPerPageChange={(event) => { setPageSize(Number(event.target.value)); setPage(0); }} />
      </Paper>
      {tab === "areas" ? <AreaDialog open={dialogOpen} area={editing} onClose={() => setDialogOpen(false)} onSubmit={save} /> : <TableDialog open={dialogOpen} table={editing} areas={areas} onClose={() => setDialogOpen(false)} onSubmit={save} />}
    </Box>
  );
}
