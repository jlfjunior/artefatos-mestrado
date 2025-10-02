export interface AuthToken {
  given_name: string;
  family_name: string;
  email: string;
  id: string;
  realm_access?: {
    roles: string[];
  };
}