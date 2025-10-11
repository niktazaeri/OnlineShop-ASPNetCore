import { fetchWithAuth } from "../../accountScripts/checkRefreshToken.js";

document.addEventListener("DOMContentLoaded", function () {
    const pathParts = window.location.pathname.split("/");
    const categoryId = pathParts[pathParts.length - 1];

    const nameInput = document.getElementById("categoryName");
    const btnUpdate = document.getElementById("btnUpdate");

    let currentCategoryParentId = null;

    // Load category details (public GET)
    fetch(`/api/categories/${categoryId}`)
        .then(res => {
            if (!res.ok) throw new Error("Failed to fetch category");
            return res.json();
        })
        .then(data => {
            nameInput.value = data.category.name || "";
            currentCategoryParentId = data.category.categoryParentId ?? null;
        })
        .catch(err => {
            console.error("Error loading category:", err);
            alert("Could not load category information.");
        });

    // Update category (requires Admin JWT)
    btnUpdate.addEventListener("click", async function () {
        const name = nameInput.value.trim();

        if (!name) {
            alert("Please enter a category name.");
            return;
        }

        try {
            const response = await fetchWithAuth(`/api/categories/edit/${categoryId}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    name: name,
                    categoryParentId: currentCategoryParentId
                })
            });

            if (!response.ok) {
                const msg = await response.text();
                throw new Error(msg || "Update failed");
            }

            alert("Category updated successfully!");
            window.location.href = "/categories";

        } catch (err) {
            console.error("Error:", err);
            alert("Failed to update category: " + err.message);
        }
    });
});
