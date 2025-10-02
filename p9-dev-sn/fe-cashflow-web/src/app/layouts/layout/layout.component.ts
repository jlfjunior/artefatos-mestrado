import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';

@Component({
    selector: 'app-layout',
    templateUrl: './layout.component.html',
    standalone: false
})
export class LayoutComponent {
  currentUser: User | null = null;
  
  constructor(private readonly authService: AuthService) {
    this.authService.currentUser.subscribe(user => {
      this.currentUser = user;
    });
  }
}