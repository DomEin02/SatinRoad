import { useEffect, useState } from "react";
import { api, errorMessage } from "./apiClient";
import type { OrderResponse, ProductResponse } from "./Api";

export function MyOrders({ refreshKey }: { refreshKey: number }) {
    const [orders, setOrders] = useState<OrderResponse[]>([]);
    const [products, setProducts] = useState<ProductResponse[]>([]);
    const [error, setError] = useState("");
    
    //Henter dine ordrer og produktlisten (til navne) når siden åbnes og ved refresh
    useEffect(() => {
        async function load() {
            const [o, p] = await Promise.all([
                api.orders.ordersGetMine(),
                api.products.productsGetAll(),
            ]);
            setOrders(o.data);
            setProducts(p.data);
        }
        load().catch((e) => setError(errorMessage(e)));
    }, [refreshKey]);
    
    //Producter der er slettet findes ikke længere
    function productName(productId: string) {
        return products.find((p) => p.id === productId)?.name ?? "removed product";
    }
    
    return (
        <div>
            <h2>My orders</h2>
            {error && <p style={{ color: "red" }}>{error}</p>}
            {orders.length === 0 ? (
                <p>You have not bought anything yet.</p>
            ) : (
                <ul>
                    {orders.map((o) => (
                        <li key={o.id}>
                            {new Date(o.createdAtUtc). toLocaleString()} - {o.quantity} x {productName(o.productId)} -{" "}
                            {o.totalPriceDkk} DKK{o.discountApplied && " (20% discount)"}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}