import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { BankingService } from '../../services/banking.service';
import { CurrentUserService } from '../../services/current-user.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-transfer',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './transfer.component.html',
  styleUrl: './transfer.component.scss'
})
export class TransferComponent {
  transferForm = {
    toAccountId: '',
    amount: 0,
    description: ''
  };

  loading = false;

  constructor(
    private bankingService: BankingService,
    private currentUserService: CurrentUserService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  async processTransfer() {
    const currentUser = this.currentUserService.getCurrentUser();
    
    if (!currentUser?.bankAccountId) {
      this.notificationService.showError('Please login to continue');
      return;
    }

    if (!this.transferForm.toAccountId || this.transferForm.amount <= 0) {
      this.notificationService.showError('Please enter a valid recipient account ID and amount');
      return;
    }

    if (this.transferForm.toAccountId === currentUser.bankAccountId) {
      this.notificationService.showError('Cannot transfer to your own account');
      return;
    }

    this.loading = true;
    this.bankingService.transferMoney({
      fromBankAccountId: currentUser.bankAccountId,
      toBankAccountId: this.transferForm.toAccountId,
      amount: this.transferForm.amount,
      description: this.transferForm.description
    }).then((response: any) => {
      this.notificationService.showSuccess('Transfer completed successfully!');
      this.router.navigate(['/']);
    }).catch((error: any) => {
      console.error('Transfer Money error:', error);
      this.notificationService.showError('Transfer failed. Please try again.');
    }).finally(() => {
      this.loading = false;
    });
  }

  cancel() {
    this.router.navigate(['/home']);
  }
}
