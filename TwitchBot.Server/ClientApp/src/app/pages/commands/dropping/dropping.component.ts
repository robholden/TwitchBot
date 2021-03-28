import { Component, HostBinding, Injector } from '@angular/core';

import { ListenToEvent } from '@bot/helpers/event-listener.component';
import { wait } from '@bot/helpers/functions';

@Component({
    selector: 'bot-dropping',
    templateUrl: './dropping.component.html',
    styleUrls: ['./dropping.component.scss'],
})
export class DroppingComponent extends ListenToEvent<string> {
    @HostBinding('class.hidden') private hidden = true;

    letters = 'ABCDEFGHIJ'.split('');
    numbers = '0123456789'.split('');

    coord: string;
    show: boolean;

    constructor(injector: Injector) {
        super(injector, 'command.dropping');
        this.listen((marker) => this.dropAt(marker));
    }

    ngOnInit(): void {}

    private async dropAt(marker: string) {
        if (!this.hidden) return;

        this.hidden = false;
        await wait(250);

        this.coord = marker;
        this.show = true;
        await wait(5000);

        this.show = false;
        this.hidden = true;
        await wait(250);

        this.coord = null;
    }
}
