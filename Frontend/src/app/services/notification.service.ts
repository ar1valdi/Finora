import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Notification {
  id: string;
  message: string;
  type: 'error' | 'success' | 'warning' | 'info';
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notifications$ = new BehaviorSubject<Notification[]>([]);
  private notificationId = 0;

  constructor() { }

  getNotifications() {
    return this.notifications$.asObservable();
  }

  showError(message: string, duration: number = 5000) {
    this.addNotification({
      id: this.generateId(),
      message,
      type: 'error',
      duration
    });
  }

  showSuccess(message: string, duration: number = 3000) {
    this.addNotification({
      id: this.generateId(),
      message,
      type: 'success',
      duration
    });
  }

  showWarning(message: string, duration: number = 4000) {
    this.addNotification({
      id: this.generateId(),
      message,
      type: 'warning',
      duration
    });
  }

  showInfo(message: string, duration: number = 3000) {
    this.addNotification({
      id: this.generateId(),
      message,
      type: 'info',
      duration
    });
  }

  private addNotification(notification: Notification) {
    const currentNotifications = this.notifications$.value;
    this.notifications$.next([...currentNotifications, notification]);

    // Auto-remove notification after duration
    if (notification.duration && notification.duration > 0) {
      setTimeout(() => {
        this.removeNotification(notification.id);
      }, notification.duration);
    }
  }

  removeNotification(id: string) {
    const currentNotifications = this.notifications$.value;
    const updatedNotifications = currentNotifications.filter(n => n.id !== id);
    this.notifications$.next(updatedNotifications);
  }

  clearAll() {
    this.notifications$.next([]);
  }

  private generateId(): string {
    return `notification-${++this.notificationId}-${Date.now()}`;
  }
}
