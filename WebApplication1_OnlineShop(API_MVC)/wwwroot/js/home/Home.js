export { }

document.addEventListener("DOMContentLoaded", async () => {
    const urlParams = new URLSearchParams(window.location.search);
    const categoryId = urlParams.get("categoryId");

    window.addToCart = addToCart;
    window.changeQuantity = changeQuantity;

    // get cart information
    const cart = await fetchCart();

    if (categoryId) {
        loadCategoryProducts(categoryId, cart);
    } else {
        loadAllProducts(cart);
    }
});

async function fetchCart() {
    try {
        const res = await fetch("/api/cart");
        if (!res.ok) return null;
        return await res.json();
    } catch {
        console.warn("⚠️ Failed to load cart");
        return null;
    }
}

function loadAllProducts(cart) {
    fetch('/api/products')
        .then(res => res.json())
        .then(data => renderProducts(data.products, data.imagePaths, cart));
}

function loadCategoryProducts(categoryId, cart) {
    fetch(`/api/categories/${categoryId}/products`)
        .then(res => res.json())
        .then(data => renderProducts(data.products, data.imagePaths, cart));
}

function renderProducts(products, imagePaths = [], cart = null) {
    const container = document.getElementById('productsList');
    if (!container) {
        console.warn("⚠️ productsList element not found. Probably not on Home page.");
        return;
    }

    let html = '';

    if (!Array.isArray(products) || products.length === 0) {
        html = `
            <div class="col-12">
                <div class="alert alert-warning text-center">
                    No products found in this category.
                </div>
            </div>`;
    } else {
        const cartItems = cart?.cartItems || [];

        products.forEach((p, i) => {
            const imgPath = imagePaths[i]
                ? `/images/${imagePaths[i]}/${p.pictureName}`
                : `/images/${p.pictureName}`;

            // if product is in cart
            const existingItem = cartItems.find(ci => ci.productId === p.id);
            let cartControlHtml = '';

            if (existingItem) {
                // show _ , +
                cartControlHtml = `
                    <div class="d-flex justify-content-center align-items-center" id="cart-controls-${p.id}">
                        <button class="btn btn-outline-secondary btn-sm"
                                onclick="changeQuantity(${p.id}, 'decrease')">-</button>
                        <span class="mx-2" id="qty-${p.id}">${existingItem.quantity}</span>
                        <button class="btn btn-outline-secondary btn-sm"
                                onclick="changeQuantity(${p.id}, 'increase')">+</button>
                    </div>`;
            } else {
                // show add to cart btn
                cartControlHtml = `
                    <div id="cart-controls-${p.id}">
                        <button class="btn btn-outline-primary btn-sm"
                                onclick="addToCart(${p.id}, '${p.name}', ${p.price})">
                            Add to Cart
                        </button>
                    </div>`;
            }

            html += `
                <div class="col-md-3 mb-3">
                    <div class="card h-100 border-primary shadow text-center p-2">
                        <img src="${imgPath}" class="card-img-top" alt="${p.name}">
                        <div class="card-body">
                            <h5 class="card-title">${p.name}</h5>
                            <p class="card-text">Price: $${p.price}</p>
                            <p class="card-text">Available: ${p.quantity}</p>
                            ${cartControlHtml}
                        </div>
                    </div>
                </div>`;
        });
    }

    container.innerHTML = html;
}
