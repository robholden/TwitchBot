import { Injectable } from '@angular/core';

import { AppConfig } from '../app.config';
import { Logger } from '../helpers/logger';

import e = require('express');
declare const __dirname;

const app: e.Express = (<any>window).require('express')();
const path = (<any>window).require('path');
const fs = (<any>window).require('fs');
var http = (<any>window).require('http').createServer(app);
var io = (<any>window).require('socket.io')(http);

@Injectable({
    providedIn: 'root',
})
export class ExpressService {
    private _running: boolean;

    constructor(private config: AppConfig) {}

    start(): Promise<boolean> {
        return new Promise<boolean>((res) => {
            if (this._running) return;

            app.get('/shared/assets/:file', (req, res) => {
                res.sendFile(path.join(__dirname, `../static/shared/assets/${req.params.file}`));
            });

            app.get('/shared/:page', (req, res) => {
                res.sendFile(path.join(__dirname, `../static/shared/${req.params.page}.html`));
            });

            app.get('/assets/:file', (req, res) => {
                res.sendFile(path.join(__dirname, `../static/${this.config.user.username.toLowerCase()}/assets/${req.params.file}`));
            });

            app.get('/:page', (req, res) => {
                let html = fs.readFileSync(path.join(__dirname, `../static/${this.config.user.username.toLowerCase()}/${req.params.page}.html`), 'utf8');
                const qKeys = Object.keys(req.query);
                for (let i = 0; i < qKeys.length; i++) {
                    html = html.replace(`<%${qKeys[i]}%>`, req.query[qKeys[i]]);
                }

                res.send(html);
            });

            http.listen(
                8998,
                () => {
                    Logger.debug('Started on 8998');
                    this._running = true;
                    res(true);
                },
                () => res(false)
            );
        });
    }

    stop() {
        if (this._running) http.stop();
    }

    emitEvent(name: string, payload: any) {
        if (!this._running) return;

        console.log(name, payload);

        io.emit(name, payload);
    }
}
