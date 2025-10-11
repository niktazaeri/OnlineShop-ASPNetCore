import { fetchWithAuth } from "../../accountScripts/checkRefreshToken.js";

document.addEventListener("DOMContentLoaded", function () {
    const categoryId = getCategoryIdFromUrl();
    const nameInput = document.getElementById("categoryName");
    const btnDelete = document.getElementById("btnDelete");

    // Load category details (public GET)
    fetch(`/api/categories/${categoryId}`)
        .then(res => {
            if (!res.ok) throw new Error("Could not fetch category");
            return res.json();
        })
        .then(data => {
            nameInput.value = data.category.name;
            document.getElementById("categoryId").value = data.category.id;
        })
        .catch(err => {
            console.error(err);
            alert("Failed to load category details.");
        });

    // Delete category (requires Admin JWT)
    btnDelete.addEventListener("click", async () => {
        if (!confirm("This will permanently delete the category. Continue?")) return;

        try {
            const res = await fetchWithAuth(`/api/categories/delete/${categoryId}`, {
                method: "DELETE"
            });

            if (!res.ok) {
                const msg = await res.text();
                throw new Error(msg || "Delete failed");
            }

            alert("Category deleted successfully.");
            window.location.href = "/categories";

        } catch (err) {
            console.error(err);
            alert("Failed to delete category: " + err.message);
        }
    });

    function getCategoryIdFromUrl() {
        const parts = window.location.pathname.split("/");
        return parts[parts.length - 1];
    }
});
