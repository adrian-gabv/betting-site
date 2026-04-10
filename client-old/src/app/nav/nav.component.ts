import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Observable } from 'rxjs';
import { LoginModalComponent } from '../_modals/login-modal/login-modal.component';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  bsModalRef?: BsModalRef;
  currentUser$: Observable<User>;
  

  constructor(private modalService: BsModalService, public accountService: AccountService, private router: Router,
              private memberService: MembersService) { }

  ngOnInit(): void {
  }

  openLoginModal() {
    const initialState = {
      list: [
        'Salut',
        'Miau',
        'Da parola frate',
        '...'
      ],
      title: 'Login'
    };
    this.bsModalRef = this.modalService.show(LoginModalComponent, {initialState});
    this.bsModalRef.content.closeBtnName = 'Cancel';
    this.bsModalRef.content.submitBtnName = 'Login';
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
