<div align="center">

# 🏠 ApnaRent

### A Peer-to-Peer Rental Marketplace

*Connecting renters and item owners through a secure, feature-rich platform*

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/apps/aspnet)
[![C#](https://img.shields.io/badge/C%23-239120?style=flat&logo=c-sharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=flat&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![EF Core](https://img.shields.io/badge/Entity%20Framework%20Core-512BD4?style=flat)](https://learn.microsoft.com/en-us/ef/core/)
[![License](https://img.shields.io/badge/License-Academic-blue?style=flat)]()

</div>

---

## 📖 Overview

**ApnaRent** is a full-stack peer-to-peer rental marketplace developed as my **Final Year Project** at SZABIST Karachi. Built on **ASP.NET Core MVC** with a production-grade architecture, it enables users to list, discover, and rent items within their community — complete with real-time messaging, smart booking logic, and a full admin management suite.

The platform is designed end-to-end: from database schema and backend business logic to a polished, user-centric interface — reflecting the ability to ship a complete, real-world software solution independently.

---

## ✨ Features

### 🙋 User-Facing

| Feature | Description |
|---|---|
| 🏡 **Landing Page** | Engaging entry point showcasing featured rentals and quick category navigation |
| 📊 **Personalized Dashboard** | Logged-in home page with saved items, recent activity, and category browsing |
| 🔍 **Category-Based Listings** | Browse and filter rental items with a clean, card-based UI |
| 📝 **Item Listing Requests** | Owners submit new items for rent, pending admin approval |
| 💬 **Real-Time Messaging** | Direct chat between renters and item owners |
| 🔔 **Notifications** | Centralized alerts for bookings, approvals, and activity updates |
| 📅 **Smart Booking Engine** | Availability logic that prevents double-booking conflicts |
| ⭐ **Saved Items** | Bookmark listings to revisit later |
| 🔐 **Secure Authentication** | JWT-based login with OTP-driven forgot-password flow |

### 🛠️ Admin Panel

| Feature | Description |
|---|---|
| 📈 **Analytics Dashboard** | Chart.js-powered visual insights into platform activity |
| 👥 **Manage Users** | View and control registered user accounts and roles |
| 📦 **Manage Bookings** | Track, update, and oversee all rental bookings platform-wide |
| ✅ **Listing Approval Workflow** | Review and approve/reject new item listings before they go live |

---

## 🖥️ Screenshots

<div align="center">

| Landing Page | User Dashboard |
|:---:|:---:|
| _add screenshot_ | _add screenshot_ |

| Item Listings | Messaging & Notifications |
|:---:|:---:|
| _add screenshot_ | _add screenshot_ |

| Admin Dashboard | Manage Bookings |
|:---:|:---:|
| _add screenshot_ | _add screenshot_ |

| Manage Users | Item Listing Requests |
|:---:|:---:|
| _add screenshot_ | _add screenshot_ |

| Login Page |
|:---:|
| _add screenshot_ |

</div>

---

## 🛠️ Tech Stack

<div align="center">

| Layer | Technology |
|---|---|
| **Backend** | ASP.NET Core MVC (C#) |
| **Database** | SQL Server |
| **ORM** | Entity Framework Core |
| **Authentication** | JWT + OTP |
| **Frontend** | Razor Views, HTML, CSS, JavaScript |
| **Data Visualization** | Chart.js |

</div>

---

## 🏗️ Architecture Highlights

- **Clean separation of concerns** — Controllers, ViewModels, and Services keep logic modular and maintainable
- **Object-oriented design** — encapsulation, abstraction, delegates, and LINQ applied throughout the codebase
- **Repository pattern** — Entity Framework Core manages data access with clear boundaries
- **Role-based access control** — distinct flows and permissions for renters, owners, and admins
- **Approval-driven workflows** — every new listing passes through an admin review pipeline before going live

---

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 (recommended)

### Setup

```bash
# Clone the repository
git clone https://github.com/abubkr321/ApnaRent.git
cd ApnaRent

# Restore dependencies
dotnet restore

# Update the database
dotnet ef database update

# Run the application
dotnet run
```

> **Note:** Copy `appsettings.example.json` to `appsettings.json` and update the connection string with your local SQL Server details before running.

---

## 📂 Project Structure

```
ApnaRent/
├── Controllers/      # MVC controllers handling requests
├── Models/            # Data models and entities
├── ViewModels/        # View-specific data structures
├── Views/              # Razor views (UI)
├── Data/               # DbContext and database configuration
├── Helpers/            # Utility and helper classes
├── Migrations/        # EF Core migration history
└── wwwroot/           # Static files (CSS, JS, images)
```

---

## 👤 Author

<div align="center">

**Ab**
Software Engineering Graduate — SZABIST Karachi

[![GitHub](https://img.shields.io/badge/GitHub-abubkr321-181717?style=flat&logo=github)](https://github.com/abubkr321)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Connect-0A66C2?style=flat&logo=linkedin)](#)

</div>

---

<div align="center">

*Developed as a Final Year Project — a complete, real-world demonstration of full-stack .NET development.*

</div>
