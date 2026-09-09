import { useCallback, useEffect, useMemo, useState } from "react";
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  MenuItem,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import {
  closeSession,
  getOperationalTables,
  getSession,
  markTableClean,
  openTable,
} from "./api/tableOperationsApi.js";
import { OpenTableDialog } from "./components/OpenTableDialog.jsx";
import { SessionDialog } from "./components/SessionDialog.jsx";

export function TableOperationsPage({ grantedPermissions = [], navigate }) {
  const [tables, setTables] = useState([]);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("all");
  const [opening, setOpening] = useState(null);
  const [session, setSession] = useState(null);
  const [loading, setLoading] = useState(true);
  const [connected, setConnected] = useState(true);
  const [error, setError] = useState("");
  const permissionSet = useMemo(
    () => new Set(grantedPermissions),
    [grantedPermissions]);

  const load = useCallback(async (showSpinner = false) => {
    if (showSpinner) setLoading(true);
    try {
      const result = await getOperationalTables({ search, status });
      setTables(result);
      setConnected(true);
      setError("");
    } catch (failure) {
      setConnected(false);
      setError(failure.message);
    } finally {
      if (showSpinner) setLoading(false);
    }
  }, [search, status]);

  useEffect(() => {
    // Remote snapshot synchronization is the purpose of this effect.
    // oxlint-disable-next-line react/set-state-in-effect
    load(true);
    const interval = window.setInterval(load, 2000);
    const reconnect = () => load(true);
    window.addEventListener("online", reconnect);
    return () => {
      window.clearInterval(interval);
      window.removeEventListener("online", reconnect);
    };
  }, [load]);

  const groups = useMemo(() => Object.values(tables.reduce((result, table) => {
    const key = String(table.areaId);
    result[key] ??= {
      id: table.areaId,
      name: table.areaName,
      tables: [],
    };
    result[key].tables.push(table);
    return result;
  }, {})), [tables]);

  const viewSession = async (sessionId) => {
    try {
      setSession(await getSession(sessionId));
    } catch (failure) {
      setError(failure.message);
    }
  };

  return (
    <Box className="table-operations-page">
      <Stack className="table-operations-heading" direction="row" spacing={2}>
        <Box>
          <Typography variant="h4" component="h1">Sơ đồ bàn</Typography>
          <Typography color="text.secondary">Theo dõi và vận hành lượt phục vụ</Typography>
        </Box>
        <Chip
          color={connected ? "success" : "warning"}
          label={connected ? "Đang đồng bộ" : "Mất kết nối"}
        />
      </Stack>
      <Paper className="table-operations-toolbar" component="form" elevation={0} onSubmit={(event) => {
        event.preventDefault();
        setSearch(searchInput.trim());
      }}>
        <TextField
          size="small"
          label="Tìm mã hoặc tên bàn"
          value={searchInput}
          onChange={(event) => setSearchInput(event.target.value)}
        />
        <TextField
          select
          size="small"
          label="Trạng thái"
          value={status}
          onChange={(event) => setStatus(event.target.value)}
        >
          {["all", "Available", "Occupied", "Cleaning", "Disabled"].map((value) => (
            <MenuItem key={value} value={value}>{value === "all" ? "Tất cả" : value}</MenuItem>
          ))}
        </TextField>
        <Button type="submit" variant="outlined">Tìm kiếm</Button>
        <Button onClick={() => load(true)}>Tải lại</Button>
      </Paper>
      {error && <Alert severity="error" action={<Button onClick={() => load(true)}>Thử lại</Button>}>{error}</Alert>}
      {loading ? (
        <Box className="table-operations-loading"><CircularProgress /></Box>
      ) : groups.length === 0 ? (
        <Box className="table-operations-loading">Không có bàn phù hợp.</Box>
      ) : groups.map((group) => (
        <section key={group.id} className="table-area-group">
          <Typography variant="h6" component="h2">{group.name}</Typography>
          <Box className="table-card-grid">
            {group.tables.map((table) => (
              <Paper
                key={table.id}
                component="article"
                className={`operation-table-card status-${table.status.toLowerCase()}`}
                variant="outlined"
              >
                <Stack direction="row" justifyContent="space-between">
                  <Typography variant="h6">{table.name}</Typography>
                  <Chip size="small" label={table.status} />
                </Stack>
                <Typography color="text.secondary">{table.code} · {table.capacity} chỗ</Typography>
                {table.sessionId && (
                  <Typography>{table.guestCount} khách · {duration(table.openedDate)}</Typography>
                )}
                <Stack direction="row" spacing={1} className="operation-card-actions">
                  {table.status === "Available" && permissionSet.has("TableOperations.Open") && (
                    <Button size="small" variant="contained" onClick={() => setOpening(table)}>Mở bàn</Button>
                  )}
                  {table.sessionId && (
                    <Button size="small" onClick={() => viewSession(table.sessionId)}>Chi tiết</Button>
                  )}
                  {table.status === "Cleaning" && permissionSet.has("TableOperations.Clean") && (
                    <Button size="small" color="success" onClick={async () => {
                      try {
                        await markTableClean(table.code);
                        await load();
                      } catch (failure) {
                        setError(failure.message);
                      }
                    }}>Đã dọn xong</Button>
                  )}
                </Stack>
              </Paper>
            ))}
          </Box>
        </section>
      ))}
      <OpenTableDialog
        open={Boolean(opening)}
        table={opening}
        canOverride={permissionSet.has("TableOperations.Override")}
        onClose={() => setOpening(null)}
        onSubmit={async (request) => {
          await openTable(opening.code, request);
          await load();
        }}
      />
      <SessionDialog
        open={Boolean(session)}
        session={session}
        canClose={permissionSet.has("TableOperations.Close")}
        canOverride={permissionSet.has("TableOperations.Override")}
        canOrder={permissionSet.has("Ordering.Create")}
        canPay={permissionSet.has("Payments.Create")}
        onClose={() => setSession(null)}
        onCloseSession={async (request) => {
          await closeSession(session.id, request);
          await load();
        }}
        onNavigate={navigate}
      />
    </Box>
  );
}

function duration(value) {
  const minutes = Math.max(0, Math.floor((Date.now() - new Date(value).getTime()) / 60000));
  return `${minutes} phút`;
}
