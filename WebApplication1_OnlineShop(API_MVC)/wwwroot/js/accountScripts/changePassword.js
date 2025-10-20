import { fetchWithAuth } from "../accountScripts/checkRefreshToken.js";

document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("changePasswordForm");
    const messageDiv = document.getElementById("message");

    form.addEventListener("submit", async (e) => {
        e.preventDefault();
        messageDiv.innerHTML = "";

        const currentPassword = document.getElementById("currentPassword").value;
        const newPassword = document.getElementById("newPassword").value;
        const confirmPassword = document.getElementById("confirmPassword").value;

        try {
            const res = await fetchWithAuth("/api/auth/change-password", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    CurrentPassword: currentPassword,
                    NewPassword: newPassword,
                    ConfirmPassword: confirmPassword
                })
            });

            let data = {};
            try {
                data = await res.json();
            } catch {
                data = {};
            }

            if (!res.ok) {
                let messages = [];
                if (data.errors && Array.isArray(data.errors)) messages.push(...data.errors);
                else if (data.error) messages.push(...(Array.isArray(data.error) ? data.error : [data.error]));
                else messages.push("Failed to change password.");

                messageDiv.innerHTML = messages
                    .map(msg => `<div class="alert alert-danger">${msg}</div>`)
                    .join("");
                return;
            }

            messageDiv.innerHTML = `<div class="alert alert-success">${data.message || "Password changed successfully."}</div>`;
            form.reset();

        } catch (err) {
            console.error(err);
            messageDiv.innerHTML = `<div class="alert alert-danger">Something went wrong. Please try again.</div>`;
        }
    });
});
