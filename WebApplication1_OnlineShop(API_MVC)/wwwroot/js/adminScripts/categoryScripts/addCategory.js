import { fetchWithAuth } from "../../accountScripts/checkRefreshToken.js";

document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("category-form");
    const nameInput = document.getElementById("name");
    const parentSelect = document.getElementById("parentCategory");
    const errorSpan = document.getElementById("name-error");

    // Load categories (public, no JWT needed)
    fetch("/api/categories")
        .then(res => res.json())
        .then(data => {
            const tree = buildCategoryTree(data);
            populateDropdown(tree, parentSelect);
        })
        .catch(err => {
            console.error("Error loading categories:", err);
        });

    // Submit new category (requires Admin JWT)
    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        const name = nameInput.value.trim();
        const parentCategoryId = parentSelect.value || null;
        errorSpan.textContent = "";

        if (!name) {
            errorSpan.textContent = "Please enter a valid category name.";
            return;
        }

        const payload = {
            name,
            categoryParentId: parentCategoryId ? parseInt(parentCategoryId) : null
        };

        try {
            const response = await fetchWithAuth("/api/categories/create", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            if (response.ok) {
                alert("Category created successfully!");
                window.location.href = "/categories/";
            } else {
                const errorText = await response.text();
                console.error("Error:", errorText);
                alert("Failed to create category.");
            }
        } catch (error) {
            console.error("Fetch error:", error);
            alert("Something went wrong while creating the category.");
        }
    });
});

// Utility to build a tree from flat category list
function buildCategoryTree(categories) {
    const map = {};
    const roots = [];

    categories.forEach(cat => {
        map[cat.id] = { ...cat, children: [] };
    });

    categories.forEach(cat => {
        if (cat.categoryParentId && map[cat.categoryParentId]) {
            map[cat.categoryParentId].children.push(map[cat.id]);
        } else {
            roots.push(map[cat.id]);
        }
    });

    return roots;
}

// Populate dropdown with hierarchy using indentation
function populateDropdown(nodes, dropdown, depth = 0) {
    nodes.forEach(node => {
        const option = document.createElement("option");
        option.value = node.id;
        option.textContent = `${'— '.repeat(depth)}${node.name}`;
        dropdown.appendChild(option);

        if (node.children.length > 0) {
            populateDropdown(node.children, dropdown, depth + 1);
        }
    });
}
