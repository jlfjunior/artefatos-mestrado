import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    standalone: false
})
export class HeaderComponent {
  userName: string = '';
  hambMenuOpen = false;
  
  constructor(private readonly authService: AuthService, private readonly router: Router) {
    const user = this.authService.currentUserValue;
    if (user) {
      this.userName = `${user.name}`;
    }
  }
  
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
  
  toggleMobileMenu(): void {
    this.hambMenuOpen = !this.hambMenuOpen;
  }
}