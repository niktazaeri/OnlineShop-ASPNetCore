import { fetchWithAuth } from "../../accountScripts/checkRefreshToken.js";

document.addEventListener("DOMContentLoaded", async () => {
    await loadOrders();
});

// Fetch and render orders history
async function loadOrders() {
    const container = document.getElementById("ordersContainer");
    container.innerHTML = "<p class='text-muted'>Loading orders...</p>";

    try {
        const res = await fetchWithAuth("/api/order-history");
        if (!res.ok) {
            container.innerHTML = "<p class='text-danger'>Failed to load orders.</p>";
            return;
        }

        const orders = await res.json();
        renderOrdersTable(orders);
    } catch (err) {
        console.error(err);
        container.innerHTML = "<p class='text-danger'>Unexpected error occurred.</p>";
    }
}

// Render orders table
function renderOrdersTable(orders) {
    const container = document.getElementById("ordersContainer");
    if (!orders.length) {
        container.innerHTML = "<p class='text-muted'>No orders found.</p>";
        return;
    }

    const table = document.createElement("table");
    table.className = "table table-bordered";

    table.innerHTML = `
        <thead class="table-light">
            <tr>
                <th>Created At</th>
                <th>Address ID</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            ${orders.map(o => `
                <tr>
                    <td>${new Date(o.createdAt).toLocaleString()}</td>
                    <td>${o.addressId}</td>
                    <td>
                        <button class="btn btn-sm btn-primary details-btn" data-id="${o.id}">Details</button>
                    </td>
                </tr>
            `).join("")}
        </tbody>
    `;

    container.innerHTML = "";
    container.appendChild(table);

    // Add click listeners for Details buttons
    document.querySelectorAll(".details-btn").forEach(btn => {
        btn.addEventListener("click", (e) => showOrderDetails(e.target.dataset.id));
    });
}

// Placeholder for showing order details
function showOrderDetails(orderId) {
    // Here you can either open a modal or navigate to a details page
    alert(`Show details for Order ID: ${orderId}`);
}
