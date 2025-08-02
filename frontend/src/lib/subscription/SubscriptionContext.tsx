import React, { createContext, useContext, useEffect, useState } from 'react';
import { useAuth } from '../auth/AuthContext';
import {
  subscriptionService,
  type SubscriptionPlan,
  type UserSubscription,
} from './subscription-service';

interface SubscriptionContextType {
  currentPlan: SubscriptionPlan | null;
  subscription: UserSubscription | null;
  invoiceCount: number;
  isLoading: boolean;
  canCreateInvoice: boolean;
  upgradeRequired: boolean;
  refreshSubscription: () => Promise<void>;
}

const SubscriptionContext = createContext<SubscriptionContextType>({
  currentPlan: null,
  subscription: null,
  invoiceCount: 0,
  isLoading: true,
  canCreateInvoice: false,
  upgradeRequired: false,
  refreshSubscription: async () => {},
});

export { SubscriptionContext };

export const SubscriptionProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const { user, isAuthenticated } = useAuth();
  const [currentPlan, setCurrentPlan] = useState<SubscriptionPlan | null>(null);
  const [subscription, setSubscription] = useState<UserSubscription | null>(
    null
  );
  const [invoiceCount, setInvoiceCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);

  const refreshSubscription = React.useCallback(async () => {
    if (!isAuthenticated || !user) return;

    try {
      setIsLoading(true);

      // Get current subscription
      const userSubscription =
        await subscriptionService.getCurrentSubscription();
      setSubscription(userSubscription);

      // Get current plan details
      const plan = await subscriptionService.getPlanDetails(
        userSubscription.planId
      );
      setCurrentPlan(plan);

      // Get current invoice count
      const count = await subscriptionService.getUserInvoiceCount();
      setInvoiceCount(count);
    } catch (error) {
      console.error('Failed to fetch subscription data:', error);
      // Set default free plan if error
      setCurrentPlan({
        id: 'free',
        name: 'Free Plan',
        price: 0,
        currency: 'USD',
        interval: 'month',
        features: {
          maxInvoices: 19,
          maxUsers: 1,
          supportLevel: 'community',
          customBranding: false,
          apiAccess: false,
        },
      });
      setSubscription({
        id: 'default',
        userId: user.id,
        planId: 'free',
        status: 'active',
        currentPeriodStart: new Date().toISOString(),
        currentPeriodEnd: new Date(
          Date.now() + 30 * 24 * 60 * 60 * 1000
        ).toISOString(),
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      });
    } finally {
      setIsLoading(false);
    }
  }, [isAuthenticated, user]);

  useEffect(() => {
    refreshSubscription();
  }, [refreshSubscription]);

  const canCreateInvoice = currentPlan
    ? invoiceCount < currentPlan.features.maxInvoices
    : false;
  const upgradeRequired = currentPlan
    ? invoiceCount >= currentPlan.features.maxInvoices
    : false;

  return (
    <SubscriptionContext.Provider
      value={{
        currentPlan,
        subscription,
        invoiceCount,
        isLoading,
        canCreateInvoice,
        upgradeRequired,
        refreshSubscription,
      }}
    >
      {children}
    </SubscriptionContext.Provider>
  );
};
