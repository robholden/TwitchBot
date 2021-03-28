import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { NewFollowComponent } from './new-follow/new-follow.component';

const routes: Routes = [
    {
        path: 'new-follower',
        component: NewFollowComponent,
    },
];

@NgModule({
    declarations: [NewFollowComponent],
    imports: [CommonModule, RouterModule.forChild(routes)],
})
export class Rob0Module {}
