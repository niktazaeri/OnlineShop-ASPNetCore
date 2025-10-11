document.addEventListener("DOMContentLoaded", function () {
    const pathParts = window.location.pathname.split('/');
    const categoriesIndex = pathParts.findIndex(part => part.toLowerCase() === 'categories');

    let categoryId = null;
    if (categoriesIndex !== -1 && pathParts.length > categoriesIndex + 1) {
        categoryId = pathParts[categoriesIndex + 1];
    }

    if (!categoryId || isNaN(categoryId)) {
        document.body.insertAdjacentHTML("beforeend", "<p>No category selected.</p>");
        return;
    }

    // Set href for Create New Product link
    const createLink = document.getElementById("createProductLink");
    if (createLink) {
        createLink.href = `/products/create/`;
    }

    const apiUrl = `/api/categories/${categoryId}/products`;

    fetch(apiUrl)
        .then(response => {
            if (!response.ok) {
                throw new Error("Failed to fetch products.");
            }
            return response.json();
        })
        .then(data => {
            const products = data.products || [];
            const imagePaths = data.imagePaths || [];
            renderProducts(products, imagePaths);
        })
        .catch(error => {
            console.error("Error:", error);
            document.body.insertAdjacentHTML("beforeend", "<p>Error loading products.</p>");
        });

    function renderProducts(products, imagePaths) {
        if (!Array.isArray(products) || products.length === 0) {
            document.body.insertAdjacentHTML("beforeend", "<p>No products found for this category.</p>");
            return;
        }

        const table = document.createElement("table");
        table.className = "table table-bordered";
        table.innerHTML = `
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Image</th>
                    <th>Price ($)</th>
                    <th>Quantity</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
                ${products.map((p, i) => {
            const imgPath = imagePaths[i]
                ? `/images/${imagePaths[i]}/${p.pictureName ?? p.PictureName}`
                : `/images/${p.pictureName ?? p.PictureName}`;

            return `
                        <tr>
                            <td>${p.name ?? p.Name}</td>
                            <td><img src="${imgPath}" alt="${p.name ?? p.Name}" style="width:80px; height:auto;"></td>
                            <td>${p.price ?? p.Price}</td>
                            <td>${p.quantity ?? p.Quantity}</td>
                            <td>
                                <a href="/products/${p.id ?? p.Id}" class="btn btn-sm btn-info">Details</a>
                                <a href="/products/edit/${p.id ?? p.Id}" class="btn btn-sm btn-warning">Edit</a>
                                <a href="/products/delete/${p.id ?? p.Id}" class="btn btn-sm btn-danger ms-2">Delete</a>
                            </td>
                        </tr>
                    `;
        }).join('')}
            </tbody>
        `;

        const container = document.getElementById("products-list");
        container.innerHTML = ""; // Clear if re-rendered
        container.appendChild(table);
    }
});
