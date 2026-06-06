import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/services/auth.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  styles: [`
    .sidebar {
      background: linear-gradient(180deg, #0c1445 0%, #0d1f5c 60%, #0f2575 100%);
    }
    .nav-link {
      display: flex; align-items: center; gap: .75rem;
      padding: .625rem .875rem; border-radius: .625rem;
      color: #93c5fd; font-size: .875rem; font-weight: 500;
      transition: background .15s, color .15s;
      text-decoration: none;
    }
    .nav-link:hover { background: rgba(255,255,255,.08); color: #fff; }
    .nav-link.active-link { background: #2563eb; color: #fff; box-shadow: 0 4px 12px rgba(37,99,235,.4); }
    .user-menu {
      position: absolute; right: 0; top: calc(100% + .5rem);
      min-width: 180px; background: #fff; border-radius: .75rem;
      box-shadow: 0 8px 30px rgba(0,0,0,.12); border: 1px solid #e2e8f0;
      padding: .375rem; z-index: 50;
    }
    .user-menu button {
      display: flex; align-items: center; gap: .5rem; width: 100%;
      padding: .5rem .75rem; border-radius: .5rem; border: none;
      background: transparent; font-size: .875rem; color: #475569;
      cursor: pointer; transition: background .12s;
    }
    .user-menu button:hover { background: #f1f5f9; color: #0f172a; }
  `],
  template: `
  <div class="flex h-screen overflow-hidden bg-slate-50">
    <!-- ═══ Sidebar ════════════════════════════════════════════════════ -->
    <aside class="sidebar hidden md:flex md:flex-col w-64 flex-shrink-0">
      <!-- Logo -->
      <div class="flex items-center gap-2.5 h-16 px-5 border-b" style="border-color:rgba(255,255,255,.08)">
        <div class="w-8 h-8 bg-blue-500 rounded-lg flex items-center justify-center shadow">
          <span class="material-symbols-outlined text-white" style="font-size:1rem">health_and_safety</span>
        </div>
        <span class="text-white font-bold text-base">MediConnect</span>
      </div>
      <!-- Nav -->
      <nav class="flex-1 px-3 py-5 space-y-1 overflow-y-auto">
        @for (item of navItems; track item.route) {
          <a [routerLink]="item.route" routerLinkActive="active-link" class="nav-link">
            <span class="material-symbols-outlined" style="font-size:1.15rem">{{ item.icon }}</span>
            {{ item.label }}
          </a>
        }
      </nav>
      <!-- Footer -->
      <div class="px-4 py-4 border-t" style="border-color:rgba(255,255,255,.08)">
        <div class="flex items-center gap-2.5">
          <div class="w-8 h-8 rounded-full bg-blue-500/30 flex items-center justify-center">
            <span class="material-symbols-outlined text-blue-300" style="font-size:1rem">person</span>
          </div>
          <div class="min-w-0">
            <p class="text-white text-xs font-semibold truncate">{{ user()?.fullName }}</p>
            <p class="text-blue-300 text-xs capitalize truncate">{{ user()?.role }}</p>
          </div>
        </div>
      </div>
    </aside>

    <!-- ═══ Mobile sidebar overlay ═══════════════════════════════════ -->
    @if (mobileOpen()) {
      <div class="fixed inset-0 z-40 flex md:hidden">
        <div class="absolute inset-0 bg-black/50" (click)="mobileOpen.set(false)"></div>
        <aside class="sidebar relative z-50 flex flex-col w-64">
          <div class="flex items-center justify-between h-16 px-5 border-b" style="border-color:rgba(255,255,255,.08)">
            <span class="text-white font-bold">MediConnect</span>
            <button (click)="mobileOpen.set(false)" style="background:transparent;border:none;cursor:pointer;color:#93c5fd">
              <span class="material-symbols-outlined">close</span>
            </button>
          </div>
          <nav class="flex-1 px-3 py-5 space-y-1">
            @for (item of navItems; track item.route) {
              <a [routerLink]="item.route" routerLinkActive="active-link" class="nav-link"
                 (click)="mobileOpen.set(false)">
                <span class="material-symbols-outlined" style="font-size:1.15rem">{{ item.icon }}</span>
                {{ item.label }}
              </a>
            }
          </nav>
        </aside>
      </div>
    }

    <!-- ═══ Main Area ═════════════════════════════════════════════════ -->
    <div class="flex-1 flex flex-col overflow-hidden">
      <!-- Top header -->
      <header class="h-16 flex items-center justify-between px-4 sm:px-6 bg-white border-b border-slate-200 flex-shrink-0">
        <div class="flex items-center gap-3">
          <button class="md:hidden p-2 rounded-lg hover:bg-slate-100 transition-colors"
                  style="border:none;background:transparent;cursor:pointer"
                  (click)="mobileOpen.set(true)">
            <span class="material-symbols-outlined text-slate-600">menu</span>
          </button>
          <div>
            <h1 class="text-sm font-semibold text-slate-900">{{ greeting() }}</h1>
            <p class="text-xs text-slate-400 hidden sm:block">{{ today() }}</p>
          </div>
        </div>
        <div class="flex items-center gap-2">
          <button (click)="toggleDark()" class="p-2 rounded-lg hover:bg-slate-100 transition-colors"
                  style="border:none;background:transparent;cursor:pointer">
            <span class="material-symbols-outlined text-slate-500" style="font-size:1.2rem">
              {{ darkMode() ? 'light_mode' : 'dark_mode' }}
            </span>
          </button>
          <div class="relative">
            <button (click)="menuOpen.update(v=>!v)"
                    class="flex items-center gap-2 pl-2 pr-3 py-1.5 rounded-xl hover:bg-slate-100 transition-colors"
                    style="border:1px solid #e2e8f0;background:transparent;cursor:pointer">
              <div class="w-7 h-7 rounded-full bg-brand-100 flex items-center justify-center">
                <span class="material-symbols-outlined text-brand-600" style="font-size:.95rem">person</span>
              </div>
              <span class="text-sm font-medium text-slate-700 hidden sm:block">{{ user()?.fullName?.split(' ')[0] }}</span>
              <span class="material-symbols-outlined text-slate-400" style="font-size:1rem">expand_more</span>
            </button>
            @if (menuOpen()) {
              <div class="user-menu">
                <button (click)="logout(); menuOpen.set(false)">
                  <span class="material-symbols-outlined text-slate-500" style="font-size:1rem">logout</span>
                  Sign out
                </button>
              </div>
            }
          </div>
        </div>
      </header>

      <!-- Page content -->
      <main class="flex-1 overflow-y-auto p-4 sm:p-6">
        <router-outlet />
      </main>
    </div>
  </div>
  `
})
export class ShellComponent {
  private readonly auth   = inject(AuthService);
  private readonly router = inject(Router);

  readonly user       = this.auth.user;
  readonly darkMode   = signal(false);
  readonly mobileOpen = signal(false);
  readonly menuOpen   = signal(false);

  readonly greeting = computed(() => {
    const name = this.user()?.fullName?.split(' ')[0] ?? '';
    const h = new Date().getHours();
    const part = h < 12 ? 'morning' : h < 17 ? 'afternoon' : 'evening';
    return `Good ${part}, ${name}`;
  });

  readonly today = computed(() =>
    new Date().toLocaleDateString('en-IN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
  );

  readonly navItems: NavItem[] = [
    { label: 'Dashboard',    icon: 'dashboard',       route: '/app/dashboard' },
    { label: 'Appointments', icon: 'event',            route: '/app/appointments' },
  ];

  toggleDark(): void {
    this.darkMode.update((v: boolean) => !v);
    document.documentElement.classList.toggle('dark', this.darkMode());
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}

