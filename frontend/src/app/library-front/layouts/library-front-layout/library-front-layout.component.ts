import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FrontNavbarComponent } from "../../components/front-navbar/front-navbar.component";
import { FrontFooterComponent } from "../../components/front-footer/front-footer.component";

@Component({
  selector: 'app-library-front-layout',
  imports: [RouterOutlet, FrontNavbarComponent, FrontFooterComponent],
  templateUrl: './library-front-layout.component.html',
})
export class LibraryFrontLayoutComponent { }
