import { useState } from "react";
import { Login } from "./src/Login";
import { CategoryAdmin } from "./src/CategoryAdmin";
import { Listings } from "./src/Listings";
import { clearSession, getUser } from "./src/apiClient";
import type { UserResponse } from "./src/Api";

export function App() {
    const [user, setUser] = useState<UserResponse | null>(getUser());

    function logout() {
        clearSession();
        setUser(null);
    }

    return (
        <div className="App">
            <h1>Satin Road</h1>
            {user ? (
                <>
                    <p>
                        Logged in as {user.username} ({user.role}) <button onClick={logout}>Log out</button>
                    </p>
                    <CategoryAdmin user={user} />
                    <Listings user={user} />
                </>
            ) : (
                <Login onLoggedIn={setUser} />
            )}
        </div>
    );
}

export default App;