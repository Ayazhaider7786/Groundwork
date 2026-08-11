/**
 * Every error shape the backend can return: a ProblemDetails `detail`, the
 * Identity `{ errors: [...] }` list, or FluentValidation's
 * `{ errors: { Field: [...] } }` map.
 */
export interface ApiErrorResponse {
  title?: string;
  detail?: string;
  errors?: string[] | Record<string, string[]>;
}
