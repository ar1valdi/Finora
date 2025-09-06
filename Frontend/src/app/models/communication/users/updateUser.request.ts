export interface UpdateUserRequest {
  id: string;
  firstName: string;
  secondName?: string;
  lastName: string;
  email: string;
  dateOfBirth: Date;
  password: string;
}
