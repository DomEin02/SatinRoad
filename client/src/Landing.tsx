import { useEffect, useState } from "react";
import { api, errorMessage } from "./apiClient";
import type { FeaturedVendorResponse, ProductResponse, UserResponse } from "./Api";

export function Landing({ user, refreshKey, onPurchased }: {
    user: UserResponse | null;
    refreshKey: number;
    onPurchased: () => void;
}) {
    const [featured, setFeatured] = useState<FeaturedVendorResponse[]>([]);
    const [products, setProducts] = useState<ProductResponse[]>([]);
    const [error, setError] = useState("");
    const [quantities, setQuantities] = useState<Record<string, string>>({});
    const [message, setMessage] = useState("");
    const [raided, setRaided] = useState(false);
    
    //Ryd beskeder, når nogen logger ind eller ud
    useEffect(() => {
        setMessage("");
        setRaided(false);
        setError("");
    }, [user?.id]);

    useEffect(() => {
        async function load() {
            const [f, p] = await Promise.all([
                api.vendors.vendorsGetFeatured(),
                api.products.productsGetAll(),
            ]);
            setFeatured(f.data);
            setProducts(p.data);
        }
        load().catch((e) => setError(errorMessage(e)));
    }, [refreshKey]);

    const featuredIds = new Set(featured.map((v) => v.vendorId));

    const sorted = [...products].sort(
        (a, b) => Number(featuredIds.has(b.vendorId)) - Number(featuredIds.has(a.vendorId))
    );

    // Man kan købe, hvis man er logget ind, produktet ikke er ens eget og der er lager
    function canBuy(p: ProductResponse) {
        return user !== null && p.vendorId !== user.id && p.stockCount > 0;
    }

    async function buy(product: ProductResponse) {
        setMessage("");
        setRaided(false);
        setError("");
        const quantity = Number(quantities[product.id] ?? "1");
        try {
            const res = await api.orders.ordersBuy({ productId: product.id, quantity });
            const order = res.data;

            let text = `Bought ${order.quantity} x ${product.name} for ${order.totalPriceDkk} DKK.`;
            if (order.discountApplied) text += " 20% loyalty discount applied!";
            if (order.vendorWasRaided) {
                text += " FBI RAID! The vendor has been shut down and all their products are gone.";
            }
            setMessage(text);
            setRaided(order.vendorWasRaided);
            onPurchased(); // får listen og "My orders" til at hente data igen
        } catch (err) {
            setError(errorMessage(err));
        }
    }

    return (
        <div>
            {error && <p style={{ color: "red" }}>{error}</p>}
            {message && <p style={{ color: raided ? "red" : "green" }}>{message}</p>}

            <h2>Featured vendors</h2>
            {featured.length === 0 ? (
                <p>No featured vendors yet.</p>
            ) : (
                <ul>
                    {featured.map((v) => (
                        <li key={v.vendorId}>
                            ★ {v.username} ({v.orderCount} orders)
                        </li>
                    ))}
                </ul>
            )}

            <h2>All listings</h2>
            {sorted.length === 0 ? (
                <p>No listings yet.</p>
            ) : (
                <ul>
                    {sorted.map((p) => (
                        <li key={p.id}>
                            {featuredIds.has(p.vendorId) && "★ "}
                            {p.name} - {p.priceDkk} DKK (stock: {p.stockCount}){" "}
                            {canBuy(p) && (
                                <>
                                    <input
                                        type="number"
                                        min="1"
                                        max={p.stockCount}
                                        style={{ width: 60 }}
                                        value={quantities[p.id] ?? "1"}
                                        onChange={(e) => setQuantities({ ...quantities, [p.id]: e.target.value })}
                                    />
                                    <button onClick={() => buy(p)}>Buy</button>
                                </>
                            )}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}