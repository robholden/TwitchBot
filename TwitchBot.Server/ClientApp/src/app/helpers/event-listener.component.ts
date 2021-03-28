import { Injector, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { EventService } from '../services/event.service';

export type EventMethod = 'channel.follow' | 'command.dropping';

export class ListenToEvent<T> implements OnDestroy {
    private activateRoute: ActivatedRoute;
    private events: EventService;
    private token: string;

    constructor(injector: Injector, private method: EventMethod) {
        this.activateRoute = injector.get<ActivatedRoute>(ActivatedRoute);
        this.events = injector.get<EventService>(EventService);

        this.token = this.activateRoute.snapshot.params['token'];
    }

    ngOnDestroy() {
        this.events.unlisten(this.method);
    }

    async listen(callback: (evt: T) => void): Promise<boolean> {
        const connected = await this.events.connect(this.token);
        if (!connected) {
            console.log(`Failed to connect to ${this.method}`);
            return false;
        }

        await this.events.listen(this.method, callback);
        return true;
    }
}
