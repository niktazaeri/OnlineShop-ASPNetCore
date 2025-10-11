document.addEventListener("DOMContentLoaded", () => {
    //setupNavbar();
    loadCategories();
    loadAllProducts();
});

//function setupNavbar() {
//    const token = localStorage.getItem("token");
//    const navBar = document.querySelectorAll(".navbar-nav")[1]; // Right side
//    navBar.innerHTML = "";

//    if (token) {
//        navBar.innerHTML = `
//            <li class="nav-item">
//                <a class="nav-link text-dark" href="/Account/logout">Logout</a>
//            </li>
//        `;
//    } else {
//        navBar.innerHTML = `
//            <li class="nav-item">
//                <a class="nav-link text-dark" href="/Account/Login">Login</a>
//            </li>
//            <li class="nav-item">
//                <a class="nav-link text-dark" href="/Account/sign-up">Register</a>
//            </li>
//        `;
//    }
//}


// Load categories on page load
function loadCategories() {
    fetch('/api/categories')
        .then(res => res.json())
        .then(data => {
            const categoryMap = {};
            const roots = [];

            data.forEach(cat => {
                cat.subs = [];
                categoryMap[cat.id] = cat;
            });

            data.forEach(cat => {
                if (cat.categoryParentId == null) {
                    roots.push(cat);
                } else {
                    const parent = categoryMap[cat.categoryParentId];
                    if (parent) parent.subs.push(cat);
                }
            });

            const menu = document.getElementById('categoriesMenu');
            menu.innerHTML = '';

            roots.forEach(cat => {
                menu.appendChild(buildCategoryItem(cat));
            });

            const categoriesList = document.getElementById('categoriesList');
            if (categoriesList) {
                let html = '';
                roots.forEach(cat => {
                    html += `
                        <div class="col-md-3 mb-3">
                            <div class="card h-100 border-secondary shadow">
                                <div class="card-body">
                                    <h5 class="card-title">${cat.name}</h5>
                                    <button class="btn btn-primary btn-sm" onclick="loadCategoryProducts(${cat.id})">View Products</button>
                                </div>
                            </div>
                        </div>`;
                });
                categoriesList.innerHTML = html;
            }
        });
}

function buildCategoryItem(category) {
    const li = document.createElement('li');

    if (category.subs.length > 0) {
        li.classList.add('dropdown-submenu');
        const a = document.createElement('a');
        a.classList.add('dropdown-item', 'dropdown-toggle');
        a.href = '#';
        a.textContent = category.name;

        a.addEventListener('click', function (e) {
            e.preventDefault();
            loadCategoryProducts(category.id);
        });

        const submenu = document.createElement('ul');
        submenu.classList.add('dropdown-menu');

        category.subs.forEach(sub => submenu.appendChild(buildCategoryItem(sub)));

        li.appendChild(a);
        li.appendChild(submenu);
    } else {
        li.innerHTML = `<a class="dropdown-item" href="#" onclick="loadCategoryProducts(${category.id})">${category.name}</a>`;
    }

    return li;
}

function loadAllProducts() {
    fetch('/api/products')
        .then(res => res.json())
        .then(data => renderProducts(data.products, data.imagePaths));
}

function loadCategoryProducts(categoryId) {
    fetch(`/api/categories/${categoryId}/products`)
        .then(res => res.json())
        .then(data => {
            console.log("Fetched products:", data);
            renderProducts(data.products, data.imagePaths);
        });

}

function renderProducts(products, imagePaths = []) {
    const container = document.getElementById('productsList');
    let html = '';

    if (!Array.isArray(products) || products.length === 0) {
        html = `
            <div class="col-12">
                <div class="alert alert-warning text-center">
                    No products found in this category.
                </div>
            </div>`;
    } else {
        products.forEach((p, i) => {
            const imgPath = imagePaths[i] ? `/images/${imagePaths[i]}/${p.pictureName}` : `/images/${p.pictureName}`;

            html += `
                <div class="col-md-3 mb-3">
                    <div class="card h-100 border-primary shadow">
                        <img src="${imgPath}" class="card-img-top" alt="${p.name}">
                        <div class="card-body">
                            <h5 class="card-title">${p.name}</h5>
                            <p class="card-text">Price: $${p.price}</p>
                            <p class="card-text">Available: ${p.quantity}</p>
                        </div>
                    </div>
                </div>`;
        });
    }

    container.innerHTML = html;
}

