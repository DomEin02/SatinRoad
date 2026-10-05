import { useState } from "react";
import type { FormEvent } from "react";
import { api, errorMessage, saveSession } from "./apiClient";
import type { UserResponse } from "./Api";

export function Login({ onLoggedIn }: { onLoggedIn: (user: UserResponse) => void }) {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");

    async function handleSubmit(e: FormEvent) {
        e.preventDefault();
        setError("");
        try {
            const res = await api.auth.authLogin({ username, password });
            saveSession(res.data.token, res.data.user);
            onLoggedIn(res.data.user);
        } catch (err) {
            setError(errorMessage(err));
        }
    }

    return (
        <form onSubmit={handleSubmit}>
            <h2>Log in</h2>
            <input placeholder="Username" value={username} onChange={(e) => setUsername(e.target.value)} />
            <input type="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)} />
            <button type="submit">Log in</button>
            {error && <p style={{ color: "red" }}>{error}</p>}
        </form>
    );
}