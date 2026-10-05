import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { api, errorMessage } from "./apiClient";
import type { CategoryResponse, ProductResponse, UserResponse } from "./Api";

export function Listings({ user }: { user: UserResponse }) {
    const [categories, setCategories] = useState<CategoryResponse[]>([]);
    const [myProducts, setMyProducts] = useState<ProductResponse[]>([]);
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [price, setPrice] = useState("");
    const [stock, setStock] = useState("");
    const [categoryId, setCategoryId] = useState("");
    const [error, setError] = useState("");

    // Henter brugerens egne produkter
    async function loadProducts() {
        const res = await api.products.productsGetByVendor({ vendorId: user.id });
        setMyProducts(res.data);
    }

    // Når siden åbnes: hent kategorier (til dropdown) og egne produkter
    useEffect(() => {
        async function init() {
            const res = await api.categories.categoriesGetAll();
            setCategories(res.data);
            if (res.data[0]) setCategoryId(res.data[0].id);
            await loadProducts();
        }
        init().catch((e) => setError(errorMessage(e)));
    }, []);

    async function handleSubmit(e: FormEvent) {
        e.preventDefault();
        setError("");
        try {
            await api.products.productsCreate({
                name,
                description,
                priceDkk: Number(price),
                stockCount: Number(stock),
                categoryId,
                vendorId: user.id,
            });
            // Tøm formularen og opdatér listen
            setName("");
            setDescription("");
            setPrice("");
            setStock("");
            await loadProducts();
        } catch (err) {
            setError(errorMessage(err));
        }
    }

    return (
        <div>
            <h2>Create a listing</h2>
            <form onSubmit={handleSubmit}>
                <input placeholder="Name" value={name} onChange={(e) => setName(e.target.value)} required />
                <input placeholder="Description" value={description} onChange={(e) => setDescription(e.target.value)} />
                <input type="number" min="0" step="0.01" placeholder="Price (DKK)" value={price} onChange={(e) => setPrice(e.target.value)} required />
                <input type="number" min="0" step="1" placeholder="Stock" value={stock} onChange={(e) => setStock(e.target.value)} required />
                <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
                    {categories.map((c) => (
                        <option key={c.id} value={c.id}>{c.name}</option>
                    ))}
                </select>
                <button type="submit">Create listing</button>
            </form>
            {error && <p style={{ color: "red" }}>{error}</p>}

            <h2>My listings</h2>
            {myProducts.length === 0 ? (
                <p>You have no listings yet.</p>
            ) : (
                <ul>
                    {myProducts.map((p) => (
                        <li key={p.id}>
                            {p.name} - {p.priceDkk} DKK (stock: {p.stockCount})
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}