export interface User {
  id: string;
  firstName: string;
  secondName?: string;
  lastName: string;
  email: string;
  dateOfBirth: string;
  isDeleted: boolean;
}
