document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("forgotPasswordForm");
    const emailInput = document.getElementById("email");
    const messageDiv = document.getElementById("message");

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        messageDiv.innerHTML = `<div class="text-info">Processing...</div>`;

        try {
            const res = await fetch("/api/auth/forgot-password", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ email: emailInput.value })
            });

            if (res.status === 202) {
                const data = await res.json();
                messageDiv.innerHTML = `<div class="alert alert-success">${data.message}</div>`;
                form.reset();
            } else {
                const errorData = await res.json();
                const errors = errorData.errors?.join("<br>") || errorData.message || "Something went wrong.";
                messageDiv.innerHTML = `<div class="alert alert-danger">${errors}</div>`;
            }
        } catch (err) {
            console.error("Forgot password error:", err);
            messageDiv.innerHTML = `<div class="alert alert-danger">Server error. Please try again later.</div>`;
        }
    });
});
