import { fetchWithAuth } from "../../accountScripts/checkRefreshToken.js";

document.addEventListener("DOMContentLoaded", async () => {
    await loadUser();

    document.getElementById("saveProfileBtn").addEventListener("click", saveProfile);
});

// Load current user data
async function loadUser() {
    const res = await fetchWithAuth("/api/auth/get-user");
    if (!res.ok) {
        console.error("Failed to fetch user info");
        return;
    }
    const user = await res.json();

    document.getElementById("username").value = user.userName || "";
    document.getElementById("firstName").value = user.firstName || "";
    document.getElementById("lastName").value = user.lastName || "";
    document.getElementById("email").value = user.email || "";
    document.getElementById("phoneNumber").value = user.phoneNumber || "";
}

// Save profile changes
async function saveProfile() {
    clearErrors();

    const payload = {
        UserName: document.getElementById("username").value.trim(),
        Name: document.getElementById("firstName").value.trim(),
        LastName: document.getElementById("lastName").value.trim(),
        Email: document.getElementById("email").value.trim(),
        PhoneNumber: document.getElementById("phoneNumber").value.trim()
    };

    const res = await fetchWithAuth("/api/edit-profile", {
        method: "PUT",
        body: JSON.stringify(payload)
    });

    if (res.ok) {
        alert("Profile updated successfully!");
    } else if (res.status === 400) {
        try {
            const data = await res.json();
            if (data.errors) {
                handleValidationErrors(data.errors);
            } else {
                document.getElementById("generalError").textContent =
                    data.title || "Validation failed.";
            }
        } catch (err) {
            console.error(err);
            document.getElementById("generalError").textContent = "Unexpected error occurred.";
        }
    } else {
        document.getElementById("generalError").textContent = "Failed to update profile.";
    }
}

function clearErrors() {
    ["errorUsername", "errorFirstName", "errorLastName", "errorEmail", "errorPhoneNumber", "generalError"]
        .forEach(id => document.getElementById(id).textContent = "");
}

function handleValidationErrors(errors) {
    for (const [key, messages] of Object.entries(errors)) {
        const msg = messages.join(" ");
        switch (key.toLowerCase()) {
            case "username":
                document.getElementById("errorUsername").textContent = msg;
                break;
            case "name":
                document.getElementById("errorFirstName").textContent = msg;
                break;
            case "lastname":
                document.getElementById("errorLastName").textContent = msg;
                break;
            case "email":
                document.getElementById("errorEmail").textContent = msg;
                break;
            case "phonenumber":
                document.getElementById("errorPhoneNumber").textContent = msg;
                break;
            default:
                const general = document.getElementById("generalError");
                general.textContent += (general.textContent ? " " : "") + msg;
                break;
        }
    }
}
