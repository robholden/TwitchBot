import { Injectable, Type } from '@angular/core';

import { ControllersService } from '../controllers.service';
import { IModalComponent } from './modal.component';

@Injectable()
export class ModalController {
    constructor(private ctrl: ControllersService) {}

    dismiss(id: string) {
        this.ctrl.destroy(id);
    }

    add<T>(id: string, component: Type<IModalComponent<T>>, props?: any): IModalComponent<T> {
        return this.ctrl.create(
            component,
            (instance) => {
                if (props) Object.keys(props).forEach((prop) => (instance[prop] = props[prop]));
            },
            id
        );
    }
}
