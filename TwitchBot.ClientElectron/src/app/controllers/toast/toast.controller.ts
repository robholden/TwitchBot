import { Injectable } from '@angular/core';

import { BotColour, ControllersService } from '../controllers.service';
import { IToastComponent, ToastComponent } from './template/toast.component';

@Injectable({
    providedIn: 'root',
})
export class ToastController {
    constructor(private ctrl: ControllersService) {}

    add(message: string, colour: BotColour = 'primary'): IToastComponent {
        return this.ctrl.create(ToastComponent, (instance) => {
            instance.message = message;
            instance.colour = colour;
        });
    }
}
