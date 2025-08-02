import {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
} from 'react';
import { Navigate } from 'react-router-dom';
import {
  AuthService,
  type LoginDto,
  type AuthResultDto,
  type UserDto,
} from '../api/auth-service';

interface AuthContextType {
  user: UserDto | null;
  login: (credentials: LoginDto) => Promise<AuthResultDto>;
  logout: () => Promise<void>;
  isAuthenticated: boolean;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Check if user is already authenticated on app load
    const savedUser = AuthService.getUser();
    const token = AuthService.getToken();

    if (savedUser && token) {
      setUser(savedUser);
    }

    setIsLoading(false);
  }, []);

  const login = async (credentials: LoginDto): Promise<AuthResultDto> => {
    setIsLoading(true);
    try {
      const result = await AuthService.login(credentials);
      if (result.success && result.user) {
        setUser(result.user);
      }
      return result;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async (): Promise<void> => {
    setIsLoading(true);
    try {
      await AuthService.logout();
      setUser(null);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        login,
        logout,
        isAuthenticated: !!user,
        isLoading,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

// Protected Route Component
interface ProtectedRouteProps {
  children: ReactNode;
  fallback?: string;
}

export function ProtectedRoute({
  children,
  fallback = '/login',
}: ProtectedRouteProps) {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (!isAuthenticated) {
    return <Navigate to={fallback} replace />;
  }

  return <>{children}</>;
}
