/**
 * A seeded development account. Carries the plaintext password because the whole
 * point is to fill the login form — the API only serves this in Development.
 */
export interface SeedUser {
  fullName: string;
  email: string;
  password: string;
  role: string;
}
