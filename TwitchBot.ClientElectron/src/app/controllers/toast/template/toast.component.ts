import { Component, HostBinding, Input, OnInit } from '@angular/core';

import { BehaviorSubject } from 'rxjs';

import { BotColour, ControllersService, IControllerComponent } from '../../controllers.service';

export interface IToastComponent extends IControllerComponent {
    colour: BotColour;
    message: string;

    present(duration?: number): Promise<void>;
    dismiss(): void;
}

@Component({
    selector: 'bot-toast-component',
    templateUrl: './toast.component.html',
    styleUrls: ['./toast.component.scss'],
})
export class ToastComponent implements IToastComponent, OnInit {
    id: string;
    duration: number = null;

    @HostBinding('class.preloaded') preloaded = true;

    @Input() message: string;
    @HostBinding('attr.background') @Input() colour: BotColour;

    private _result = new BehaviorSubject<boolean>(false);

    constructor(private ctrl: ControllersService) {}

    ngOnInit() {}

    present(duration?: number) {
        this.ctrl.show(this.id);

        return new Promise<void>((res) => {
            this._result.subscribe((value) => {
                if (!value) return;

                this.ctrl.destroy(this.id);
                res();
            });

            if (duration) setTimeout(() => this.dismiss(), duration);

            setTimeout(() => {
                this.duration = duration || 0;
                this.preloaded = false;
            }, 10);
        });
    }

    dismiss() {
        this._result.next(true);
    }
}
