import { Injectable } from '@angular/core';

import { ModalController } from '../modal/modal.controller';
import { AlertComponent, IButton, IInput } from './template/alert.component';

export interface IAlertOptions {
    title?: string;
    message?: string;
    buttons?: IButton[];
    inputs?: IInput[];
    focusFirst?: boolean;
}

export interface IConfirmOptions {
    title?: string;
    message?: string;
    confirmBtn?: IButton;
    cancelBtn?: IButton;
    focusFirst?: boolean;
}

@Injectable({
    providedIn: 'root',
})
export class AlertController {
    constructor(private modalCtrl: ModalController) {}

    async alert(title: string, message: string, closeText: string) {
        await this.create({
            title,
            message,
            buttons: [
                {
                    text: closeText,
                    role: 'submit',
                    colour: 'primary',
                    className: 'mx-a',
                },
            ],
        });
    }

    async confirm(options: IConfirmOptions): Promise<boolean> {
        const confirmBtn = options.confirmBtn || {
            text: 'Confirm',
            role: 'submit',
        };

        const cancelBtn = options.cancelBtn || {
            text: 'Cancel',
            role: 'cancel',
        };

        const result = await this.create({
            title: options.title,
            message: options.message,
            buttons: [
                {
                    text: cancelBtn.text,
                    role: 'cancel',
                    colour: cancelBtn.colour || 'neutral',
                    className: 'mr-a',
                },
                {
                    text: confirmBtn.text,
                    role: 'submit',
                    colour: confirmBtn.colour || 'primary',
                    className: 'ml-a',
                },
            ],
            focusFirst: options.focusFirst,
        });

        return result !== null;
    }

    create(options: IAlertOptions) {
        const modal = this.modalCtrl.add(new Date().toString(), AlertComponent, {
            title: options.title,
            message: options.message,
            inputs: options.inputs,
            buttons: options.buttons,
            focusFirst: options.focusFirst,
        });

        return modal.present();
    }
}
