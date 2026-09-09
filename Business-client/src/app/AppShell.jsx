import { useState } from "react";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Divider,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import LogoutRoundedIcon from "@mui/icons-material/LogoutRounded";
import StorefrontOutlinedIcon from "@mui/icons-material/StorefrontOutlined";
import RestaurantMenuRoundedIcon from "@mui/icons-material/RestaurantMenuRounded";
import { logout } from "../services/identity/identityClient.js";
import { getSessionUser } from "../shared/auth/sessionUser.js";
import { FoodsPage } from "../features/restaurant/foods/FoodsPage.jsx";
import { CategoryPage } from "../features/restaurant/categories/CategoryPage.jsx";
import { LayoutsPage } from "../features/restaurant/layouts/LayoutsPage.jsx";
import { TableOperationsPage } from "../features/restaurant/tableOperations/TableOperationsPage.jsx";
import { OrderingPage } from "../features/restaurant/ordering/OrderingPage.jsx";

const FOOD_PATH = "/restaurant/foods";
const CATEGORY_PATH = "/restaurant/categories";
const LAYOUT_PATH = "/restaurant/layouts";
const TABLE_OPERATIONS_PATH = "/restaurant/table-operations";
const ORDERING_PATH = "/restaurant/ordering";

export function AppShell({ session, path, navigate, onLogout }) {
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const [error, setError] = useState("");
  const sessionUser = getSessionUser(session);
  const foodMenu = findFoodMenu(session.authorization?.menus) ?? {
    name: "Foods",
    route: FOOD_PATH,
  };
  const categoryMenu = findMenu(session.authorization?.menus, "categories", CATEGORY_PATH);
  const layoutMenu = findMenu(session.authorization?.menus, "layouts", LAYOUT_PATH);
  const tableOperationsMenu = findMenu(
    session.authorization?.menus,
    "table-operations",
    TABLE_OPERATIONS_PATH);
  const orderingMenu = findMenu(session.authorization?.menus, "ordering", ORDERING_PATH);

  const handleLogout = async () => {
    setError("");
    setIsLoggingOut(true);
    try {
      await logout();
    } catch (logoutError) {
      setError(logoutError.message);
    } finally {
      onLogout();
    }
  };

  return (
    <Box className="business-shell">
      <Paper component="aside" className="business-sidebar" square elevation={0}>
        <Stack className="business-brand" direction="row" spacing={1.5}>
          <Box className="business-brand-icon">
            <StorefrontOutlinedIcon fontSize="small" />
          </Box>
          <Box>
            <Typography variant="h6" fontWeight={700}>Restaurant</Typography>
            <Typography variant="caption" color="text.secondary">Restaurant</Typography>
          </Box>
        </Stack>

        <Stack className="sidebar-account" direction="row" spacing={1}>
          <Box className="sidebar-user-name">
            <Typography variant="body2">{sessionUser.displayName}</Typography>
            <Typography variant="caption">{sessionUser.code}</Typography>
          </Box>
          <Button color="inherit" onClick={handleLogout} disabled={isLoggingOut}
            startIcon={isLoggingOut
              ? <CircularProgress size={16} color="inherit" />
              : <LogoutRoundedIcon />}>
            Logout
          </Button>
        </Stack>

        <Divider />
        <List className="business-menu" aria-label="Main navigation">
          <ListItemButton
            selected={path === FOOD_PATH}
            onClick={() => navigate(foodMenu.route || FOOD_PATH)}
          >
            <ListItemIcon><RestaurantMenuRoundedIcon /></ListItemIcon>
            <ListItemText primary={foodMenu.name} />
          </ListItemButton>
          {categoryMenu && (
            <ListItemButton
              selected={path === CATEGORY_PATH}
              onClick={() => navigate(categoryMenu.route || CATEGORY_PATH)}
            >
              <ListItemIcon><RestaurantMenuRoundedIcon /></ListItemIcon>
              <ListItemText primary={categoryMenu.name || "Nhóm món"} />
            </ListItemButton>
          )}
          {layoutMenu && (
            <ListItemButton
              selected={path === LAYOUT_PATH}
              onClick={() => navigate(layoutMenu.route || LAYOUT_PATH)}
            >
              <ListItemIcon><StorefrontOutlinedIcon /></ListItemIcon>
              <ListItemText primary={layoutMenu.name || "Thiết lập khu vực & bàn"} />
            </ListItemButton>
          )}
          {tableOperationsMenu && (
            <ListItemButton
              selected={path === TABLE_OPERATIONS_PATH}
              onClick={() => navigate(
                tableOperationsMenu.route || TABLE_OPERATIONS_PATH)}
            >
              <ListItemIcon><StorefrontOutlinedIcon /></ListItemIcon>
              <ListItemText primary={tableOperationsMenu.name || "Sơ đồ bàn"} />
            </ListItemButton>
          )}
          {orderingMenu && (
            <ListItemButton selected={path === ORDERING_PATH}
              onClick={() => navigate(orderingMenu.route || ORDERING_PATH)}>
              <ListItemIcon><RestaurantMenuRoundedIcon /></ListItemIcon>
              <ListItemText primary={orderingMenu.name || "Gọi món"} />
            </ListItemButton>
          )}
        </List>
      </Paper>

      <Box className="business-workspace">
        <Box component="header" className="business-topbar">
          <Typography className="business-page-title">
            {path === CATEGORY_PATH
              ? "Nhóm món"
              : path === LAYOUT_PATH
                ? "Thiết lập khu vực & bàn"
                : path === TABLE_OPERATIONS_PATH
                  ? "Sơ đồ bàn"
                  : "Foods"}
          </Typography>
          <Box className="business-user-avatar" aria-hidden="true">
            {sessionUser.displayName.charAt(0).toUpperCase()}
          </Box>
        </Box>
        <Box component="main" className="business-content">
          {error && <Alert severity="error">{error}</Alert>}
          {path === ORDERING_PATH ? (
            <OrderingPage />
          ) : path === CATEGORY_PATH ? (
            <CategoryPage grantedPermissions={session.authorization?.permissions} />
          ) : path === LAYOUT_PATH ? (
            <LayoutsPage grantedPermissions={session.authorization?.permissions} />
          ) : path === TABLE_OPERATIONS_PATH ? (
            <TableOperationsPage
              grantedPermissions={session.authorization?.permissions}
              navigate={navigate}
            />
          ) : (
            <FoodsPage grantedPermissions={session.authorization?.permissions} />
          )}
        </Box>
      </Box>
    </Box>
  );
}

function findMenu(menus = [], code, route) {
  for (const menu of menus) {
    if (menu.route === route || menu.code?.toLowerCase() === code) return menu;
    const child = findMenu(menu.children, code, route);
    if (child) return child;
  }
  return null;
}

function findFoodMenu(menus = []) {
  for (const menu of menus) {
    if (menu.route === FOOD_PATH || menu.code?.toLowerCase() === "foods") {
      return menu;
    }
    const child = findFoodMenu(menu.children);
    if (child) return child;
  }
  return null;
}
