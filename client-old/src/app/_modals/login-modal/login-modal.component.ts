import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { AccountService } from 'src/app/_services/account.service';

@Component({
  selector: 'app-login-modal',
  templateUrl: './login-modal.component.html',
  styleUrls: ['./login-modal.component.css']
})
export class LoginModalComponent implements OnInit {
  public event: EventEmitter<any> = new EventEmitter();
  title: string;
  list: any[] = [];
  closeBtnName: string;
  submitBtnName: string;
  model: any = {};

  constructor(public bsModalRef: BsModalRef, private accountService: AccountService, private router: Router) { }

  ngOnInit(): void {
  }

  login() {
    this.accountService.login(this.model).subscribe(response => {
      this.router.navigateByUrl('/games');
    }, error => {
      console.log(error);
    })
  }
}
