document.addEventListener("DOMContentLoaded", () => {
    addAddress(); // Add initial address

    const form = document.getElementById("registerForm");
    const messageBox = document.getElementById("messageBox");

    form.addEventListener("submit", function (e) {
        e.preventDefault();

        const formData = new FormData(form);

        const user = {
            name: formData.get("firstName"),
            lastName: formData.get("lastName"),
            email: formData.get("email"),
            username: formData.get("username"),
            password: formData.get("password"),
            confirmedPassword: formData.get("confirmedPassword"), 
            phoneNumber: formData.get("phoneNumber"),
            addresses: []
        };


        document.querySelectorAll(".address-group").forEach(group => {
            const address = {
                title: group.querySelector(".address-title").value,
                city: group.querySelector(".address-city").value,
                addressText: group.querySelector(".address-text").value,
                zipCode: group.querySelector(".address-zip").value
            };
            user.addresses.push(address);
        });

        fetch("/api/auth/register", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(user)
        })
            .then(async res => {
                const data = await res.json();

                if (res.status === 201) {
                    // Success: store token & redirect
                    localStorage.setItem("token", data.token);
                    window.location.href = "/Home/Index";
                } else {
                    // Error: throw full response
                    throw data;
                }
            })
            .catch(err => {
                console.error("Register error:", err);

                let errorText = "Registration failed.";

                // handle ASP.NET Core style model errors
                if (err.errors && typeof err.errors === "object") {
                    errorText = Object.values(err.errors)
                        .flat()
                        .join("<br>");
                } else if (Array.isArray(err)) {
                    errorText = err.join("<br>");
                } else if (typeof err === "object") {
                    errorText = Object.values(err).flat().join("<br>");
                } else if (typeof err === "string") {
                    errorText = err;
                }

                showMessage("danger", errorText);
            });


    });
});

function addAddress() {
    const container = document.getElementById("addressesContainer");

    const div = document.createElement("div");
    div.classList.add("address-group", "mb-3", "border", "p-3", "rounded", "bg-white");
    div.innerHTML = `
        <div class="row mb-2">
            <div class="col"><input type="text" class="form-control address-title" placeholder="Title" required></div>
            <div class="col"><input type="text" class="form-control address-city" placeholder="City" required></div>
        </div>
        <input type="text" class="form-control address-text mb-2" placeholder="Address" required>
        <input type="text" class="form-control address-zip mb-2" placeholder="Zip Code" required>
        <button type="button" class="btn btn-sm btn-danger" onclick="this.parentElement.remove()">Remove</button>
    `;
    container.appendChild(div);
}

function showMessage(type, message) {
    const msgBox = document.getElementById("messageBox");

    msgBox.className = `alert alert-${type} mt-4`; // Bootstrap styles
    msgBox.innerHTML = message;
    msgBox.classList.remove("d-none");
}

