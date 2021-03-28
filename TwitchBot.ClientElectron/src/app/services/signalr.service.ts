import { Injectable } from '@angular/core';

import { AppConfig } from '../app.config';
import { Sub } from '../models/subscription';
import { ExpressService } from './express.service';

import * as signalR from '@microsoft/signalr';

@Injectable({
    providedIn: 'root',
})
export class SignalRService {
    private connection: signalR.HubConnection;

    constructor(private config: AppConfig, private express: ExpressService) {}

    async connect(subs: Sub[]) {
        const hub_url = `${this.config.address}/hubs/event`;
        console.log(`Establishing socket connection to: ${hub_url}`);

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(hub_url, { accessTokenFactory: () => this.config.token })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.None)
            .build();

        await this.connection.start();
        console.log('Connection established with hub');
        console.log();

        await this.express.start();

        console.log(`${subs.length} subscriptions found`);
        for (let i in subs) this.listenToSub(subs[i]);
        console.log();
    }

    async disconnect() {
        if (this.connection.state !== signalR.HubConnectionState.Connected) return;
        await this.connection.stop();
        await this.express.stop();
    }

    private listenToSub(sub: any) {
        this.connection.on(sub.type, (evt: any) => this.express.emitEvent(sub.type, evt));
        console.log(`Listening to ${sub.type}`);
    }
}
