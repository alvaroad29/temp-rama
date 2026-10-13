import { Component, computed, inject, input } from '@angular/core';
import { Announcement } from '../../interfaces/announcement-interface';
import { RelativeTimePipe } from '../../../shared/pipes/relative-time.pipe';
import { RouterLink } from '@angular/router';
import { Calendar, ChevronRight, LucideAngularModule } from 'lucide-angular';
import { rxResource } from '@angular/core/rxjs-interop';
import { AnnoucementsService } from '../../../shared/services/annoucements.service';

@Component({
  selector: 'announcement',
  imports: [RelativeTimePipe, RouterLink, LucideAngularModule],
  templateUrl: './announcement.component.html',
})
export class AnnouncementComponent {
  private annoucementService = inject(AnnoucementsService);

  annoucementResource = rxResource({
    loader: () => {
        return this.annoucementService.getLatestAnnoucement();
      },
  });

  announcement = computed(() => this.annoucementResource.value() ?? null);

  readonly right = ChevronRight;
  readonly calendar = Calendar;
}
