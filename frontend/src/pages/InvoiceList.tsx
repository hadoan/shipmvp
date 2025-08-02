import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  Plus,
  Edit,
  Trash2,
  Check,
  Search,
  Crown,
  AlertTriangle,
} from 'lucide-react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { useToast } from '@/hooks/use-toast';
import { DeleteConfirmationModal } from '@/components/DeleteConfirmationModal';
import { useSubscription } from '@/lib/subscription/useSubscription';
import {
  invoiceService,
  type InvoiceDto,
  type GetInvoicesParams,
  type InvoiceStatus,
} from '@/lib/api/invoice-service';

const statusLabels = {
  0: 'Draft',
  1: 'Sent',
  2: 'Paid',
  3: 'Cancelled',
} as const;

const statusColors = {
  0: 'secondary',
  1: 'default',
  2: 'default',
  3: 'destructive',
} as const;

function InvoiceList() {
  const [filters, setFilters] = useState<GetInvoicesParams>({});
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [invoiceToDelete, setInvoiceToDelete] = useState<string | null>(null);
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const {
    canCreateInvoice,
    upgradeRequired,
    currentPlan,
    invoiceCount,
    refreshSubscription,
  } = useSubscription();

  // Fetch invoices with filters
  const {
    data: invoices = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ['invoices', filters],
    queryFn: () => invoiceService.getInvoices(filters),
  });

  // Delete invoice mutation
  const deleteMutation = useMutation({
    mutationFn: invoiceService.deleteInvoice,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
      refreshSubscription(); // Refresh subscription data after deletion
      toast({
        title: 'Success',
        description: 'Invoice deleted successfully',
      });
    },
    onError: error => {
      toast({
        title: 'Error',
        description: `Failed to delete invoice: ${error.message}`,
        variant: 'destructive',
      });
    },
  });

  // Mark as paid mutation
  const markPaidMutation = useMutation({
    mutationFn: invoiceService.markInvoiceAsPaid,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
      toast({
        title: 'Success',
        description: 'Invoice marked as paid',
      });
    },
    onError: error => {
      toast({
        title: 'Error',
        description: `Failed to mark invoice as paid: ${error.message}`,
        variant: 'destructive',
      });
    },
  });

  const handleSearch = (customerName: string) => {
    setFilters(prev => ({ ...prev, customerName: customerName || undefined }));
  };

  const handleStatusFilter = (status: string) => {
    setFilters(prev => ({
      ...prev,
      status:
        status === 'all' ? undefined : (parseInt(status) as InvoiceStatus),
    }));
  };

  const handleDelete = (id: string) => {
    setInvoiceToDelete(id);
    setDeleteModalOpen(true);
  };

  const confirmDelete = () => {
    if (invoiceToDelete) {
      deleteMutation.mutate(invoiceToDelete);
      setDeleteModalOpen(false);
      setInvoiceToDelete(null);
    }
  };

  const cancelDelete = () => {
    setDeleteModalOpen(false);
    setInvoiceToDelete(null);
  };

  const handleMarkPaid = (id: string) => {
    markPaidMutation.mutate(id);
  };

  const formatCurrency = (amount: number, currency: string = 'USD') => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency,
    }).format(amount);
  };

  const formatDate = (date: string) => {
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    }).format(new Date(date));
  };

  if (error) {
    return (
      <div className="text-center py-8">
        <p className="text-destructive">
          Error loading invoices: {(error as Error).message}
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Upgrade Alert */}
      {upgradeRequired && (
        <Alert>
          <AlertTriangle className="h-4 w-4" />
          <AlertDescription className="flex items-center justify-between">
            <span>
              You've reached your invoice limit ({invoiceCount}/
              {currentPlan?.features.maxInvoices}). Upgrade to create more
              invoices.
            </span>
            <Button asChild size="sm" variant="outline">
              <Link to="/billing">
                <Crown className="mr-2 h-4 w-4" />
                Upgrade Now
              </Link>
            </Button>
          </AlertDescription>
        </Alert>
      )}

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Invoice Management</h1>
          <p className="text-muted-foreground">
            Manage your invoices efficiently
            {currentPlan && (
              <span className="ml-2">
                • {invoiceCount}/
                {currentPlan.features.maxInvoices === -1
                  ? '∞'
                  : currentPlan.features.maxInvoices}{' '}
                invoices used
              </span>
            )}
          </p>
        </div>
        {canCreateInvoice ? (
          <Button asChild>
            <Link to="/invoices/new">
              <Plus className="mr-2 h-4 w-4" />
              Create Invoice
            </Link>
          </Button>
        ) : (
          <Button asChild variant="outline">
            <Link to="/billing">
              <Crown className="mr-2 h-4 w-4" />
              Upgrade to Create More
            </Link>
          </Button>
        )}
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filters</CardTitle>
          <CardDescription>Search and filter your invoices</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex gap-4">
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search by customer name..."
                  className="pl-10"
                  onChange={e => handleSearch(e.target.value)}
                />
              </div>
            </div>
            <Select onValueChange={handleStatusFilter}>
              <SelectTrigger className="w-48">
                <SelectValue placeholder="Filter by status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Statuses</SelectItem>
                <SelectItem value="0">Draft</SelectItem>
                <SelectItem value="1">Sent</SelectItem>
                <SelectItem value="2">Paid</SelectItem>
                <SelectItem value="3">Cancelled</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Invoices</CardTitle>
          <CardDescription>
            {Array.isArray(invoices)
              ? `${invoices.length} invoice(s) found`
              : 'Loading invoices...'}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="text-center py-8">
              <p>Loading invoices...</p>
            </div>
          ) : !Array.isArray(invoices) || invoices.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-muted-foreground">No invoices found</p>
              <Button asChild className="mt-4">
                <Link to="/invoices/new">Create your first invoice</Link>
              </Button>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Customer</TableHead>
                  <TableHead>Total Amount</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Created</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {invoices.map((invoice: InvoiceDto) => (
                  <TableRow key={invoice.id}>
                    <TableCell className="font-medium">
                      {invoice.customerName}
                    </TableCell>
                    <TableCell>
                      {formatCurrency(invoice.totalAmount, invoice.currency)}
                    </TableCell>
                    <TableCell>
                      <Badge
                        variant={
                          statusColors[
                            parseInt(
                              invoice.status
                            ) as keyof typeof statusColors
                          ]
                        }
                      >
                        {
                          statusLabels[
                            parseInt(
                              invoice.status
                            ) as keyof typeof statusLabels
                          ]
                        }
                      </Badge>
                    </TableCell>
                    <TableCell>{formatDate(invoice.createdAt)}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        {parseInt(invoice.status) === 0 && (
                          <>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handleMarkPaid(invoice.id)}
                              disabled={markPaidMutation.isPending}
                            >
                              <Check className="h-4 w-4" />
                            </Button>
                            <Button variant="outline" size="sm" asChild>
                              <Link to={`/invoices/${invoice.id}/edit`}>
                                <Edit className="h-4 w-4" />
                              </Link>
                            </Button>
                          </>
                        )}
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleDelete(invoice.id)}
                          disabled={deleteMutation.isPending}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <DeleteConfirmationModal
        isOpen={deleteModalOpen}
        onClose={cancelDelete}
        onConfirm={confirmDelete}
        title="Delete Invoice"
        description="Are you sure you want to delete this invoice? This action cannot be undone and will permanently remove the invoice from your system."
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
}

export default InvoiceList;
