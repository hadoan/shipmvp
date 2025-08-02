import { useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, Plus, Trash2, Crown, AlertTriangle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { useToast } from '@/hooks/use-toast';
import { useSubscription } from '@/lib/subscription/useSubscription';
import {
  invoiceService,
  type CreateInvoiceDto,
  type UpdateInvoiceDto,
} from '@/lib/api/invoice-service';

const invoiceSchema = z.object({
  customerName: z.string().min(1, 'Customer name is required'),
  items: z
    .array(
      z.object({
        description: z.string().min(1, 'Description is required'),
        amount: z.number().min(0.01, 'Amount required'),
      })
    )
    .min(1, 'At least one item is required'),
});

type InvoiceFormData = z.infer<typeof invoiceSchema>;

function InvoiceForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const { canCreateInvoice, refreshSubscription } = useSubscription();
  const isEditing = Boolean(id);

  const form = useForm<InvoiceFormData>({
    resolver: zodResolver(invoiceSchema),
    defaultValues: {
      customerName: '',
      items: [{ description: '', amount: 0 }],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control: form.control,
    name: 'items',
  });

  // Fetch invoice for editing
  const { data: invoice, isLoading } = useQuery({
    queryKey: ['invoice', id],
    queryFn: () => invoiceService.getInvoice(id!),
    enabled: isEditing,
  });

  // Create invoice mutation
  const createMutation = useMutation({
    mutationFn: (data: CreateInvoiceDto) => invoiceService.createInvoice(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
      toast({
        title: 'Success',
        description: 'Invoice created successfully',
      });
      navigate('/invoices');
    },
    onError: error => {
      toast({
        title: 'Error',
        description: `Failed to create invoice: ${error.message}`,
        variant: 'destructive',
      });
    },
  });

  // Update invoice mutation
  const updateMutation = useMutation({
    mutationFn: (data: UpdateInvoiceDto) =>
      invoiceService.updateInvoice(id!, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
      queryClient.invalidateQueries({ queryKey: ['invoice', id] });
      toast({
        title: 'Success',
        description: 'Invoice updated successfully',
      });
      navigate('/invoices');
    },
    onError: error => {
      toast({
        title: 'Error',
        description: `Failed to update invoice: ${error.message}`,
        variant: 'destructive',
      });
    },
  });

  // Load invoice data when editing
  useEffect(() => {
    if (invoice) {
      form.reset({
        customerName: invoice.customerName,
        items: invoice.items.map(item => ({
          description: item.description,
          amount: item.amount,
        })),
      });
    }
  }, [invoice, form]);

  const onSubmit = (data: InvoiceFormData) => {
    if (isEditing) {
      updateMutation.mutate(data);
    } else {
      createMutation.mutate(data);
    }
  };

  const addItem = () => {
    append({ description: '', amount: 0 });
  };

  const removeItem = (index: number) => {
    if (fields.length > 1) {
      remove(index);
    }
  };

  const calculateTotal = () => {
    const items = form.watch('items');
    return items.reduce((total, item) => total + (item.amount || 0), 0);
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  if (isEditing && isLoading) {
    return (
      <div className="text-center py-8">
        <p>Loading invoice...</p>
      </div>
    );
  }

  // Don't allow editing non-draft invoices
  if (isEditing && invoice && parseInt(invoice.status) !== 0) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="outline" asChild>
            <Link to="/invoices">
              <ArrowLeft className="mr-2 h-4 w-4" />
              Back
            </Link>
          </Button>
          <h1 className="text-3xl font-bold">Cannot Edit Invoice</h1>
        </div>
        <Card>
          <CardContent className="pt-6">
            <p className="text-muted-foreground">
              This invoice cannot be edited because it's not in draft status.
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="outline" asChild>
          <Link to="/invoices">
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">
            {isEditing ? 'Edit Invoice' : 'Create New Invoice'}
          </h1>
          <p className="text-muted-foreground">
            {isEditing
              ? 'Update invoice details'
              : 'Add customer information and line items'}
          </p>
        </div>
      </div>

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Customer Information</CardTitle>
              <CardDescription>
                Enter the customer details for this invoice
              </CardDescription>
            </CardHeader>
            <CardContent>
              <FormField
                control={form.control}
                name="customerName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Customer Name</FormLabel>
                    <FormControl>
                      <Input placeholder="Enter customer name" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Line Items</CardTitle>
                  <CardDescription>
                    Add the products or services for this invoice
                  </CardDescription>
                </div>
                <Button type="button" variant="outline" onClick={addItem}>
                  <Plus className="mr-2 h-4 w-4" />
                  Add Item
                </Button>
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              {fields.map((field, index) => (
                <div key={field.id} className="flex gap-4 items-start">
                  <div className="flex-1">
                    <FormField
                      control={form.control}
                      name={`items.${index}.description`}
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Description</FormLabel>
                          <FormControl>
                            <Input
                              placeholder="Product or service description"
                              {...field}
                            />
                          </FormControl>
                          <div className="h-5">
                            <FormMessage />
                          </div>
                        </FormItem>
                      )}
                    />
                  </div>
                  <div className="w-32">
                    <FormField
                      control={form.control}
                      name={`items.${index}.amount`}
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Amount</FormLabel>
                          <FormControl>
                            <Input
                              type="number"
                              step="0.01"
                              min="0"
                              placeholder="0.00"
                              {...field}
                              onChange={e =>
                                field.onChange(parseFloat(e.target.value) || 0)
                              }
                            />
                          </FormControl>
                          <div className="h-5">
                            <FormMessage />
                          </div>
                        </FormItem>
                      )}
                    />
                  </div>
                  <div className="w-10 pt-8">
                    <Button
                      type="button"
                      variant="outline"
                      size="icon"
                      onClick={() => removeItem(index)}
                      disabled={fields.length === 1}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              ))}

              <div className="border-t pt-4">
                <div className="flex justify-between items-center font-medium">
                  <span>Total:</span>
                  <span className="text-lg">
                    {formatCurrency(calculateTotal())}
                  </span>
                </div>
              </div>
            </CardContent>
          </Card>

          <div className="flex gap-4">
            <Button
              type="submit"
              disabled={createMutation.isPending || updateMutation.isPending}
            >
              {createMutation.isPending || updateMutation.isPending
                ? isEditing
                  ? 'Updating...'
                  : 'Creating...'
                : isEditing
                  ? 'Update Invoice'
                  : 'Create Invoice'}
            </Button>
            <Button type="button" variant="outline" asChild>
              <Link to="/invoices">Cancel</Link>
            </Button>
          </div>
        </form>
      </Form>
    </div>
  );
}

export default InvoiceForm;
