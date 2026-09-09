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

const FOOD_PATH = "/restaurant/foods";
const CATEGORY_PATH = "/restaurant/categories";

export function AppShell({ session, path, navigate, onLogout }) {
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const [error, setError] = useState("");
  const sessionUser = getSessionUser(session);
  const foodMenu = findFoodMenu(session.authorization?.menus) ?? {
    name: "Foods",
    route: FOOD_PATH,
  };
  const categoryMenu = findMenu(session.authorization?.menus, "categories", CATEGORY_PATH);

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
        </List>
      </Paper>

      <Box className="business-workspace">
        <Box component="header" className="business-topbar">
          <Typography className="business-page-title">
            {path === CATEGORY_PATH ? "Nhóm món" : "Foods"}
          </Typography>
          <Box className="business-user-avatar" aria-hidden="true">
            {sessionUser.displayName.charAt(0).toUpperCase()}
          </Box>
        </Box>
        <Box component="main" className="business-content">
          {error && <Alert severity="error">{error}</Alert>}
          {path === CATEGORY_PATH ? (
            <CategoryPage grantedPermissions={session.authorization?.permissions} />
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
