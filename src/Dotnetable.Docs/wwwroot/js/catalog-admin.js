/**
 * Dotnetable Admin documentation catalog.
 *
 * MAINTENANCE RULE
 * ----------------
 * When you add/rename/remove an Admin page or change how a feature works:
 * 1) Update NavMenu.razor + AdminSearchCatalog.cs (existing rule)
 * 2) Update the matching entry in THIS file (section/page id, adminPath, purpose, howTo)
 *
 * Page ids are stable doc slugs (not always identical to admin routes).
 * adminPath mirrors the Blazor @page route for quick cross-reference.
 */
window.DOCS_ADMIN = {
  meta: {
    version: "1.0",
    sourceOfTruth: [
      "src/Dotnetable.Admin/Components/Layout/NavMenu.razor",
      "src/Dotnetable.Admin/Navigation/AdminSearchCatalog.cs",
      "src/Dotnetable.Admin/Components/Pages/**",
    ],
    maintenance: {
      title: {
        en: "Keep docs in sync with Admin",
        fa: "هم‌گام‌سازی مستندات با ادمین",
      },
      body: {
        en: "This static site is the human-readable guide for every Admin area. Treat documentation updates as part of the same change that touches Admin UI or business rules.",
        fa: "این سایت استاتیک، راهنمای خواندنی همه بخش‌های ادمین است. هر تغییری در UI یا منطق ادمین باید همان‌جا با به‌روزرسانی همین صفحات همراه باشد.",
      },
      items: [
        {
          en: "New menu item → add a page here and link it under the same section.",
          fa: "منوی جدید → صفحه جدید اینجا و لینک در همان بخش.",
        },
        {
          en: "Behavior change (e.g. inventory reservation) → rewrite purpose / how-to / tips.",
          fa: "تغییر رفتار (مثل رزرو موجودی) → بخش کارایی / نحوه کار / نکات را بازنویسی کنید.",
        },
        {
          en: "Removed feature → remove or mark the docs page so nobody follows stale steps.",
          fa: "حذف قابلیت → صفحه مستند را حذف یا به‌روز کنید تا کسی مسیر قدیمی را دنبال نکند.",
        },
      ],
    },
  },

  sections: [
    /* ───────────────────── Overview ───────────────────── */
    {
      id: "overview",
      title: { en: "Overview", fa: "نمای کلی" },
      summary: {
        en: "How the admin panel is organized and how to navigate it.",
        fa: "ساختار پنل ادمین و نحوه حرکت بین بخش‌ها.",
      },
      pages: [
        {
          id: "home",
          title: { en: "Welcome", fa: "خوش‌آمدید" },
          adminPath: "/",
          summary: {
            en: "Start here. Each docs page answers three questions: what is this screen, when do I need it, how do I click through it.",
            fa: "از اینجا شروع کنید. هر صفحه مستند سه سؤال را جواب می‌دهد: این صفحه چیست، کی به آن نیاز دارم، قدم‌به‌قدم چطور کار کنم.",
          },
          access: { en: "Any signed-in admin member", fa: "هر عضو ادمین واردشده" },
          purpose: [
            {
              en: "This site is the **operator manual** for Dotnetable Admin — same sections as the Admin menu, written in plain language (FA + EN).",
              fa: "این سایت **دفترچه راهنمای اپراتور** برای Dotnetable Admin است — همان بخش‌های منوی ادمین، با زبان ساده (فارسی + انگلیسی).",
            },
            {
              en: "When you forget a screen (classic example: **Inventory / Stock**), open that topic and read the example before clicking around blindly in Admin.",
              fa: "هر وقت صفحه‌ای یادتان رفت (مثال کلاسیک: **موجودی / Stock**)، همان موضوع را اینجا باز کنید و قبل از کلیک‌های سردرگم در ادمین، مثال را بخوانید.",
            },
            {
              en: "Switch language anytime with **FA / EN**. Search works on titles, admin paths, and keywords.",
              fa: "هر لحظه با **FA / EN** زبان را عوض کنید. جستجو روی عنوان، مسیر ادمین و کلمات کلیدی کار می‌کند.",
            },
          ],
          howTo: [
            {
              en: "In the sidebar, open the section that matches the Admin menu group (Catalog, Inventory, Finance, …).",
              fa: "در منوی کناری، بخشی هم‌نام گروه منوی ادمین را باز کنید (کاتالوگ، موجودی، مالی، …).",
            },
            {
              en: "On each page read in order: **In plain words** → **columns/example** (if any) → **Step by step**.",
              fa: "در هر صفحه به ترتیب بخوانید: **به زبان ساده** → **ستون‌ها/مثال** (اگر هست) → **قدم‌به‌قدم**.",
            },
            {
              en: "Use search for words like `stock`, `sku`, `wallet`, `کوپن`.",
              fa: "برای کلماتی مثل `stock`، `sku`، `کیف پول`، `کوپن` از جستجو استفاده کنید.",
            },
          ],
          tips: [
            {
              en: "Master-website members see cross-site selectors; site-scoped members only see their website.",
              fa: "اعضای سایت مستر انتخاب‌گر چندسایته می‌بینند؛ اعضای محدود به سایت فقط سایت خودشان را می‌بینند.",
            },
            {
              en: "Many screens are role-gated; if a menu item is missing, check Roles / Access Levels.",
              fa: "خیلی از صفحات با نقش محدود شده‌اند؛ اگر منو نیست، Roles و Access Levels را چک کنید.",
            },
          ],
          related: ["dashboard", "inbox", "members", "inventory-stock"],
          keywords: [
            { en: "guide start help", fa: "راهنما شروع کمک" },
          ],
        },
        {
          id: "dashboard",
          title: { en: "Dashboard", fa: "داشبورد" },
          adminPath: "/",
          summary: {
            en: "Landing page after login: open tasks, notifications, and store vs website context.",
            fa: "صفحه ورود بعد از لاگین: کارهای باز، اعلان‌ها و زمینه فروشگاه یا وب‌سایت.",
          },
          access: { en: "Any signed-in admin member", fa: "هر عضو ادمین واردشده" },
          purpose: [
            {
              en: "Shows a welcome header and whether you are on a Store-capable site or a pure Website.",
              fa: "خوش‌آمد و اینکه سایت شما فروشگاهی است یا فقط وب‌سایت محتوایی را نشان می‌دهد.",
            },
            {
              en: "Surfaces open operational queues (orders, bank payments, withdrawals) via the open-tasks panel.",
              fa: "صف‌های عملیاتی باز (سفارش، پرداخت بانکی، برداشت) را در پنل کارهای باز نشان می‌دهد.",
            },
            {
              en: "Lists recent activity/notifications with a shortcut to the full Inbox.",
              fa: "فعالیت‌ها و اعلان‌های اخیر را با میانبر به Inbox کامل نشان می‌دهد.",
            },
          ],
          howTo: [
            {
              en: "After login you land on `/`. Review chips for open tasks and unread notifications.",
              fa: "بعد از لاگین روی `/` هستید. چیپ‌های کارهای باز و اعلان‌های خوانده‌نشده را ببینید.",
            },
            {
              en: "Click the inbox chip or “Open inbox” to work through tasks and alerts.",
              fa: "روی چیپ Inbox یا «Open inbox» بزنید تا کارها و هشدارها را انجام دهید.",
            },
            {
              en: "Use the global header search (Admin) or this docs search to jump to a module.",
              fa: "از جستجوی هدر ادمین یا جستجوی همین مستندات به ماژول موردنظر بروید.",
            },
          ],
          tips: [
            {
              en: "Dashboard is a hub, not a full report center; deep work happens in each module.",
              fa: "داشبورد هاب است نه مرکز گزارش کامل؛ کار اصلی داخل هر ماژول انجام می‌شود.",
            },
          ],
          related: ["inbox", "orders", "payments", "withdrawals"],
        },
        {
          id: "inbox",
          title: { en: "Inbox", fa: "صندوق ورودی" },
          adminPath: "/inbox",
          summary: {
            en: "Combined open tasks and notifications in one place.",
            fa: "ترکیب کارهای باز و اعلان‌ها در یک صفحه.",
          },
          purpose: [
            {
              en: "Central place for operational to-dos generated by the system (payments to review, support, etc.).",
              fa: "مرکز کارهای عملیاتی تولیدشده توسط سیستم (پرداخت برای بررسی، پشتیبانی و …).",
            },
            {
              en: "Also holds member notifications so you do not miss alerts.",
              fa: "اعلان‌های عضو را هم نگه می‌دارد تا هشداری از دست نرود.",
            },
          ],
          howTo: [
            {
              en: "Open `/inbox` from the nav or dashboard chip.",
              fa: "از منو یا چیپ داشبورد `/inbox` را باز کنید.",
            },
            {
              en: "Work open tasks first; each task usually deep-links to the related admin screen.",
              fa: "اول کارهای باز را انجام دهید؛ معمولاً هر کار به صفحه ادمین مرتبط لینک می‌شود.",
            },
            {
              en: "Mark or open notifications so unread counts drop on the dashboard.",
              fa: "اعلان‌ها را باز/خوانده کنید تا شمارنده خوانده‌نشده داشبورد کم شود.",
            },
          ],
          related: ["notifications", "dashboard"],
        },
        {
          id: "notifications",
          title: { en: "Notifications", fa: "اعلان‌ها" },
          adminPath: "/notifications",
          summary: {
            en: "Notification history for the signed-in admin member.",
            fa: "تاریخچه اعلان‌ها برای عضو ادمین واردشده.",
          },
          purpose: {
            en: "Dedicated list of admin notifications (separate from full inbox task queues when you only need alerts).",
            fa: "فهرست اختصاصی اعلان‌های ادمین (جدا از صف کارهای Inbox وقتی فقط هشدار می‌خواهید).",
          },
          howTo: [
            {
              en: "Open Notifications from the nav.",
              fa: "اعلان‌ها را از منو باز کنید.",
            },
            {
              en: "Review unread items and open the linked resource when available.",
              fa: "موارد خوانده‌نشده را ببینید و در صورت وجود لینک، منبع مرتبط را باز کنید.",
            },
          ],
          related: ["inbox"],
        },
      ],
    },

    /* ───────────────────── Users ───────────────────── */
    {
      id: "users",
      title: { en: "User management", fa: "مدیریت کاربران" },
      summary: {
        en: "Admin members, storefront customers, roles, policies, login logs.",
        fa: "اعضای ادمین، مشتریان فروشگاه، نقش‌ها، سیاست‌ها و لاگ ورود.",
      },
      pages: [
        {
          id: "members",
          title: { en: "Members", fa: "اعضا (ادمین)" },
          adminPath: "/members",
          summary: {
            en: "People who can sign into the Admin panel (staff / operators).",
            fa: "افرادی که می‌توانند وارد پنل ادمین شوند (کارکنان / اپراتورها).",
          },
          access: { en: "MembersView (+ edit/insert policies as configured)", fa: "MembersView (ویرایش/ایجاد طبق سیاست)" },
          purpose: [
            {
              en: "Create and manage admin-side accounts (not storefront customers).",
              fa: "ایجاد و مدیریت حساب‌های سمت ادمین (نه مشتریان فروشگاه).",
            },
            {
              en: "Assign access via policies/roles so each person only sees allowed menus.",
              fa: "از طریق سیاست/نقش دسترسی بدهید تا هر نفر فقط منوهای مجاز را ببیند.",
            },
          ],
          howTo: [
            {
              en: "Open Members → list shows staff for your scope (master may see more).",
              fa: "Members را باز کنید → فهرست کارکنان در محدوده شما (مستر ممکن است بیشتر ببیند).",
            },
            {
              en: "Create a member, set credentials/profile, and attach the correct policy/roles.",
              fa: "عضو بسازید، مشخصات/ورود را تنظیم کنید و سیاست/نقش درست را وصل کنید.",
            },
            {
              en: "Disable rather than delete when someone leaves, if you need audit continuity.",
              fa: "اگر ردگیری لازم است، به‌جای حذف، حساب را غیرفعال کنید.",
            },
          ],
          tips: [
            {
              en: "Do not confuse Members (admin) with Clients/Customers (storefront buyers).",
              fa: "Members (ادمین) را با Clients/Customers (خریدار فروشگاه) اشتباه نگیرید.",
            },
          ],
          related: ["clients", "policies", "roles", "login-logs"],
        },
        {
          id: "clients",
          title: { en: "Customers (Clients)", fa: "مشتریان" },
          adminPath: "/clients",
          summary: {
            en: "Storefront / website customers: profiles, addresses, wallet linkage.",
            fa: "مشتریان فروشگاه/وب‌سایت: پروفایل، آدرس، ارتباط با کیف پول.",
          },
          purpose: [
            {
              en: "Manage end-users who shop or register on the public site.",
              fa: "مدیریت کاربران نهایی که در سایت عمومی خرید یا ثبت‌نام می‌کنند.",
            },
            {
              en: "Master view can span all websites; site operators see their own customers.",
              fa: "نمای مستر می‌تواند همه سایت‌ها را ببیند؛ اپراتور سایت فقط مشتریان خودش را می‌بیند.",
            },
          ],
          howTo: [
            {
              en: "Open Customers → search/filter the grid.",
              fa: "Customers را باز کنید → در گرید جستجو/فیلتر کنید.",
            },
            {
              en: "Create or edit a client profile; maintain addresses used at checkout.",
              fa: "پروفایل مشتری را بسازید یا ویرایش کنید؛ آدرس‌های تسویه را نگه دارید.",
            },
            {
              en: "From wallet-related flows you may jump to client wallet detail when finance features are enabled.",
              fa: "در جریان‌های مالی در صورت فعال بودن، به جزئیات کیف پول مشتری بروید.",
            },
          ],
          related: ["members", "orders", "withdrawals", "wallet-detail"],
        },
        {
          id: "policies",
          title: { en: "Access levels (Policies)", fa: "سطح دسترسی (سیاست‌ها)" },
          adminPath: "/policies",
          summary: {
            en: "Named permission bundles assigned to members (what menus/actions they can use).",
            fa: "بسته‌های مجوز نام‌دار که به اعضا داده می‌شود (کدام منو/عمل).",
          },
          purpose: {
            en: "Policies group fine-grained role keys (view/edit products, payments, etc.) into reusable access levels.",
            fa: "سیاست‌ها کلیدهای نقش ریز (مشاهده/ویرایش محصول، پرداخت و …) را در سطوح قابل‌استفاده دوباره جمع می‌کنند.",
          },
          howTo: [
            {
              en: "Open Access Levels → create a policy for a job function (e.g. Content Editor, Finance).",
              fa: "Access Levels را باز کنید → برای یک نقش شغلی (مثلاً ادیتور محتوا، مالی) سیاست بسازید.",
            },
            {
              en: "Toggle the permission keys required, save, then assign the policy to members.",
              fa: "کلیدهای مجوز لازم را روشن کنید، ذخیره کنید، سپس سیاست را به اعضا بدهید.",
            },
          ],
          tips: [
            {
              en: "Prefer least privilege: grant view without edit when possible.",
              fa: "حداقل دسترسی: تا جای ممکن view بدون edit بدهید.",
            },
          ],
          related: ["roles", "members"],
        },
        {
          id: "roles",
          title: { en: "Roles", fa: "نقش‌ها" },
          adminPath: "/roles",
          summary: {
            en: "Super-admin catalog of role keys used by policies and authorization.",
            fa: "کاتالوگ کلید نقش‌ها (سوپرادمین) که سیاست‌ها و احراز از آن استفاده می‌کنند.",
          },
          access: { en: "Super admin / master only", fa: "فقط سوپرادمین / مستر" },
          purpose: {
            en: "Defines the building blocks of authorization. Day-to-day staff usually get Policies, not raw role editing.",
            fa: "اجزای پایه مجوزدهی را تعریف می‌کند. کار روزمره معمولاً با Policies است نه ویرایش خام نقش.",
          },
          howTo: [
            {
              en: "Only on master: open Roles to review or maintain role definitions.",
              fa: "فقط روی مستر: Roles را برای مرور یا نگهداری تعریف نقش‌ها باز کنید.",
            },
            {
              en: "After changing roles, revisit Policies that reference them and re-test member logins.",
              fa: "بعد از تغییر نقش‌ها، سیاست‌های وابسته را بازبینی و ورود اعضا را تست کنید.",
            },
          ],
          related: ["policies", "members"],
        },
        {
          id: "login-logs",
          title: { en: "Login logs", fa: "لاگ ورود" },
          adminPath: "/login-logs",
          summary: {
            en: "Audit trail of admin sign-in attempts and related security events.",
            fa: "ردپای تلاش‌های ورود ادمین و رویدادهای امنیتی مرتبط.",
          },
          purpose: {
            en: "Investigate suspicious access, failed passwords, or confirm who signed in when.",
            fa: "بررسی دسترسی مشکوک، رمز ناموفق، یا تأیید اینکه چه کسی چه زمانی وارد شده.",
          },
          howTo: [
            {
              en: "Open Login Logs and filter by date/member when investigating an incident.",
              fa: "Login Logs را باز کنید و هنگام بررسی حادثه بر اساس تاریخ/عضو فیلتر کنید.",
            },
          ],
          related: ["members", "settings"],
        },
      ],
    },

    /* ───────────────────── Website ───────────────────── */
    {
      id: "website",
      title: { en: "Website", fa: "وب‌سایت" },
      summary: {
        en: "Multi-site management, SEO, scripts, IP allowlist, languages, API keys.",
        fa: "مدیریت چندسایته، SEO، اسکریپت، IP، زبان‌ها و کلید API.",
      },
      pages: [
        {
          id: "websites",
          title: { en: "Websites", fa: "وب‌سایت‌ها" },
          adminPath: "/websites",
          summary: {
            en: "Super-admin list of all websites/stores hosted on this Dotnetable instance.",
            fa: "فهرست همه وب‌سایت‌ها/فروشگاه‌های این نمونه Dotnetable (سوپرادمین).",
          },
          access: { en: "Super admin / master only", fa: "فقط سوپرادمین / مستر" },
          purpose: [
            {
              en: "Create and configure websites (domains, defaults, shop capability).",
              fa: "ایجاد و پیکربندی وب‌سایت‌ها (دامنه، پیش‌فرض‌ها، قابلیت فروشگاه).",
            },
            {
              en: "Master website (often id 1) is special for cross-site administration.",
              fa: "وب‌سایت مستر (معمولاً id=1) برای مدیریت بین‌سایتی خاص است.",
            },
          ],
          howTo: [
            {
              en: "Open Websites → create or edit a site.",
              fa: "Websites را باز کنید → سایت بسازید یا ویرایش کنید.",
            },
            {
              en: "Set identity fields and operational defaults, then configure languages/SEO under Website submenu for that context.",
              fa: "هویت و پیش‌فرض‌های عملیاتی را بگذارید؛ بعد زبان/SEO را از زیرمنوی Website همان زمینه تنظیم کنید.",
            },
            {
              en: "Set **Product code prefix** (1–3 letters, default `DN`) so products are addressed as `DN-{ProductID}` on invoices and storefront URLs.",
              fa: "**پیشوند کد کالا** (۱–۳ حرف، پیش‌فرض `DN`) را بگذارید تا کالاها به شکل `DN-{ProductID}` در فاکتور و آدرس فروشگاه قابل آدرس‌دهی باشند.",
            },
            {
              en: "Under **Tax & seller registration**, set tax flags and **Report offline / social orders to tax by default** (when off, Instagram/WhatsApp admin orders default to excluded from VAT filings; each order can override).",
              fa: "در **مالیات و ثبت فروشنده**، پرچم‌های مالیات و **اعلام پیش‌فرض سفارش‌های آفلاین/اجتماعی به مالیات** را تنظیم کنید (اگر خاموش باشد، سفارش‌های اینستاگرام/واتساپ ادمین پیش‌فرض از VAT خارج‌اند؛ هر سفارش override دارد).",
            },
          ],
          related: ["website-seo", "website-languages", "website-api-key", "orders", "tax"],
        },
        {
          id: "website-ips",
          title: { en: "IP whitelist", fa: "لیست سفید IP" },
          adminPath: "/website/ips",
          summary: {
            en: "Restrict sensitive admin/API access by IP when configured.",
            fa: "محدودکردن دسترسی حساس ادمین/API بر اساس IP (در صورت پیکربندی).",
          },
          purpose: {
            en: "Extra security layer: only listed IPs may reach certain endpoints for the website.",
            fa: "لایه امنیتی اضافه: فقط IPهای لیست‌شده به برخی endpointهای سایت برسند.",
          },
          howTo: [
            {
              en: "Add office/VPN IPs carefully; a wrong list can lock operators out.",
              fa: "IP دفتر/VPN را با دقت اضافه کنید؛ لیست اشتباه اپراتور را قفل می‌کند.",
            },
            {
              en: "Keep at least one known-good admin path before enabling strict enforcement.",
              fa: "قبل از اعمال سخت‌گیرانه، حداقل یک مسیر ادمین مطمئن نگه دارید.",
            },
          ],
          tips: [
            {
              en: "Dynamic home IPs break allowlists — prefer stable egress or VPN.",
              fa: "IP خانگی پویا لیست سفید را می‌شکند — egress ثابت یا VPN بهتر است.",
            },
          ],
          related: ["websites", "settings"],
        },
        {
          id: "website-scripts",
          title: { en: "Scripts", fa: "اسکریپت‌ها" },
          adminPath: "/website/scripts",
          summary: {
            en: "Custom head/body scripts (analytics, pixels, widgets) per website.",
            fa: "اسکریپت‌های سفارشی head/body (آنالیتیکس، پیکسل، ویجت) per سایت.",
          },
          purpose: {
            en: "Inject third-party or custom JS/CSS snippets without redeploying the frontend theme.",
            fa: "تزریق JS/CSS شخص‌ثالث یا سفارشی بدون redeploy تم فرانت.",
          },
          howTo: [
            {
              en: "Create a script entry, choose placement, paste trusted content, enable it.",
              fa: "ورودی اسکریپت بسازید، محل درج را انتخاب کنید، محتوای معتبر بچسبانید و فعال کنید.",
            },
            {
              en: "Verify on the storefront after save (cache/CDN may delay visibility).",
              fa: "بعد از ذخیره روی استورفرانت چک کنید (کش/CDN ممکن است تأخیر بدهد).",
            },
          ],
          tips: [
            {
              en: "Never paste untrusted scripts — XSS risk on the public site.",
              fa: "اسکریپت نامعتبر نگذارید — خطر XSS روی سایت عمومی.",
            },
          ],
          related: ["website-seo", "themes"],
        },
        {
          id: "website-seo",
          title: { en: "SEO settings", fa: "تنظیمات SEO" },
          adminPath: "/website/seo",
          summary: {
            en: "Site-level SEO defaults: titles, meta, indexing helpers.",
            fa: "پیش‌فرض‌های SEO سطح سایت: عنوان، متا، کمک‌های ایندکس.",
          },
          purpose: {
            en: "Configure search-engine facing defaults for the website (complemented by page/post SEO fields).",
            fa: "پیکربندی پیش‌فرض‌های موتور جستجو برای سایت (مکمل فیلدهای SEO صفحه/پست).",
          },
          howTo: [
            {
              en: "Open SEO Settings and fill default title/description patterns and related options.",
              fa: "SEO Settings را باز کنید و الگوی عنوان/توضیح پیش‌فرض و گزینه‌های مرتبط را پر کنید.",
            },
            {
              en: "Save, then spot-check a public page’s meta tags.",
              fa: "ذخیره کنید و متای یک صفحه عمومی را نمونه چک کنید.",
            },
          ],
          related: ["pages", "posts", "website-social"],
        },
        {
          id: "website-social",
          title: { en: "Social links", fa: "لینک‌های اجتماعی" },
          adminPath: "/website/social",
          summary: {
            en: "Profile URLs for social networks shown on the storefront/theme.",
            fa: "آدرس پروفایل شبکه‌های اجتماعی برای نمایش در استورفرانت/تم.",
          },
          purpose: {
            en: "Central place to maintain Instagram, X, LinkedIn, etc. without hardcoding in templates.",
            fa: "نگهداری مرکزی اینستاگرام، X، لینکدین و … بدون هاردکد در قالب.",
          },
          howTo: [
            {
              en: "Add or edit social rows with the correct absolute URLs.",
              fa: "ردیف‌های اجتماعی را با URL مطلق درست اضافه/ویرایش کنید.",
            },
          ],
          related: ["website-seo", "menus"],
        },
        {
          id: "website-api-key",
          title: { en: "Website API key", fa: "کلید API وب‌سایت" },
          adminPath: "/website/api-key",
          summary: {
            en: "Credentials the public site / integrations use to call Dotnetable APIs for this website.",
            fa: "اعتبارنامه‌هایی که سایت عمومی/یکپارچه‌سازی‌ها برای API این وب‌سایت استفاده می‌کنند.",
          },
          purpose: {
            en: "Issue and rotate API keys scoped to a website so frontends can authenticate safely.",
            fa: "صدور و چرخش کلید API محدود به وب‌سایت تا فرانت امن احراز هویت کند.",
          },
          howTo: [
            {
              en: "Open Website API Key, generate/rotate keys, store secrets outside git.",
              fa: "Website API Key را باز کنید، کلید بسازید/بچرخانید، secret را خارج از git نگه دارید.",
            },
            {
              en: "Update the consuming app configuration immediately after rotation.",
              fa: "بلافاصله بعد از چرخش، پیکربندی اپ مصرف‌کننده را به‌روز کنید.",
            },
          ],
          tips: [
            {
              en: "Treat keys like passwords; revoke compromised keys at once.",
              fa: "کلید مثل رمز است؛ کلید لو رفته را فوراً باطل کنید.",
            },
          ],
          related: ["websites", "settings"],
        },
        {
          id: "website-languages",
          title: { en: "Website languages", fa: "زبان‌های وب‌سایت" },
          adminPath: "/website/languages",
          summary: {
            en: "Languages enabled for a single website’s storefront and content editors.",
            fa: "زبان‌های فعال برای استورفرانت و ادیتور محتوای یک وب‌سایت.",
          },
          purpose: [
            {
              en: "Independent from the global Admin language catalog (Initial Data → Languages).",
              fa: "مستقل از کاتالوگ زبان ادمین (Initial Data → Languages).",
            },
            {
              en: "A new site typically starts with only its default language.",
              fa: "سایت جدید معمولاً فقط با زبان پیش‌فرض شروع می‌شود.",
            },
          ],
          howTo: [
            {
              en: "Enable languages needed for content; set the default carefully.",
              fa: "زبان‌های لازم محتوا را فعال کنید؛ پیش‌فرض را با دقت بگذارید.",
            },
            {
              en: "Then translate content entities (posts, products, menus) per language.",
              fa: "سپس موجودیت‌های محتوا (پست، محصول، منو) را per زبان ترجمه کنید.",
            },
          ],
          related: ["website-translations", "languages-catalog"],
        },
        {
          id: "website-translations",
          title: { en: "Website translations", fa: "ترجمه‌های وب‌سایت" },
          adminPath: "/website/translations",
          summary: {
            en: "UI/string translations for the selected website storefront.",
            fa: "ترجمه‌های رشته/UI برای استورفرانت وب‌سایت انتخاب‌شده.",
          },
          purpose: {
            en: "Override or fill localization keys used by the public site for each enabled language.",
            fa: "کلیدهای بومی‌سازی سایت عمومی را برای هر زبان فعال پر یا override کنید.",
          },
          howTo: [
            {
              en: "Open Translations for the website, filter keys, edit values per language, save.",
              fa: "Translations وب‌سایت را باز کنید، کلیدها را فیلتر کنید، per زبان ویرایش و ذخیره کنید.",
            },
          ],
          related: ["website-languages", "admin-translations"],
        },
      ],
    },

    /* ───────────────────── Content ───────────────────── */
    {
      id: "content",
      title: { en: "Content", fa: "محتوا" },
      summary: {
        en: "CMS: pages, posts, menus, slideshows, forms, taxonomy, redirects, themes.",
        fa: "CMS: صفحه، پست، منو، اسلایدشو، فرم، طبقه‌بندی، ریدایرکت، تم.",
      },
      pages: [
        {
          id: "pages",
          title: { en: "Pages", fa: "صفحات" },
          adminPath: "/content/pages",
          summary: {
            en: "Static/content pages (About, Terms, landing pages) with rich text and SEO.",
            fa: "صفحات ثابت/محتوایی (درباره ما، قوانین، لندینگ) با متن غنی و SEO.",
          },
          purpose: {
            en: "Publish long-lived pages that are not blog posts; used by menus and redirects.",
            fa: "انتشار صفحات ماندگار غیر از پست وبلاگ؛ مصرف در منو و ریدایرکت.",
          },
          howTo: [
            {
              en: "Open Pages → New. Fill title/slug, body (editor), SEO fields, publish state.",
              fa: "Pages → New. عنوان/اسلاگ، بدنه (ادیتور)، SEO و وضعیت انتشار را پر کنید.",
            },
            {
              en: "Link the page from Menus when it should appear in navigation.",
              fa: "اگر باید در ناوبری باشد، از Menus به آن لینک دهید.",
            },
          ],
          related: ["menus", "redirects", "themes"],
        },
        {
          id: "posts",
          title: { en: "Posts", fa: "پست‌ها" },
          adminPath: "/content/posts",
          summary: {
            en: "Blog/articles with categories, tags, post types, and moderation-related flows.",
            fa: "مقالات/بلاگ با دسته، تگ، نوع پست و جریان‌های مرتبط با نظارت.",
          },
          purpose: {
            en: "Manage time-based or article content for the storefront/blog area.",
            fa: "مدیریت محتوای مقاله‌ای یا زمان‌دار برای بلاگ/استورفرانت.",
          },
          howTo: [
            {
              en: "Ensure categories/tags/post types exist first if your workflow needs them.",
              fa: "اگر لازم است، اول دسته/تگ/نوع پست را بسازید.",
            },
            {
              en: "Create a post, set taxonomy, body, media, SEO, then publish.",
              fa: "پست بسازید، طبقه‌بندی، بدنه، مدیا، SEO را بگذارید و منتشر کنید.",
            },
          ],
          related: ["content-categories", "content-tags", "post-types", "media-library"],
        },
        {
          id: "content-categories",
          title: { en: "Content categories", fa: "دسته‌های محتوا" },
          adminPath: "/content/categories",
          summary: {
            en: "Taxonomy for posts (not product categories).",
            fa: "طبقه‌بندی پست‌ها (نه دسته‌های محصول).",
          },
          purpose: {
            en: "Group blog/content posts for navigation and filtering on the public site.",
            fa: "گروه‌بندی پست‌ها برای ناوبری و فیلتر در سایت عمومی.",
          },
          howTo: [
            {
              en: "Create categories before bulk-authoring posts; assign posts on the post editor.",
              fa: "قبل از تولید انبوه پست، دسته بسازید؛ در ادیتور پست انتساب دهید.",
            },
          ],
          tips: [
            {
              en: "Product categories live under Catalog → Categories (`/catalog/categories`).",
              fa: "دسته‌های محصول در Catalog → Categories (`/catalog/categories`) هستند.",
            },
          ],
          related: ["posts", "content-tags", "catalog-categories"],
        },
        {
          id: "content-tags",
          title: { en: "Tags", fa: "برچسب‌ها" },
          adminPath: "/content/tags",
          summary: {
            en: "Free-form labels for posts to improve discovery.",
            fa: "برچسب‌های آزاد برای کشف‌پذیری بهتر پست‌ها.",
          },
          purpose: {
            en: "Lightweight taxonomy complementary to categories.",
            fa: "طبقه‌بندی سبک مکمل دسته‌ها.",
          },
          howTo: [
            {
              en: "Maintain the tag list; attach tags while editing posts.",
              fa: "فهرست تگ را نگه دارید؛ هنگام ویرایش پست وصل کنید.",
            },
          ],
          related: ["posts", "content-categories"],
        },
        {
          id: "post-types",
          title: { en: "Post types", fa: "انواع پست" },
          adminPath: "/content/post-types",
          summary: {
            en: "Define kinds of posts (news, tutorial, …) beyond a single blog stream.",
            fa: "تعریف گونه‌های پست (خبر، آموزش، …) فراتر از یک جریان بلاگ.",
          },
          purpose: {
            en: "Structure content models so the frontend can render different post families.",
            fa: "ساختار مدل محتوا تا فرانت خانواده‌های مختلف پست را رندر کند.",
          },
          howTo: [
            {
              en: "Create post types you need, then select them on each post.",
              fa: "نوع‌های لازم را بسازید، سپس روی هر پست انتخاب کنید.",
            },
          ],
          related: ["posts"],
        },
        {
          id: "menus",
          title: { en: "Menus", fa: "منوها" },
          adminPath: "/menus",
          summary: {
            en: "Navigation trees (header/footer/…) with nested items and icons.",
            fa: "درخت ناوبری (هدر/فوتر/…) با آیتم تو در تو و آیکن.",
          },
          purpose: {
            en: "Build site navigation linking to pages, posts, custom URLs, or app routes.",
            fa: "ساخت ناوبری سایت با لینک به صفحه، پست، URL سفارشی یا مسیر اپ.",
          },
          howTo: [
            {
              en: "Create a menu, open it, add items in a tree, reorder, set icons/labels per language.",
              fa: "منو بسازید، باز کنید، آیتم درختی اضافه کنید، ترتیب و آیکن/برچسب per زبان را تنظیم کنید.",
            },
            {
              en: "Publish/enable the menu so the theme can load it by key/name.",
              fa: "منو را فعال/منتشر کنید تا تم با کلید/نام بارگذاری‌اش کند.",
            },
          ],
          related: ["pages", "posts", "slideshows"],
        },
        {
          id: "slideshows",
          title: { en: "Slideshows", fa: "اسلایدشوها" },
          adminPath: "/slideshows",
          summary: {
            en: "Hero carousels and promotional slides with media and links.",
            fa: "کاروسل هیرو و اسلایدهای تبلیغاتی با مدیا و لینک.",
          },
          purpose: {
            en: "Manage marketing banners without code changes.",
            fa: "مدیریت بنرهای بازاریابی بدون تغییر کد.",
          },
          howTo: [
            {
              en: "Create a slideshow, add slides (image, title, link), order them, enable.",
              fa: "اسلایدشو بسازید، اسلاید (تصویر، عنوان، لینک) اضافه کنید، مرتب و فعال کنید.",
            },
          ],
          related: ["media-library", "menus"],
        },
        {
          id: "forms",
          title: { en: "Forms & surveys", fa: "فرم‌ها و نظرسنجی" },
          adminPath: "/forms",
          summary: {
            en: "Form builder, field definitions, and submission reports.",
            fa: "فرم‌ساز، تعریف فیلد و گزارش ارسال‌ها.",
          },
          purpose: {
            en: "Collect structured input from the public site (contact variants, surveys) beyond simple contact messages.",
            fa: "جمع‌آوری ورودی ساخت‌یافته از سایت عمومی (انواع تماس، نظرسنجی) فراتر از پیام تماس ساده.",
          },
          howTo: [
            {
              en: "Create a form, open the builder, add fields, configure validation, publish.",
              fa: "فرم بسازید، بیلدر را باز کنید، فیلد اضافه کنید، اعتبارسنجی را تنظیم و منتشر کنید.",
            },
            {
              en: "Use Form Report to export/review submissions.",
              fa: "از Form Report برای مرور/خروجی ارسال‌ها استفاده کنید.",
            },
          ],
          related: ["contact-messages", "email-templates"],
        },
        {
          id: "themes",
          title: { en: "Themes", fa: "تم‌ها" },
          adminPath: "/themes",
          summary: {
            en: "Select/manage presentation themes available to websites.",
            fa: "انتخاب/مدیریت تم‌های نمایشی در دسترس وب‌سایت‌ها.",
          },
          purpose: {
            en: "Switch the visual shell of the public site among registered themes.",
            fa: "پوسته بصری سایت عمومی را بین تم‌های ثبت‌شده عوض کنید.",
          },
          howTo: [
            {
              en: "Open Themes, review available skins, activate the one for the site.",
              fa: "Themes را باز کنید، پوسته‌ها را ببینید و یکی را برای سایت فعال کنید.",
            },
          ],
          related: ["website-scripts", "pages"],
        },
        {
          id: "redirects",
          title: { en: "Redirects", fa: "ریدایرکت‌ها" },
          adminPath: "/content/redirects",
          summary: {
            en: "HTTP redirects for moved URLs (SEO-safe path changes).",
            fa: "ریدایرکت HTTP برای URLهای جابه‌جا شده (تغییر مسیر دوستانه SEO).",
          },
          purpose: {
            en: "Map old paths to new ones after renames or migrations so links and search rankings do not break.",
            fa: "مسیر قدیمی را به جدید نگاشت کنید تا لینک‌ها و رتبه جستجو نشکند.",
          },
          howTo: [
            {
              en: "Add source path → target path, choose redirect type if offered, save.",
              fa: "مسیر مبدأ → مقصد را اضافه کنید، نوع ریدایرکت را اگر هست انتخاب و ذخیره کنید.",
            },
          ],
          related: ["pages", "website-seo"],
        },
        {
          id: "moderation-reviews",
          title: { en: "Review moderation", fa: "نظارت نظرات" },
          adminPath: "/moderation/reviews",
          summary: {
            en: "Approve/reject product (or content) reviews submitted by customers.",
            fa: "تأیید/رد نظرات محصول (یا محتوا) ارسال‌شده توسط مشتریان.",
          },
          purpose: {
            en: "Quality control before reviews go public.",
            fa: "کنترل کیفیت قبل از عمومی شدن نظرات.",
          },
          howTo: [
            {
              en: "Open the reviews queue, read each item, approve or reject.",
              fa: "صف نظرات را باز کنید، هر مورد را بخوانید، تأیید یا رد کنید.",
            },
          ],
          related: ["moderation-questions", "products"],
        },
        {
          id: "moderation-questions",
          title: { en: "Q&A moderation", fa: "نظارت پرسش و پاسخ" },
          adminPath: "/moderation/questions",
          summary: {
            en: "Moderate product questions and answers from the storefront.",
            fa: "نظارت پرسش و پاسخ محصول از استورفرانت.",
          },
          purpose: {
            en: "Keep Q&A helpful and free of spam before publishing.",
            fa: "Q&A را قبل از انتشار مفید و بدون اسپم نگه دارید.",
          },
          howTo: [
            {
              en: "Work the questions queue similarly to reviews: accept, answer context, or reject.",
              fa: "صف پرسش‌ها را مانند نظرات: قبول، پاسخ‌گویی، یا رد.",
            },
          ],
          related: ["moderation-reviews", "products"],
        },
      ],
    },

    /* ───────────────────── Catalog ───────────────────── */
    {
      id: "catalog",
      title: { en: "Catalog", fa: "کاتالوگ" },
      summary: {
        en: "Products, categories, attributes, brands, warranties, vendors.",
        fa: "محصول، دسته، ویژگی، برند، گارانتی، فروشنده.",
      },
      pages: [
        {
          id: "products",
          title: { en: "Products", fa: "محصولات" },
          adminPath: "/catalog/products",
          summary: {
            en: "Core catalog: physical/digital products, variants/SKU, pricing, media, stock linkage.",
            fa: "هسته کاتالوگ: محصول فیزیکی/دیجیتال، واریانت/SKU، قیمت، مدیا، پیوند موجودی.",
          },
          purpose: [
            {
              en: "Create sellable catalog items with type (physical, download, code, service).",
              fa: "ایجاد اقلام قابل‌فروش با نوع (فیزیکی، دانلود، کد، سرویس).",
            },
            {
              en: "Variants carry SKU and connect to inventory and vendor listings.",
              fa: "واریانت‌ها SKU دارند و به موجودی و لیستینگ فروشنده وصل می‌شوند.",
            },
            {
              en: "Each product has a stable **product code** `{prefix}-{ProductID}` (default `DN-42`) — not the variant SKU. Prefix is set on the website (1–3 letters). Usable in admin search, invoices, and storefront URLs `/product/DN-42`.",
              fa: "هر کالا یک **کد کالا** پایدار `{پیشوند}-{ProductID}` دارد (پیش‌فرض `DN-42`) — نه SKU واریانت. پیشوند در وب‌سایت (۱–۳ حرف) تنظیم می‌شود. در جستجوی ادمین، فاکتور و آدرس `/product/DN-42` قابل استفاده است.",
            },
            {
              en: "Pricing supports compare-at and history tools from the product editor.",
              fa: "قیمت‌گذاری compare-at و ابزار تاریخچه را از ادیتور محصول پشتیبانی می‌کند.",
            },
          ],
          howTo: [
            {
              en: "Prerequisites: categories, brands, attributes, warranties, vendors as needed.",
              fa: "پیش‌نیاز: دسته، برند، ویژگی، گارانتی، فروشنده در صورت نیاز.",
            },
            {
              en: "Products → New. Fill General (title, slug, type, descriptions), taxonomy, variants/SKU, prices, media.",
              fa: "Products → New. General (عنوان، اسلاگ، نوع، توضیح)، طبقه‌بندی، واریانت/SKU، قیمت، مدیا را پر کنید.",
            },
            {
              en: "Save. For physical goods, ensure inventory/vendor stock is set so items are sellable.",
              fa: "ذخیره. برای کالای فیزیکی موجودی/استوک فروشنده را بگذارید تا قابل‌فروش شود.",
            },
            {
              en: "Use **Preview** (list eye icon, edit header, or invoice/order line link) at `/catalog/products/{id}/preview?variantId=` — storefront-like view of gallery, description, variants, attributes, warranties. Works for **unpublished / inactive** products so you can QA before publish.",
              fa: "از **پیش‌نمایش** (آیکون چشم در لیست، هدر ویرایش، یا لینک خط فاکتور/سفارش) در `/catalog/products/{id}/preview?variantId=` استفاده کنید — نمای شبیه فروشگاه: گالری، توضیح، واریانت، ویژگی، گارانتی. برای محصولات **unpublished / غیرفعال** هم کار می‌کند تا قبل از انتشار بررسی کنید.",
            },
            {
              en: "Optional: change product code prefix under Websites → edit site (default **DN**).",
              fa: "اختیاری: پیشوند کد کالا را در Websites → ویرایش سایت عوض کنید (پیش‌فرض **DN**).",
            },
          ],
          sections: [
            {
              title: { en: "Product types", fa: "انواع محصول" },
              items: [
                {
                  en: "Physical — requires shipping and stock thinking.",
                  fa: "فیزیکی — نیاز به ارسال و فکر موجودی.",
                },
                {
                  en: "Digital download / code / service — delivery differs; stock rules may differ.",
                  fa: "دانلود دیجیتال / کد / سرویس — تحویل فرق دارد؛ قوانین موجودی ممکن است فرق کند.",
                },
              ],
            },
          ],
          tips: [
            {
              en: "Slug becomes the URL key; edit carefully after publish and use Redirects if changed.",
              fa: "اسلاگ کلید URL است؛ بعد از انتشار با احتیاط عوض کنید و در صورت نیاز Redirect بگذارید.",
            },
            {
              en: "Without store listings / on-hand stock, units may not be sellable (see Inventory).",
              fa: "بدون لیستینگ فروشگاه / موجودی on-hand ممکن است قابل‌فروش نباشد (ببینید Inventory).",
            },
          ],
          relatedApi: ["products", "inventory"],
          related: ["catalog-categories", "attributes", "brands", "vendors", "inventory-stock"],
          keywords: [
            { en: "sku variant price", fa: "کد کالا واریانت قیمت" },
          ],
        },
        {
          id: "catalog-categories",
          title: { en: "Product categories", fa: "دسته‌های محصول" },
          adminPath: "/catalog/categories",
          summary: {
            en: "Hierarchical product taxonomy and attribute assignment per category.",
            fa: "طبقه‌بندی سلسله‌مراتبی محصول و انتساب ویژگی به هر دسته.",
          },
          purpose: {
            en: "Organize the shop catalog and control which attributes appear for products in a category.",
            fa: "سازمان‌دهی کاتالوگ فروشگاه و کنترل اینکه کدام ویژگی‌ها برای محصولات یک دسته ظاهر شوند.",
          },
          howTo: [
            {
              en: "Create categories (tree if supported), then attach attribute definitions used at product edit time.",
              fa: "دسته بسازید (درختی اگر هست)، سپس تعریف ویژگی‌های لازم برای ویرایش محصول را وصل کنید.",
            },
          ],
          tips: [
            {
              en: "Different from Content → Categories (`/content/categories`).",
              fa: "متفاوت از Content → Categories (`/content/categories`).",
            },
          ],
          related: ["products", "attributes"],
        },
        {
          id: "attributes",
          title: { en: "Attributes", fa: "ویژگی‌ها" },
          adminPath: "/catalog/attributes",
          summary: {
            en: "Attribute definitions (color, size, material, …) reused across categories/products.",
            fa: "تعریف ویژگی‌ها (رنگ، سایز، جنس، …) قابل‌استفاده در دسته/محصول.",
          },
          purpose: {
            en: "Standardize product specs and variant option axes.",
            fa: "استانداردسازی مشخصات محصول و محورهای گزینه واریانت.",
          },
          howTo: [
            {
              en: "Define attributes first, map them onto categories, then fill values on products/variants.",
              fa: "اول ویژگی تعریف کنید، به دسته نگاشت کنید، بعد روی محصول/واریانت مقدار بدهید.",
            },
          ],
          related: ["catalog-categories", "products"],
        },
        {
          id: "brands",
          title: { en: "Brands", fa: "برندها" },
          adminPath: "/catalog/brands",
          summary: {
            en: "Brand master data attached to products for filtering and merchandising.",
            fa: "داده اصلی برند متصل به محصول برای فیلتر و فروش.",
          },
          purpose: {
            en: "Maintain brand names/logos used in the catalog.",
            fa: "نگهداری نام/لوگوی برندهای کاتالوگ.",
          },
          howTo: [
            {
              en: "Create brands, then select them on product edit.",
              fa: "برند بسازید، سپس در ویرایش محصول انتخاب کنید.",
            },
          ],
          related: ["products"],
        },
        {
          id: "warranties",
          title: { en: "Warranties", fa: "گارانتی‌ها" },
          adminPath: "/catalog/warranties",
          summary: {
            en: "Warranty plans you can attach to products.",
            fa: "طرح‌های گارانتی قابل اتصال به محصول.",
          },
          purpose: {
            en: "Describe guarantee duration/terms shown with catalog items.",
            fa: "مدت/شرایط ضمانت نمایش‌داده‌شده کنار اقلام کاتالوگ.",
          },
          howTo: [
            {
              en: "Define warranty records, assign on products as needed.",
              fa: "رکورد گارانتی تعریف کنید و در صورت نیاز به محصول وصل کنید.",
            },
          ],
          related: ["products"],
        },
        {
          id: "vendors",
          title: { en: "Vendors", fa: "فروشندگان" },
          adminPath: "/catalog/vendors",
          summary: {
            en: "Sellers on a marketplace-style site: display vendors, member-managed vendors, or linked websites with virtual credit.",
            fa: "فروشندگان در سایت مارکت‌پلیس: نمایشی، مدیریت‌شده توسط عضو، یا وب‌سایت لینک‌شده با اعتبار مجازی.",
          },
          purpose: [
            {
              en: "Represent who sells a listing — essential for multi-vendor and inter-site commerce.",
              fa: "مشخص می‌کند چه کسی لیستینگ را می‌فروشد — ضروری برای چندفروشنده و تجارت بین‌سایتی.",
            },
            {
              en: "Vendor products (listings) carry price/stock that feed sellable inventory.",
              fa: "محصولات فروشنده (لیستینگ) قیمت/موجودی دارند که موجودی قابل‌فروش را تغذیه می‌کند.",
            },
            {
              en: "Virtual credit and settlements relate to vendor economics.",
              fa: "اعتبار مجازی و تسویه‌ها به اقتصاد فروشنده مربوط است.",
            },
          ],
          howTo: [
            {
              en: "Create a vendor with the correct type, then open Vendor Products to attach catalog products.",
              fa: "فروشنده با نوع درست بسازید، سپس Vendor Products را برای اتصال محصولات کاتالوگ باز کنید.",
            },
            {
              en: "Maintain listing price and stock; inventory on-hand syncs from store listings where applicable.",
              fa: "قیمت و استوک لیستینگ را نگه دارید؛ on-hand موجودی در صورت کاربرد از لیستینگ‌ها sync می‌شود.",
            },
            {
              en: "Use vendor credit screens when the business model uses internal credit (separate from formal Settlements).",
              fa: "اگر مدل کسب‌وکار اعتبار داخلی دارد از صفحات اعتبار فروشنده استفاده کنید (جدا از Settlements رسمی).",
            },
          ],
          tips: [
            {
              en: "Marketplace seller accounts may see a reduced admin surface (their products/orders).",
              fa: "حساب فروشنده مارکت‌پلیس ممکن است سطح ادمین محدود (محصول/سفارش خودش) ببیند.",
            },
          ],
          related: ["products", "inventory-stock", "settlements", "vendor-products", "vendor-credit"],
        },
        {
          id: "vendor-products",
          title: { en: "Vendor products (listings)", fa: "محصولات فروشنده (لیستینگ)" },
          adminPath: "/catalog/vendors/{id}/products",
          summary: {
            en: "Per-vendor sellable listings: which products a vendor offers, at what price/stock.",
            fa: "لیستینگ قابل‌فروش per فروشنده: چه محصولی، با چه قیمت/موجودی.",
          },
          purpose: {
            en: "Bridge between catalog products and a concrete seller’s offer — this is what customers actually buy in multi-vendor setups.",
            fa: "پل بین محصول کاتالوگ و پیشنهاد فروشنده — در چندفروشنده همان چیزی است که مشتری می‌خرد.",
          },
          howTo: [
            {
              en: "From Vendors, open a vendor’s products page.",
              fa: "از Vendors صفحه محصولات یک فروشنده را باز کنید.",
            },
            {
              en: "Add product/variant listings, set commercial fields and stock.",
              fa: "لیستینگ محصول/واریانت اضافه کنید، فیلدهای تجاری و موجودی را بگذارید.",
            },
          ],
          related: ["vendors", "products", "inventory-stock"],
        },
        {
          id: "vendor-credit",
          title: { en: "Vendor credit", fa: "اعتبار فروشنده" },
          adminPath: "/catalog/vendors/{id}/credit",
          summary: {
            en: "Virtual credit between host and linked site/vendor (catalog visibility while credit remains). Not the same as Finance → Settlements.",
            fa: "اعتبار مجازی بین میزبان و سایت/فروشنده لینک‌شده (نمایش کاتالوگ تا وقتی اعتبار هست). با Finance → Settlements یکی نیست.",
          },
          howTo: [
            {
              en: "Open Vendors → credit for a vendor. Grant/adjust amount, review history.",
              fa: "Vendors → credit یک فروشنده. مبلغ grant/adjust و تاریخچه را ببینید.",
            },
            {
              en: "Attach credit agreements / settlement scans with **Upload scan** on the same page.",
              fa: "قرارداد اعتبار / اسکن تسویه را با **Upload scan** روی همان صفحه پیوست کنید.",
            },
          ],
          related: ["vendors", "settlements", "products"],
        },
      ],
    },

    /* ───────────────────── Inventory ───────────────────── */
    {
      id: "inventory",
      title: { en: "Inventory", fa: "موجودی انبار" },
      summary: {
        en: "Track how many units you can sell, what is reserved for open orders, and who supplies you.",
        fa: "ببینید چند عدد کالا برای فروش دارید، چندتایش برای سفارش‌های باز قفل شده، و از چه کسانی تأمین می‌کنید.",
      },
      pages: [
        {
          id: "inventory-stock",
          title: { en: "Stock", fa: "موجودی (Stock)" },
          adminPath: "/inventory",
          summary: {
            en: "This is your stock board: for every product code (SKU) you see how many you have, how many are locked for unpaid orders, and how many you can still sell.",
            fa: "این صفحه تابلوی موجودی شماست: برای هر کد کالا (SKU) می‌بینید چند تا دارید، چند تا برای سفارش‌های پرداخت‌نشده قفل شده، و چند تا هنوز می‌توانید بفروشید.",
          },
          access: {
            en: "Need InventoryView to open; InventoryEdit to press Adjust",
            fa: "برای دیدن: InventoryView — برای دکمه Adjust: InventoryEdit",
          },
          purpose: [
            {
              en: "**Why this page exists:** products alone do not tell you “can a customer buy 3 more?” Stock answers that with real numbers.",
              fa: "**این صفحه برای چیست؟** فقط ساختن محصول کافی نیست؛ مشتری می‌پرسد «۳ تا دیگه موجوده؟». جواب این سؤال همین‌جاست.",
            },
            {
              en: "**What you do here day to day:** watch quantities, spot Low stock, and fix wrong counts with **Adjust** (receive goods, break/damage write-off, count correction).",
              fa: "**کار روزمره شما اینجا:** نگاه به تعدادها، دیدن هشدار کمبود (Low)، و درست‌کردن عدد اشتباه با **Adjust** (ورود کالا، ضایعات، اصلاح شمارش).",
            },
            {
              en: "**What you do *not* do here:** you do not create products (that is Catalog → Products), and you do not manage suppliers’ company data (that is Suppliers).",
              fa: "**اینجا چه کاری *نمی‌کنید*:** محصول جدید نمی‌سازید (آن کار Catalog → Products است) و مشخصات شرکت تأمین‌کننده را اینجا وارد نمی‌کنید (آن کار Suppliers است).",
            },
          ],
          columns: [
            {
              term: { en: "Product", fa: "محصول" },
              def: {
                en: "Display name of the product (for example “Blue T-Shirt”).",
                fa: "نام نمایشی کالا (مثلاً «تی‌شرت آبی»).",
              },
            },
            {
              term: { en: "SKU", fa: "SKU" },
              def: {
                en: "The unique code of that size/color variant. Stock is always per SKU, not only per product title.",
                fa: "کد یکتای همان مدل/سایز/رنگ. موجودی همیشه روی SKU است، نه فقط روی اسم محصول.",
              },
            },
            {
              term: { en: "On Hand", fa: "موجود (On Hand)" },
              def: {
                en: "How many physical (or digital-tracked) units you currently count as in stock for this website.\nIn Dotnetable this total comes from **store listings** (vendor product stock). If nothing is listed for sale, it is not sellable even if a product card exists.",
                fa: "چند واحد الان به‌عنوان موجودی این سایت شمرده می‌شود.\nدر Dotnetable این عدد از **لیستینگ فروش** (موجودی محصول فروشنده) جمع می‌شود. اگر چیزی برای فروش لیست نشده باشد، حتی با وجود کارت محصول، قابل‌فروش نیست.",
              },
            },
            {
              term: { en: "Reserved", fa: "رزرو (Reserved)" },
              def: {
                en: "Units **already promised** to open orders that are not finished yet (typically waiting for payment).\nThey still sit in the warehouse, but another customer must not take them.",
                fa: "تعدادی که **از قبل به سفارش‌های باز وعده داده شده** (معمولاً منتظر پرداخت).\nهنوز در انبار هستند، ولی مشتری دیگری نباید همان‌ها را بخرد.",
              },
            },
            {
              term: { en: "Available", fa: "قابل فروش (Available)" },
              def: {
                en: "**Available = On Hand − Reserved**\nThis is the number new carts/orders can still take right now.",
                fa: "**قابل‌فروش = موجود − رزرو**\nهمین عددی است که سبد/سفارش جدید الان می‌تواند بردارد.",
              },
            },
            {
              term: { en: "Status", fa: "وضعیت" },
              def: {
                en: "**OK** = enough stock relative to reorder level. **Low** = On Hand is at or below the reorder warning level — time to restock.",
                fa: "**OK** یعنی نسبت به سطح هشدار وضعیت خوب است. **Low** یعنی موجودی به سطح سفارش‌مجدد رسیده یا پایین‌تر — وقت تأمین دوباره است.",
              },
            },
            {
              term: { en: "Adjust", fa: "Adjust" },
              def: {
                en: "Button to change stock by a plus or minus amount (not by typing a random absolute number in the grid).",
                fa: "دکمه تغییر موجودی با عدد مثبت یا منفی (نه با تایپ مستقیم یک عدد مطلق وسط جدول).",
              },
            },
          ],
          example: {
            intro: [
              {
                en: "Imagine SKU `TSHIRT-BLU-L` (Blue T-Shirt, size L).",
                fa: "فرض کنید SKU `TSHIRT-BLU-L` (تی‌شرت آبی سایز L).",
              },
            ],
            steps: [
              {
                en: "You received 20 pieces from the supplier → after Adjust **+20**, **On Hand = 20**, Reserved = 0, **Available = 20**.",
                fa: "از تأمین‌کننده ۲۰ عدد آمد → با Adjust **+20** می‌شود: **موجود = ۲۰**، رزرو = ۰، **قابل‌فروش = ۲۰**.",
              },
              {
                en: "A customer places an order for 3 but has not paid yet → system **reserves** 3. Now On Hand = 20, **Reserved = 3**, **Available = 17**.",
                fa: "مشتری ۳ عدد سفارش می‌دهد ولی هنوز پرداخت نکرده → سیستم **۳ تا رزرو** می‌کند. حالا موجود = ۲۰، **رزرو = ۳**، **قابل‌فروش = ۱۷**.",
              },
              {
                en: "Customer pays and the order is fulfilled → stock leaves as a **Sale**. On Hand becomes 17, Reserved back to 0, Available = 17.",
                fa: "مشتری پرداخت می‌کند و سفارش نهایی/تحویل می‌شود → موجودی با حرکت **Sale** کم می‌شود. موجود = ۱۷، رزرو دوباره ۰، قابل‌فروش = ۱۷.",
              },
              {
                en: "If instead the customer never pays and the order is cancelled → reservation is **released**. Numbers return to On Hand 20 / Reserved 0 / Available 20 (nothing left the shelf).",
                fa: "اگر مشتری پرداخت نکند و سفارش لغو شود → رزرو **آزاد** می‌شود. اعداد برمی‌گردند به موجود ۲۰ / رزرو ۰ / قابل‌فروش ۲۰ (چیزی از قفسه کم نشده).",
              },
              {
                en: "You find 1 damaged unit in the warehouse → Adjust **−1** with note “damaged”. On Hand 19.",
                fa: "۱ عدد در انبار خراب است → Adjust **−1** با یادداشت «ضایعات». موجود ۱۹.",
              },
            ],
            result: {
              en: "Rule of thumb: **On Hand** = what is on the shelf; **Reserved** = promised to open orders; **Available** = what a new customer can still buy.",
              fa: "قانون سرانگشتی: **موجود** = روی قفسه؛ **رزرو** = وعده به سفارش‌های باز؛ **قابل‌فروش** = چیزی که مشتری جدید هنوز می‌تواند بخرد.",
            },
          },
          howTo: [
            {
              en: "In Admin open **Inventory → Stock** (path `/inventory`).",
              fa: "در ادمین بروید **Inventory → Stock** (مسیر `/inventory`).",
            },
            {
              en: "If you are a **master** user, first choose the **Website** in the top selector — stock is per website.",
              fa: "اگر کاربر **مستر** هستید، اول از انتخاب‌گر بالا **وب‌سایت** را انتخاب کنید — موجودی برای هر سایت جداست.",
            },
            {
              en: "Use search on product name or SKU to find a row.",
              fa: "با جستجوی نام محصول یا SKU ردیف را پیدا کنید.",
            },
            {
              en: "Read the three numbers: On Hand, Reserved, Available. If Status is **Low**, plan a restock.",
              fa: "سه عدد را بخوانید: موجود، رزرو، قابل‌فروش. اگر وضعیت **Low** بود، برای تأمین برنامه‌ریزی کنید.",
            },
            {
              en: "To change stock: click **Adjust** → enter how many to add (`+5`) or remove (`-2`) → optional unit cost → write a short note → Save.",
              fa: "برای تغییر موجودی: **Adjust** → چند تا اضافه (`+5`) یا کم (`-2`) → هزینه واحد اختیاری → یک یادداشت کوتاه → Save.",
            },
            {
              en: "To see *why* a number changed yesterday, open **Stock Ledger** (`/inventory/movements`) from the button on this page.",
              fa: "برای فهمیدن *چرا* دیروز عدد عوض شده، از دکمه همین صفحه **Stock Ledger** (`/inventory/movements`) را باز کنید.",
            },
          ],
          sections: [
            {
              title: { en: "How stock connects to the rest of the shop", fa: "ارتباط موجودی با بقیه فروشگاه" },
              body: [
                {
                  en: "Think of three layers:",
                  fa: "سه لایه را جدا کنید:",
                },
              ],
              items: [
                {
                  en: "**Product (Catalog)** = the catalog card (name, photos, price fields). Without this, there is nothing to sell.",
                  fa: "**محصول (Catalog)** = کارت کاتالوگ (اسم، عکس، قیمت). بدون این چیزی برای فروش وجود ندارد.",
                },
                {
                  en: "**Vendor listing** = “this seller offers that product with X units.” Sellable stock is driven by these store listings.",
                  fa: "**لیستینگ فروشنده** = «این فروشنده این محصول را با X عدد می‌فروشد.» موجودی قابل‌فروش از همین لیستینگ‌ها می‌آید.",
                },
                {
                  en: "**Stock page** = the live scoreboard + manual corrections (Adjust) + link to the ledger history.",
                  fa: "**صفحه Stock** = تابلوی زنده + اصلاح دستی (Adjust) + لینک به تاریخچه دفتر.",
                },
              ],
              callout: {
                tone: "warn",
                text: {
                  en: "Common confusion: you created a product but customers still cannot buy it. Usually the listing/stock path was never filled, so Available stays 0.",
                  fa: "سردرگمی رایج: محصول ساختید ولی مشتری نمی‌تواند بخرد. معمولاً مسیر لیستینگ/موجودی پر نشده و Available صفر مانده است.",
                },
              },
            },
            {
              title: { en: "What the system does automatically", fa: "سیستم خودش چه کار می‌کند" },
              definitions: [
                {
                  term: { en: "Reserve", fa: "رزرو خودکار" },
                  def: {
                    en: "When an order holds stock (unpaid / in progress), Reserved goes up so two customers do not buy the last unit.",
                    fa: "وقتی سفارشی موجودی را نگه می‌دارد (پرداخت‌نشده / در جریان)، رزرو بالا می‌رود تا دو نفر آخرین عدد را همزمان نخرند.",
                  },
                },
                {
                  term: { en: "Release", fa: "آزادسازی" },
                  def: {
                    en: "If the order is cancelled or the hold expires, Reserved goes back down.",
                    fa: "اگر سفارش لغو شود یا مهلت نگه‌داشت تمام شود، رزرو پایین می‌آید.",
                  },
                },
                {
                  term: { en: "Sale", fa: "فروش / تحویل" },
                  def: {
                    en: "When the order is fulfilled, both On Hand and Reserved decrease (goods left the business).",
                    fa: "وقتی سفارش نهایی/تحویل می‌شود، هم موجود و هم رزرو کم می‌شوند (کالا از کسب‌وکار خارج شده).",
                  },
                },
                {
                  term: { en: "Adjust", fa: "تعدیل دستی" },
                  def: {
                    en: "Only when **you** click Adjust: you add or remove units and a history row is written.",
                    fa: "فقط وقتی **شما** Adjust می‌زنید: تعداد را زیاد/کم می‌کنید و یک ردیف تاریخچه ثبت می‌شود.",
                  },
                },
              ],
            },
            {
              title: { en: "When should I use Adjust?", fa: "کی باید Adjust بزنم؟" },
              items: [
                {
                  en: "Goods arrived from a supplier and you want stock to go up.",
                  fa: "کالا از تأمین‌کننده رسید و باید موجودی بالا برود.",
                },
                {
                  en: "Warehouse count does not match the screen (inventory count correction).",
                  fa: "شمارش انبار با صفحه یکی نیست (اصلاح شمارش).",
                },
                {
                  en: "Damage, loss, sample giveaway — stock must go down without a customer order.",
                  fa: "خراب شدن، گم شدن، نمونه اهدایی — موجودی باید بدون سفارش مشتری کم شود.",
                },
              ],
              callout: {
                tone: "",
                text: {
                  en: "Do **not** use Adjust to “simulate a sale”. Real sales should go through Orders so money, status, and stock stay consistent.",
                  fa: "برای «شبیه‌سازی فروش» از Adjust استفاده **نکنید**. فروش واقعی باید از Orders برود تا پول، وضعیت و موجودی با هم جور بمانند.",
                },
              },
            },
          ],
          tips: [
            {
              en: "Always write a **note** on Adjust (“received from Supplier X”, “damaged box #12”). Future-you will thank you.",
              fa: "روی Adjust حتماً **یادداشت** بنویسید («ورود از تأمین‌کننده فلان»، «کارتن خراب ۱۲»). بعداً خودتان ممنون می‌شوید.",
            },
            {
              en: "Unit cost is optional; fill it when you care about purchase cost later.",
              fa: "هزینه واحد اختیاری است؛ اگر بعداً به قیمت خرید اهمیت می‌دهید پر کنید.",
            },
            {
              en: "If two people Adjust the same SKU at the same second, one may need to retry — that is normal concurrency protection.",
              fa: "اگر دو نفر همزمان روی یک SKU Adjust بزنند، یکی ممکن است مجبور به تلاش دوباره شود — محافظت هم‌زمانی طبیعی است.",
            },
            {
              en: "Empty grid? Either wrong website selected, or no inventory rows exist yet (no listings/stock created).",
              fa: "جدول خالی است؟ یا وب‌سایت اشتباه انتخاب شده، یا هنوز ردیف موجودی ساخته نشده (لیستینگ/استوک نداده‌اید).",
            },
          ],
          relatedApi: ["inventory"],
          related: ["inventory-movements", "suppliers", "products", "vendors", "orders"],
          keywords: [
            {
              en: "inventory stock onhand reserved available warehouse sku adjust low",
              fa: "انبار موجودی رزرو قابل فروش تعدیل کمبود sku",
            },
          ],
        },
        {
          id: "inventory-movements",
          title: { en: "Stock ledger (Movements)", fa: "دفتر موجودی (حرکت‌ها)" },
          adminPath: "/inventory/movements",
          summary: {
            en: "The diary of stock: every time units go in or out, one line is written here. You can read it; you cannot edit it.",
            fa: "دفترچه خاطرات موجودی: هر بار که کالا وارد یا خارج می‌شود، یک خط اینجا ثبت می‌شود. فقط می‌خوانید؛ ویرایش نمی‌کنید.",
          },
          purpose: [
            {
              en: "**Why open this page?** When On Hand looks wrong, the ledger answers: *who/what changed it, when, and by how many?*",
              fa: "**چرا این صفحه را باز کنیم؟** وقتی موجود عجیب به نظر می‌رسد، دفتر جواب می‌دهد: *چه چیزی/چه زمانی/چند تا عوض شد؟*",
            },
            {
              en: "Each row is one movement: date, product, SKU, type, quantity, optional supplier, note.",
              fa: "هر ردیف یک حرکت است: تاریخ، محصول، SKU، نوع، تعداد، تأمین‌کننده (اگر باشد)، یادداشت.",
            },
          ],
          columns: [
            {
              term: { en: "Date", fa: "تاریخ" },
              def: {
                en: "When the movement was recorded.",
                fa: "زمان ثبت حرکت.",
              },
            },
            {
              term: { en: "Type", fa: "نوع" },
              def: {
                en: "**Purchase** goods in · **Sale** sold/fulfilled · **Adjustment** manual admin change · **Return** stock returned.",
                fa: "**Purchase** ورود کالا · **Sale** فروش/تحویل · **Adjustment** اصلاح دستی ادمین · **Return** برگشت موجودی.",
              },
            },
            {
              term: { en: "Quantity", fa: "تعداد" },
              def: {
                en: "How many units that movement moved.",
                fa: "چند واحد در آن حرکت جابه‌جا شده.",
              },
            },
            {
              term: { en: "Supplier / Note", fa: "تأمین‌کننده / یادداشت" },
              def: {
                en: "Extra context (who you bought from, why you adjusted).",
                fa: "زمینه اضافه (از چه کسی خریدید، چرا تعدیل کردید).",
              },
            },
          ],
          howTo: [
            {
              en: "Open **Inventory → Movements** (`/inventory/movements`), or the Stock Ledger button on the Stock page.",
              fa: "**Inventory → Movements** (`/inventory/movements`) را باز کنید، یا از دکمه Stock Ledger در صفحه Stock.",
            },
            {
              en: "Master users: pick the website first.",
              fa: "کاربر مستر: اول وب‌سایت را انتخاب کنید.",
            },
            {
              en: "Search/sort until you find the SKU or date you care about.",
              fa: "جستجو/مرتب‌سازی کنید تا SKU یا تاریخ موردنظر را پیدا کنید.",
            },
            {
              en: "If the history shows a bad Adjust, fix stock with a **new** Adjust on the Stock page (do not expect to edit old ledger lines).",
              fa: "اگر تاریخچه یک Adjust اشتباه نشان می‌دهد، موجودی را با یک Adjust **جدید** در صفحه Stock درست کنید (خطوط قدیمی دفتر را ویرایش نمی‌کنید).",
            },
          ],
          tips: [
            {
              en: "This page is read-only on purpose — the ledger is an audit log.",
              fa: "این صفحه عمداً فقط‌خواندنی است — دفتر، لاگ حسابرسی است.",
            },
            {
              en: "When numbers disagree with the warehouse, compare ledger lines day-by-day with physical count.",
              fa: "اگر عدد با انبار جور نیست، خطوط دفتر را روزبه‌روز با شمارش فیزیکی مقایسه کنید.",
            },
          ],
          relatedApi: ["inventory"],
          related: ["inventory-stock", "warehouses", "stock-documents", "suppliers", "orders"],
        },
        {
          id: "warehouses",
          title: { en: "Warehouses", fa: "انبارها" },
          adminPath: "/inventory/warehouses",
          summary: {
            en: "Physical warehouse locations and per-bin stock. When a site has at least one active warehouse, orders use the WMS path (outbound pick → post on ship).",
            fa: "محل‌های فیزیکی انبار و موجودی هر انبار. وقتی سایت حداقل یک انبار فعال دارد، سفارش‌ها مسیر WMS را می‌روند (برداشت خروجی → ثبت هنگام ارسال).",
          },
          access: {
            en: "warehouse.view to open; warehouse roles for receive/issue/approve/post on documents",
            fa: "warehouse.view برای دیدن؛ نقش‌های انبار برای دریافت/صدور/تأیید/ثبت اسناد",
          },
          purpose: [
            {
              en: "**WarehouseStock** is the physical book (on hand + reserved). At checkout, listings **and** the default warehouse are reserved.",
              fa: "**WarehouseStock** دفتر فیزیکی است (موجود + رزرو). هنگام checkout هم لیستینگ و هم انبار پیش‌فرض رزرو می‌شوند.",
            },
            {
              en: "**InventoryItem** mirrors warehouse totals after stock documents are posted (and still holds reservations for open orders).",
              fa: "**InventoryItem** بعد از ثبت اسناد انبار با مجموع انبار همگام می‌شود (و رزرو سفارش‌های باز را نگه می‌دارد).",
            },
          ],
          howTo: [
            {
              en: "Open **Inventory → Warehouses**. Ensure a default MAIN warehouse exists (auto-created when WMS is first used).",
              fa: "**Inventory → Warehouses** را باز کنید. انبار پیش‌فرض MAIN را داشته باشید (با اولین استفاده WMS ساخته می‌شود).",
            },
            {
              en: "Receive stock with **Inbound** stock documents; sell via order outbound pick; returns create **Return** documents after refund.",
              fa: "ورود کالا با سند **Inbound**؛ فروش با برداشت خروجی سفارش؛ برگشت بعد از استرداد سند **Return** می‌سازد.",
            },
          ],
          tips: [
            {
              en: "Ship is blocked in the order UI when warehouse on-hand is insufficient — restock (inbound) or refund the customer.",
              fa: "اگر موجودی انبار کافی نباشد، ارسال در UI سفارش قفل می‌شود — تأمین (inbound) یا استرداد به مشتری.",
            },
          ],
          related: ["stock-documents", "warehouse-tasks", "inventory-stock", "orders", "payments-refunds"],
        },
        {
          id: "stock-documents",
          title: { en: "Stock documents", fa: "اسناد انبار" },
          adminPath: "/inventory/stock-documents",
          summary: {
            en: "WMS documents: Inbound, Outbound, Transfer, Adjustment, Count, **Return**. Only **Posted** docs change balances.",
            fa: "اسناد WMS: ورود، خروج، انتقال، تعدیل، شمارش، **برگشت**. فقط اسناد **Posted** موجودی را عوض می‌کنند.",
          },
          purpose: [
            {
              en: "Lifecycle: Draft → Submitted → Approved (Ready to pick) → Posted (or Rejected / Cancelled).",
              fa: "چرخه: Draft → Submitted → Approved (آماده برداشت) → Posted (یا Rejected / Cancelled).",
            },
            {
              en: "Paid orders auto-create **Submitted Outbound** picks. Shipping posts them. Refund after ship auto-creates **Return** with QC (Sellable / Defective).",
              fa: "سفارش پرداخت‌شده خودکار **Outbound Submitted** می‌سازد. ارسال آن را Post می‌کند. استرداد بعد از ارسال **Return** با QC (سالم / معیوب) می‌سازد.",
            },
          ],
          howTo: [
            {
              en: "Open a document → set QC on return lines → Approve / Post to stock. Attach packing slips via **Upload scan**.",
              fa: "سند را باز کنید → QC خطوط برگشت را بگذارید → Approve / Post. برگه بسته‌بندی را با **Upload scan** پیوست کنید.",
            },
            {
              en: "**Transfer:** pick From + To warehouses → Create transfer → add lines (qty). **Count:** pick warehouse → Physical count (loads book on-hand) → edit Counted → Variance = Counted − Book posts as adjustment only.",
              fa: "**انتقال:** انبار مبدأ/مقصد → Create transfer → افزودن خطوط. **شمارش:** انبار → Physical count (بارگذاری موجودی دفتری) → Counted را اصلاح کنید → Variance = Counted − Book فقط اختلاف را Post می‌کند.",
            },
            {
              en: "Set **condition** on each return line: New, Like new, Open box, Display, Used, Defective. Non-new (except Defective) **require a health grade** A–F. New restocks original listing; Open box / Display / Used restock a used-channel listing and map the product to category slug `used-goods`; Defective is scrap-only.",
              fa: "روی هر خط برگشت **وضعیت** بگذارید: نو، در حد نو، جعبه باز، ویترینی، کارکرده، معیوب. غیر از نو (به‌جز معیوب) **گرید سلامت A–F اجباری** است. نو → لیستینگ اصلی؛ جعبه باز/ویترینی/کارکرده → لیستینگ کانال کارکرده + دسته `used-goods`؛ معیوب فقط ضایعات.",
            },
            {
              en: "Submit/approve/post warehouse docs notify members with warehouse roles (in-app + email + WhatsApp when gateway configured).",
              fa: "Submit/Approve/Post اسناد انبار به نقش‌های انبار نوتیف می‌دهد (داخل ادمین + ایمیل + واتساپ در صورت پیکربندی).",
            },
          ],
          related: ["warehouses", "warehouse-tasks", "orders", "payments-refunds", "inventory-stock"],
        },
        {
          id: "warehouse-tasks",
          title: { en: "My warehouse tasks", fa: "کارهای من (انبار)" },
          adminPath: "/inventory/my-tasks",
          summary: {
            en: "Worker queue of outbound picks: **Submitted** (new) and **Ready to pick** (Approved).",
            fa: "صف کار انباردار برای برداشت‌های خروجی: **Submitted** (جدید) و **Ready to pick** (تأییدشده).",
          },
          howTo: [
            {
              en: "Filter by status chips → **Ready to pick** to approve → **Pick & post** when goods leave the bin.",
              fa: "با چیپ وضعیت فیلتر کنید → **Ready to pick** برای تأیید → **Pick & post** وقتی کالا از قفسه خارج شد.",
            },
            {
              en: "Open the linked order if stock is missing — ship will stay blocked until inbound restock or customer refund.",
              fa: "اگر موجودی نیست سفارش را باز کنید — ارسال تا inbound یا استرداد مشتری قفل می‌ماند.",
            },
          ],
          related: ["stock-documents", "warehouses", "orders"],
        },
        {
          id: "suppliers",
          title: { en: "Suppliers", fa: "تأمین‌کنندگان" },
          adminPath: "/inventory/suppliers",
          summary: {
            en: "Address book of companies you buy stock from (and some tax counterparties). Not the stock quantities themselves.",
            fa: "دفترچه آدرس شرکت‌هایی که از آن‌ها کالا می‌خرید (و بعضی طرف‌های مالیاتی). خودِ تعداد موجودی اینجا نیست.",
          },
          purpose: [
            {
              en: "**Simple idea:** Stock = how many units. Suppliers = **who** you buy those units from.",
              fa: "**ایده ساده:** Stock = چند تا دارید. Suppliers = **از چه کسی** می‌خرید.",
            },
            {
              en: "You store name, type, tax/VAT IDs, phone, active flag — so movements and settlements can point to a real counterparty.",
              fa: "نام، نوع، شناسه مالیات/VAT، تلفن، فعال بودن را نگه می‌دارید تا حرکت‌های موجودی و تسویه‌ها به یک طرف واقعی وصل شوند.",
            },
          ],
          howTo: [
            {
              en: "Open **Inventory → Suppliers** (`/inventory/suppliers`).",
              fa: "**Inventory → Suppliers** (`/inventory/suppliers`) را باز کنید.",
            },
            {
              en: "Click **New Supplier**, fill identity + tax fields you need, save.",
              fa: "**New Supplier**، مشخصات و فیلدهای مالیاتی لازم را پر کنید، ذخیره.",
            },
            {
              en: "Later, when stock arrives, you still change quantities on **Stock → Adjust**; the supplier record is the company card behind that purchase story.",
              fa: "بعداً وقتی کالا می‌رسد، تعداد را همچنان در **Stock → Adjust** عوض می‌کنید؛ رکورد تأمین‌کننده کارت شرکت پشت آن داستان خرید است.",
            },
          ],
          tips: [
            {
              en: "Do not confuse **Suppliers** (you buy from them) with **Vendors** (they sell on your marketplace).",
              fa: "**Suppliers** (از آن‌ها می‌خرید) را با **Vendors** (روی مارکت‌پلیس شما می‌فروشند) قاطی نکنید.",
            },
          ],
          related: ["inventory-stock", "inventory-movements", "settlements", "tax"],
        },
      ],
    },

    /* ───────────────────── Finance ledger ───────────────────── */
    {
      id: "finance-ledger",
      title: { en: "Finance", fa: "مالی" },
      summary: {
        en: "Operational money ledger, settlements, payments.",
        fa: "دفتر کل عملیاتی پول، تسویه، پرداخت‌ها.",
      },
      pages: [
        {
          id: "ledger",
          title: { en: "Financial ledger", fa: "دفتر کل مالی" },
          adminPath: "/finance/ledger",
          summary: {
            en: "All inflows/outflows and invoice components: payment, shipping, markup, line revenue/cost/profit, vendor settlement, refunds, additional charges. Date+time separate. Editable rows keep history.",
            fa: "همه دریافت/پرداخت و اجزای فاکتور: پرداخت، ارسال، روکشی، درآمد/هزینه/سود خط، تسویه فروشنده، استرداد، هزینه اضافه. تاریخ و زمان جدا. ویرایش تاریخچه نگه می‌دارد.",
          },
          purpose: [
            {
              en: "Report every money behavior on the site without locking transaction types (string codes).",
              fa: "گزارش هر رفتار پولی سایت بدون محدود کردن نوع تراکنش (کد رشته‌ای).",
            },
            {
              en: "Invoice screen shows final totals + additional charges; tax flag per line.",
              fa: "صفحه سفارش جمع نهایی + هزینه اضافه را نشان می‌دهد؛ پرچم مالیات روی هر خط.",
            },
          ],
          howTo: [
            {
              en: "Open Finance → Financial ledger. Filter by date/type. Paid orders auto-post a breakdown.",
              fa: "Finance → Financial ledger. فیلتر تاریخ/نوع. سفارش‌های پرداخت‌شده breakdown می‌گیرند.",
            },
            {
              en: "On an order, open Financial breakdown to review components; use Additional charge for extra amounts. Sellers open My settlements (vendor-visible amounts only). Ledger rows are created automatically when payment is recorded — no backfill UI.",
              fa: "روی سفارش Financial breakdown را برای اجزا ببینید؛ برای مبلغ اضافه Additional charge. فروشنده: My settlements (فقط مبلغ تسویه). ردیف‌های دفتر کل با ثبت پرداخت خودکار ساخته می‌شوند — بدون UI بازسازی سفارش‌های قدیم.",
            },
          ],
          related: ["orders", "settlements", "payments"],
        },
      ],
    },

    /* ───────────────────── Orders & Support ───────────────────── */
    {
      id: "orders-support",
      title: { en: "Orders & support", fa: "سفارش و پشتیبانی" },
      summary: {
        en: "Order lifecycle, invoices, support desk and tickets.",
        fa: "چرخه عمر سفارش، فاکتور، میز پشتیبانی و تیکت.",
      },
      pages: [
        {
          id: "orders",
          title: { en: "Orders", fa: "سفارش‌ها" },
          adminPath: "/orders",
          summary: {
            en: "List, create, and manage customer orders: manual social/offline orders, markups, payments with receipts, address print labels, tax reporting flags, status, refunds, invoice.",
            fa: "فهرست، ثبت و مدیریت سفارش مشتری: سفارش دستی شبکه‌های اجتماعی/آفلاین، روکشی، پرداخت با فیش، برچسب آدرس، پرچم مالیات، وضعیت، استرداد، فاکتور.",
          },
          purpose: [
            {
              en: "Operational backbone of commerce after checkout — and for Instagram/WhatsApp/card-to-card sales entered by admin.",
              fa: "ستون فقرات عملیاتی فروش بعد از تسویه — و برای فروش اینستاگرام/واتساپ/کارت‌به‌کارت که ادمین ثبت می‌کند.",
            },
            {
              en: "Status history tracks every transition; payments/refunds and sales channel attach here.",
              fa: "تاریخچه وضعیت هر انتقال را نگه می‌دارد؛ پرداخت/استرداد و کانال فروش اینجا وصل می‌شود.",
            },
          ],
          howTo: [
            {
              en: "Open Orders, filter by status/date. Use **Create order** (orders.edit) for offline channels.",
              fa: "Orders را باز کنید، بر اساس وضعیت/تاریخ فیلتر کنید. برای کانال‌های آفلاین **ثبت سفارش** (orders.edit) را بزنید.",
            },
            {
              en: "**Create order** flow: pick or register customer → add catalog products (**variant picker** when several variants; **no listing** still allowed without stock) or a **free-form item** (title/price only) → charged vs catalog price (markup / روکشی) → address → shipping → sales channel → report-to-tax → optional payment (card-to-card + receipt, cash, COD).",
              fa: "روند **ثبت سفارش**: انتخاب یا ثبت مشتری → محصول کاتالوگ (**انتخاب واریانت** اگر چندتاست؛ بدون listing هم مجاز بدون موجودی) یا **آیتم آزاد** (فقط عنوان/قیمت) → قیمت دریافتی در برابر کاتالوگ (روکشی) → آدرس → ارسال → کانال → مالیات → پرداخت اختیاری (کارت‌به‌کارت + فیش، نقد، COD).",
            },
            {
              en: "Open an order to review items (catalog / charged / markup), shipping address, totals, payments, channel, tax flag, and history. Click a catalog line title to open **product preview** (works for unpublished products; `?variantId=` highlights the ordered variant).",
              fa: "یک سفارش را برای اقلام (کاتالوگ / دریافتی / روکشی)، آدرس ارسال، جمع‌ها، پرداخت‌ها، کانال، پرچم مالیات و تاریخچه باز کنید. روی عنوان خط کاتالوگ کلیک کنید تا **پیش‌نمایش محصول** باز شود (حتی unpublished؛ `?variantId=` واریانت سفارش را مشخص می‌کند).",
            },
            {
              en: "Use **Shipping label** to print receiver name, phone, and address for fulfillment.",
              fa: "با **برچسب ارسال** نام گیرنده، تلفن و آدرس را برای بسته‌بندی چاپ کنید.",
            },
            {
              en: "On the order or **Invoice** page, set **Preparation status** (warehouse), **Shipping status**, and **Tracking code** (up to 100 chars). Customers see these on their account order detail. Saving can sync main order status (Paid→Processing, Processing→Shipped, Shipped→Completed).",
              fa: "در صفحه سفارش یا **فاکتور** وضعیت **آماده‌سازی** (انبار)، **وضعیت ارسال** و **کد پیگیری** (تا ۱۰۰ کاراکتر) را ست کنید. مشتری این‌ها را در جزئیات سفارش حساب کاربری می‌بیند. ذخیره می‌تواند وضعیت اصلی سفارش را هم هم‌راستا کند (Paid→Processing، Processing→Shipped، Shipped→Completed).",
            },
            {
              en: "When a **tracking code** is set or changed, the customer is notified on every **configured** channel: **email** (template `OrderShipped`, Sales account), **SMS**, and **WhatsApp**. Message language follows the website **default language** (built-in FA/EN). SMS/WhatsApp only send if a real provider is registered (`ISmsSender` / `IWhatsAppSender` with `IsConfigured = true`); stubs log only.",
              fa: "با **ست یا تغییر کد پیگیری**، مشتری روی هر کانال **فعال** خبر می‌گیرد: **ایمیل** (قالب `OrderShipped`، حساب Sales)، **SMS** و **واتساپ**. زبان پیام = **زبان پیش‌فرض** وب‌سایت (FA/EN داخلی). SMS/واتساپ فقط اگر provider واقعی ثبت شده باشد (`IsConfigured = true`)؛ stub فقط لاگ می‌کند.",
            },
            {
              en: "Use Change Status only with valid transitions for your process (paid → processing → shipped → …).",
              fa: "Change Status را فقط با انتقال‌های معتبر فرایند خودتان بزنید (پرداخت‌شده → پردازش → ارسال → …).",
            },
            {
              en: "**Record payment received** (needs payments.verify): offline COD, cash, POS, or **bank/card-to-card** with optional bank account + receipt file. Creates a Paid payment and moves PendingPayment → Paid.",
              fa: "**ثبت دریافت وجه** (نقش payments.verify): COD، نقد، کارتخوان، یا **کارت‌به‌کارت/بانکی** با حساب و فیش اختیاری. پرداخت Paid می‌سازد و PendingPayment → Paid می‌کند.",
            },
            {
              en: "**Refund** (needs payments.refund): after a Paid payment exists — amount is in **order/payment currency** (site money, e.g. IRR), never forced to USD. Wallet credit, bank queue, or cash hand-back. After ship, auto-creates a warehouse **Return** for QC + restock. Ship is blocked when warehouse stock is short — restock or refund.",
              fa: "**برگشت وجه** (نقش payments.refund): بعد از پرداخت Paid — مبلغ با **ارز سفارش/پرداخت** (ارز سایت، مثلاً ریال) است، نه اجباری دلار. اعتبار کیف پول، صف بانکی، یا برگشت نقدی. بعد از ارسال، سند **Return** انبار برای QC و برگشت موجودی ساخته می‌شود. اگر موجودی انبار کم باشد ارسال قفل است — تأمین یا استرداد.",
            },
            {
              en: "Open **Invoice** for a framed customer document (margins/borders). Lines show **product code** (`DN-42` = site prefix + ProductID) and link to **product preview**. From the same page set preparation / shipping status and tracking code. Print PDF, or **email / WhatsApp**. Charged unit prices only — markup is folded into price. Orders with **Report to tax = No** are excluded from the VAT report.",
              fa: "از **فاکتور** سند مشتری با حاشیه و قاب را باز کنید. روی خطوط **کد کالا** (`DN-42`) و لینک **پیش‌نمایش محصول** می‌آید. از همان صفحه وضعیت آماده‌سازی/ارسال و کد پیگیری را ست کنید. Print PDF یا **ایمیل / واتساپ**. فقط قیمت فروش — مارک‌آپ داخل قیمت است. سفارش‌هایی که **اعلام به مالیات = خیر** دارند در گزارش VAT نمی‌آیند.",
            },
          ],
          tips: [
            {
              en: "Website setting **Report offline / social orders to tax by default** controls the default for non-online admin orders (can override per order).",
              fa: "تنظیم وب‌سایت **اعلام پیش‌فرض سفارش‌های آفلاین/اجتماعی به مالیات** پیش‌فرض سفارش‌های غیراینترنتی ادمین را می‌سازد (هر سفارش قابل override است).",
            },
            {
              en: "Stock reservation/sale movements are driven by order lifecycle — cancel carefully. Ship is blocked when warehouse stock is insufficient (inbound restock or refund).",
              fa: "رزرو/فروش موجودی از چرخه سفارش می‌آید — لغو را با دقت انجام دهید. اگر موجودی انبار کم باشد ارسال قفل است (تأمین inbound یا استرداد).",
            },
            {
              en: "COD collection should be recorded with method Cash on delivery so finance reports stay accurate.",
              fa: "وصول پرداخت در محل را با روش Cash on delivery ثبت کنید تا گزارش مالی دقیق بماند.",
            },
          ],
          relatedApi: ["orders", "checkout", "payments"],
          related: ["payments", "payments-refunds", "inventory-stock", "warehouses", "stock-documents", "shipping", "support-desk", "coupons", "websites", "tax"],
        },
        {
          id: "support-desk",
          title: { en: "Support desk", fa: "میز پشتیبانی" },
          adminPath: "/support",
          summary: {
            en: "Support workspace overview for customer care operations.",
            fa: "نمای کلی فضای پشتیبانی برای مراقبت از مشتری.",
          },
          purpose: {
            en: "Entry point for support agents — sessions/tickets and related stats.",
            fa: "نقطه ورود پشتیبان‌ها — نشست/تیکت و آمار مرتبط.",
          },
          howTo: [
            {
              en: "Open Support Desk for the overview, then drill into tickets/sessions.",
              fa: "Support Desk را برای نمای کلی باز کنید، سپس به تیکت/نشست بروید.",
            },
          ],
          related: ["support-tickets", "orders", "contact-messages"],
        },
        {
          id: "support-tickets",
          title: { en: "Support tickets / sessions", fa: "تیکت‌ها / نشست‌های پشتیبانی" },
          adminPath: "/support/tickets",
          summary: {
            en: "Ticket/session list and detail editing for customer support threads.",
            fa: "فهرست تیکت/نشست و ویرایش جزئیات رشته‌های پشتیبانی.",
          },
          purpose: {
            en: "Track conversations with customers until resolution.",
            fa: "پیگیری گفتگو با مشتری تا حل شدن.",
          },
          howTo: [
            {
              en: "Open the queue, claim/update status, document the resolution.",
              fa: "صف را باز کنید، وضعیت را به‌روز کنید، راه‌حل را مستند کنید.",
            },
          ],
          related: ["support-desk", "orders", "email-templates"],
        },
      ],
    },

    /* ───────────────────── Finance ───────────────────── */
    {
      id: "finance",
      title: { en: "Finance", fa: "مالی" },
      summary: {
        en: "Payments, refunds, wallets, settlements, banks, currencies, rates.",
        fa: "پرداخت، استرداد، کیف پول، تسویه، بانک، ارز، نرخ.",
      },
      pages: [
        {
          id: "payments",
          title: { en: "Payments", fa: "پرداخت‌ها" },
          adminPath: "/payments",
          summary: {
            en: "Bank-transfer receipt queue; offline/COD receipts are recorded on the order detail page.",
            fa: "صف فیش کارت‌به‌کارت؛ دریافت وجه نقد/پرداخت در محل از جزئیات سفارش ثبت می‌شود.",
          },
          purpose: {
            en: "Human-in-the-loop confirmation for customer-uploaded bank receipts. Cash, COD, and other offline collection are recorded by admin on Orders → order detail (Record payment received).",
            fa: "تأیید دستی فیش‌های بانکی آپلودشده مشتری. نقد، پرداخت در محل و سایر وصول‌های آفلاین را ادمین در Orders → جزئیات سفارش (ثبت دریافت وجه) ثبت می‌کند.",
          },
          howTo: [
            {
              en: "Open Payments queue, inspect each pending bank receipt, approve or reject.",
              fa: "صف Payments را باز کنید، هر فیش بانکی معلق را ببینید، تأیید یا رد کنید.",
            },
            {
              en: "Approvals mark the payment Paid and advance the order; rejections leave the order unpaid.",
              fa: "تأیید پرداخت را Paid و سفارش را جلو می‌برد؛ رد سفارش را unpaid نگه می‌دارد.",
            },
            {
              en: "Attach extra scans on the queue row with **Upload scan** (one step) or from media library — not only from the order detail page.",
              fa: "اسکن اضافه را مستقیم روی ردیف صف با **Upload scan** (یک‌مرحله‌ای) یا از کتابخانه رسانه پیوست کنید — نه فقط از جزئیات سفارش.",
            },
            {
              en: "For COD/cash/POS collection, open the order and use **Record payment received** (payments.verify) — not this queue.",
              fa: "برای وصول COD/نقد/کارتخوان، سفارش را باز کنید و **ثبت دریافت وجه** (payments.verify) را بزنید — نه این صف.",
            },
          ],
          related: ["orders", "payments-refunds", "bank-accounts"],
        },
        {
          id: "payments-refunds",
          title: { en: "Bank refunds", fa: "استرداد بانکی" },
          adminPath: "/payments/refunds",
          summary: {
            en: "Queue for refunds that must be paid out via bank transfer (started from order detail).",
            fa: "صف استردادهایی که باید با حواله بانکی پرداخت شوند (شروع از جزئیات سفارش).",
          },
          purpose: {
            en: "After an admin starts a bank-destination refund on an order, this queue tracks the offline outgoing transfer until Mark completed.",
            fa: "بعد از شروع استرداد مقصد بانکی روی سفارش، این صف حواله خروجی آفلاین را تا Mark completed پیگیری می‌کند.",
          },
          howTo: [
            {
              en: "Start refunds from order detail in the **payment currency** (same as order total): wallet (instant, same currency ledger), bank (appears here), or cash/manual (completed immediately). Default amount is the full paid amount; you may enter a lower partial refund (capped at paid).",
              fa: "استرداد را از جزئیات سفارش به **ارز پرداخت** شروع کنید: کیف پول (فوری)، بانک (این صف)، یا نقدی/دستی (فوری). مبلغ پیش‌فرض کل پرداخت است؛ می‌توانید مبلغ جزئی کمتر وارد کنید (سقف = مبلغ پرداخت‌شده).",
            },
            {
              en: "After goods left stock (posted outbound / shipped), refund auto-creates a warehouse **Return** document. Warehouse sets QC Sellable vs Defective, then Posts to restock (or scrap).",
              fa: "بعد از خروج کالا از انبار (خروج Posted / ارسال‌شده)، استرداد خودکار سند **Return** می‌سازد. انبار QC سالم/معیوب می‌گذارد و با Post موجودی را برمی‌گرداند (یا ضایعات).",
            },
            {
              en: "Attach bank transfer proof directly on each refund row (**Upload scan**) — no need to open the order first.",
              fa: "رسید حواله را مستقیم روی هر ردیف استرداد (**Upload scan**) پیوست کنید — لازم نیست اول سفارش را باز کنید.",
            },
            {
              en: "Process the bank queue; mark completed after the real outgoing transfer.",
              fa: "صف بانکی را انجام دهید؛ بعد از حواله واقعی Mark completed بزنید.",
            },
          ],
          related: ["payments", "orders", "stock-documents", "warehouses"],
        },
        {
          id: "withdrawals",
          title: { en: "Wallet withdrawals", fa: "برداشت کیف پول" },
          adminPath: "/wallets/withdrawals",
          summary: {
            en: "Customer cash-out requests from client wallets; approve holds funds flow, reject reverses hold. Attach bank payout receipts on the row.",
            fa: "درخواست برداشت مشتری از کیف پول؛ تأیید جریان hold را جلو می‌برد، رد hold را برمی‌گرداند. رسید بانکی پرداخت را روی ردیف پیوست کنید.",
          },
          purpose: [
            {
              en: "Clients request withdrawal to a saved bank account; amount is held immediately on the wallet ledger.",
              fa: "مشتری به حساب بانکی ذخیره‌شده درخواست برداشت می‌دهد؛ مبلغ فوراً در دفتر کیف پول hold می‌شود.",
            },
            {
              en: "Finance approves payout or rejects (hold released). Upload the bank receipt scan on the queue row.",
              fa: "مالی پرداخت را تأیید یا رد می‌کند (hold آزاد می‌شود). اسکن رسید بانکی را روی ردیف صف آپلود کنید.",
            },
          ],
          howTo: [
            {
              en: "Open Withdrawals queue; review amount, client, bank account.",
              fa: "صف Withdrawals را باز کنید؛ مبلغ، مشتری، حساب بانکی را ببینید.",
            },
            {
              en: "Upload payout receipt via **Upload scan** on the row, then approve after real-world payout (or reject with a reason).",
              fa: "رسید پرداخت را با **Upload scan** روی ردیف بگذارید، بعد از پرداخت واقعی تأیید کنید (یا با دلیل رد).",
            },
          ],
          tips: [
            {
              en: "Wallet balance only changes through the ledger Apply API — never expect silent edits.",
              fa: "مانده کیف پول فقط از مسیر دفتر Apply عوض می‌شود — انتظار ویرایش بی‌صدا نداشته باشید.",
            },
          ],
          related: ["clients", "bank-accounts", "wallet-detail"],
        },
        {
          id: "wallet-detail",
          title: { en: "Client wallet detail", fa: "جزئیات کیف پول مشتری" },
          adminPath: "/clients/{id} (wallet)",
          summary: {
            en: "Per-client wallet balance and transaction ledger; admin adjust when permitted.",
            fa: "مانده و دفتر تراکنش per مشتری؛ تعدیل ادمین در صورت مجوز.",
          },
          purpose: {
            en: "Inspect why a balance is what it is; apply manual credit/debit with an audit trail.",
            fa: "ببینید مانده چرا این عدد است؛ بستانکار/بدهکار دستی با ردپای حسابرسی.",
          },
          howTo: [
            {
              en: "Open from client/finance flows; review history before adjusting.",
              fa: "از جریان مشتری/مالی باز کنید؛ قبل از تعدیل تاریخچه را ببینید.",
            },
            {
              en: "Use Adjust Wallet dialog only with a clear business reason recorded.",
              fa: "دیالوگ Adjust Wallet را فقط با دلیل کسب‌وکاری ثبت‌شده استفاده کنید.",
            },
          ],
          related: ["withdrawals", "clients"],
        },
        {
          id: "settlements",
          title: { en: "Settlements", fa: "تسویه‌ها" },
          adminPath: "/finance/settlements",
          summary: {
            en: "Vendor / inter-site / supplier settlements with net, tax and gross amounts.",
            fa: "تسویه فروشنده / بین‌سایتی / تأمین‌کننده با مبالغ خالص، مالیات و ناخالص.",
          },
          purpose: {
            en: "Formalize money owed between parties (marketplace sellers, linked sites, suppliers) including tax lines.",
            fa: "رسمی‌کردن پول بین طرف‌ها (فروشنده مارکت‌پلیس، سایت لینک‌شده، تأمین‌کننده) شامل خطوط مالیاتی.",
          },
          howTo: [
            {
              en: "Open Settlements list → open a settlement detail for counterparty, lines, amounts, actions.",
              fa: "فهرست Settlements → جزئیات یک تسویه برای طرف، خطوط، مبالغ، اقدامات.",
            },
            {
              en: "Complete workflow states your finance process defines (draft → confirmed → paid, etc.).",
              fa: "وضعیت‌های گردش‌کار فرایند مالی خودتان را کامل کنید (پیش‌نویس → تأیید → پرداخت و …).",
            },
          ],
          related: ["vendors", "suppliers", "tax", "bank-accounts"],
        },
        {
          id: "payroll",
          title: { en: "Payroll", fa: "حقوق و دستمزد" },
          adminPath: "/hr/payroll",
          summary: {
            en: "Payroll runs, insurance/tax withholdings, Excel export for insurance payable and tax payable, multi-tier rate brackets.",
            fa: "دوره‌های حقوق، کسر بیمه/مالیات، خروجی Excel بیمه و مالیات قابل‌پرداخت، پله‌های نرخ چندسطحی.",
          },
          howTo: [
            {
              en: "Create a run for a period → Submit → Approve → Mark paid (posts GL). Submit notifies HR/payroll roles (email + WhatsApp when configured).",
              fa: "برای بازه Create run → Submit → Approve → Mark paid (ثبت حسابداری). Submit به نقش‌های HR/حقوق نوتیف می‌دهد.",
            },
            {
              en: "Export Excel from run detail or **Payroll reports** (insurance payable / tax payable / full statutory).",
              fa: "خروجی Excel از جزئیات دوره یا **گزارش‌های حقوق** (بیمه قابل‌پرداخت / مالیات / کامل).",
            },
            {
              en: "Configure progressive brackets under **Rate brackets**. On each contract set **Use flat rates** off to apply brackets (otherwise contract %).",
              fa: "پله‌ها را در **Rate brackets** بگذارید. روی قرارداد **Use flat rates** را خاموش کنید تا پله‌ها اعمال شوند (وگرنه درصد ثابت قرارداد).",
            },
          ],
          related: ["settlements", "bank-accounts", "tax-periods"],
        },
        {
          id: "tax-periods",
          title: { en: "Tax periods", fa: "دوره‌های مالیاتی" },
          adminPath: "/sales/tax/periods",
          summary: {
            en: "Persisted filing windows with VAT snapshots (output + settlement). Attach authority letters. Separate from live VAT report and GL fiscal periods.",
            fa: "بازه‌های اظهارنامه با اسنپ‌شات VAT (خروجی + تسویه). پیوست نامه سازمان. جدا از گزارش زنده VAT و دوره حسابداری.",
          },
          howTo: [
            {
              en: "Create period with code + dates → Generate snapshot or Close (auto-snapshots) → Mark filed. Upload scans on the period detail.",
              fa: "دوره با کد و تاریخ بسازید → Generate snapshot یا Close → Mark filed. اسکن‌ها را روی جزئیات دوره آپلود کنید.",
            },
          ],
          related: ["payroll", "settlements"],
        },
        {
          id: "bank-accounts",
          title: { en: "Bank accounts", fa: "حساب‌های بانکی" },
          adminPath: "/finance/bank-accounts",
          summary: {
            en: "Organization/website bank accounts used for payouts, receipts, and display to customers.",
            fa: "حساب‌های بانکی سازمان/وب‌سایت برای پرداخت، دریافت و نمایش به مشتری.",
          },
          purpose: {
            en: "Master list of accounts linked to banks; needed for manual payment instructions and finance ops.",
            fa: "فهرست اصلی حساب‌های متصل به بانک؛ لازم برای دستور پرداخت دستی و عملیات مالی.",
          },
          howTo: [
            {
              en: "Ensure Banks exist (super admin) → create Bank Accounts with IBAN/account fields.",
              fa: "ابتدا Banks (سوپرادمین) → سپس Bank Accounts با فیلدهای شبا/حساب.",
            },
          ],
          related: ["banks", "payments", "withdrawals"],
        },
        {
          id: "currency-rates",
          title: { en: "Exchange rates", fa: "نرخ ارز" },
          adminPath: "/finance/currency-rates",
          summary: {
            en: "How many units of each currency equal 1 USD; multi-currency mode toggle per site.",
            fa: "چند واحد از هر ارز برابر ۱ USD است؛ سوییچ حالت چندارزی per سایت.",
          },
          purpose: [
            {
              en: "Catalog prices stay in site currency; USD dual storage enables conversion for other storefront currencies.",
              fa: "قیمت کاتالوگ در ارز سایت می‌ماند؛ ذخیره دوگانه USD تبدیل برای ارزهای دیگر استورفرانت را ممکن می‌کند.",
            },
            {
              en: "Single-currency sites still set the default rate for the site currency.",
              fa: "سایت‌های تک‌ارزی هم نرخ پیش‌فرض ارز سایت را تنظیم می‌کنند.",
            },
          ],
          howTo: [
            {
              en: "Open Exchange Rates; set units-per-1-USD (e.g. IRR ≈ 2,000,000).",
              fa: "Exchange Rates را باز کنید؛ واحد per ۱ USD را بگذارید (مثلاً IRR ≈ ۲٬۰۰۰٬۰۰۰).",
            },
            {
              en: "Enable multi-currency only if you truly need extra display currencies and dual USD storage.",
              fa: "multi-currency را فقط اگر واقعاً ارز نمایشی اضافه و ذخیره USD لازم دارید روشن کنید.",
            },
          ],
          related: ["currencies", "products", "orders"],
        },
        {
          id: "banks",
          title: { en: "Banks", fa: "بانک‌ها" },
          adminPath: "/finance/banks",
          summary: {
            en: "Global bank directory (super admin) referenced by bank accounts.",
            fa: "فهرست سراسری بانک‌ها (سوپرادمین) مرجع حساب‌های بانکی.",
          },
          access: { en: "Super admin / master only", fa: "فقط سوپرادمین / مستر" },
          purpose: {
            en: "Canonical bank names/codes so accounts are consistent across websites.",
            fa: "نام/کد استاندارد بانک تا حساب‌ها بین وب‌سایت‌ها یکدست باشند.",
          },
          howTo: [
            {
              en: "Maintain the bank list; then site finance users attach accounts to these banks.",
              fa: "فهرست بانک را نگه دارید؛ بعد کاربران مالی سایت حساب را به این بانک‌ها وصل می‌کنند.",
            },
          ],
          related: ["bank-accounts"],
        },
        {
          id: "currencies",
          title: { en: "Currencies", fa: "ارزها" },
          adminPath: "/finance/currencies",
          summary: {
            en: "Global currency catalog (super admin): codes, precision, display.",
            fa: "کاتالوگ سراسری ارز (سوپرادمین): کد، دقت، نمایش.",
          },
          access: { en: "Super admin / master only", fa: "فقط سوپرادمین / مستر" },
          purpose: {
            en: "Define which currency codes exist in the system before sites use them in rates/pricing.",
            fa: "تعریف کدهای ارز موجود در سیستم قبل از استفاده سایت‌ها در نرخ/قیمت.",
          },
          howTo: [
            {
              en: "Add/edit currencies carefully — changing precision later is painful for historical orders.",
              fa: "ارز را با دقت اضافه/ویرایش کنید — عوض کردن precision بعداً برای سفارش‌های تاریخی سخت است.",
            },
          ],
          related: ["currency-rates"],
        },
      ],
    },

    /* ───────────────────── Promotions & shipping ───────────────────── */
    {
      id: "promotions",
      title: { en: "Promotions & shipping", fa: "پروموشن و ارسال" },
      summary: {
        en: "Coupons, shipping methods/rates, tax rates, VAT report.",
        fa: "کوپن، روش/نرخ ارسال، نرخ مالیات، گزارش VAT.",
      },
      pages: [
        {
          id: "coupons",
          title: { en: "Coupons", fa: "کوپن‌ها" },
          adminPath: "/sales/coupons",
          summary: {
            en: "Discount codes and promotional rules applied at checkout.",
            fa: "کد تخفیف و قوانین پروموشن در تسویه حساب.",
          },
          purpose: {
            en: "Drive campaigns with percentage/fixed discounts, limits, validity windows.",
            fa: "کمپین با تخفیف درصدی/مبلغی، سقف، بازه اعتبار.",
          },
          howTo: [
            {
              en: "Create a coupon with code, discount type/value, constraints, active dates. Min order 0 means no minimum; when set, cart must exceed it.",
              fa: "کوپن با کد، نوع/مقدار تخفیف، محدودیت‌ها، تاریخ فعال بسازید. حداقل سفارش 0 یعنی بدون حداقل؛ اگر مقدار داشت سبد باید بیشتر باشد.",
            },
            {
              en: "Amounts are in site currency (no dollar sign). Test on storefront checkout before announcing publicly.",
              fa: "مبالغ به ارز سایت است (بدون علامت دلار). قبل از اعلام عمومی روی تسویه استورفرانت تست کنید.",
            },
          ],
          related: ["orders"],
        },
        {
          id: "shipping",
          title: { en: "Shipping", fa: "ارسال" },
          adminPath: "/sales/shipping",
          summary: {
            en: "Shipping methods (post, motorcycle, …), prepaid/COD modes, free-shipping thresholds, and rate tables.",
            fa: "روش‌های ارسال (پست، پیک، …)، حالت پیش‌پرداخت/COD، آستانه ارسال رایگان و جدول نرخ.",
          },
          purpose: {
            en: "Tell checkout how orders can be delivered, what they cost, and when shipping is free.",
            fa: "به تسویه بگوید سفارش چطور ارسال می‌شود، چقدر هزینه دارد و کی رایگان است.",
          },
          howTo: [
            {
              en: "Define methods with prepaid and/or COD, optional free-shipping min order, then attach zone rates.",
              fa: "روش‌ها را با prepaid و/یا COD و حداقل سفارش برای ارسال رایگان تعریف کنید، سپس نرخ منطقه‌ای وصل کنید.",
            },
            {
              en: "Site-wide COD and free-shipping floors are also on Website edit (Shipping & delivery).",
              fa: "سوییچ COD و آستانه ارسال رایگان سراسری در ویرایش وب‌سایت (Shipping & delivery) هم هست.",
            },
            {
              en: "Validate with sample carts for different destinations and cart totals.",
              fa: "با سبد نمونه برای مقصدها و مبالغ مختلف اعتبارسنجی کنید.",
            },
          ],
          related: ["orders", "tax", "initial-countries", "websites"],
        },
        {
          id: "tax",
          title: { en: "Tax rates", fa: "نرخ مالیات" },
          adminPath: "/sales/tax",
          summary: {
            en: "Tax/VAT rate configuration applied to sellable amounts.",
            fa: "پیکربندی نرخ مالیات/VAT اعمال‌شده روی مبالغ فروش.",
          },
          purpose: {
            en: "Compliance and correct invoice totals for jurisdictions you sell in.",
            fa: "انطباق و جمع صحیح فاکتور برای حوزه‌هایی که می‌فروشید.",
          },
          howTo: [
            {
              en: "Create tax rates with the correct percentage and scope.",
              fa: "نرخ مالیات با درصد و محدوده درست بسازید.",
            },
            {
              en: "Confirm product/order totals include or exclude tax per your pricing policy.",
              fa: "تأیید کنید جمع محصول/سفارش مطابق سیاست قیمت‌گذاری tax-inclusive یا exclusive است.",
            },
          ],
          related: ["vat-report", "orders", "settlements"],
        },
        {
          id: "vat-report",
          title: { en: "VAT report", fa: "گزارش VAT" },
          adminPath: "/sales/tax/report",
          summary: {
            en: "Reporting view over VAT/tax collected for finance/compliance.",
            fa: "نمای گزارش مالیات/VAT جمع‌شده برای مالی/انطباق.",
          },
          purpose: {
            en: "Summarize taxable sales for a period without exporting the whole order database manually.",
            fa: "خلاصه فروش مشمول برای یک بازه بدون خروجی دستی کل دیتابیس سفارش.",
          },
          howTo: [
            {
              en: "Open VAT report, set period, run the report, then use Print PDF to print the rendered report HTML (not the full admin chrome).",
              fa: "گزارش VAT را باز کنید، بازه بگذارید، اجرا کنید، سپس Print PDF تا HTML رندر‌شده گزارش چاپ/PDF شود (نه کل پنل ادمین).",
            },
          ],
          related: ["tax", "orders", "settlements"],
        },
      ],
    },

    /* ───────────────────── Messages ───────────────────── */
    {
      id: "messages",
      title: { en: "Messages", fa: "پیام‌ها" },
      summary: {
        en: "Contact inbox, SMTP accounts, email templates.",
        fa: "صندوق تماس، حساب SMTP، قالب ایمیل.",
      },
      pages: [
        {
          id: "contact-messages",
          title: { en: "Contact messages", fa: "پیام‌های تماس" },
          adminPath: "/messages/contacts",
          summary: {
            en: "Messages submitted via contact forms on the public site.",
            fa: "پیام‌های ارسال‌شده از فرم تماس سایت عمومی.",
          },
          purpose: {
            en: "Respond to inbound leads/support requests that are not full support tickets.",
            fa: "پاسخ به لید/درخواست‌هایی که تیکت کامل پشتیبانی نیستند.",
          },
          howTo: [
            {
              en: "Open Contact Messages, read threads, mark handled according to your process.",
              fa: "Contact Messages را باز کنید، بخوانید، طبق فرایند handle کنید.",
            },
          ],
          related: ["forms", "support-tickets", "email-accounts"],
        },
        {
          id: "email-accounts",
          title: { en: "Email accounts", fa: "حساب‌های ایمیل" },
          adminPath: "/messages/email-accounts",
          summary: {
            en: "SMTP senders per purpose: No-Reply, Support, Sales, Marketing, …",
            fa: "فرستنده SMTP per کاربرد: No-Reply، Support، Sales، Marketing، …",
          },
          purpose: {
            en: "Register real mailboxes/SMTP credentials the platform uses to send mail.",
            fa: "ثبت صندوق/SMTP واقعی که پلتفرم برای ارسال ایمیل استفاده می‌کند.",
          },
          howTo: [
            {
              en: "Create one account per purpose; test send if available.",
              fa: "به ازای هر کاربرد یک حساب بسازید؛ در صورت وجود ارسال آزمایشی کنید.",
            },
            {
              en: "Keep secrets out of source control; rotate passwords when staff changes.",
              fa: "secret را در سورس نگذارید؛ با تغییر نیرو رمز را بچرخانید.",
            },
          ],
          related: ["email-templates", "settings"],
        },
        {
          id: "email-templates",
          title: { en: "Email templates", fa: "قالب‌های ایمیل" },
          adminPath: "/messages/email-templates",
          summary: {
            en: "Subject & HTML for system emails; website overrides built-in defaults.",
            fa: "موضوع و HTML ایمیل‌های سیستم؛ وب‌سایت پیش‌فرض داخلی را override می‌کند.",
          },
          purpose: {
            en: "Customize OTP, order, support, and other automated messages per site/language.",
            fa: "سفارشی‌سازی OTP، سفارش، پشتیبانی و دیگر پیام‌های خودکار per سایت/زبان.",
          },
          howTo: [
            {
              en: "Open a template, edit subject/body per language, keep required placeholders intact.",
              fa: "یک قالب را باز کنید، موضوع/بدنه per زبان را ویرایش کنید، placeholderهای لازم را حفظ کنید.",
            },
          ],
          tips: [
            {
              en: "Until you customize a template, the system uses the built-in default.",
              fa: "تا وقتی سفارشی نکنید، سیستم از پیش‌فرض داخلی استفاده می‌کند.",
            },
          ],
          related: ["email-accounts", "orders", "support-tickets"],
        },
      ],
    },

    /* ───────────────────── Media ───────────────────── */
    {
      id: "media",
      title: { en: "Media", fa: "مدیا" },
      summary: {
        en: "Media library, folders, tags, storage providers, watermarks.",
        fa: "کتابخانه مدیا، پوشه، تگ، ذخیره‌سازی، واترمارک.",
      },
      pages: [
        {
          id: "media-library",
          title: { en: "Media library", fa: "کتابخانه مدیا" },
          adminPath: "/media",
          summary: {
            en: "Upload, browse, preview, and pick files used across content and catalog.",
            fa: "آپلود، مرور، پیش‌نمایش و انتخاب فایل برای محتوا و کاتالوگ.",
          },
          purpose: {
            en: "Central asset manager (images and files) with folder/tag organization.",
            fa: "مدیر مرکزی دارایی (تصویر و فایل) با سازمان پوشه/تگ.",
          },
          howTo: [
            {
              en: "Open Media Library → upload via dialog, organize into folders, tag for search.",
              fa: "Media Library → آپلود از دیالوگ، پوشه‌بندی، تگ برای جستجو.",
            },
            {
              en: "From product/post editors, use the file picker to attach existing assets.",
              fa: "از ادیتور محصول/پست با file picker دارایی موجود را وصل کنید.",
            },
          ],
          related: ["media-folders", "media-tags", "storage", "watermark", "products", "posts"],
        },
        {
          id: "media-folders",
          title: { en: "Media folders", fa: "پوشه‌های مدیا" },
          adminPath: "/media/folders",
          summary: {
            en: "Folder tree for organizing library files.",
            fa: "درخت پوشه برای نظم فایل‌های کتابخانه.",
          },
          purpose: {
            en: "Keep large libraries navigable (products, banners, documents).",
            fa: "کتابخانه‌های بزرگ را قابل‌پیمایش نگه می‌دارد (محصول، بنر، سند).",
          },
          howTo: [
            {
              en: "Create a folder hierarchy that matches your team’s mental model, then move uploads into it.",
              fa: "سلسله‌مراتب پوشه مطابق ذهن تیم بسازید، بعد آپلودها را جابه‌جا کنید.",
            },
          ],
          related: ["media-library", "media-tags"],
        },
        {
          id: "media-tags",
          title: { en: "Media tags", fa: "برچسب‌های مدیا" },
          adminPath: "/media/tags",
          summary: {
            en: "Labels for filtering media assets quickly.",
            fa: "برچسب برای فیلتر سریع دارایی‌های مدیا.",
          },
          purpose: {
            en: "Cross-cutting organization when folders alone are not enough.",
            fa: "سازمان‌دهی عرضی وقتی فقط پوشه کافی نیست.",
          },
          howTo: [
            {
              en: "Define tags, apply them in the library, filter by tag when picking files.",
              fa: "تگ تعریف کنید، در کتابخانه بزنید، هنگام انتخاب فایل فیلتر کنید.",
            },
          ],
          related: ["media-library"],
        },
        {
          id: "storage",
          title: { en: "Storage", fa: "ذخیره‌سازی" },
          adminPath: "/media/storage",
          summary: {
            en: "Configure where files live (local/cloud providers such as S3, Azure, …).",
            fa: "پیکربندی محل فایل‌ها (لوکال/ابر مثل S3، Azure، …).",
          },
          purpose: {
            en: "Connect Dotnetable to object storage so media scales beyond the app server disk.",
            fa: "اتصال Dotnetable به object storage تا مدیا فراتر از دیسک سرور اپ مقیاس شود.",
          },
          howTo: [
            {
              en: "Open Storage settings, choose provider, enter credentials/bucket, save, test upload.",
              fa: "تنظیمات Storage را باز کنید، provider را انتخاب کنید، credential/bucket بدهید، ذخیره و آپلود آزمایشی.",
            },
          ],
          tips: [
            {
              en: "Wrong credentials break all new uploads — keep a rollback plan.",
              fa: "credential اشتباه همه آپلودهای جدید را می‌شکند — برنامه برگشت داشته باشید.",
            },
          ],
          related: ["media-library", "watermark"],
        },
        {
          id: "watermark",
          title: { en: "Watermark", fa: "واترمارک" },
          adminPath: "/media/watermark",
          summary: {
            en: "Automatic watermarking rules for images (brand protection).",
            fa: "قوانین واترمارک خودکار تصاویر (حفاظت برند).",
          },
          purpose: {
            en: "Apply logo/text overlays on uploaded images when enabled.",
            fa: "در صورت فعال بودن، لوگو/متن را روی تصاویر آپلودی بنشاند.",
          },
          howTo: [
            {
              en: "Configure watermark image/position/opacity; verify on a sample upload.",
              fa: "تصویر/موقعیت/شفافیت واترمارک را تنظیم کنید؛ روی آپلود نمونه تست کنید.",
            },
          ],
          related: ["media-library", "storage"],
        },
      ],
    },

    /* ───────────────────── Administration / initial data ───────────────────── */
    {
      id: "administration",
      title: { en: "Administration / initial data", fa: "اداره / داده اولیه" },
      summary: {
        en: "Countries, states, cities, admin language catalog, admin translations.",
        fa: "کشور، استان، شهر، کاتالوگ زبان ادمین، ترجمه‌های ادمین.",
      },
      pages: [
        {
          id: "initial-countries",
          title: { en: "Countries", fa: "کشورها" },
          adminPath: "/initial-data/countries",
          summary: {
            en: "Reference data for countries used by addresses, shipping, and selectors.",
            fa: "داده مرجع کشورها برای آدرس، ارسال و انتخاب‌گرها.",
          },
          access: { en: "Super admin / master only", fa: "فقط سوپرادمین / مستر" },
          purpose: {
            en: "Shared geo foundation for the whole instance.",
            fa: "پایه جغرافیایی مشترک برای کل نمونه.",
          },
          howTo: [
            {
              en: "Maintain country list; then add states and cities beneath.",
              fa: "فهرست کشور را نگه دارید؛ بعد استان و شهر زیرش بسازید.",
            },
          ],
          related: ["initial-states", "initial-cities", "shipping"],
        },
        {
          id: "initial-states",
          title: { en: "States / provinces", fa: "استان‌ها" },
          adminPath: "/initial-data/states",
          summary: {
            en: "State/province reference data under countries.",
            fa: "داده مرجع استان/ایالت زیر کشورها.",
          },
          access: { en: "Super admin / master only", fa: "فقط سوپرادمین / مستر" },
          purpose: {
            en: "Second level of address hierarchy.",
            fa: "سطح دوم سلسله‌مراتب آدرس.",
          },
          howTo: [
            {
              en: "Select country context, create/edit states used by city lists and shipping.",
              fa: "زمینه کشور را انتخاب کنید، استان‌های مورد استفاده شهر و ارسال را بسازید/ویرایش کنید.",
            },
          ],
          related: ["initial-countries", "initial-cities"],
        },
        {
          id: "initial-cities",
          title: { en: "Cities", fa: "شهرها" },
          adminPath: "/initial-data/cities",
          summary: {
            en: "City reference data for address pickers and logistics.",
            fa: "داده مرجع شهر برای انتخاب آدرس و لجستیک.",
          },
          access: { en: "Super admin / master only", fa: "فقط سوپرادمین / مستر" },
          purpose: {
            en: "Leaf level of geo reference used widely in checkout and profiles.",
            fa: "سطح برگ geo که در تسویه و پروفایل زیاد استفاده می‌شود.",
          },
          howTo: [
            {
              en: "Create cities under the correct state; keep names consistent with postal realities.",
              fa: "شهرها را زیر استان درست بسازید؛ نام‌ها را با واقعیت پستی هماهنگ نگه دارید.",
            },
          ],
          related: ["initial-states", "shipping", "clients"],
        },
        {
          id: "languages-catalog",
          title: { en: "Language catalog (Admin UI)", fa: "کاتالوگ زبان (UI ادمین)" },
          adminPath: "/languages",
          summary: {
            en: "Admin panel language catalog only (WebsiteID empty). Controls which languages the admin UI can switch to.",
            fa: "فقط کاتالوگ زبان پنل ادمین (WebsiteID خالی). مشخص می‌کند UI ادمین به چه زبان‌هایی سوییچ کند.",
          },
          access: { en: "Super admin / master only", fa: "فقط سوپرادمین / مستر" },
          purpose: [
            {
              en: "Not the same as Website → Languages (storefront languages).",
              fa: "همان Website → Languages (زبان استورفرانت) نیست.",
            },
            {
              en: "Even master site 1 has its own website languages separately.",
              fa: "حتی سایت مستر ۱ هم زبان وب‌سایت جدا دارد.",
            },
          ],
          howTo: [
            {
              en: "Enable admin UI languages operators need; pair with Admin Translations for strings.",
              fa: "زبان‌های UI ادمین لازم اپراتور را فعال کنید؛ با Admin Translations برای رشته‌ها جفت کنید.",
            },
          ],
          related: ["admin-translations", "website-languages"],
        },
        {
          id: "admin-translations",
          title: { en: "Admin translations", fa: "ترجمه‌های ادمین" },
          adminPath: "/translations",
          summary: {
            en: "Localization strings for the Admin panel UI itself.",
            fa: "رشته‌های بومی‌سازی خود UI پنل ادمین.",
          },
          access: { en: "Super admin / master only", fa: "فقط سوپرادمین / مستر" },
          purpose: {
            en: "Translate menu labels and admin screens; separate from storefront translations.",
            fa: "ترجمه برچسب منو و صفحات ادمین؛ جدا از ترجمه‌های استورفرانت.",
          },
          howTo: [
            {
              en: "Search keys, edit per language, add missing keys when new UI ships.",
              fa: "کلید جستجو کنید، per زبان ویرایش کنید، با UI جدید کلید گم‌شده اضافه کنید.",
            },
          ],
          related: ["languages-catalog", "website-translations"],
        },
      ],
    },

    /* ───────────────────── System ───────────────────── */
    {
      id: "system",
      title: { en: "System", fa: "سیستم" },
      summary: {
        en: "Database updates and system-wide settings.",
        fa: "به‌روزرسانی دیتابیس و تنظیمات سراسری سیستم.",
      },
      pages: [
        {
          id: "db-updates",
          title: { en: "DB updates", fa: "به‌روزرسانی دیتابیس" },
          adminPath: "/system/updates",
          summary: {
            en: "Apply pending database schema/data updates after deployments.",
            fa: "اعمال به‌روزرسانی‌های schema/داده دیتابیس بعد از استقرار.",
          },
          access: { en: "Super admin / master only", fa: "فقط سوپرادمین / مستر" },
          purpose: {
            en: "Safe operator surface for running versioned DB updates required by new Admin/API builds.",
            fa: "سطح امن اپراتور برای اجرای آپدیت‌های نسخه‌دار DB لازم برای بیلدهای جدید Admin/API.",
          },
          howTo: [
            {
              en: "After deploying a new version, open DB Updates and apply pending steps in order.",
              fa: "بعد از deploy نسخه جدید، DB Updates را باز کنید و مراحل معلق را به ترتیب اعمال کنید.",
            },
            {
              en: "Backup first on production; never interrupt a long migration without a plan.",
              fa: "در production اول backup؛ مهاجرت طولانی را بدون برنامه قطع نکنید.",
            },
          ],
          tips: [
            {
              en: "If the app redirects to setup or complains about schema, this page is often the fix path.",
              fa: "اگر اپ به setup ببرد یا از schema شکایت کند، اغلب مسیر رفع از این صفحه است.",
            },
          ],
          related: ["settings"],
        },
        {
          id: "settings",
          title: { en: "Settings", fa: "تنظیمات" },
          adminPath: "/settings",
          summary: {
            en: "System-wide configuration: bot protection (Turnstile/math captcha) and shortcuts to email setup.",
            fa: "پیکربندی سراسری: محافظت ربات (Turnstile/math captcha) و میانبر تنظیم ایمیل.",
          },
          access: { en: "Super admin / master only", fa: "فقط سوپرادمین / مستر" },
          purpose: [
            {
              en: "Bot protection for login and forgot-password: Cloudflare Turnstile when keys are set, otherwise math captcha.",
              fa: "محافظت ربات برای لاگین و فراموشی رمز: Turnstile اگر کلید باشد، وگرنه captcha ریاضی.",
            },
            {
              en: "Email SMTP accounts and HTML templates live on their own Messages pages — Settings only links there.",
              fa: "حساب SMTP و قالب HTML در صفحات Messages هستند — Settings فقط لینک می‌دهد.",
            },
          ],
          howTo: [
            {
              en: "Choose Captcha mode (Auto / Turnstile / Math), paste Turnstile keys if used, Save Protection.",
              fa: "حالت Captcha را انتخاب کنید (Auto / Turnstile / Math)، در صورت استفاده کلید Turnstile بگذارید، Save Protection.",
            },
            {
              en: "Use the buttons to jump to Email Accounts and Email Templates.",
              fa: "با دکمه‌ها به Email Accounts و Email Templates بروید.",
            },
          ],
          related: ["email-accounts", "email-templates", "login-logs"],
        },
      ],
    },
  ],
};
