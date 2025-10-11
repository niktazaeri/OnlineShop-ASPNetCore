import { fetchWithAuth } from "../../accountScripts/checkRefreshToken.js";

document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("product-form");
    const categorySelect = document.querySelector("select[name='CategoryId']");
    const productId = getProductIdFromUrl();

    if (!productId) {
        alert("Product ID not found in URL.");
        return;
    }

    // Load categories (public GET)
    loadCategories().then(() => loadProduct(productId));

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
    
    // Load product (public GET)
    async function loadProduct(id) {
        try {
            const response = await fetch(`/api/products/${id}`);
            if (!response.ok) throw new Error("Failed to fetch product");

            const result = await response.json();
            const product = result.product;
            const categoryName = result.categoryName;

            document.querySelector("input[name='Name']").value = product.name;
            document.querySelector("input[name='Description']").value = product.description;
            document.querySelector("input[name='Price']").value = product.price;
            document.querySelector("input[name='Quantity']").value = product.quantity;

            // Set selected category
            const selectedOption = [...categorySelect.options].find(opt => opt.textContent === categoryName);
            if (selectedOption) selectedOption.selected = true;
        } catch (error) {
            console.error("Error loading product:", error);
            alert("Failed to load product.");
        }
    }

    // Update (Admin only) with FormData for image upload
    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        const formData = new FormData(form); // includes file + other fields
        for (var pair of formData.entries()) {
            console.log(pair);
        }
        try {
            const response = await fetchWithAuth(`/api/products/edit/${productId}`, {
                method: "PUT",
                body: formData // browser auto-sets Content-Type
            });

            if (response.ok) {
                alert("Product updated successfully!");
                const categoryId = formData.get("CategoryId");
                window.location.href = `/categories/${categoryId}`;
            } else {
                const error = await response.text();
                console.error("Error:", error);
                alert("Failed to update product.");
            }
        } catch (error) {
            console.error("Fetch error:", error);
            alert("Something went wrong.");
        }
    });

    function getProductIdFromUrl() {
        const pathParts = window.location.pathname.split("/");
        const editIndex = pathParts.findIndex(p => p.toLowerCase() === "edit");
        return (editIndex !== -1 && pathParts.length > editIndex + 1)
            ? pathParts[editIndex + 1]
            : null;
    }
});
