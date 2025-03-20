// Copied from Auth.js docs
import NextAuth, { type DefaultSession } from "next-auth";
import {JWT} from 'next-auth/jwt';


declare module "next-auth" {
  /**
   * Returned by `auth`, `useSession`, `getSession` and received as a prop on the `SessionProvider` React Context
   */
  interface Session {
    user: {
      username: string;
    } & DefaultSession["user"];
    accessToken: string;
  }

  interface Profile {
    username: string;
  }

  interface User {
    username: string;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    username: string
    accessToken: string
  }
}
