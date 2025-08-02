import React from 'react';
import { useState } from 'react';
import { loadStripe } from '@stripe/stripe-js';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Progress } from '@/components/ui/progress';
import { Check, Crown, Zap, Shield, AlertTriangle } from 'lucide-react';
import { useSubscription } from '@/lib/subscription/useSubscription';
import {
  subscriptionService,
  type SubscriptionPlan,
} from '@/lib/subscription/subscription-service';
import { useToast } from '@/hooks/use-toast';

// Initialize Stripe (you'll need to add your publishable key)
const stripePromise = loadStripe(
  import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY || ''
);

export default function BillingPage() {
  const {
    currentPlan,
    subscription,
    invoiceCount,
    isLoading,
    canCreateInvoice,
    upgradeRequired,
    refreshSubscription,
  } = useSubscription();
  const [plans, setPlans] = useState<SubscriptionPlan[]>([]);
  const [loadingPlans, setLoadingPlans] = useState(true);
  const [processingCheckout, setProcessingCheckout] = useState<string | null>(
    null
  );
  const { toast } = useToast();

  React.useEffect(() => {
    const loadPlans = async () => {
      try {
        const availablePlans = await subscriptionService.getPlans();
        setPlans(availablePlans);
      } catch (error) {
        console.error('Failed to load plans:', error);
        toast({
          title: 'Error',
          description: 'Failed to load subscription plans',
          variant: 'destructive',
        });
      } finally {
        setLoadingPlans(false);
      }
    };

    loadPlans();
  }, [toast]);

  const handleUpgrade = async (planId: string) => {
    try {
      setProcessingCheckout(planId);

      const stripe = await stripePromise;
      if (!stripe) {
        throw new Error('Stripe not loaded');
      }

      const { sessionId } = await subscriptionService.createCheckoutSession({
        planId,
        successUrl: `${window.location.origin}/billing?success=true`,
        cancelUrl: `${window.location.origin}/billing?canceled=true`,
      });

      const result = await stripe.redirectToCheckout({ sessionId });

      if (result.error) {
        throw new Error(result.error.message);
      }
    } catch (error) {
      console.error('Checkout error:', error);
      toast({
        title: 'Error',
        description: `Failed to start checkout: ${error}`,
        variant: 'destructive',
      });
    } finally {
      setProcessingCheckout(null);
    }
  };

  const handleCancelSubscription = async () => {
    if (
      !confirm(
        'Are you sure you want to cancel your subscription? You will lose access to premium features at the end of your billing period.'
      )
    ) {
      return;
    }

    try {
      await subscriptionService.cancelSubscription();
      await refreshSubscription();
      toast({
        title: 'Success',
        description: 'Subscription canceled successfully',
      });
    } catch (error) {
      console.error('Cancel subscription error:', error);
      toast({
        title: 'Error',
        description: `Failed to cancel subscription: ${error}`,
        variant: 'destructive',
      });
    }
  };

  const formatCurrency = (amount: number, currency: string = 'USD') => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency,
    }).format(amount);
  };

  const getUsagePercentage = () => {
    if (!currentPlan || currentPlan.features.maxInvoices === -1) return 0;
    return Math.min(
      (invoiceCount / currentPlan.features.maxInvoices) * 100,
      100
    );
  };

  const getPlanIcon = (planId: string) => {
    switch (planId) {
      case 'pro':
        return <Zap className="h-5 w-5 text-blue-600" />;
      case 'enterprise':
        return <Crown className="h-5 w-5 text-purple-600" />;
      default:
        return <Shield className="h-5 w-5 text-green-600" />;
    }
  };

  if (isLoading || loadingPlans) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold">Billing & Subscription</h1>
          <p className="text-muted-foreground">
            Loading your subscription details...
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Billing & Subscription</h1>
        <p className="text-muted-foreground">
          Manage your subscription and billing information
        </p>
      </div>

      {/* Current Plan */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              {currentPlan && getPlanIcon(currentPlan.id)}
              <div>
                <CardTitle>{currentPlan?.name || 'Free Plan'}</CardTitle>
                <CardDescription>
                  {currentPlan?.price === 0
                    ? 'Free forever'
                    : `${formatCurrency(currentPlan?.price || 0)}/${currentPlan?.interval}`}
                </CardDescription>
              </div>
            </div>
            <Badge
              variant={
                subscription?.status === 'active' ? 'default' : 'destructive'
              }
            >
              {subscription?.status || 'active'}
            </Badge>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Usage */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium">Invoice Usage</span>
              <span className="text-sm text-muted-foreground">
                {invoiceCount} /{' '}
                {currentPlan?.features.maxInvoices === -1
                  ? '∞'
                  : currentPlan?.features.maxInvoices || 19}{' '}
                invoices
              </span>
            </div>
            <Progress value={getUsagePercentage()} className="h-2" />
            {upgradeRequired && (
              <Alert className="mt-3">
                <AlertTriangle className="h-4 w-4" />
                <AlertDescription>
                  You've reached your invoice limit. Upgrade to create more
                  invoices.
                </AlertDescription>
              </Alert>
            )}
          </div>

          {/* Features */}
          <div>
            <h4 className="text-sm font-medium mb-2">Current Plan Features</h4>
            <div className="grid gap-2 text-sm">
              <div className="flex items-center gap-2">
                <Check className="h-4 w-4 text-green-600" />
                <span>
                  {currentPlan?.features.maxInvoices === -1
                    ? 'Unlimited invoices'
                    : `Up to ${currentPlan?.features.maxInvoices || 19} invoices`}
                </span>
              </div>
              <div className="flex items-center gap-2">
                <Check className="h-4 w-4 text-green-600" />
                <span>
                  {currentPlan?.features.maxUsers === -1
                    ? 'Unlimited users'
                    : `Up to ${currentPlan?.features.maxUsers || 1} user${(currentPlan?.features.maxUsers || 1) > 1 ? 's' : ''}`}
                </span>
              </div>
              <div className="flex items-center gap-2">
                <Check className="h-4 w-4 text-green-600" />
                <span>
                  {currentPlan?.features.supportLevel || 'Community'} support
                </span>
              </div>
              {currentPlan?.features.customBranding && (
                <div className="flex items-center gap-2">
                  <Check className="h-4 w-4 text-green-600" />
                  <span>Custom branding</span>
                </div>
              )}
              {currentPlan?.features.apiAccess && (
                <div className="flex items-center gap-2">
                  <Check className="h-4 w-4 text-green-600" />
                  <span>API access</span>
                </div>
              )}
            </div>
          </div>

          {/* Cancel subscription button */}
          {currentPlan?.id !== 'free' && subscription?.status === 'active' && (
            <div className="pt-4 border-t">
              <Button
                variant="outline"
                onClick={handleCancelSubscription}
                className="text-destructive hover:text-destructive"
              >
                Cancel Subscription
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Available Plans */}
      {currentPlan?.id === 'free' && (
        <div>
          <h2 className="text-2xl font-bold mb-4">Upgrade Your Plan</h2>
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {plans
              .filter(plan => plan.id !== 'free')
              .map(plan => (
                <Card key={plan.id} className="relative">
                  <CardHeader>
                    <div className="flex items-center gap-3">
                      {getPlanIcon(plan.id)}
                      <div>
                        <CardTitle>{plan.name}</CardTitle>
                        <CardDescription>
                          {formatCurrency(plan.price)}/{plan.interval}
                        </CardDescription>
                      </div>
                    </div>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div className="space-y-2">
                      <div className="flex items-center gap-2 text-sm">
                        <Check className="h-4 w-4 text-green-600" />
                        <span>
                          {plan.features.maxInvoices === -1
                            ? 'Unlimited invoices'
                            : `Up to ${plan.features.maxInvoices} invoices`}
                        </span>
                      </div>
                      <div className="flex items-center gap-2 text-sm">
                        <Check className="h-4 w-4 text-green-600" />
                        <span>
                          {plan.features.maxUsers === -1
                            ? 'Unlimited users'
                            : `Up to ${plan.features.maxUsers} users`}
                        </span>
                      </div>
                      <div className="flex items-center gap-2 text-sm">
                        <Check className="h-4 w-4 text-green-600" />
                        <span>{plan.features.supportLevel} support</span>
                      </div>
                      {plan.features.customBranding && (
                        <div className="flex items-center gap-2 text-sm">
                          <Check className="h-4 w-4 text-green-600" />
                          <span>Custom branding</span>
                        </div>
                      )}
                      {plan.features.apiAccess && (
                        <div className="flex items-center gap-2 text-sm">
                          <Check className="h-4 w-4 text-green-600" />
                          <span>API access</span>
                        </div>
                      )}
                    </div>

                    <Button
                      className="w-full"
                      onClick={() => handleUpgrade(plan.id)}
                      disabled={processingCheckout === plan.id}
                    >
                      {processingCheckout === plan.id
                        ? 'Processing...'
                        : `Upgrade to ${plan.name}`}
                    </Button>
                  </CardContent>
                </Card>
              ))}
          </div>
        </div>
      )}
    </div>
  );
}
