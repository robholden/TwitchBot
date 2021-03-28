import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';

import { Observable, Subject } from 'rxjs';
import { catchError, tap, timeout } from 'rxjs/operators';
import { AppConfig } from '../app.config';
import { ToastController } from '../controllers/toast/toast.controller';
import { SMap } from '../models/smap';

export interface IApiOptions {
    toastError?: boolean;
}

@Injectable({
    providedIn: 'root',
})
export class HttpApiService {
    private _calls: SMap<Subject<any | HttpErrorResponse>> = {};

    constructor(private http: HttpClient, private config: AppConfig, private toastCtrl: ToastController) {}

    /**
     * Constructs a `GET` request that interprets the body as a JSON object and returns
     * the response body in a given type.
     *
     * @param url The endpoint url
     * @param options The http options to send with the request
     */
    async get<T>(url: string, options?: IApiOptions): Promise<T | HttpErrorResponse> {
        const meta = await this.callMeta(url, options);
        const req = this.http.get<T>(meta.url, meta.options);
        return this.handleResponse(url, req, options);
    }

    /**
     * Constructs a `PUT` request that interprets the body as a JSON object and returns
     * the response body in a given type.
     *
     * @param url The endpoint url
     * @param body The resources to add/update.
     * @param options The http options to send with the request
     */
    async put<T>(url: string, body?: any, options?: IApiOptions): Promise<T | HttpErrorResponse> {
        const meta = await this.callMeta(url, options);
        const req = this.http.put<T>(meta.url, body, meta.options);
        return this.handleResponse(url, req, options);
    }

    /**
     * Constructs a `POST` request that interprets the body as a JSON object and returns
     * the response body in a given type.
     *
     * @param url The endpoint url
     * @param body The content to replace with.
     * @param options The http options to send with the request
     */
    async post<T>(url: string, body?: any, options?: IApiOptions): Promise<T | HttpErrorResponse> {
        const meta = await this.callMeta(url, options);
        const req = this.http.post<T>(meta.url, body, meta.options);
        return this.handleResponse(url, req, options);
    }

    /**
     * Constructs a `DELETE` request that interprets the body as a JSON object and returns
     * the response body in a given type.
     *
     * @param url The endpoint url
     * @param options The http options to send with the request
     */
    async delete<T>(url: string, options?: IApiOptions): Promise<T | HttpErrorResponse> {
        const meta = await this.callMeta(url, options);
        const req = this.http.delete<T>(meta.url, meta.options);
        return this.handleResponse(url, req, options);
    }

    private async callMeta(url: string, options?: IApiOptions): Promise<{ url: string; options: { headers: HttpHeaders } }> {
        options = options || {};

        let headers: HttpHeaders = new HttpHeaders();
        const httpOptions = { headers };

        return { url: (url || '').startsWith('http') ? url : this.config.address + url, options: httpOptions };
    }

    private async handleResponse<T>(url: string, obs: Observable<T>, options?: IApiOptions): Promise<T | HttpErrorResponse> {
        options = options || {};
        options.toastError = options.toastError !== false;

        if (this._calls[url]) {
            return await new Promise<T | HttpErrorResponse>((res) => this._calls[url].subscribe((result) => res(result)));
        }

        const subject = new Subject<T | HttpErrorResponse>();
        this._calls[url] = subject;

        const resError = async (resolve: any, httpError: HttpErrorResponse) => {
            if (options.toastError) this.toastCtrl.add(httpError.message);
            resolve(httpError);
            subject.next(httpError);
        };

        return new Promise<T | HttpErrorResponse>((resolve) => {
            obs.pipe(
                timeout(60 * 1000),
                catchError(async (httpError: HttpErrorResponse) => {
                    await resError(resolve, httpError);
                    if (typeof httpError === 'string') console.error(httpError);
                }),
                tap(() => (this._calls[url] = null))
            ).subscribe(
                (value: T) => {
                    resolve(value);
                    subject.next(value);
                },
                async (httpError: HttpErrorResponse) => await resError(resolve, httpError)
            );
        });
    }
}
