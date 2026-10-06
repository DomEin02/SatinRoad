import { useEffect, useState } from "react";
import { api, errorMessage } from "./apiClient";
import type { FeaturedVendorResponse, ProductResponse } from "./Api";

export function Landing() {
    const [featured, setFeatured] = useState<FeaturedVendorResponse[]>([]);
    const [products, setProducts] = useState<ProductResponse[]>([]);
    const [error, setError] = useState("");
    
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
    }, []);
    
    const featuredIds = new Set(featured.map((v) => v.vendorId));
    
    const sorted = [...products].sort(
        (a, b) => Number(featuredIds.has(b.vendorId)) - Number(featuredIds.has(a.vendorId))
    );
    
    return (
        <div>
            {error && <p style={{ color: "red" }}>{error}</p>}
            
            <h2>Featured vendors</h2>
            {featured.length === 0 ? (
                <p>No featured vendors yet.</p>
            ) : (
                <ul>
                    {featured.map((v) =>(
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
                            {p.name} - {p.priceDkk} DKK (stock: {p.stockCount})
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}