# OnlineShop-ASPNetCore

🛒 **Online Shop (ASP.NET Core MVC + API)**

## Overview

This project is a **full-featured online shop** built with **ASP.NET Core MVC + Web API** architecture.  
It includes a category system (e.g., _Clothes_, _Kitchen_, _Electronics_), an **Admin Panel** for product and category management, and a **JWT-based authentication** system with cookie-based frontend session handling.

The frontend pages are rendered by MVC views, while all dynamic content is loaded via **JavaScript API calls**.

---

## Features

### General Features
- Multi-category product structure (e.g., _Clothes_, _Kitchen_, _Electronics_) including subcategories using LinQ and entity relations
- register , login , logout
- API-first architecture – frontend interacts with backend through RESTful APIs
- MVC controllers are only responsible for rendering views
- Fully asynchronous data loading via JavaScript `fetch()` calls

### Authentication & Security
- **JWT Authentication** for API requests
- **Cookies** used for accessing the rezor pages
- Login / Register, Password Reset, Forgot Password, Change Password

### Admin Panel
- CRUD Categories
- CRUD Products
- Update Profile Information including changing profile photo

### Customer Area _(In Progress)_
- User Profile Management
- CRUD for Addresses
- Shopping Cart
- Order History
- Invoice View

## Tech Stack
- ASP.NET Core MVC
- ASP.NET Core Web API
- Entity Framework Core
- Identity Core
- JWT Authentication
- JavaScript (Fetch API)
- HTML5 / Bootstrap

## Architecture
- Controllers (MVC): Render pages only (razor pages)
- Controllers (API): Provide RESTful endpoints for frontend
- JavaScript (Frontend): Calls APIs to display dynamic content

## Author
Developed by **Nikta Zaeri**  
Built with ❤️ using **ASP.NET Core MVC + Web API**
