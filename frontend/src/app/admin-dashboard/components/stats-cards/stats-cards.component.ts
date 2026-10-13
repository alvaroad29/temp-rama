import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface StatsCard {
  title: string;
  value: string | number;
  change?: string;
  changeType?: 'positive' | 'negative';
  link: string;
}

@Component({
  selector: 'app-stats-cards',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './stats-cards.component.html',
})
export class StatsCardsComponent {
  statsCards: StatsCard[] = [
    {
      title: 'Users',
      value: '2',
      link: 'View'
    },
    {
      title: 'Companies',
      value: '100',
      change: '+30%',
      changeType: 'positive',
      link: 'View'
    },
    {
      title: 'Blogs',
      value: '100',
      link: 'View'
    }
  ];
}
