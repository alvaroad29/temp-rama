
export interface StatsCard {
  title: string;
  value: string | number;
  change?: string;
  changeType?: 'positive' | 'negative';
  link: string;
}
