import { SlideData } from './../../interfaces/slidedata.interface';
import { AfterViewInit, Component, ElementRef, inject, input, signal, viewChild, ViewEncapsulation } from '@angular/core';

// import Swiper JS
import Swiper from 'swiper';
// import Swiper styles
import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';
import { Navigation, Pagination } from 'swiper/modules';
import { CarouselSlideComponent } from "./carousel-slide/carousel-slide.component";
import { BookList } from '../../../features/lists/interfaces/list-detailed.interface';

@Component({
  selector: 'app-carousel',
  imports: [CarouselSlideComponent],
  templateUrl: './carousel.component.html',
  styleUrls: ['carousel.component.css'],
  // encapsulation: ViewEncapsulation.None, // para que los estilos que aplico al hijo desde el padre
})
export class CarouselComponent implements AfterViewInit {

  books = input<BookList[]>();

  swiperDiv = viewChild.required<ElementRef>('swiperDiv');

  ngAfterViewInit(): void {
    const element = this.swiperDiv().nativeElement;
    if ( !element ) return;

    const swiper = new Swiper( element, {
      //
      // slidesPerView: 3.5,
      // spaceBetween: 10,
      centeredSlides: true, // Slide central activa

      // Optional parameters
      direction: 'horizontal',
      loop: true,

      modules: [
        // Pagination,
        Navigation
      ],


      // preventClicks: false,
      // preventClicksPropagation: false,

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
          slidesPerView: 1.2,
          spaceBetween: 10,
        },
        // Móviles
        480: {
          slidesPerView: 1.5,
          spaceBetween: 15,
        },
        // Tablets
        768: {
          slidesPerView: 2.5,
          spaceBetween: 10,
        },
        // Desktop pequeño
        1024: {
          slidesPerView: 3.5,
          spaceBetween: 15,
        },
        // Desktop grande
        1200: {
          slidesPerView: 3.9, // lo que veo
          spaceBetween: 20,
        }
      }
    });
  }

}
