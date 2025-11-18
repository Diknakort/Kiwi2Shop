export const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

export const API_ENDPOINTS = {
  auth: {
    login: `${API_BASE_URL}/api/auth/login`,
    register: `${API_BASE_URL}/api/auth/register`,
    logout: `${API_BASE_URL}/api/auth/logout`,
    me: `${API_BASE_URL}/api/auth/me`,
  },
  users: {
    getAll: `${API_BASE_URL}/api/users`,
    getById: (id: string) => `${API_BASE_URL}/api/users/${id}`,
  },
  roles: {
    getAll: `${API_BASE_URL}/api/roles`,
    assign: `${API_BASE_URL}/api/roles/assign`,
  },
};





/** CODIGO PREFABRICADO POR VISUAL STUDIO
 * 
 * import axios, {
  AxiosError,
  AxiosInstance,
  AxiosRequestConfig,
  AxiosResponse,
} from "axios";

export interface APIError {
  message: string;
  status?: number;
  data?: any;
}

/**
 * Obtiene la URL base de la API desde variables de entorno.
 * Revisa varias convenciones usadas en diferentes toolchains.

const getApiBaseUrl = (): string => {
  // Variables comunes: CRA, Vite, Next, fallback a '/api'
  const env = (process && (process.env as any)) || {};
  const maybe =
    env.REACT_APP_API_URL || env.VITE_API_URL || env.NEXT_PUBLIC_API_URL;
  return (maybe as string) || "/api";
};

const API_BASE_URL = getApiBaseUrl();

/**
 * Instancia axios configurada para la aplicación.
 *
const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    "Content-Type": "application/json",
    Accept: "application/json",
  },
});

/**
 * Intérprete seguro para obtener el token de almacenamiento local.
 * Cambiar la clave si la app usa otro nombre.
 *
const getStoredToken = (): string | null => {
  try {
    return localStorage.getItem("authToken");
  } catch {
    return null;
  }
};

/**
 * Permite establecer o eliminar el token de autorización usado por las peticiones.
 *
export const setAuthToken = (token: string | null): void => {
  try {
    if (token) {
      localStorage.setItem("authToken", token);
      api.defaults.headers.common["Authorization"] = `Bearer ${token}`;
    } else {
      localStorage.removeItem("authToken");
      delete api.defaults.headers.common["Authorization"];
    }
  } catch {
    // Silenciar errores de almacenamiento en entornos sin localStorage
  }
};

// Inicializar header Authorization si hay token almacenado
const initialToken = getStoredToken();
if (initialToken) {
  api.defaults.headers.common["Authorization"] = `Bearer ${initialToken}`;
}

/**
 * Interceptor de respuestas para normalizar errores.
 *
api.interceptors.response.use(
  (response: AxiosResponse) => response,
  (error: AxiosError) => {
    const apiError: APIError = {
      message:
        error.response?.data?.message ||
        error.message ||
        "Error desconocido en la petición",
      status: error.response?.status,
      data: error.response?.data,
    };
    return Promise.reject(apiError);
  }
);

/**
 * Función genérica para peticiones API que devuelve el `data` tipado.
 *
export async function apiRequest<T = any>(
  config: AxiosRequestConfig
): Promise<T> {
  const response = await api.request<T>(config);
  return response.data;
}

/**
 * Métodos auxiliares convenientes: get, post, put, delete
 *
export async function apiGet<T = any>(
  url: string,
  config?: AxiosRequestConfig
): Promise<T> {
  return apiRequest<T>({ method: "GET", url, ...config });
}

export async function apiPost<T = any, B = any>(
  url: string,
  body?: B,
  config?: AxiosRequestConfig
): Promise<T> {
  return apiRequest<T>({ method: "POST", url, data: body, ...config });
}

export async function apiPut<T = any, B = any>(
  url: string,
  body?: B,
  config?: AxiosRequestConfig
): Promise<T> {
  return apiRequest<T>({ method: "PUT", url, data: body, ...config });
}

export async function apiDelete<T = any>(
  url: string,
  config?: AxiosRequestConfig
): Promise<T> {
  return apiRequest<T>({ method: "DELETE", url, ...config });
}

export default api;

   */