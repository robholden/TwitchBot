import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { AppConfig } from '../app.config';
import { Sub } from '../models/subscription';
import { User } from '../models/user';
import { HttpApiService } from './http-api.service';
import { SignalRService } from './signalr.service';

interface AuthResult {
    user: User;
    subscriptions: Sub[];
}

interface LoginResult {
    token: string;
    subscriptions: Sub[];
    user: User;
}

@Injectable({
    providedIn: 'root',
})
export class UserService {
    constructor(private api: HttpApiService, private config: AppConfig, private signalr: SignalRService) {}

    async login(username: string, password: string): Promise<boolean> {
        const resp = await this.api.post<LoginResult>(`/auth/login`, { username, password });
        if (resp instanceof HttpErrorResponse) return false;

        this.config.user = resp.user;
        this.config.token = resp.token;
        this.signalr.connect(resp.subscriptions);
        return true;
    }

    async auth(): Promise<boolean> {
        const resp = await this.api.post<AuthResult>(`/auth`, {});
        if (resp instanceof HttpErrorResponse) return false;

        this.config.user = resp.user;
        this.signalr.connect(resp.subscriptions);
        return true;
    }

    async logout(): Promise<boolean> {
        const resp = await this.api.get<void>(`/auth/logout`);
        if (resp instanceof HttpErrorResponse) return false;

        this.config.user = null;
        this.config.token = null;
        this.signalr.disconnect();
        return true;
    }
}
