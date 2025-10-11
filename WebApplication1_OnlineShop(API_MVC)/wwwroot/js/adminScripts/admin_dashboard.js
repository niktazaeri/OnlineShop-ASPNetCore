import { fetchWithAuth } from "../accountScripts/checkRefreshToken.js";

document.addEventListener("DOMContentLoaded", async () => {
    const welcomeDiv = document.getElementById("welcomeMessage");

    try {
        const res = await fetchWithAuth("/api/auth/get-user", { method: "GET" });

        if (!res.ok) {
            alert(res);
            throw new Error("Failed to fetch user data.");
        }

        let user = {};
        try {
            user = await res.json();
        } catch {
            console.warn("No JSON returned from /get-user");
        }

        if (user && user.firstName && user.lastName) {
            welcomeDiv.textContent = `Welcome ${user.firstName} ${user.lastName}`;
        } else {
            welcomeDiv.textContent = "User data unavailable";
        }

    } catch (error) {
        console.error("Error fetching user data:", error);
        welcomeDiv.textContent = "Error fetching user data";
    }
});
