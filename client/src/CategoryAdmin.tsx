import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { api, errorMessage } from "./apiClient";
import type { CategoryResponse, UserResponse } from "./Api";

export function CategoryAdmin({ user }: { user: UserResponse }) {
    const isAdmin = user.role === "Admin";
    const [categories, setCategories] = useState<CategoryResponse[]>([]);
    const [newName, setNewName] = useState("");
    const [editingId, setEditingId] = useState<string | null>(null);
    const [editingName, setEditingName] = useState("");
    const [error, setError] = useState("");

    // Henter alle kategorier fra API'et
    async function load() {
        const res = await api.categories.categoriesGetAll();
        setCategories(res.data);
    }

    useEffect(() => {
        load().catch((e) => setError(errorMessage(e)));
    }, []);

    // Kører en handling, genindlæser listen, og viser en evt. fejl
    async function run(action: () => Promise<unknown>) {
        setError("");
        try {
            await action();
            await load();
        } catch (e) {
            setError(errorMessage(e));
        }
    }

    function handleCreate(e: FormEvent) {
        e.preventDefault();
        run(async () => {
            await api.categories.categoriesCreate({ name: newName });
            setNewName("");
        });
    }

    function handleRename(id: string) {
        run(async () => {
            await api.categories.categoriesUpdate({ id, name: editingName });
            setEditingId(null);
        });
    }

    function handleDelete(id: string) {
        run(() => api.categories.categoriesDelete({ id }));
    }

    return (
        <div>
            <h2>Categories</h2>
            {error && <p style={{ color: "red" }}>{error}</p>}

            <ul>
                {categories.map((c) => (
                    <li key={c.id}>
                        {editingId === c.id ? (
                            <>
                                <input value={editingName} onChange={(e) => setEditingName(e.target.value)} />
                                <button onClick={() => handleRename(c.id)}>Save</button>
                                <button onClick={() => setEditingId(null)}>Cancel</button>
                            </>
                        ) : (
                            <>
                                {c.name}{" "}
                                {isAdmin && (
                                    <>
                                        <button onClick={() => { setEditingId(c.id); setEditingName(c.name); }}>Rename</button>
                                        <button onClick={() => handleDelete(c.id)}>Delete</button>
                                    </>
                                )}
                            </>
                        )}
                    </li>
                ))}
            </ul>

            {isAdmin ? (
                <form onSubmit={handleCreate}>
                    <input placeholder="New category name" value={newName} onChange={(e) => setNewName(e.target.value)} />
                    <button type="submit">Add category</button>
                </form>
            ) : (
                <p>Only admins can manage categories.</p>
            )}
        </div>
    );
}