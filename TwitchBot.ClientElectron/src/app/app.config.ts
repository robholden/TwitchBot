import { Injectable } from '@angular/core';

import { User } from './models/user';

const { getValue, setValue } = (<any>window).require('electron').remote.require('./app');

@Injectable({
    providedIn: 'root',
})
export class AppConfig {
    initialising: boolean = true;
    user: User;

    private _token: string;
    get token(): string {
        return this._token;
    }
    set token(value: string) {
        this._token = value;
        setValue('token', value);
    }

    private _address: string;
    get address(): string {
        return this._address;
    }
    set address(value: string) {
        this._address = value;
        setValue('address', value);
    }

    constructor() {
        this._address = getValue('address');
        this._token = getValue('token');

        setTimeout(() => (this.initialising = false), 500);
    }
}
