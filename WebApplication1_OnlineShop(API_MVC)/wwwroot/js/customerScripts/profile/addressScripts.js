import { fetchWithAuth } from "../../accountScripts/checkRefreshToken.js";

document.addEventListener("DOMContentLoaded", async () => {
    const addresses = await loadAddresses();
    renderAddressTable(addresses);

    // Hook up Create button
    document.getElementById("createAddressBtn").addEventListener("click", showCreateModal);
});

// Load all addresses
async function loadAddresses() {
    const res = await fetchWithAuth("/api/address");
    if (!res.ok) {
        console.error("Failed to load addresses");
        return [];
    }
    return await res.json();
}

// Render address table
function renderAddressTable(addresses) {
    const container = document.getElementById("addressTableContainer");
    container.innerHTML = "";

    if (!addresses.length) {
        container.innerHTML = `<p class="mt-3 text-muted">No addresses found. Add one to get started!</p>`;
        return;
    }

    const table = document.createElement("table");
    table.className = "table table-bordered mt-3";
    table.innerHTML = `
        <thead class="table-light">
            <tr>
                <th>Title</th>
                <th>Address</th>
                <th>City</th>
                <th>Zip Code</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            ${addresses.map(addr => `
                <tr>
                    <td>${addr.title}</td>
                    <td>${addr.addressText}</td>
                    <td>${addr.city}</td>
                    <td>${addr.zipCode}</td>
                    <td>
                        <button class="btn btn-sm btn-primary me-2 edit-btn" data-id="${addr.id}">Edit</button>
                        <button class="btn btn-sm btn-danger delete-btn" data-id="${addr.id}">Delete</button>
                    </td>
                </tr>`).join("")}
        </tbody>
    `;

    container.appendChild(table);

    // Event listeners
    document.querySelectorAll(".delete-btn").forEach(btn =>
        btn.addEventListener("click", handleDelete)
    );
    document.querySelectorAll(".edit-btn").forEach(btn =>
        btn.addEventListener("click", handleEdit)
    );
}

// Handle delete
async function handleDelete(e) {
    const id = e.target.dataset.id;
    if (!confirm("Are you sure you want to delete this address?")) return;

    const res = await fetchWithAuth(`/api/address/delete/${id}`, { method: "DELETE" });

    if (res.status === 204) {
        alert("Address deleted successfully.");
        const updated = await loadAddresses();
        renderAddressTable(updated);
    } else {
        alert("Failed to delete address.");
    }
}

// Handle edit
async function handleEdit(e) {
    const id = e.target.dataset.id;
    const res = await fetchWithAuth(`/api/address/${id}`);
    if (!res.ok) return alert("Failed to fetch address details.");
    const addr = await res.json();
    showEditModal(addr);
}

// Show Edit Modal
function showEditModal(addr) {
    const modalHtml = `
        <div class="modal fade" id="editModal" tabindex="-1">
          <div class="modal-dialog">
            <div class="modal-content">
              <div class="modal-header">
                <h5 class="modal-title">Edit Address</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
              </div>

              <div class="modal-body">
                <form id="editForm" novalidate>
                    <div class="mb-3">
                        <label class="form-label">Title</label>
                        <input class="form-control" id="editTitle" value="${addr.title}" required>
                        <div class="text-danger small mt-1" id="editErrorTitle"></div>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Address</label>
                        <input class="form-control" id="editAddressText" value="${addr.addressText}" required>
                        <div class="text-danger small mt-1" id="editErrorAddressText"></div>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">City</label>
                        <input class="form-control" id="editCity" value="${addr.city}" required>
                        <div class="text-danger small mt-1" id="editErrorCity"></div>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Zip Code</label>
                        <input class="form-control" id="editZip" value="${addr.zipCode}">
                        <div class="text-danger small mt-1" id="editErrorZip"></div>
                    </div>
                </form>
                <div id="editGeneralError" class="text-danger small"></div>
              </div>

              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-primary" id="saveEditBtn">Save Changes</button>
              </div>
            </div>
          </div>
        </div>`;

    // Inject into DOM
    document.getElementById("modalContainer").innerHTML = modalHtml;

    // Initialize Bootstrap modal correctly
    const modalElement = document.getElementById("editModal");
    const modal = new bootstrap.Modal(modalElement);
    modal.show();

    // Hook up save button
    document.getElementById("saveEditBtn").addEventListener("click", async () => {
        const updatedData = {
            title: document.getElementById("editTitle").value.trim(),
            addressText: document.getElementById("editAddressText").value.trim(),
            city: document.getElementById("editCity").value.trim(),
            zipCode: document.getElementById("editZip").value.trim()
        };

        const updateRes = await fetchWithAuth(`/api/address/edit/${addr.id}`, {
            method: "PUT",
            body: JSON.stringify(updatedData)
        });

        if (updateRes.ok) {
            alert("Address updated successfully!");
            modal.hide();
            const updated = await loadAddresses();
            renderAddressTable(updated);
        } else if (updateRes.status === 400) {
            try {
                const data = await updateRes.json();
                console.log(data);
                if (data.errors) {
                    handleEditModelStateErrors(data.errors);
                } else {
                    document.getElementById("editGeneralError").textContent =
                        data.title || "Validation failed.";
                }
            } catch (err) {
                console.error(err);
                document.getElementById("editGeneralError").textContent = "Unexpected error occurred.";
            }
        } else {
            document.getElementById("editGeneralError").textContent = "Failed to update address.";
        }
    });
}


// Show Create Modal
function showCreateModal() {
    const modalHtml = `
        <div class="modal fade" id="createModal" tabindex="-1">
          <div class="modal-dialog">
            <div class="modal-content">
              <div class="modal-header">
                <h5 class="modal-title">Add New Address</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
              </div>
              <div class="modal-body">
                <form id="createForm" novalidate>
                    <div class="mb-3">
                        <label class="form-label">Title</label>
                        <input class="form-control" id="newTitle" placeholder="Home, Office..." required>
                        <div class="text-danger small mt-1" id="errorTitle"></div>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Address</label>
                        <input class="form-control" id="newAddressText" placeholder="Street address" required>
                        <div class="text-danger small mt-1" id="errorAddressText"></div>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">City</label>
                        <input class="form-control" id="newCity" placeholder="City" required>
                        <div class="text-danger small mt-1" id="errorCity"></div>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Zip Code</label>
                        <input class="form-control" id="newZip" placeholder="Zip code">
                        <div class="text-danger small mt-1" id="errorZip"></div>
                    </div>
                </form>
                <div id="generalError" class="text-danger small"></div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-success" id="saveNewBtn">Create</button>
              </div>
            </div>
          </div>
        </div>`;

    document.getElementById("modalContainer").innerHTML = modalHtml;
    const modal = new bootstrap.Modal(document.getElementById("createModal"));
    modal.show();

    document.getElementById("saveNewBtn").addEventListener("click", async () => {
        // clear previous errors
        ["errorTitle", "errorAddressText", "errorCity", "errorZip", "generalError"].forEach(id => {
            document.getElementById(id).textContent = "";
        });

        const newData = {
            title: document.getElementById("newTitle").value.trim(),
            addressText: document.getElementById("newAddressText").value.trim(),
            city: document.getElementById("newCity").value.trim(),
            zipCode: document.getElementById("newZip").value.trim()
        };

        const res = await fetchWithAuth("/api/address/create", {
            method: "POST",
            body: JSON.stringify(newData)
        });

        if (res.ok) {
            alert("Address created successfully!");
            modal.hide();
            const updated = await loadAddresses();
            renderAddressTable(updated);
        } else if (res.status === 400) {
            // handle validation errors from backend
            try {
                const data = await res.json(); // ASP.NET validation problem object
                console.log(data);

                if (data.errors) {
                    // Flatten errors like { Title: ["Required"], City: ["Required"] }
                    handleModelStateErrors(data.errors);
                } else {
                    document.getElementById("generalError").textContent =
                        data.title || "Validation failed.";
                }
            } catch (err) {
                console.error(err);
                document.getElementById("generalError").textContent = "Unexpected error occurred.";
            }
        } else {
            document.getElementById("generalError").textContent = "Failed to create address.";
        }
    });
}

function handleModelStateErrors(errorObj) {
    // Clear existing errors
    ["errorTitle", "errorAddressText", "errorCity", "errorZip", "generalError"].forEach(id => {
        document.getElementById(id).textContent = "";
    });

    for (const [key, messages] of Object.entries(errorObj)) {
        const msg = messages.join(" ");
        switch (key.toLowerCase()) {
            case "title":
                document.getElementById("errorTitle").textContent = msg;
                break;
            case "addresstext":
                document.getElementById("errorAddressText").textContent = msg;
                break;
            case "city":
                document.getElementById("errorCity").textContent = msg;
                break;
            case "zipcode":
                document.getElementById("errorZip").textContent = msg;
                break;
            default:
                const general = document.getElementById("generalError");
                general.textContent += (general.textContent ? " " : "") + msg;
                break;
        }
    }
}

function handleEditModelStateErrors(errorObj) {
    // Clear existing edit errors
    ["editErrorTitle", "editErrorAddressText", "editErrorCity", "editErrorZip", "editGeneralError"].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.textContent = "";
    });

    for (const [key, messages] of Object.entries(errorObj)) {
        const msg = messages.join(" ");
        switch (key.toLowerCase()) {
            case "title":
                document.getElementById("editErrorTitle").textContent = msg;
                break;
            case "addresstext":
                document.getElementById("editErrorAddressText").textContent = msg;
                break;
            case "city":
                document.getElementById("editErrorCity").textContent = msg;
                break;
            case "zipcode":
                document.getElementById("editErrorZip").textContent = msg;
                break;
            default:
                const general = document.getElementById("editGeneralError");
                general.textContent += (general.textContent ? " " : "") + msg;
                break;
        }
    }
}



