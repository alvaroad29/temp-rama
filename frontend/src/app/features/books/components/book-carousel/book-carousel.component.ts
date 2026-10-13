import { CardData } from './../../interfaces/card-data.interface';
import { AfterViewInit, Component, effect, ElementRef, input, OnDestroy, untracked, viewChild } from '@angular/core';

// import Swiper JS
import Swiper from 'swiper';

import { Navigation, Pagination } from 'swiper/modules';
import { BookCardComponent } from "../book-card/book-card.component";

@Component({
  selector: 'book-carousel',
  imports: [BookCardComponent],
  templateUrl: './book-carousel.component.html',
  styleUrls: ['./book-carousel.component.css']
})
export class BookCarouselComponent implements AfterViewInit, OnDestroy{

  cards = input<CardData[]>();

  swiperDiv = viewChild.required<ElementRef>('swiperDivBook');

  private swiperInstance: Swiper | null = null;

  constructor() {
    // Efecto para detectar cambios en cards
    effect(() => {
      const cards = this.cards();
      untracked(() => {
        if (this.swiperInstance && cards) {
          // Forzar actualización de Swiper cuando cambian las cards
          setTimeout(() => {
            this.swiperInstance?.update();
            this.swiperInstance?.slideTo(0);
          }, 0);
        }
      });
    });
  }

  ngAfterViewInit(): void {
    this.initializeSwiper();
  }



  private initializeSwiper(): void {
    const element = this.swiperDiv().nativeElement;
    if ( !element ) return;

    const total = this.cards()?.length ?? 0;

    this.swiperInstance = new Swiper( element, {
      // comento xq sino cuando baja del minimo(320px) se aplica
      // slidesPerView: 3.5,
      // spaceBetween: 10,

      // Optional parameters
      direction: 'horizontal',
      loop: total > 5,

      modules: [
        // Pagination,
        Navigation
      ],

      // If we need pagination
      pagination: {
        el: '.swiper-pagination',
        type: 'fraction'
      },

      // Navigation arrows
      navigation: {
        nextEl: '.swiper-button-next',
        prevEl: '.swiper-button-prev',
      },

      // And if we need scrollbar
      scrollbar: {
        el: '.swiper-scrollbar',
      },

       // Responsive breakpoints
      breakpoints: {
        // Móviles pequeños
        320: {
          slidesPerView: 1,
          spaceBetween: 10,
        },
        // Móviles
        480: {
          slidesPerView: 2,
          spaceBetween: 10,
        },
        // Tablets
        768: {
          slidesPerView: 3,
          spaceBetween: 15,
        },
        // Desktop pequeño
        1024: {
          slidesPerView: 4,
          spaceBetween: 15,
        },
        // Desktop grande
        1200: {
          slidesPerView: 5, // lo que veo
          spaceBetween: 20,
        }
      },



    });
  }

  ngOnDestroy(): void {
    if (this.swiperInstance) {
      this.swiperInstance.destroy(true, true);
      this.swiperInstance = null;
    }
  }

}
