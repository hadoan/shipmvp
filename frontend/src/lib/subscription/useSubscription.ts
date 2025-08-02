import { useContext } from 'react';
import { SubscriptionContext } from './SubscriptionContext';

export const useSubscription = () => useContext(SubscriptionContext);
