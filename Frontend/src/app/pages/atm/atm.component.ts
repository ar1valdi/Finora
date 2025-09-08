import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { BankingService } from '../../services/banking.service';
import { CurrentUserService } from '../../services/current-user.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-atm',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './atm.component.html',
  styleUrl: './atm.component.scss'
})
export class AtmComponent {
  atmForm = {
    amount: 0,
    operation: 'deposit'
  };

  loading = false;

  constructor(
    private bankingService: BankingService, 
    private currentUserService: CurrentUserService,
    private notificationService: NotificationService
  ) {}

  async processTransaction() {
    const bankAccountId = this.currentUserService.getCurrentUser()?.bankAccountId ?? '';
    if (this.atmForm.amount <= 0) {
      this.notificationService.showError('Please enter a valid amount');
      return;
    }
    if (!bankAccountId) {
      this.notificationService.showError('Please login to continue');
      return;
    }

    this.loading = true;
    const amount = this.atmForm.operation === 'withdraw' 
      ? -Math.abs(this.atmForm.amount) 
      : Math.abs(this.atmForm.amount);

    this.bankingService.depositWithdrawl({
      bankAccountId: bankAccountId,
      amount: amount
    }).then((response: any) => {
      const operationType = this.atmForm.operation === 'withdraw' ? 'Withdrawal' : 'Deposit';
      this.notificationService.showSuccess(`${operationType} completed successfully!`);
      this.atmForm = { amount: 0, operation: 'deposit' };
    }).catch((error: any) => {
      console.error('ATM Transaction error:', error);
      this.notificationService.showError('Transaction failed. Please try again.');
    }).finally(() => {
      this.loading = false;
    });
  }
}
