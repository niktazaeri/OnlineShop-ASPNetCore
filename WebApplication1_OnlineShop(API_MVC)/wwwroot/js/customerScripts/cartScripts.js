export { updateCartCount, addToCart, changeQuantity, removeItem, emptyCart, loadCart };

// Load cart when page is ready
document.addEventListener("DOMContentLoaded", loadCart);

//  Load and display cart contents
async function loadCart() {
    const container = document.getElementById("cartContent");
    if (!container) return;

    container.innerHTML = `
        <div class="card-body text-center py-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-3 text-muted">Loading your cart...</p>
        </div>`;

    try {
        const res = await fetch("/api/cart");
        if (!res.ok) throw new Error("Failed to load cart");
        const cart = await res.json();

        if (!cart || !cart.cartItems || cart.cartItems.length === 0) {
            container.innerHTML = `
                <div class="text-center py-5">
                    <h5 class="text-muted">Your cart is empty 🛍️</h5>
                    <a href="/Home" class="btn btn-outline-primary mt-3">
                        Browse Products
                    </a>
                </div>`;
            updateCartCount(0);
            return;
        }

        let totalItems = 0;
        let itemsHtml = cart.cartItems.map(item => {
            totalItems += item.quantity;
            return `
                <tr>
                    <td>${item.name}</td>
                    <td>
                        <div class="btn-group" role="group">
                            <button class="btn btn-outline-secondary btn-sm"
                                    onclick="changeQuantity(${item.productId}, 'decrease')">-</button>
                            <span class="px-3" id="qty-${item.productId}">${item.quantity}</span>
                            <button class="btn btn-outline-secondary btn-sm"
                                    onclick="changeQuantity(${item.productId}, 'increase')">+</button>
                        </div>
                    </td>
                    <td>
                        <button class="btn btn-outline-danger btn-sm"
                                onclick="removeItem(${item.id})">
                            <i class="bi bi-trash"></i> Remove
                        </button>
                    </td>
                </tr>`;
        }).join("");

        container.innerHTML = `
            <div class="table-responsive">
                <table class="table table-striped align-middle text-center mb-0">
                    <thead class="table-light">
                        <tr>
                            <th>Product</th>
                            <th>Quantity</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>${itemsHtml}</tbody>
                </table>
            </div>
            <div class="d-flex justify-content-between align-items-center p-3 border-top mt-3">
                <h5 class="m-0">Total Items:
                    <span class="text-primary fw-bold">${totalItems}</span>
                </h5>
                <button class="btn btn-outline-danger" onclick="emptyCart(${cart.id})">
                    <i class="bi bi-x-circle"></i> Empty Cart
                </button>
            </div>`;

        updateCartCount(cart.cartItems.length);

    } catch (err) {
        console.error(err);
        container.innerHTML = `
            <div class="alert alert-danger m-0">
                Failed to load your cart. Please try again later.
            </div>`;
    }
}

//  Add product to cart
async function addToCart(productId, productName , productPrice) {
    try {
        const res = await fetch("/api/cart/add-to-cart", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name: productName, quantity: 1, productId: productId , price: productPrice  })
        });

        if (res.ok) {
            await loadCart();
            updateCartCount();
            showQuantityControls(productId, 1);
        } else {
            alert("Failed to add item to cart.");
        }
    } catch (err) {
        console.error(err);
        alert("An error occurred while adding the item.");
    }
}

function showQuantityControls(productId, qty) {
    const container = document.getElementById(`cart-controls-${productId}`);
    if (!container) return;

    container.innerHTML = `
        <div class="d-flex justify-content-center align-items-center">
            <button class="btn btn-outline-secondary btn-sm"
                    onclick="changeQuantity(${productId}, 'decrease')">-</button>
            <span class="mx-2" id="qty-${productId}">${qty}</span>
            <button class="btn btn-outline-secondary btn-sm"
                    onclick="changeQuantity(${productId}, 'increase')">+</button>
        </div>`;
}


// Change quantity (increase/decrease)
async function changeQuantity(itemId, action) {
    let endpoint = action === 'increase'
        ? `/api/cart/increase-item/${itemId}`
        : `/api/cart/decrease-item/${itemId}`;

    try {
        const res = await fetch(endpoint, { method: "POST" });
        if (res.ok) {
            await updateCartCount();

            const qtyElement = document.getElementById(`qty-${itemId}`);
            if (qtyElement) {
                let currentQty = parseInt(qtyElement.textContent);
                if (action === 'increase') currentQty++;
                else if (action === 'decrease') currentQty--;

                if (currentQty <= 0) {
                    const container = document.getElementById(`cart-controls-${itemId}`);
                    if (container) {
                        container.innerHTML = `
                            <button class="btn btn-outline-primary btn-sm"
                                    onclick="addToCart(${itemId}, '', 0)">
                                Add to Cart
                            </button>`;
                    }
                } else {
                    qtyElement.textContent = currentQty;
                }
            }

            const cartContent = document.getElementById("cartContent");
            if (cartContent) await loadCart();

        } else {
            alert("Failed to update item quantity.");
        }
    } catch (err) {
        console.error(err);
        alert("Error updating item quantity.");
    }
}


//  Remove single item
async function removeItem(itemId) {
    if (!confirm("Remove this item from your cart?")) return;
    try {
        const res = await fetch(`/api/cart/remove-item/${itemId}`, { method: "POST" });
        if (res.ok) {
            await loadCart();
        } else {
            alert("Failed to remove item.");
        }
    } catch (err) {
        console.error(err);
        alert("Error removing item.");
    }
}

//  Empty the entire cart
async function emptyCart(cartId) {
    if (!confirm("Are you sure you want to empty your cart?")) return;
    try {
        const res = await fetch(`/api/cart/${cartId}/empty`, { method: "POST" });
        if (res.ok) {
            await loadCart();
        } else {
            alert("Failed to empty cart.");
        }
    } catch (err) {
        console.error(err);
        alert("Error emptying cart.");
    }
}

//  Update the cart count badge in navbar
async function updateCartCount(count) {
    const badge = document.getElementById("cartCountBadge");
    if (!badge) return;

    if (typeof count === "number") {
        badge.textContent = count;
        return;
    }

    try {
        const res = await fetch("/api/cart");
        if (res.ok) {
            const cart = await res.json();
            const total = cart?.cartItems?.length || 0;
            badge.textContent = total;
        } else {
            badge.textContent = "0";
        }
    } catch {
        badge.textContent = "0";
    }
}

window.addToCart = addToCart;
window.changeQuantity = changeQuantity;
window.removeItem = removeItem;
window.emptyCart = emptyCart;
window.loadCart = loadCart;