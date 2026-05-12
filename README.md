# 🌐 Dotnetable

A **lightweight, modular, and open-source content management admin panel** built with **.NET 9**.  
Easily deployable, database-agnostic, and extendable — **Dotnetable** gives you full control over your backend with maximum flexibility.

🔗 **Repository:** [https://github.com/Ayrinsoft/Dotnetable](https://github.com/Ayrinsoft/Dotnetable)

---

## 🌍 Language / زبان / اللغة
[🇬🇧 English](#-english-version) | [🇮🇷 فارسی](#-نسخه-فارسی) | [🇸🇦 العربية](#-النسخة-العربية)

---

## 🇬🇧 English Version

### 🚀 Overview

**Dotnetable** is a minimal yet powerful **Admin Panel & CMS backend** written in **C# (.NET 9)**.  
It’s built to be **cross-platform** — running seamlessly on **Linux**, **Windows**, or **Docker containers**.

When you run it for the first time, it automatically:
- Guides you through setup
- Creates the database schema
- Seeds initial data

You’ll get a full-featured **admin dashboard** for managing content, users, and posts —  
while data is served via **RESTful APIs**, ready for any frontend stack.

---

### ⚙️ Key Features

✅ **Cross-platform** – Linux / Windows / Docker  
✅ **Auto setup** on first launch  
✅ **Multi-database support:**
  - MSSQL  
  - PostgreSQL  
  - MySQL  
  - MariaDB  
✅ **EF Core** – migrations & seeding  
✅ **Modular and extensible**  
✅ **RESTful API** for all content  
✅ **JWT Authentication**  
✅ **Customizable Admin Panel**  
✅ **Frontend-agnostic** (React, Blazor, Vue, Angular, PHP, etc.)

---

### 🧠 Tech Stack

- .NET 9 / C#
- ASP.NET Core Web API
- Entity Framework Core
- Razor / Blazor
- JWT Authentication
- Docker support

---

### 🛠️ Installation & Setup

```bash
# 1. Clone the repository
git clone https://github.com/Ayrinsoft/Dotnetable.git
cd Dotnetable

# 2. Restore dependencies
dotnet restore

# 3. Run the project locally
dotnet run

# OR: publish for production
dotnet publish -c Release
```

Then open your browser at:

```
http://localhost:5000
```

You’ll be guided through the initial setup process — including database connection, configuration, and automatic seeding of initial data.

---

### Optional: Run with Docker (basic example)

```bash
# Build the Docker image
docker build -t dotnetable .

# Run container, mapping port 5000
docker run -p 5000:80   -e ASPNETCORE_ENVIRONMENT=Production   dotnetable
```

> Note: adjust the Dockerfile, environment variables, and ports as needed for your environment.

---

### Optional: Use Entity Framework Migrations Manually

If you want to run migrations or seed data manually, make sure the `dotnet-ef` tool is installed:

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update
```

Run the above commands from the project folder where the EF Core migrations are defined.

