import { useState } from "react";
import {
  Alert, Box, Button, Checkbox, CircularProgress, FormControlLabel,
  IconButton, InputAdornment, Paper, Stack, TextField, Typography,
} from "@mui/material";
import LockRoundedIcon from "@mui/icons-material/LockRounded";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import VisibilityOffOutlinedIcon from "@mui/icons-material/VisibilityOffOutlined";
import { login } from "../services/identity/identityClient.js";

export function LoginPage({ onLoginSuccess }) {
  const [code, setCode] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setIsLoading(true);
    try {
      onLoginSuccess(await login({ code, password }));
    } catch (loginError) {
      setError(loginError instanceof TypeError
        ? "Không thể kết nối đến Identity API."
        : loginError.message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Box className="login-page">
      <Paper className="login-card" elevation={0}>
        <Stack className="login-brand" direction="row" spacing={1.25}>
          <Box className="login-logo"><LockRoundedIcon fontSize="small" /></Box>
          <Box>
            <Typography className="login-brand-name">Restaurant</Typography>
            <Typography className="login-brand-caption">MANAGEMENT</Typography>
          </Box>
        </Stack>
        <Box className="login-heading">
          <Typography component="h1">Sign in to account</Typography>
          <Typography>Enter your code &amp; password to login</Typography>
        </Box>
        <Box component="form" onSubmit={handleSubmit} noValidate>
          <Stack spacing={2.25}>
            {error && <Alert severity="error">{error}</Alert>}
            <TextField label="Code" placeholder="Enter your code" value={code}
              onChange={(event) => setCode(event.target.value)} autoComplete="username"
              required fullWidth autoFocus />
            <TextField label="Password" placeholder="Enter your password"
              type={showPassword ? "text" : "password"} value={password}
              onChange={(event) => setPassword(event.target.value)}
              autoComplete="current-password" required fullWidth
              slotProps={{ input: { endAdornment: (
                <InputAdornment position="end">
                  <IconButton aria-label={showPassword ? "Hide password" : "Show password"}
                    onClick={() => setShowPassword((visible) => !visible)} edge="end">
                    {showPassword ? <VisibilityOffOutlinedIcon /> : <VisibilityOutlinedIcon />}
                  </IconButton>
                </InputAdornment>
              ) } }} />
            <Stack className="login-options" direction="row">
              <FormControlLabel control={<Checkbox size="small" />} label="Remember password" />
            </Stack>
            <Button className="login-submit" type="submit" variant="contained"
              disabled={isLoading || !code || !password}>
              {isLoading ? <CircularProgress size={22} color="inherit" /> : "Login"}
            </Button>
          </Stack>
        </Box>
      </Paper>
    </Box>
  );
}
