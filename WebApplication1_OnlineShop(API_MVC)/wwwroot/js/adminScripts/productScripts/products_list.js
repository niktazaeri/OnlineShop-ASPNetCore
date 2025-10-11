document.addEventListener("DOMContentLoaded", function () {
    const container = document.getElementById("products-container");
    const categoryDropdown = document.getElementById("categoryFilter");

    const categoryCache = {};

    // Load categories
    fetch("/api/categories")
        .then(res => res.json())
        .then(categories => {
            const categoryMap = {};
            const roots = [];

            categories.forEach(cat => categoryMap[cat.id] = { ...cat, children: [] });
            categories.forEach(cat => {
                const parentId = cat.categoryParentId;
                if (parentId && categoryMap[parentId]) {
                    categoryMap[parentId].children.push(categoryMap[cat.id]);
                } else {
                    roots.push(categoryMap[cat.id]);
                }
            });

            function buildOptions(categories, depth = 0) {
                categories.forEach(cat => {
                    const option = document.createElement("option");
                    option.value = cat.id;
                    option.textContent = `${"—".repeat(depth)} ${cat.name}`;
                    categoryDropdown.appendChild(option);
                    if (cat.children.length > 0) {
                        buildOptions(cat.children, depth + 1);
                    }
                });
            }

            buildOptions(roots);
        })
        .catch(err => console.error("Failed to load categories:", err));

    // Load products
    loadProducts();

    categoryDropdown.addEventListener("change", function () {
        const selectedCategoryId = this.value || null;
        loadProducts(selectedCategoryId);
    });

    async function loadProducts(categoryId = null) {
        container.innerHTML = "";

        const url = categoryId ? `/api/categories/${categoryId}/products` : "/api/products";

        try {
            const res = await fetch(url);
            if (!res.ok) throw new Error("Failed to fetch products");

            const data = await res.json();
            const { products, imagePaths } = data;

            if (!products || !products.length) {
                container.innerHTML = "<p>No products found.</p>";
                return;
            }

            for (let i = 0; i < products.length; i++) {
                const product = products[i];
                const imagePath = `/images/${imagePaths[i]}/${product.pictureName}`;

                console.log(imagePaths[2]);

                try {
                    let productCategoryId = product.categoryId || categoryId;

                    if (!productCategoryId) {
                        container.appendChild(createProductCard(product, "Uncategorized", imagePath));
                        continue;
                    }

                    if (!categoryCache[productCategoryId]) {
                        const catRes = await fetch(`/api/categories/${productCategoryId}`);
                        if (!catRes.ok) throw new Error("Failed to fetch category");
                        categoryCache[productCategoryId] = await catRes.json();
                    }

                    const categoryData = categoryCache[productCategoryId];
                    const breadcrumbNames = [categoryData.category.name];

                    const parentIds = categoryData.categoryParentId ?? [];
                    if (Array.isArray(parentIds)) {
                        for (const id of parentIds) {
                            if (!categoryCache[id]) {
                                const parentRes = await fetch(`/api/categories/${id}`);
                                if (!parentRes.ok) throw new Error("Failed to fetch parent category");
                                categoryCache[id] = await parentRes.json();
                            }
                            breadcrumbNames.unshift(categoryCache[id].category.name);
                        }
                    }

                    const breadcrumb = breadcrumbNames.join(" > ");
                    container.appendChild(createProductCard(product, breadcrumb, imagePath));

                } catch (err) {
                    console.error(`Error loading product ${product.id}:`, err);
                }
            }
        } catch (err) {
            console.error("Error loading products:", err);
            container.innerHTML = `<p class="text-danger">Could not load products.</p>`;
        }
    }

    function createProductCard(product, categoryBreadcrumb, imagePath) {
        const card = document.createElement("div");
        card.className = "col-md-4 mb-3";

        card.innerHTML = `
            <div class="card shadow-sm h-100">
                <img src="${imagePath}" class="card-img-top" alt="${product.name}">
                <div class="card-body">
                    <h5 class="card-title">${product.name}</h5>
                    <p class="card-text">${product.description || "No description"}</p>
                    <p><strong>Price:</strong> $${product.price}</p>
                    <p><strong>Quantity:</strong> ${product.quantity}</p>
                    <p class="text-muted"><em>Category:</em> ${categoryBreadcrumb}</p>
                    <div class="d-flex justify-content-between mt-3">
                        <button class="btn btn-sm btn-info btn-details">Details</button>
                        <button class="btn btn-sm btn-warning btn-edit">Edit</button>
                        <button class="btn btn-sm btn-danger btn-delete">Delete</button>
                    </div>
                </div>
            </div>
        `;

        card.querySelector(".btn-details").addEventListener("click", () => {
            window.location.href = `/products/${product.id}`;
        });

        card.querySelector(".btn-edit").addEventListener("click", () => {
            window.location.href = `/products/edit/${product.id}`;
        });

        card.querySelector(".btn-delete").addEventListener("click", () => {
            window.location.href = `/products/delete/${product.id}`;
        });

        return card;
    }
});
