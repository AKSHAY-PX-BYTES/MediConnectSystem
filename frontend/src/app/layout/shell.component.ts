import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from '../core/services/auth.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule
  ],
  template: `
    <div class="flex h-screen overflow-hidden" [class.dark]="darkMode()">
      <!-- Sidebar -->
      <aside class="hidden md:flex md:flex-col w-64 bg-slate-900 text-slate-100">
        <div class="h-16 flex items-center gap-2 px-6 border-b border-slate-800">
          <span class="material-symbols-outlined text-brand-500">health_and_safety</span>
          <span class="text-lg font-bold">MediConnect</span>
        </div>
        <nav class="flex-1 px-3 py-4 space-y-1">
          @for (item of navItems; track item.route) {
            <a
              [routerLink]="item.route"
              routerLinkActive="bg-brand-600 text-white"
              class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-slate-300
                     hover:bg-slate-800 transition-colors"
            >
              <span class="material-symbols-outlined text-[20px]">{{ item.icon }}</span>
              <span class="text-sm font-medium">{{ item.label }}</span>
            </a>
          }
        </nav>
        <div class="p-4 border-t border-slate-800 text-xs text-slate-400">
          {{ user()?.role }} workspace
        </div>
      </aside>

      <!-- Main -->
      <div class="flex-1 flex flex-col overflow-hidden">
        <header class="h-16 flex items-center justify-between px-6 bg-white dark:bg-slate-800
                       border-b border-slate-200 dark:border-slate-700">
          <h1 class="text-lg font-semibold">{{ greeting() }}</h1>
          <div class="flex items-center gap-2">
            <button mat-icon-button (click)="toggleDark()">
              <span class="material-symbols-outlined">
                {{ darkMode() ? 'light_mode' : 'dark_mode' }}
              </span>
            </button>
            <button mat-button [matMenuTriggerFor]="menu" class="!font-medium">
              {{ user()?.fullName }}
            </button>
            <mat-menu #menu="matMenu">
              <button mat-menu-item (click)="logout()">
                <span class="material-symbols-outlined mr-2 align-middle">logout</span>
                Logout
              </button>
            </mat-menu>
          </div>
        </header>

        <main class="flex-1 overflow-y-auto p-6 bg-slate-50 dark:bg-slate-900">
          <router-outlet />
        </main>
      </div>
    </div>
  `
})
export class ShellComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly user = this.auth.user;
  readonly darkMode = signal(false);
  readonly greeting = computed(() => `Welcome back, ${this.user()?.fullName?.split(' ')[0] ?? ''}`);

  readonly navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/app/dashboard' },
    { label: 'Appointments', icon: 'event', route: '/app/appointments' }
  ];

  toggleDark(): void {
    this.darkMode.update((v) => !v);
    document.documentElement.classList.toggle('dark', this.darkMode());
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}
