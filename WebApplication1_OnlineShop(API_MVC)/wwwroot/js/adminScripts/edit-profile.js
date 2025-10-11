import { fetchWithAuth } from "../accountScripts/checkRefreshToken.js";

document.addEventListener("DOMContentLoaded", () => {
    const firstName = document.getElementById("firstName");
    const lastName = document.getElementById("lastName");
    const username = document.getElementById("username");
    const email = document.getElementById("email");
    const phoneNumber = document.getElementById("phoneNumber");
    const profileImage = document.getElementById("profileImage");
    const profilePreview = document.getElementById("profilePreview");
    const profileForm = document.getElementById("profileForm");
    const messageDiv = document.getElementById("message");
    const removePhotoBtn = document.getElementById("removePhotoBtn");
    let previousProfilePic = null;

    // 1. Fetch current admin info
    fetchWithAuth("/api/auth/get-user")
        .then(res => res.json())
        .then(user => {
            firstName.value = user.firstName || "";
            lastName.value = user.lastName || "";
            username.value = user.userName || "";
            email.value = user.email || "";
            phoneNumber.value = user.phoneNumber || "";
            profilePreview.src = user.profilePicture
                ? `/images/admin_profile_photos/${user.profilePicture}`
                : "/images/admin_profile_photos/default-avatar-icon-of-social-media-user-vector.jpg";
        })
        .catch(err => console.error("Error fetching user:", err));

    // 2. Preview profile image
    profileImage.addEventListener("change", () => {
        const file = profileImage.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = e => profilePreview.src = e.target.result;
            reader.readAsDataURL(file);
        }
    });

    // 3. Submit updated info
    profileForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        messageDiv.innerHTML = "";

        const formData = new FormData();
        formData.append("Name", firstName.value);
        formData.append("LastName", lastName.value);
        formData.append("UserName", username.value);
        formData.append("Email", email.value);
        formData.append("PhoneNumber", phoneNumber.value);

        if (previousProfilePic) {
            formData.append("ProfilePicture", previousProfilePic);
        }

        if (profileImage.files.length > 0) {
            formData.append("ProfileImage", profileImage.files[0]);
        }

        try {
            const res = await fetchWithAuth("/api/auth/edit-profile", {
                method: "PUT",
                body: formData //fetchWithAuth auto-handles token
            });

            if (!res.ok) {
                const error = await res.json();
                messageDiv.innerHTML = `<div class="alert alert-danger">${error.error || "Failed to update profile."}</div>`;
                return;
            }

            const updatedUser = await res.json();
            messageDiv.innerHTML = `<div class="alert alert-success">Profile updated successfully!</div>`;
            profilePreview.src = updatedUser.profilePicture
                ? `/images/admin_profile_photos/${updatedUser.profilePicture}`
                : profilePreview.src;

        } catch (err) {
            console.error(err);
            messageDiv.innerHTML = `<div class="alert alert-danger">Error updating profile.</div>`;
        }
    });

    // 4. Remove profile photo
    removePhotoBtn.addEventListener("click", async () => {
        try {
            const res = await fetchWithAuth("/api/auth/remove-profile-photo", {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    profilePicture: profilePreview.src.split("/").pop()
                })
            });

            if (!res.ok) {
                const error = await res.json();
                messageDiv.innerHTML = `<div class="alert alert-danger">${error.error || "Failed to remove photo."}</div>`;
                return;
            }

            const data = await res.json();
            profilePreview.src = `/images/admin_profile_photos/${data.profilePicture}`;
            messageDiv.innerHTML = `<div class="alert alert-success">Profile photo removed successfully!</div>`;
            previousProfilePic = data.previousProfilePic;

        } catch (err) {
            console.error(err);
            messageDiv.innerHTML = `<div class="alert alert-danger">Error removing photo.</div>`;
        }
    });
});
