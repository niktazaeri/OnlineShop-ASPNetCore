import { fetchWithAuth } from "../../accountScripts/checkRefreshToken.js";

document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("product-form");
    const categorySelect = document.querySelector("select[name='categoryId']");

    // Load categories (public GET)
    async function loadCategories() {
        try {
            const response = await fetch("/api/categories");
            if (!response.ok) throw new Error("Failed to fetch categories");

            const categories = await response.json();

            categories.forEach(category => {
                const option = document.createElement("option");
                option.value = category.id;
                option.textContent = category.name;
                categorySelect.appendChild(option);
            });
        } catch (error) {
            console.error("Error loading categories:", error);
            alert("Failed to load categories.");
        }
    }

    loadCategories();

    // Create product (Admin only)
    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        const name = document.querySelector("input[name='name']").value.trim();
        const description = document.querySelector("input[name='description']").value.trim();
        const price = parseFloat(document.querySelector("input[name='price']").value);
        const quantity = parseInt(document.querySelector("input[name='quantity']").value);
        const categoryId = parseInt(categorySelect.value);

        if (!name || isNaN(price) || isNaN(quantity) || isNaN(categoryId)) {
            alert("Please fill in all fields correctly.");
            return;
        }

        const product = { name, description, price, quantity, categoryId };

        try {
            const response = await fetchWithAuth("/api/products/create", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(product)
            });

            if (response.ok || response.status === 201) {
                alert("Product created successfully!");
                window.location.href = `/categories/${categoryId}`;
            } else {
                const error = await response.text();
                console.error("Error:", error);
                alert("Failed to create product.");
            }
        } catch (error) {
            console.error("Fetch error:", error);
            alert("Something went wrong.");
        }
    });
});
