import { Injectable } from '@angular/core';

import { ControllersService } from '../controllers.service';

export interface ILoadingBtn {
    id: string;
    element: HTMLElement;
    present: () => void;
    dismiss: () => void;
}

@Injectable({
    providedIn: 'root',
})
export class LoadingController {
    constructor(private ctrl: ControllersService) {}

    add(id: string): ILoadingBtn {
        const element = document.getElementById(id);
        return {
            id,
            element,
            present: () => this.setButtonState(element, true),
            dismiss: () => this.setButtonState(element, false),
        };
    }

    private setButtonState(btn: HTMLElement, load: boolean): void {
        if (!btn) return;

        if (load) {
            const content = btn.innerHTML;
            const loadingContent = '<i class="far fa-spin fa-spinner-third"></i>';

            btn.setAttribute('data-content', content);
            btn.innerHTML = loadingContent;
            btn.setAttribute('disabled', 'disabled');
        } else {
            const html = btn.getAttribute('data-content');
            btn.removeAttribute('disabled');
            btn.removeAttribute('data-content');
            btn.innerHTML = html;
        }
    }
}
