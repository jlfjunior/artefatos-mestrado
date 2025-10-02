export interface Transaction {
  transactionId: string;
  description: string;
  category: string;
  type: string;
  date: Date;
  amount: number;
}