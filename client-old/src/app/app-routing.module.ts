import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminToolsComponent } from './admin/admin-tools/admin-tools.component';
import { AffiliatesComponent } from './affiliates/affiliates.component';
import { NotFoundComponent } from './errors/not-found/not-found.component';
import { ServerErrorComponent } from './errors/server-error/server-error.component';
import { TestErrorsComponent } from './errors/test-errors/test-errors.component';
import { GamesListComponent } from './games/games-list/games-list.component';
import { HomeComponent } from './home/home.component';
import { InfoComponent } from './info/info.component';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { MemberEditComponent } from './members/member-edit/member-edit.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { WithdrawComponent } from './withdraw/withdraw.component';
import { AdminGuard } from './_guards/admin.guard';
import { AuthGuard } from './_guards/auth.guard';
import { PreventUnsavedChangesGuard } from './_guards/prevent-unsaved-changes.guard';

const routes: Routes = [
  {path: '', component: HomeComponent},
  {
    path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [AuthGuard],
    children: [
      {path: 'games', component: GamesListComponent},
      {path: 'games/:id', component: GamesListComponent},
      {path: 'withdraw', component: WithdrawComponent},
      {path: 'affiliates', component: AffiliatesComponent},
      {path: 'info', component: InfoComponent},
      {path: 'members', component: MemberListComponent},
      {path: 'members/:username', component: MemberDetailComponent},
      {path: 'member/edit', component: MemberEditComponent, canDeactivate: [PreventUnsavedChangesGuard]},
      {path: 'admin', component: AdminToolsComponent, canActivate: [AdminGuard]}
    ]
  },
  {path: 'errors', component: TestErrorsComponent},
  {path: 'not-found', component: NotFoundComponent},
  {path: 'server-error', component: ServerErrorComponent},
  {path: '**', component: NotFoundComponent, pathMatch: 'full'},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
