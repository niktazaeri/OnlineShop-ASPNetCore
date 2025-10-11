function getIdFromPath() {
    const segments = window.location.pathname.split('/');
    console.log(segments)
    return segments.length >= 3 ? segments[2] : null;
}

async function fetchProduct(id) {
    if (!id || id === "0") {
        document.body.innerHTML += `<p>Invalid product ID.</p>`;
        return;
    }

    try {
        const response = await fetch(`/api/products/${id}`);
        if (response.status === 404) {
            document.body.innerHTML += `<p>Product not found.</p>`;
            return;
        }
        if (!response.ok) {
            throw new Error("Network response was not ok");
        }

        const data = await response.json();
        displayProduct(data);
    } catch (error) {
        console.error("Fetch error:", error);
        document.body.innerHTML += `<p>Error fetching product data.</p>`;
    }
}

function displayProduct(data) {
    const { categoryName, category_parents_name, product } = data;
    let imagePath = "";
    if (category_parents_name.length === 0) {
        imagePath = `/images/${product.pictureName}`;
        console.log("empty");
    } else {
        // category_parents_name is from child → root, so reverse it
        const fullPath = [...category_parents_name].reverse().join("/");
        imagePath = `/images/${fullPath}/${categoryName}/${product.pictureName}`;
    }
    const productHtml = `
        <img src="${imagePath}" alt="${product.name}" width="300" height="300">
        <h2>Category: ${categoryName}</h2>
        <h3>Product: ${product.name}</h3>
        <p>Description: ${product.description}</p>
        <p>Price: $${product.price}</p>
    `;
    console.log(imagePath);
    document.body.innerHTML += productHtml;
}


document.addEventListener("DOMContentLoaded", () => {
    const productId = getIdFromPath();
    console.log(productId)
    fetchProduct(productId);
});


