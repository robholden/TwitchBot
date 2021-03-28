import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

import { AppConfig } from '../app.config';

@Injectable({
    providedIn: 'root',
})
export class AppInterceptor implements HttpInterceptor {
    constructor(private config: AppConfig) {}

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (this.config.token) {
            request = request.clone({ headers: request.headers.set('Event-Token', this.config.token) });
        }

        // Send back event
        return next.handle(request);
    }
}
