export type AppUser = {
  id: string;
  fullName: string;
  email: string;
  avatarUrl?: string | null;
  roles: string[];            // ["SuperAdmin","Admin","User"]
  permissions: string[];      // ["Dashboard.View","Apps.Read",...]
  organization?: {
    id: string;
    name: string;
  } | null;
  app?: {
    id: string;
    code: string;
    name: string;
  } | null;
};
