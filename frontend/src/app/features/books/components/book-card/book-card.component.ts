import { Component, inject, input } from '@angular/core';
import { CardData } from '../../interfaces/card-data.interface';
import { Router, RouterLink } from '@angular/router';
import { LucideAngularModule, Star } from 'lucide-angular';

@Component({
  selector: 'book-card',
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './book-card.component.html',
})
export class BookCardComponent {
  card = input<CardData>();

  readonly star = Star;

}
