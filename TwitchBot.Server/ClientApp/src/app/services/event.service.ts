import { Inject, Injectable } from '@angular/core';

import * as signalR from '@microsoft/signalr';

@Injectable({
    providedIn: 'root',
})
export class EventService {
    private connection: signalR.HubConnection;

    constructor(@Inject('BASE_URL') private baseUrl: string) {}

    async connect(token: string): Promise<boolean> {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(this.baseUrl + 'hubs/event', { accessTokenFactory: () => token })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.None)
            .build();

        try {
            await this.connection.start();
        } catch (err) {
            return false;
        }

        return true;
    }

    async listen<T>(method: string, callback: (payload: T) => void) {
        if (!this.connection) return;
        this.connection.on(method, (payload: T) => {
            console.log(method, payload);
            callback(payload);
        });

        console.log(`Listening to ${method}`);
    }

    unlisten(method: string) {
        if (!this.connection) return;
        this.connection.off(method);
    }
}
