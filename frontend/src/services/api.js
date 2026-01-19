import axios from "axios";

// Detect API base URL - use current host if accessed via network, otherwise localhost
const getApiBaseURL = () => {
  // If accessed via IP address (network), use that IP
  if (
    window.location.hostname !== "localhost" &&
    window.location.hostname !== "127.0.0.1"
  ) {
    return `${window.location.protocol}//${window.location.hostname}:5000/api`;
  }
  // Default to localhost for local access
  return "http://localhost:5000/api";
};

const api = axios.create({
  baseURL: getApiBaseURL(),
  headers: {
    "Content-Type": "application/json",
  },
});

// Request interceptor
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("auth-storage");
    if (token) {
      try {
        const parsed = JSON.parse(token);
        if (parsed.state?.token) {
          config.headers.Authorization = `Bearer ${parsed.state.token}`;
        }
      } catch (e) {
        // Ignore parse errors
      }
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Only redirect to login if we're not already on the login page
    if (
      error.response?.status === 401 &&
      !window.location.pathname.includes("/login")
    ) {
      // Handle unauthorized
      localStorage.removeItem("auth-storage");
      window.location.href = "/login";
    }

    // Handle connection errors
    if (
      error.code === "ERR_NETWORK" ||
      error.message === "Network Error" ||
      error.code === "ERR_CONNECTION_REFUSED"
    ) {
      const errorMessage =
        "API connection error. Make sure the backend server is running on http://localhost:5000";
      console.error(errorMessage);

      // Show user-friendly error message
      if (typeof window !== "undefined" && window.reactHotToast) {
        window.reactHotToast.error(errorMessage, {
          duration: 5000,
        });
      }

      // Don't redirect on network errors, just show error
      return Promise.reject(new Error(errorMessage));
    }

    return Promise.reject(error);
  }
);

export default api;
