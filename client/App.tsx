import { useState } from "react";
import { Login } from "./src/Login";
import { CategoryAdmin } from "./src/CategoryAdmin";
import { Listings } from "./src/Listings";
import { Landing } from "./src/Landing";
import { clearSession, getUser } from "./src/apiClient";
import type { UserResponse } from "./src/Api";
import { MyOrders } from "./src/MyOrders";

export function App() {
    const [user, setUser] = useState<UserResponse | null>(getUser());
    const [listingsVersion, setListingsVersion] = useState(0);

    function logout() {
        clearSession();
        setUser(null);
    }
    
    function refresh() {
        setListingsVersion((v) => v + 1);
    }

    return (
        <main className="App">
            <h1>Satin Road</h1>
            {user ? (
                <p>
                    Logged in as {user.username} ({user.role}) <button onClick={logout}>Log out</button>
                </p>
            ) : (
                <Login onLoggedIn={setUser} />
            )}

            {/* Forsiden er offentlig: alle kan se featured vendors og listings */}
            <Landing user={user} refreshKey={listingsVersion} onPurchased={refresh} />

            {user && (
                <>
                    <CategoryAdmin user={user} />
                    <Listings user={user} onChanged={refresh} />
                    <MyOrders refreshKey={listingsVersion} />
                </>
            )}
        </main>
    );
}

export default App;