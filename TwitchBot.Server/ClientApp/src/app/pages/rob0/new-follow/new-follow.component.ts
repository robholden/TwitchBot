import { Component, Injector } from '@angular/core';

import { ListenToEvent } from '@bot/helpers/event-listener.component';

export interface FollowerEvent {
    user_name: string;
}

@Component({
    selector: 'bot-new-follow',
    templateUrl: './new-follow.component.html',
    styleUrls: ['./new-follow.component.scss'],
})
export class NewFollowComponent extends ListenToEvent<FollowerEvent> {
    message: string;

    constructor(injector: Injector) {
        super(injector, 'channel.follow');
        this.listen((evt) => this.followed(evt));
    }

    followed(evt: FollowerEvent) {
        this.message = `${evt.user_name} just followed`;
    }
}
