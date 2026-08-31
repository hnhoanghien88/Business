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
import { ProductPage } from "../features/restaurant/products/ProductPage.jsx";

const PRODUCT_PATH = "/product";

export function AppShell({ session, path, navigate, onLogout }) {
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const [error, setError] = useState("");
  const sessionUser = getSessionUser(session);
  const productMenu = findProductMenu(session.authorization?.menus) ?? {
    name: "Product",
    route: PRODUCT_PATH,
  };

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
            selected={path === PRODUCT_PATH}
            onClick={() => navigate(productMenu.route || PRODUCT_PATH)}
          >
            <ListItemIcon><RestaurantMenuRoundedIcon /></ListItemIcon>
            <ListItemText primary={productMenu.name} />
          </ListItemButton>
        </List>
      </Paper>

      <Box className="business-workspace">
        <Box component="header" className="business-topbar">
          <Typography className="business-page-title">Product</Typography>
          <Box className="business-user-avatar" aria-hidden="true">
            {sessionUser.displayName.charAt(0).toUpperCase()}
          </Box>
        </Box>
        <Box component="main" className="business-content">
          {error && <Alert severity="error">{error}</Alert>}
          <ProductPage grantedPermissions={session.authorization?.permissions} />
        </Box>
      </Box>
    </Box>
  );
}

function findProductMenu(menus = []) {
  for (const menu of menus) {
    if (menu.route === PRODUCT_PATH || menu.code?.toLowerCase() === "product") {
      return menu;
    }
    const child = findProductMenu(menu.children);
    if (child) return child;
  }
  return null;
}
