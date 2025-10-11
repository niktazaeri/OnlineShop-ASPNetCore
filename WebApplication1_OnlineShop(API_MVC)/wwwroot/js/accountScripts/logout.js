import { fetchWithAuth } from "./checkRefreshToken.js";

// Send logout request to backend API
async function confirmLogout() {
    try {

        // Call the logout API and send both the cookie + Authorization header
        const res = await fetchWithAuth("/api/auth/logout", { method: 'POST' });
        if (res.ok) {
            //setLoggedOut();    // set flag so auto-refresh stops
            window.location.href = "/"; // redirect to homepage
        }
    } catch (err) {
        console.log("Logout failed:", err);
    }
}


// Attach logout click event after DOM is ready
document.addEventListener("DOMContentLoaded", () => {
    const logoutBtn = document.getElementById("logoutBtn");
    if (logoutBtn) {
        logoutBtn.addEventListener("click", confirmLogout);
    }
});
