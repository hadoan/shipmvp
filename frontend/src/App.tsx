import { ThemeProvider } from '@/components/ThemeProvider';
import { Toaster as Sonner } from '@/components/ui/sonner';
import { Toaster } from '@/components/ui/toaster';
import { TooltipProvider } from '@/components/ui/tooltip';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import Sidebar from './components/Sidebar.tsx';
import { AuthProvider, ProtectedRoute } from './lib/auth/AuthContext';
import { SubscriptionProvider } from './lib/subscription/SubscriptionContext';
import BillingPage from './pages/BillingPage.tsx';
import FileUploadDemo from './pages/FileUploadDemo.tsx';
import InvoiceForm from './pages/InvoiceForm.tsx';
import InvoiceList from './pages/InvoiceList.tsx';
import LoginPage from './pages/LoginPage.tsx';
import NotFound from './pages/NotFound.tsx';
import SettingsPage from './pages/SettingsPage.tsx';
import UsersPage from './pages/UsersPage.tsx';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      retry: 2,
    },
  },
});

// Layout component for protected routes
const ProtectedLayout = ({ children }: { children: React.ReactNode }) => (
  <ProtectedRoute>
    <div className="min-h-screen bg-background flex">
      <Sidebar />
      <main className="flex-1 lg:ml-0 ml-0 transition-all duration-200">
        <div className="p-6 lg:p-8">{children}</div>
      </main>
    </div>
  </ProtectedRoute>
);

const App = () => (
  <QueryClientProvider client={queryClient}>
    <TooltipProvider>
      <Toaster />
      <Sonner />
      <AuthProvider>
        <SubscriptionProvider>
          <ThemeProvider>
            <BrowserRouter>
              <Routes>
                {/* Public routes */}
                <Route path="/login" element={<LoginPage />} />

                {/* Protected routes */}
                <Route
                  path="/"
                  element={
                    <ProtectedRoute>
                      <Navigate to="/invoices" replace />
                    </ProtectedRoute>
                  }
                />

                <Route
                  path="/invoices"
                  element={
                    <ProtectedLayout>
                      <InvoiceList />
                    </ProtectedLayout>
                  }
                />

                <Route
                  path="/invoices/new"
                  element={
                    <ProtectedLayout>
                      <InvoiceForm />
                    </ProtectedLayout>
                  }
                />

                <Route
                  path="/invoices/:id/edit"
                  element={
                    <ProtectedLayout>
                      <InvoiceForm />
                    </ProtectedLayout>
                  }
                />

                <Route
                  path="/users"
                  element={
                    <ProtectedLayout>
                      <UsersPage />
                    </ProtectedLayout>
                  }
                />

                <Route
                  path="/settings"
                  element={
                    <ProtectedLayout>
                      <SettingsPage />
                    </ProtectedLayout>
                  }
                />

                <Route
                  path="/billing"
                  element={
                    <ProtectedLayout>
                      <BillingPage />
                    </ProtectedLayout>
                  }
                />

                <Route
                  path="/files"
                  element={
                    <ProtectedLayout>
                      <FileUploadDemo />
                    </ProtectedLayout>
                  }
                />

                {/* ADD ALL CUSTOM ROUTES ABOVE THE CATCH-ALL "*" ROUTE */}
                <Route path="*" element={<NotFound />} />
              </Routes>
            </BrowserRouter>
          </ThemeProvider>
        </SubscriptionProvider>
      </AuthProvider>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;
