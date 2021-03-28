import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';

const routes: Routes = [
    {
        path: ':token',
        children: [
            {
                path: 'rob_0',
                loadChildren: () => import('./pages/rob0/rob0.module').then((m) => m.Rob0Module),
            },
            {
                path: 'commands',
                loadChildren: () => import('./pages/commands/commands.module').then((m) => m.CommandsModule),
            },
        ],
    },
];

@NgModule({
    imports: [RouterModule.forRoot(routes, { useHash: true, preloadingStrategy: PreloadAllModules, relativeLinkResolution: 'legacy' })],
    exports: [RouterModule],
})
export class AppRoutingModule {}
