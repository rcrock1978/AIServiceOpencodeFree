import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/navbar.component';
import { ChatbotComponent } from './core/chatbot/chatbot.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, ChatbotComponent],
  template: `
    <app-navbar />
    <main>
      <router-outlet />
    </main>
    <app-chatbot />
  `,
  styles: [`
    main { min-height: calc(100vh - 64px); padding-top: 64px; }
  `]
})
export class AppComponent {}
