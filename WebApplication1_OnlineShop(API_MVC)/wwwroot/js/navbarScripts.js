import { updateCartCount } from "./customerScripts/cartScripts.js";

document.addEventListener("DOMContentLoaded", () => {
    updateCartCount();
    loadCategories();
});

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
                                    <a class="btn btn-primary btn-sm" href="/Home?categoryId=${cat.id}">
                                        View Products
                                    </a>
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

        const submenu = document.createElement('ul');
        submenu.classList.add('dropdown-menu');

        category.subs.forEach(sub => submenu.appendChild(buildCategoryItem(sub)));

        li.appendChild(a);
        li.appendChild(submenu);
    } else {
        li.innerHTML = `<a class="dropdown-item" href="/Home?categoryId=${category.id}">
                            ${category.name}
                        </a>`;
    }

    return li;
}
