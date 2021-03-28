import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';

import { AlertComponent } from './alert/template/alert.component';
import { ControllersService } from './controllers.service';
import { ModalComponent } from './modal/modal.component';
import { ModalController } from './modal/modal.controller';
import { ToastComponent } from './toast/template/toast.component';

@NgModule({
    imports: [CommonModule, ReactiveFormsModule],
    declarations: [ModalComponent, ToastComponent, AlertComponent],
    entryComponents: [ModalComponent, ToastComponent, AlertComponent],
    exports: [],
    providers: [ControllersService, ModalController],
})
export class ControllersModule {}
