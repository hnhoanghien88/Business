import { useEffect, useState } from "react";
import { Box, CircularProgress, CssBaseline, ThemeProvider, createTheme } from "@mui/material";
import { LoginPage } from "./LoginPage.jsx";
import { AppShell } from "./AppShell.jsx";
import {
  clearSession,
  publishSession,
  restoreSession,
  subscribeToSession,
} from "../services/identity/session.js";

const LOGIN_PATH = "/login";
const PRODUCT_PATH = "/product";
const CATEGORY_PATH = "/restaurant/categories";

const theme = createTheme({
  palette: {
    primary: { main: "#4b5ed3" },
    background: { default: "#eef2f7" },
  },
  typography: {
    fontFamily: 'Inter, ui-sans-serif, system-ui, -apple-system, "Segoe UI", sans-serif',
  },
  shape: { borderRadius: 4 },
});

function App() {
  const [session, setSession] = useState(null);
  const [isRestoring, setIsRestoring] = useState(true);
  const [path, setPath] = useState(window.location.pathname);

  const navigate = (nextPath, replace = false) => {
    window.history[replace ? "replaceState" : "pushState"]({}, "", nextPath);
    setPath(nextPath);
  };

  useEffect(() => {
    const unsubscribe = subscribeToSession(setSession);
    restoreSession()
      .then((restoredSession) => {
        setSession(restoredSession);
        if (restoredSession && ["/", LOGIN_PATH].includes(window.location.pathname)) {
          navigate(hasCategoryMenu(restoredSession.authorization?.menus)
            ? CATEGORY_PATH
            : PRODUCT_PATH, true);
        }
      })
      .catch(() => setSession(null))
      .finally(() => setIsRestoring(false));
    return unsubscribe;
  }, []);

  useEffect(() => {
    const handlePopState = () => setPath(window.location.pathname);
    window.addEventListener("popstate", handlePopState);
    return () => window.removeEventListener("popstate", handlePopState);
  }, []);

  const handleLogin = (nextSession) => {
    publishSession(nextSession);
    navigate(PRODUCT_PATH, true);
  };

  const handleLogout = () => {
    clearSession();
    setSession(null);
    navigate(LOGIN_PATH, true);
  };

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      {isRestoring ? (
        <Box className="session-loading">
          <CircularProgress size={32} />
        </Box>
      ) : session ? (
        <AppShell
          session={session}
          path={path}
          navigate={navigate}
          onLogout={handleLogout}
        />
      ) : (
        <LoginPage onLoginSuccess={handleLogin} />
      )}
    </ThemeProvider>
  );
}

export default App;

function hasCategoryMenu(menus = []) {
  return menus.some((menu) =>
    menu.route === CATEGORY_PATH
    || menu.code?.toLowerCase() === "categories"
    || hasCategoryMenu(menu.children));
}
