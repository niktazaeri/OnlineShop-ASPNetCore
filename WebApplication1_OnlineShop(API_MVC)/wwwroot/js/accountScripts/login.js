document.addEventListener("DOMContentLoaded", () => {
    const loginButton = document.querySelector("button[type='submit']");
    loginButton.addEventListener("click", handleLogin);
});

function handleLogin(event) {
    event.preventDefault();
    
    // Clear previous errors
    clearErrors();

    const username = document.getElementById("typeUserNameX").value.trim();
    const password = document.getElementById("typePasswordX").value.trim();

    let hasError = false;

    if (!username) {
        showError("usernameError", "Username is required.");
        hasError = true;
    }

    if (!password) {
        showError("passwordError", "Password is required.");
        hasError = true;
    }

    if (hasError) return;

    fetch("/api/auth/login", {
        method: "POST",
        credentials: "include", //includes refresh token cookie
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ username, password })
    })
        .then(async res => {
            const data = await res.json();

            if (res.ok) {

                // refresh token is already stored in HttpOnly cookie by the server
                // (JS can't and shouldn't touch it)

                const role = Array.isArray(data.user_role) ? data.user_role[0] : data.user_role;
                if (role === "Admin") {
                    window.location.href = "/Admin/";
                } else {
                    window.location.href = "/Home/Index";
                }
            } else {
                if (Array.isArray(data.errors)) {
                    data.errors.forEach(error => assignError(error));
                } else if (data.error) {
                    showError("generalError", data.error);
                } else {
                    showError("generalError", "Login failed.");
                }
            }
        })
        .catch(err => {
            console.error("Login error:", err);
            showError("generalError", "Something went wrong. Please try again.");
        });
}

function showError(elementId, message) {
    const el = document.getElementById(elementId);
    if (el) {
        el.textContent = message;
    }
}

function clearErrors() {
    showError("usernameError", "");
    showError("passwordError", "");
    showError("generalError", "");
}

function assignError(message) {
    const msg = message.toLowerCase();
    if (msg.includes("username")) {
        showError("usernameError", message);
    } else if (msg.includes("password")) {
        showError("passwordError", message);
    } else {
        showError("generalError", message);
    }
}
