import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, OnInit } from '@angular/core';

import { AppConfig } from './app.config';
import { UserService } from './services/user.service';

@Component({
    selector: 'bot-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
    animations: [
        trigger('loading', [
            state('in', style({ opacity: 1 })),
            transition(':enter', [style({ opacity: 0 }), animate(500)]),
            transition(':leave', animate(250, style({ opacity: 0 }))),
        ]),
        trigger('router', [state('in', style({ opacity: 1 })), transition(':enter', [style({ opacity: 0 }), animate(250)])]),
    ],
})
export class AppComponent implements OnInit {
    address: string;
    username: string;
    password: string;
    loading: boolean;

    constructor(public config: AppConfig, public userService: UserService) {}

    ngOnInit() {}

    async login() {
        this.loading = true;
        this.config.address = this.address;
        await this.userService.login(this.username, this.password);
        this.password = '';
        this.loading = false;
    }
}
