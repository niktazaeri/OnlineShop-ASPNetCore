document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("resetPasswordForm");
    const alertBox = document.getElementById("alertBox");

    // Extract email and token from URL
    const urlParams = new URLSearchParams(window.location.search);
    const email = urlParams.get("email");
    const token = urlParams.get("token");

    if (!email || !token) {
        showAlert("Invalid password reset link.", "danger");
        form.querySelector("button").disabled = true;
        return;
    }

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const newPassword = document.getElementById("newPassword").value;
        const confirmPassword = document.getElementById("confirmPassword").value;

        if (newPassword !== confirmPassword) {
            showAlert("Passwords do not match!", "danger");
            return;
        }

        try {
            const res = await fetch("/api/auth/reset-password", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    Email: email,
                    Token: token,
                    NewPassword: newPassword,
                    ConfirmPassword: confirmPassword 
                })
            });

            const data = await res.json();

            if (res.ok) {
                showAlert(data.message || "Password reset successful!", "success");
                setTimeout(() => {
                    window.location.href = "/Account/login";
                }, 2000);
            } else {
                const errorMsg = data.errors
                    ? data.errors.join(", ")
                    : (data.error || "Reset failed.");
                showAlert(errorMsg, "danger");
            }

        } catch (err) {
            showAlert("An error occurred. Please try again.", "danger");
            console.error(err);
        }
    });

    function showAlert(message, type) {
        alertBox.textContent = message;
        alertBox.className = `alert alert-${type}`;
        alertBox.classList.remove("d-none");
    }
});
