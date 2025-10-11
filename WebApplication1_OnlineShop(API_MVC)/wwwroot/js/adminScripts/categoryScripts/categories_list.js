document.addEventListener('DOMContentLoaded', function () {
    fetch('/api/categories/')
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            const tree = buildCategoryTree(data);
            renderCategoriesTable(tree);
        })
        .catch(error => {
            console.error('Error fetching categories:', error);
        });
});

// Build category tree from flat list
function buildCategoryTree(categories) {
    const map = {};
    const roots = [];

    categories.forEach(cat => {
        map[cat.id] = { ...cat, children: [] };
    });

    categories.forEach(cat => {
        if (cat.categoryParentId && map[cat.categoryParentId]) {
            map[cat.categoryParentId].children.push(map[cat.id]);
        } else {
            roots.push(map[cat.id]);
        }
    });

    return roots;
}

// Render hierarchical table
function renderCategoriesTable(treeData) {
    const container = document.getElementById('categories-container');
    const table = document.createElement('table');
    table.className = 'table table-striped table-bordered mt-4';

    const thead = document.createElement('thead');
    thead.innerHTML = `
        <tr>
            <th>ID</th>
            <th>Name</th>
            <th style="width: 200px;">Actions</th>
        </tr>
    `;
    table.appendChild(thead);

    const tbody = document.createElement('tbody');
    appendCategoryRows(treeData, tbody, 0);
    table.appendChild(tbody);

    container.innerHTML = '';
    container.appendChild(table);
}

// Append rows with indentation
function appendCategoryRows(categories, tbody, level) {
    categories.forEach(category => {
        const tr = document.createElement('tr');
        const indent = '&nbsp;&nbsp;&nbsp;'.repeat(level);

        tr.innerHTML = `
            <td>${category.id}</td>
            <td>${indent}${level > 0 ? '↳ ' : ''}${category.name}</td>
            <td>
                <a href="/categories/${category.id}" class="btn btn-sm btn-info me-2">View</a>
                <a href="/categories/edit/${category.id}" class="btn btn-sm btn-warning me-2">Edit</a>
                <a href="/categories/delete/${category.id}" class="btn btn-sm btn-danger">Delete</a>
            </td>
        `;

        tbody.appendChild(tr);

        if (category.children.length > 0) {
            appendCategoryRows(category.children, tbody, level + 1);
        }
    });
}
