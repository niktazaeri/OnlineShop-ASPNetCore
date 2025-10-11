let accessToken = null;
// refresh operation 
export async function checkRefreshToken() {
    try {
        const res = await fetch("/api/auth/refresh-token", {
            method: "POST",
            credentials: "include",
            headers: { "Content-Type": "application/json" }
        });
        
        if (!res.ok) {
            window.location.href = "/Account/login";
            return null;
        }

        const data = await res.json();
        accessToken = data.token;
        return accessToken;
    } catch (err) {
        window.location.href = "/Account/login";
        return null;
    }
}


// fetch wrapper with auto-refresh 
export async function fetchWithAuth(url, options = {}) {
    if (!accessToken) {
        accessToken = await checkRefreshToken(); 
        if (!accessToken) return null; 
    }

    if (!options.headers) options.headers = {};
    if (!(options.body instanceof FormData)) {
        options.headers["Content-Type"] = "application/json";
    }

    options.headers["Authorization"] = `Bearer ${accessToken}`;

    let res = await fetch(url, options);

    if (res.status === 401) {
        const newToken = await checkRefreshToken();
        if (!newToken) return res;
        options.headers["Authorization"] = `Bearer ${newToken}`;
        res = await fetch(url, options);
    }

    return res;
}

