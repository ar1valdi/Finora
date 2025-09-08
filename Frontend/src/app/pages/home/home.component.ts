import { Component, effect, OnInit, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CurrentUserService } from '../../services/current-user.service';
import { BankingService } from '../../services/banking.service';
import { UserTransactionDTO } from '../../models/banking/bankTransaction.model';
import { User } from '../../models/users/user.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  currentUser: User | null = null;
  transactions: UserTransactionDTO[] = [];
  loading = false;
  currentPage = 1;
  pageSize = 10;
  totalTransactions = 0;
  totalPages = 0;
  $currentUser: Signal<User | null>;
  
  constructor(
    private currentUserService: CurrentUserService,
    private bankingService: BankingService
  ) {
    this.$currentUser = this.currentUserService.getCurrentUserSignal();
    effect(() => {
      this.currentUser = this.$currentUser();
      if (this.currentUser) {
        this.loadTransactions();
      }
    });
  }

  ngOnInit() {
  }

  async loadTransactions() {
    if (!this.currentUser) return;

    this.loading = true;
    try {
      const response = await this.bankingService.getUserTransactions(
        this.currentUser.id,
        this.currentPage,
        this.pageSize
      );

      this.transactions = response.items || [];
      this.totalTransactions = response.total || 0;
      this.totalPages = Math.ceil(this.totalTransactions / this.pageSize);
    } catch (error) {
      console.error('Error loading transactions:', error);
      this.transactions = [];
    } finally {
      this.loading = false;
    }
  }

  async goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      await this.loadTransactions();
    }
  }

  async previousPage() {
    if (this.currentPage > 1) {
      await this.goToPage(this.currentPage - 1);
    }
  }

  async nextPage() {
    if (this.currentPage < this.totalPages) {
      await this.goToPage(this.currentPage + 1);
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisiblePages = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
    let endPage = Math.min(this.totalPages, startPage + maxVisiblePages - 1);

    if (endPage - startPage < maxVisiblePages - 1) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
  }

  getEndIndex(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalTransactions);
  }
}
