import { fetchWithAuth } from "../../accountScripts/checkRefreshToken.js";

document.addEventListener("DOMContentLoaded", () => {
    const productId = getProductIdFromUrl();
    let categoryId = null;

    if (!productId) {
        alert("Product ID not found in URL.");
        return;
    }

    loadProduct(productId);

    document.body.insertAdjacentHTML("beforeend", `
        <div class="alert alert-warning mt-4">
            <p>Are you sure you want to delete this product?</p>
            <button id="confirmDeleteBtn" class="btn btn-danger">Yes, Delete</button>
            <a href="#" id="cancelBtn" class="btn btn-secondary ms-2">Cancel</a>
        </div>
    `);

    // Delete (Admin only)
    document.getElementById("confirmDeleteBtn").addEventListener("click", async () => {
        try {
            const response = await fetchWithAuth(`/api/products/delete/${productId}`, {
                method: "DELETE"
            });

            if (response.ok || response.status === 204) {
                alert("Product deleted successfully.");
                window.location.href = `/categories/${categoryId ?? ""}`;
            } else {
                const error = await response.text();
                console.error("Error:", error);
                alert("Failed to delete product.");
            }
        } catch (error) {
            console.error("Fetch error:", error);
            alert("Something went wrong.");
        }
    });

    document.getElementById("cancelBtn").addEventListener("click", (e) => {
        e.preventDefault();
        window.history.back();
    });

    // Product details (public GET)
    async function loadProduct(id) {
        try {
            const response = await fetch(`/api/products/${id}`);
            if (!response.ok) throw new Error("Failed to fetch product");

            const result = await response.json();
            const product = result.product;
            categoryId = result.category_id;

            document.body.insertAdjacentHTML("afterbegin", `
                <div class="card p-3 mt-3">
                    <h4>Product Info</h4>
                    <p><strong>Name:</strong> ${product.name}</p>
                    <p><strong>Description:</strong> ${product.description}</p>
                    <p><strong>Price:</strong> $${product.price}</p>
                    <p><strong>Quantity:</strong> ${product.quantity}</p>
                    <p><strong>Category:</strong> ${result.category_name}</p>
                </div>
            `);
        } catch (error) {
            console.error("Error loading product:", error);
            alert("Failed to load product details.");
        }
    }

    function getProductIdFromUrl() {
        const pathParts = window.location.pathname.split("/");
        const deleteIndex = pathParts.findIndex(p => p.toLowerCase() === "delete");
        return (deleteIndex !== -1 && pathParts.length > deleteIndex + 1)
            ? pathParts[deleteIndex + 1]
            : null;
    }
});
