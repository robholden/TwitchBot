import { Component, HostBinding, Injector, Input, OnDestroy, OnInit } from '@angular/core';

import { BehaviorSubject } from 'rxjs';

import { ControllersService, IControllerComponent } from '../controllers.service';

import { OnDestroyMixin } from '@w11k/ngx-componentdestroyed';

export interface IModalComponent<T> extends IControllerComponent {
    present(): Promise<T>;
    dismiss(): void;
}

@Component({
    selector: 'bot-modal-component',
    template: '',
})
export class ModalComponent<T> extends OnDestroyMixin implements IModalComponent<T>, OnInit, OnDestroy {
    id: string;

    protected ctrl: ControllersService;
    protected result = new BehaviorSubject<T>(null);

    @HostBinding('attr.class') className = 'modal-content';
    @Input() text: string;

    constructor(injector: Injector) {
        super();

        this.ctrl = injector.get<ControllersService>(ControllersService);
    }

    ngOnInit() {}

    ngOnDestroy() {}

    present() {
        const wrapper = document.createElement('div');
        wrapper.classList.add('modal-wrapper');

        const overlay = document.createElement('div');
        overlay.classList.add('modal-overlay');

        wrapper.appendChild(overlay);
        this.ctrl.show(this.id, wrapper, (el) => el.classList.add('loaded'));

        let init = false;
        return new Promise<T>((res) => {
            this.result.asObservable().subscribe(
                (value: T) => {
                    if (init) {
                        this.dismiss();
                        res(value);
                    }

                    init = true;
                },
                () => res(this.result.value),
                () => res(this.result.value)
            );
        });
    }

    dismiss() {
        this.ctrl.destroy(this.id);
        this.result.complete();
    }
}
