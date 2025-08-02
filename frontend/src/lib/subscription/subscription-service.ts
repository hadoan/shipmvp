import { getApiConfig } from '../config';
import { AuthService } from '../api/auth-service';

export interface SubscriptionPlan {
  id: string;
  name: string;
  price: number;
  currency: string;
  interval: 'month' | 'year';
  features: {
    maxInvoices: number;
    maxUsers: number;
    supportLevel: 'community' | 'email' | 'priority';
    customBranding: boolean;
    apiAccess: boolean;
  };
}

export interface UserSubscription {
  id: string;
  userId: string;
  planId: string;
  status: 'active' | 'cancelled' | 'past_due' | 'unpaid';
  stripeSubscriptionId?: string;
  stripeCustomerId?: string;
  currentPeriodStart: string;
  currentPeriodEnd: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCheckoutSessionRequest {
  planId: string;
  successUrl: string;
  cancelUrl: string;
}

export interface CheckoutSession {
  sessionId: string;
  url: string;
}

class SubscriptionService {
  private apiConfig = getApiConfig();

  /**
   * Get available subscription plans
   */
  async getPlans(): Promise<SubscriptionPlan[]> {
    try {
      const response = await AuthService.authenticatedFetch<SubscriptionPlan[]>(
        '/api/subscriptions/plans'
      );
      return response;
    } catch (error) {
      console.error('Failed to fetch plans:', error);
      return this.getDefaultPlans();
    }
  }

  /**
   * Get current user's subscription
   */
  async getCurrentSubscription(): Promise<UserSubscription> {
    try {
      const response = await AuthService.authenticatedFetch<UserSubscription>(
        '/api/subscriptions/current'
      );
      return response;
    } catch (error) {
      console.error('Failed to fetch current subscription:', error);
      return this.getDefaultSubscription();
    }
  }

  /**
   * Get plan details by ID
   */
  async getPlanDetails(planId: string): Promise<SubscriptionPlan> {
    const plans = await this.getPlans();
    return plans.find(plan => plan.id === planId) || this.getDefaultPlans()[0];
  }

  /**
   * Get user's current invoice count
   */
  async getUserInvoiceCount(): Promise<number> {
    try {
      const response = await AuthService.authenticatedFetch<{ count: number }>(
        '/api/invoices/count'
      );
      return response.count || 0;
    } catch (error) {
      console.error('Failed to fetch invoice count:', error);
      return 0;
    }
  }

  /**
   * Create Stripe checkout session
   */
  async createCheckoutSession(
    request: CreateCheckoutSessionRequest
  ): Promise<CheckoutSession> {
    try {
      const response = await AuthService.authenticatedFetch<CheckoutSession>(
        '/api/subscriptions/checkout',
        {
          method: 'POST',
          body: JSON.stringify(request),
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );
      return response;
    } catch (error) {
      throw new Error(`Failed to create checkout session: ${error}`);
    }
  }

  /**
   * Cancel subscription
   */
  async cancelSubscription(): Promise<void> {
    try {
      await AuthService.authenticatedFetch('/api/subscriptions/current', {
        method: 'DELETE',
      });
    } catch (error) {
      throw new Error(`Failed to cancel subscription: ${error}`);
    }
  }

  /**
   * Get default plans (fallback when API is not available)
   */
  private getDefaultPlans(): SubscriptionPlan[] {
    return [
      {
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
      },
      {
        id: 'pro',
        name: 'Pro Plan',
        price: 29,
        currency: 'USD',
        interval: 'month',
        features: {
          maxInvoices: 1000,
          maxUsers: 10,
          supportLevel: 'email',
          customBranding: true,
          apiAccess: true,
        },
      },
      {
        id: 'enterprise',
        name: 'Enterprise Plan',
        price: 99,
        currency: 'USD',
        interval: 'month',
        features: {
          maxInvoices: -1, // unlimited
          maxUsers: -1, // unlimited
          supportLevel: 'priority',
          customBranding: true,
          apiAccess: true,
        },
      },
    ];
  }

  /**
   * Get default subscription (fallback)
   */
  private getDefaultSubscription(): UserSubscription {
    return {
      id: 'default',
      userId: 'current-user',
      planId: 'free',
      status: 'active',
      currentPeriodStart: new Date().toISOString(),
      currentPeriodEnd: new Date(
        Date.now() + 30 * 24 * 60 * 60 * 1000
      ).toISOString(),
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
  }
}

export const subscriptionService = new SubscriptionService();
