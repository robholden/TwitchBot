import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DroppingComponent } from './dropping/dropping.component';

const routes: Routes = [
    {
        path: 'dropping',
        component: DroppingComponent,
    },
];

@NgModule({
    declarations: [DroppingComponent],
    imports: [CommonModule, RouterModule.forChild(routes)],
})
export class CommandsModule {}
