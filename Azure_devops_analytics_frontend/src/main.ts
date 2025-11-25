import { bootstrapApplication } from '@angular/platform-browser';
import { App } from './app/app';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

bootstrapApplication(App, {
  providers: [
    provideCharts(withDefaultRegisterables())
  ]
}).catch(err => console.error(err));
