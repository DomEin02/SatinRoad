import { Api } from "./Api";
import type { UserResponse } from "./Api";

const TOKEN_KEY = "satinroad-token";
const USER_KEY = "satinroad-user";

export function saveSession(token: string, user: UserResponse) {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
}

export function clearSession() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
}

export function getUser(): UserResponse | null {
    try {
        const raw = localStorage.getItem(USER_KEY);
        return raw ? (JSON.parse(raw) as UserResponse) : null;
    } catch {
        return null;
    }
}

export const api = new Api({
    baseUrl: location.port === "3000" ? "http://localhost:5167" : "/api", customFetch: (input, init) => {
        const headers = new Headers(init?.headers);
        const token = localStorage.getItem(TOKEN_KEY);
        if (token) headers.set("Authorization", `Bearer ${token}`);
        return fetch(input, { ...init, headers });
    } ,
});

export function errorMessage(e: unknown): string {
    const err = e as { status?: number; error?: { title?: string}; message?: string };
    if (err?.status === 401 || err?.status === 403) return "You must be logged in as admin to do that.";
    return err?.error?.title ?? err?.message ?? "Something went wrong.";
}