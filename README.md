# 🌐 Dotnetable

**A lightweight, modular, open-source admin panel & CMS backend built with .NET 10.**
Database-agnostic, cross-platform, and fully extendable — Dotnetable gives you complete control over your backend with maximum flexibility.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-see%20LICENSE.txt-blue)](LICENSE.txt)
[![Repo](https://img.shields.io/badge/GitHub-Ayrinsoft%2FDotnetable-181717?logo=github)](https://github.com/Ayrinsoft/Dotnetable)

🔗 **Repository:** [github.com/Ayrinsoft/Dotnetable](https://github.com/Ayrinsoft/Dotnetable)

---

## 🌍 Language

**[🇬🇧 English](#-english)** | **[🇮🇷 فارسی](#-فارسی)**

---
---

## 🇬🇧 English

### Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Running Each Project](#running-each-project)
- [Database & Migrations](#database--migrations)
- [Docker](#docker)
- [Documentation Site](#documentation-site)
- [Contributing](#contributing)
- [License](#license)

### Overview

**Dotnetable** is a minimal yet powerful **Admin Panel & CMS backend** written in **C# (.NET 10)**. It runs seamlessly on **Linux**, **Windows**, or inside **Docker containers**.

On first launch it walks you through a guided setup that creates the database schema and seeds initial data. You get a full-featured **admin dashboard** for managing content, users, and orders, while all data is exposed through a **RESTful API** that any frontend (React, Blazor, Vue, Angular, mobile, etc.) can consume.

### Key Features

- ✅ **Cross-platform** — Linux / Windows / Docker
- ✅ **Guided setup** on first launch (database connection, schema, seed data)
- ✅ **Multi-database support** — SQL Server, PostgreSQL, MySQL / MariaDB
- ✅ **EF Core** — code-first schema with SSDT-mirrored `.sql` scripts
- ✅ **Modular, layered architecture** (Domain / Application / Infrastructure / API / Admin)
- ✅ **RESTful API** with versioning, for any frontend
- ✅ **JWT authentication**, rate limiting, and other security invariants baked in
- ✅ **Customizable Blazor admin panel**
- ✅ **Pluggable providers** for SMS gateways, payment gateways, and file storage
- ✅ **React sample frontend** included (`src/dotnetable-react`)

### Tech Stack

- .NET 10 / C#
- ASP.NET Core Web API
- Entity Framework Core 10
- Blazor Server (Admin panel)
- React + TypeScript (sample frontend)
- JWT Authentication
- Docker

> All NuGet/npm dependencies are kept on their **latest stable versions**. Run `dotnet list package --outdated` (and `npm outdated` inside `src/dotnetable-react`) periodically to check for updates.

### Project Structure

```
Dotnetable/
├─ src/
│  ├─ Dotnetable.Domain            # Entities, enums, domain interfaces
│  ├─ Dotnetable.Application       # Use cases, DTOs, business rules
│  ├─ Dotnetable.Infrastructure    # EF Core, repositories, external services
│  ├─ Dotnetable.Database          # SSDT project — canonical .sql schema scripts
│  ├─ Dotnetable.Migrations.SqlServer
│  ├─ Dotnetable.Migrations.MySql
│  ├─ Dotnetable.Migrations.PostgreSql
│  ├─ Dotnetable.API               # Public REST API
│  ├─ Dotnetable.Admin             # Blazor Server admin panel
│  ├─ Dotnetable.Web               # Public-facing website / storefront
│  ├─ Dotnetable.Hosting           # Shared hosting/cross-cutting concerns
│  ├─ Dotnetable.Docs              # Static documentation site (Admin + API)
│  └─ dotnetable-react             # Sample React frontend consuming the API
└─ tests/
```

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- One of: SQL Server, PostgreSQL, MySQL, or MariaDB (a local/dev instance is enough to get started)
- (Optional) [Docker](https://www.docker.com/) if you prefer running in a container
- (Optional) Node.js 18+ if you want to run the `dotnetable-react` sample frontend

### Getting Started

```bash
# 1. Clone the repository
git clone https://github.com/Ayrinsoft/Dotnetable.git
cd Dotnetable

# 2. Restore all dependencies
dotnet restore Dotnetable.sln

# 3. Build the solution
dotnet build Dotnetable.sln
```

### Running Each Project

Each app runs independently with `dotnet run --project <path>`. Pick the one(s) you need:

```bash
# Public REST API
dotnet run --project src/Dotnetable.API

# Blazor Server admin panel
dotnet run --project src/Dotnetable.Admin

# Public-facing website / storefront
dotnet run --project src/Dotnetable.Web

# Documentation site (Admin + API docs)
dotnet run --project src/Dotnetable.Docs
```

The first time you open the **Admin** panel or **API**, you'll be guided through initial setup: choosing your database provider, entering the connection string, and seeding the first admin account and sample data.

Optional — run the React sample frontend against the API:

```bash
cd src/dotnetable-react
npm install
npm run dev
```

### Database & Migrations

Dotnetable supports **SQL Server**, **PostgreSQL**, and **MySQL/MariaDB** through three separate EF Core migration projects. Each keeps a single `InitialCreate` migration that always reflects the current model (no incremental migration chain).

Install the EF Core CLI tool if you don't have it yet:

```bash
dotnet tool install --global dotnet-ef
```

Apply migrations manually (pick the provider you're using):

```bash
dotnet ef database update --project src/Dotnetable.Migrations.SqlServer   --startup-project src/Dotnetable.Migrations.SqlServer
dotnet ef database update --project src/Dotnetable.Migrations.MySql       --startup-project src/Dotnetable.Migrations.MySql
dotnet ef database update --project src/Dotnetable.Migrations.PostgreSql  --startup-project src/Dotnetable.Migrations.PostgreSql
```

> In most cases you don't need to run this manually — the guided setup in the Admin panel creates and seeds the database for you.

The canonical schema (kept in lockstep with the EF model) also lives as plain SQL under `src/Dotnetable.Database` (an SSDT project), useful for SQL Server Schema Compare workflows.

### Docker

```bash
# Build the image
docker build -t dotnetable .

# Run the container
docker run -p 5000:80 -e ASPNETCORE_ENVIRONMENT=Production dotnetable
```

Then open:

```
http://localhost:5000
```

> Adjust the Dockerfile, environment variables, and exposed ports to match the project(s) and environment you're deploying.

### Documentation Site

A unified static documentation site (Admin usage + API reference) ships with the repo:

```bash
dotnet run --project src/Dotnetable.Docs
```

### Contributing

This repository is **not currently accepting outside contributions**. Write access (pushing directly, merging pull requests) is limited to the project owner and one collaborator. External pull requests won't be merged at this time.

### License & Usage Rules

Dotnetable is **open source** under the [MIT License](LICENSE.txt): you are free to clone, fork, use, modify, and personalize it for your own projects — including commercially — as long as the original copyright notice is kept.

That permission covers *using the code*, not *write access to this repository*:

- ✅ Anyone may clone or fork this repo and adapt it for themselves.
- ❌ Only the project owner and their collaborator can push to this repository or merge changes into it.
- Pull requests from other accounts may be closed without merging — this is a personal/team project, not a community-governed one.

> Repository write access is a GitHub permission setting, not something the license or this README can enforce on its own — see the note at the end of this document for the GitHub settings that back this up.

---
---

## 🇮🇷 فارسی

<div dir="rtl">

### فهرست مطالب

- [معرفی](#معرفی)
- [ویژگی‌های کلیدی](#ویژگی‌های-کلیدی)
- [پشته فناوری](#پشته-فناوری)
- [ساختار پروژه](#ساختار-پروژه)
- [پیش‌نیازها](#پیش‌نیازها)
- [شروع به کار](#شروع-به-کار)
- [اجرای هر پروژه](#اجرای-هر-پروژه)
- [پایگاه داده و مایگریشن‌ها](#پایگاه-داده-و-مایگریشن‌ها)
- [داکر](#داکر)
- [سایت مستندات](#سایت-مستندات)
- [مشارکت در پروژه](#مشارکت-در-پروژه)
- [لایسنس](#لایسنس)

### معرفی

**Dotnetable** یک **پنل مدیریت و بک‌اند CMS** سبک اما قدرتمند است که با **زبان سی‌شارپ (.NET 10)** نوشته شده. این پروژه به‌صورت کامل **چندسکویی** است و روی **لینوکس**، **ویندوز** یا داخل **کانتینر داکر** بدون مشکل اجرا می‌شود.

در اولین اجرا، برنامه شما را در یک فرایند راه‌اندازی گام‌به‌گام همراهی می‌کند: ساخت شمای پایگاه داده و درج داده‌های اولیه به‌صورت خودکار انجام می‌شود. در نتیجه یک **داشبورد مدیریتی کامل** برای مدیریت محتوا، کاربران و سفارش‌ها در اختیار دارید، در حالی که تمام داده‌ها از طریق یک **API مبتنی بر REST** در دسترس هر فرانت‌اندی (React، Blazor، Vue، Angular، موبایل و ...) قرار می‌گیرد.

### ویژگی‌های کلیدی

- ✅ **چندسکویی** — لینوکس / ویندوز / داکر
- ✅ **راه‌اندازی هدایت‌شده** در اولین اجرا (اتصال پایگاه داده، ساخت شما، درج داده اولیه)
- ✅ **پشتیبانی از چند پایگاه داده** — SQL Server، PostgreSQL، MySQL / MariaDB
- ✅ **Entity Framework Core** — شمای کد-محور همراه با اسکریپت‌های `.sql` هم‌تراز در SSDT
- ✅ **معماری لایه‌ای و ماژولار** (Domain / Application / Infrastructure / API / Admin)
- ✅ **API مبتنی بر REST** با نسخه‌بندی، مناسب برای هر فرانت‌اند
- ✅ **احراز هویت JWT**، محدودسازی نرخ درخواست و سایر ملاحظات امنیتی به‌صورت پیش‌فرض فعال است
- ✅ **پنل مدیریت Blazor قابل شخصی‌سازی**
- ✅ **درگاه‌های افزونه‌محور** برای پیامک، درگاه‌های پرداخت و ذخیره‌سازی فایل
- ✅ **نمونه فرانت‌اند React** به همراه پروژه (`src/dotnetable-react`)

### پشته فناوری

- ‎.NET 10 / #C
- ASP.NET Core Web API
- Entity Framework Core 10
- Blazor Server (پنل مدیریت)
- React + TypeScript (نمونه فرانت‌اند)
- احراز هویت JWT
- داکر

> تمام وابستگی‌های NuGet/npm روی **آخرین نسخه پایدار** نگه داشته می‌شوند. برای بررسی به‌روزرسانی‌ها به‌صورت دوره‌ای دستور `dotnet list package --outdated` (و `npm outdated` داخل مسیر `src/dotnetable-react`) را اجرا کنید.

### ساختار پروژه

```
Dotnetable/
├─ src/
│  ├─ Dotnetable.Domain            # موجودیت‌ها، Enum ها، اینترفیس‌های دامنه
│  ├─ Dotnetable.Application       # منطق کسب‌وکار، DTOها، Use Case ها
│  ├─ Dotnetable.Infrastructure    # EF Core، ریپازیتوری‌ها، سرویس‌های خارجی
│  ├─ Dotnetable.Database          # پروژه SSDT — اسکریپت‌های اصلی شمای .sql
│  ├─ Dotnetable.Migrations.SqlServer
│  ├─ Dotnetable.Migrations.MySql
│  ├─ Dotnetable.Migrations.PostgreSql
│  ├─ Dotnetable.API               # API عمومی REST
│  ├─ Dotnetable.Admin             # پنل مدیریت Blazor Server
│  ├─ Dotnetable.Web               # وب‌سایت عمومی / فروشگاه
│  ├─ Dotnetable.Hosting           # موارد مشترک هاستینگ
│  ├─ Dotnetable.Docs              # سایت مستندات استاتیک (مدیریت + API)
│  └─ dotnetable-react             # نمونه فرانت‌اند React مصرف‌کننده API
└─ tests/
```

### پیش‌نیازها

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- یکی از: SQL Server، PostgreSQL، MySQL یا MariaDB (یک نمونه لوکال/توسعه کافی است)
- (اختیاری) [داکر](https://www.docker.com/) در صورت تمایل به اجرا داخل کانتینر
- (اختیاری) Node.js نسخه ۱۸ به بالا برای اجرای نمونه فرانت‌اند `dotnetable-react`

### شروع به کار

```bash
# ۱. کلون کردن مخزن
git clone https://github.com/Ayrinsoft/Dotnetable.git
cd Dotnetable

# ۲. بازیابی وابستگی‌ها
dotnet restore Dotnetable.sln

# ۳. بیلد کردن سولوشن
dotnet build Dotnetable.sln
```

### اجرای هر پروژه

هر برنامه به‌صورت مستقل با دستور `dotnet run --project <path>` اجرا می‌شود. هرکدام را که نیاز دارید اجرا کنید:

```bash
# API عمومی REST
dotnet run --project src/Dotnetable.API

# پنل مدیریت Blazor Server
dotnet run --project src/Dotnetable.Admin

# وب‌سایت عمومی / فروشگاه
dotnet run --project src/Dotnetable.Web

# سایت مستندات (مستندات مدیریت + API)
dotnet run --project src/Dotnetable.Docs
```

اولین باری که پنل **Admin** یا **API** را باز می‌کنید، در یک فرایند راه‌اندازی اولیه راهنمایی می‌شوید: انتخاب نوع پایگاه داده، وارد کردن رشته اتصال، و ساخت اولین حساب ادمین به همراه داده‌های نمونه.

اختیاری — اجرای نمونه فرانت‌اند React در کنار API:

```bash
cd src/dotnetable-react
npm install
npm run dev
```

### پایگاه داده و مایگریشن‌ها

Dotnetable از **SQL Server**، **PostgreSQL** و **MySQL/MariaDB** از طریق سه پروژه جداگانه مایگریشن EF Core پشتیبانی می‌کند. هر پروژه تنها یک مایگریشن `InitialCreate` دارد که همیشه با مدل فعلی هماهنگ نگه داشته می‌شود (بدون زنجیره مایگریشن افزایشی).

در صورت نیاز، ابزار خط فرمان EF Core را نصب کنید:

```bash
dotnet tool install --global dotnet-ef
```

اعمال دستی مایگریشن‌ها (بسته به پایگاه داده مورد استفاده):

```bash
dotnet ef database update --project src/Dotnetable.Migrations.SqlServer   --startup-project src/Dotnetable.Migrations.SqlServer
dotnet ef database update --project src/Dotnetable.Migrations.MySql       --startup-project src/Dotnetable.Migrations.MySql
dotnet ef database update --project src/Dotnetable.Migrations.PostgreSql  --startup-project src/Dotnetable.Migrations.PostgreSql
```

> در بیشتر موارد نیازی به اجرای دستی این دستورات نیست — فرایند راه‌اندازی هدایت‌شده در پنل مدیریت، پایگاه داده را می‌سازد و داده‌های اولیه را درج می‌کند.

شمای اصلی پایگاه داده (که همیشه با مدل EF هماهنگ نگه داشته می‌شود) به‌صورت اسکریپت‌های خام SQL نیز در مسیر `src/Dotnetable.Database` (یک پروژه SSDT) موجود است؛ این مسیر برای گردش‌کار Schema Compare در SQL Server مفید است.

### داکر

```bash
# ساخت ایمیج
docker build -t dotnetable .

# اجرای کانتینر
docker run -p 5000:80 -e ASPNETCORE_ENVIRONMENT=Production dotnetable
```

سپس آدرس زیر را باز کنید:

```
http://localhost:5000
```

> Dockerfile، متغیرهای محیطی و پورت‌های در معرض دید را متناسب با پروژه(های) موردنظر و محیط استقرار خود تنظیم کنید.

### سایت مستندات

یک سایت مستندات استاتیک یکپارچه (راهنمای پنل مدیریت + مرجع API) به همراه مخزن ارائه شده است:

```bash
dotnet run --project src/Dotnetable.Docs
```

### مشارکت در پروژه

این مخزن در حال حاضر **مشارکت از بیرون تیم را نمی‌پذیرد**. دسترسی نوشتن (پوش مستقیم، مرج کردن Pull Request) فقط در اختیار مالک پروژه و یک همکار است. Pull Request از حساب‌های دیگر در حال حاضر مرج نخواهد شد.

### لایسنس و قوانین استفاده

پروژه Dotnetable تحت [لایسنس MIT](LICENSE.txt) **متن‌باز (Open Source)** است: هرکس می‌تواند آن را کلون یا فورک کند، استفاده کند، تغییر دهد و برای پروژه‌های شخصی خودش (حتی به‌صورت تجاری) شخصی‌سازی کند، به شرطی که اعلامیه کپی‌رایت اصلی حفظ شود.

این اجازه مربوط به **استفاده از کد** است، نه **دسترسی نوشتن به این مخزن**:

- ✅ هرکسی می‌تواند این مخزن را کلون یا فورک کرده و برای خودش شخصی‌سازی کند.
- ❌ فقط مالک پروژه و همکارش می‌توانند روی این مخزن پوش بزنند یا تغییری را در آن مرج کنند.
- Pull Request از حساب‌های دیگر ممکن است بدون مرج شدن بسته شود — این یک پروژه شخصی/تیمی است، نه پروژه‌ای با مدیریت جمعی (community-governed).

> دسترسی نوشتن به مخزن یک تنظیم سطح گیت‌هاب است، نه چیزی که لایسنس یا این فایل readme به‌تنهایی بتواند اعمال کند — برای تنظیمات لازم در گیت‌هاب به یادداشت انتهای همین فایل مراجعه کنید.

</div>

---

<sub>GitHub access note / یادداشت دسترسی گیت‌هاب: repository **Settings → Collaborators and teams** should list only the owner and the one collaborator with write access, with no other outside collaborators or teams attached; if the default branch is protected, its branch-protection rule's "restrict who can push" list should match the same two accounts. این تنظیم را در **Settings → Collaborators and teams** انجام دهید و اگر روی شاخه اصلی قانون محافظتی (branch protection) دارید، فهرست "restrict who can push" آن را هم با همین دو حساب هماهنگ کنید.</sub>
