import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from '../services/auth.service';
import { CartService } from '../services/cart.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    RouterLink,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatBadgeModule,
    MatMenuModule,
  ],
  template: `
    <mat-toolbar color="primary">
      <a mat-button routerLink="/store" class="brand">AI Shop</a>
      <span class="spacer"></span>

      <a mat-button routerLink="/store">
        <mat-icon>store</mat-icon>
        Store
      </a>

      <a mat-button routerLink="/store/cart"
         [matBadge]="cartService.itemCount()"
         matBadgeColor="warn"
         [matBadgeHidden]="cartService.itemCount() === 0">
        <mat-icon>shopping_cart</mat-icon>
        Cart
      </a>

      <a mat-button routerLink="/store/orders">
        <mat-icon>receipt</mat-icon>
        Orders
      </a>

      @if (authService.isLoggedIn()) {
        <button mat-button [matMenuTriggerFor]="menu">
          <mat-icon>account_circle</mat-icon>
          {{ authService.currentUser()?.username }}
        </button>
        <mat-menu #menu="matMenu">
          <button mat-menu-item (click)="logout()">
            <mat-icon>logout</mat-icon>
            Logout
          </button>
        </mat-menu>
      } @else {
        <a mat-button routerLink="/auth/login">
          <mat-icon>login</mat-icon>
          Login
        </a>
      }
    </mat-toolbar>
  `,
  styles: `
    .spacer { flex: 1 1 auto; }
    .brand { font-weight: bold; font-size: 1.2em; }
    mat-toolbar a, mat-toolbar button { min-width: auto; }
    mat-icon { margin-right: 4px; vertical-align: middle; }
  `
})
export class NavbarComponent {
  readonly authService = inject(AuthService);
  readonly cartService = inject(CartService);

  logout(): void {
    this.authService.logout();
  }
}
