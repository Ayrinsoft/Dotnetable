/**
 * Dotnetable public API documentation catalog.
 *
 * Auth is explained once on page id "auth". Other pages only mark auth level and link back.
 * Keep in sync with src/Dotnetable.API/Controllers/** and BaseController headers.
 */
window.DOCS_API = {
  meta: {
    version: "1.0",
    maintenance: {
      title: {
        en: "Keep API docs in sync",
        fa: "هم‌گام‌سازی مستندات API",
      },
      body: {
        en: "When you add or change a controller endpoint, update the matching page here (path, body, sample response). Cross-link Admin pages when the same concept is managed in Admin.",
        fa: "وقتی اندپوینت کنترلر اضافه یا عوض می‌شود، همان صفحه را اینجا (مسیر، بدنه، نمونه پاسخ) به‌روز کنید. اگر مفهوم در ادمین هم مدیریت می‌شود، لینک متقابل بگذارید.",
      },
      items: [
        {
          en: "Auth rules change → update only the Authentication page (unless a specific endpoint differs).",
          fa: "تغییر قوانین احراز → فقط صفحه Authentication (مگر اندپوینت خاصی فرق کند).",
        },
        {
          en: "New public feature → Admin docs + API docs + relatedAdmin/relatedApi links.",
          fa: "قابلیت عمومی جدید → مستند ادمین + API + لینک relatedAdmin/relatedApi.",
        },
      ],
    },
  },

  sections: [
    /* ───────── Start ───────── */
    {
      id: "start",
      title: { en: "Start here", fa: "شروع" },
      summary: {
        en: "How every request is scoped to a website and when you need a customer token.",
        fa: "هر درخواست چطور به یک وب‌سایت محدود می‌شود و کی به توکن مشتری نیاز دارید.",
      },
      pages: [
        {
          id: "home",
          title: { en: "Welcome", fa: "خوش‌آمدید" },
          summary: {
            en: "Public REST API used by storefronts (Web/React) and other clients.",
            fa: "API عمومی REST که استورفرانت (Web/React) و کلاینت‌های دیگر صدا می‌زنند.",
          },
          purpose: [
            {
              en: "Base path pattern: `/api/{Controller}`. Version is **not** in the URL — send **`X-Api-Version`** (current **1.0**). Old clients that omit it still get 1.0 after a later version ships.",
              fa: "الگوی مسیر: `/api/{Controller}`. نسخه داخل URL نیست — هدر **`X-Api-Version`** (فعلی **1.0**). کلاینت قدیمی که هدر ندهد بعد از آمدن نسخه جدید هم 1.0 می‌گیرد.",
            },
            {
              en: "Almost every call needs **`X-Website-Key`** so the server knows which site’s catalog/orders to use. Customer actions also need a JWT — details are only on the Authentication page.",
              fa: "تقریباً همه درخواست‌ها **`X-Website-Key`** می‌خواهند تا سرور بداند کاتالوگ/سفارش کدام سایت است. کارهای مشتری JWT هم می‌خواهند — جزئیات فقط در صفحه احراز هویت.",
            },
            {
              en: "Use the **Admin / API** switch above to jump to the operator panel guide for the same business concepts (products, stock, coupons, …).",
              fa: "با سوییچ **ادمین / API** بالا به راهنمای پنل همان مفهوم کسب‌وکار (محصول، موجودی، کوپن، …) بروید.",
            },
          ],
          howTo: [
            {
              en: "Read **Authentication** once.",
              fa: "یک‌بار **احراز هویت** را بخوانید.",
            },
            {
              en: "Open the resource you need (Products, Cart, Checkout, …) and copy the sample request.",
              fa: "منبع لازم (Products، Cart، Checkout، …) را باز کنید و نمونه request را کپی کنید.",
            },
          ],
          related: ["auth", "versioning", "products", "cart", "checkout"],
        },
        {
          id: "versioning",
          title: { en: "API versioning", fa: "نسخه‌گذاری API" },
          summary: {
            en: "Send the contract version in a header so a storefront built against 1.0 keeps working after 2.0 exists.",
            fa: "نسخه قرارداد را در هدر بفرستید تا استورفرانت ساخته‌شده روی 1.0 بعد از آمدن 2.0 هم کار کند.",
          },
          purpose: [
            {
              en: "URLs stay `/api/{Controller}`. The server picks the implementation from **`X-Api-Version`**.",
              fa: "مسیر همان `/api/{Controller}` می‌ماند. سرور پیاده‌سازی را از **`X-Api-Version`** انتخاب می‌کند.",
            },
            {
              en: "Additive changes (new optional fields, new endpoints) stay on **1.0**. A breaking change is a new version on the same path; v1 is left mapped.",
              fa: "تغییرات افزودنی (فیلد/اندپوینت جدید اختیاری) روی **1.0** می‌مانند. تغییر شکننده نسخهٔ جدید روی همان مسیر است؛ v1 سر جایش می‌ماند.",
            },
            {
              en: "If the header is omitted, the API assumes **1.0**. First-party Web and React clients always send `1.0` so they stay pinned when a later default is introduced.",
              fa: "اگر هدر نباشد API **1.0** فرض می‌کند. کلاینت‌های Web و React همیشه `1.0` می‌فرستند تا بعداً هم روی همان قرارداد بمانند.",
            },
          ],
          howTo: [
            {
              en: "On every storefront call add `-H \"X-Api-Version: 1.0\"` (together with `X-Website-Key`).",
              fa: "روی هر فراخوانی استورفرانت `-H \"X-Api-Version: 1.0\"` بگذارید (همراه `X-Website-Key`).",
            },
            {
              en: "Read response header `api-supported-versions` (and `api-deprecated-versions` when a version is being retired).",
              fa: "هدر پاسخ `api-supported-versions` را بخوانید (و در صورت بازنشستگی نسخه، `api-deprecated-versions`).",
            },
            {
              en: "Unknown versions return **400** with ProblemDetails. The 1.0 contract is unchanged.",
              fa: "نسخه ناشناخته **400** با ProblemDetails برمی‌گردد. قرارداد 1.0 عوض نمی‌شود.",
            },
          ],
          sections: [
            {
              title: { en: "Header", fa: "هدر" },
              definitions: [
                {
                  term: { en: "`X-Api-Version`", fa: "`X-Api-Version`" },
                  def: {
                    en: "Required for new integrations. Value is `1.0` today (`1` is accepted as the same version). The negotiated value is echoed on the response as `X-Api-Version`.",
                    fa: "برای یکپارچه‌سازی جدید لازم است. مقدار امروز `1.0` است (`1` همان نسخه است). مقدار توافق‌شده در پاسخ هم با `X-Api-Version` برمی‌گردد.",
                  },
                },
              ],
            },
            {
              title: { en: "Version-neutral routes", fa: "مسیرهای بدون نسخه" },
              items: [
                {
                  en: "`/api/files/…` — local file download used by `<img>` tags; browsers cannot send custom headers.",
                  fa: "`/api/files/…` — دانلود فایل محلی برای تگ `<img>`؛ مرورگر هدر سفارشی نمی‌فرستد.",
                },
                {
                  en: "`/api/cache/invalidate` — internal Admin → API cache flush, not a public contract.",
                  fa: "`/api/cache/invalidate` — پاک‌سازی کش داخلی ادمین → API، قرارداد عمومی نیست.",
                },
              ],
              callout: {
                tone: "info",
                text: {
                  en: "Do not put the version in the path (`/api/v1/…`). Path versioning would break every bookmark, image URL, and already-shipped client.",
                  fa: "نسخه را در مسیر نگذارید (`/api/v1/…`). نسخه‌گذاری در URL بوکمارک، آدرس تصویر و کلاینت‌های منتشرشده را می‌شکند.",
                },
              },
            },
          ],
          endpoints: [
            {
              title: { en: "Example (any 1.0 resource)", fa: "نمونه (هر منبع 1.0)" },
              method: "GET",
              path: "/api/siteinfo",
              auth: "website",
              summary: {
                en: "Same path for every version; the header selects 1.0.",
                fa: "برای همه نسخه‌ها همان مسیر؛ هدر نسخه 1.0 را انتخاب می‌کند.",
              },
              headers: [
                { name: "X-Website-Key", desc: { en: "Website AuthCode GUID", fa: "GUID کلید وب‌سایت" } },
                { name: "X-Api-Version", desc: { en: "1.0", fa: "1.0" } },
              ],
            },
          ],
          related: ["auth", "home"],
          relatedAdmin: ["website-api-key"],
        },
        {
          id: "auth",
          title: { en: "Authentication", fa: "احراز هویت" },
          summary: {
            en: "One place for website key, customer JWT, cart session, and API version. Other pages only say which of these they need.",
            fa: "یک‌جا برای کلید وب‌سایت، JWT مشتری، نشست سبد و نسخه API. بقیه صفحات فقط می‌گویند کدام‌ها را می‌خواهند.",
          },
          purpose: [
            {
              en: "Dotnetable hosts many websites in one API. Every public call must say **which website** is calling. That is **`X-Website-Key`** (the site AuthCode GUID from Admin → Website API Key).",
              fa: "Dotnetable چند وب‌سایت را با یک API میزبانی می‌کند. هر فراخوانی عمومی باید بگوید **کدام وب‌سایت** صدا می‌زند. آن **`X-Website-Key`** است (GUID همان AuthCode از ادمین → Website API Key).",
            },
            {
              en: "Browsing the catalog can be anonymous (only website key). Buying, wallet, addresses, orders need the **customer JWT** from login/register.",
              fa: "مرور کاتالوگ می‌تواند ناشناس باشد (فقط کلید سایت). خرید، کیف پول، آدرس، سفارش به **JWT مشتری** از login/register نیاز دارد.",
            },
            {
              en: "Guest carts use **`X-Cart-Session`** (random string you store in a cookie). After login call cart **merge**.",
              fa: "سبد مهمان با **`X-Cart-Session`** (رشته تصادفی در کوکی) است. بعد از لاگین **merge** سبد را صدا بزنید.",
            },
          ],
          sections: [
            {
              title: { en: "Headers (remember once)", fa: "هدرها (یک‌بار یاد بگیرید)" },
              definitions: [
                {
                  term: { en: "`X-Website-Key`", fa: "`X-Website-Key`" },
                  def: {
                    en: "Required for almost all storefront endpoints. Value = website `AuthCode` (GUID). Missing/invalid → website not resolved (often 400/404).",
                    fa: "برای تقریباً همه اندپوینت‌های استورفرانت لازم است. مقدار = `AuthCode` وب‌سایت (GUID). نبودن/نامعتبر → سایت resolve نمی‌شود (معمولاً 400/404).",
                  },
                },
                {
                  term: { en: "`Authorization`", fa: "`Authorization`" },
                  def: {
                    en: "`Bearer {access_token}` after successful login / OTP verify. Required on endpoints marked JWT.",
                    fa: "بعد از login یا تأیید OTP: `Bearer {access_token}`. روی اندپوینت‌های JWT لازم است.",
                  },
                },
                {
                  term: { en: "`X-Cart-Session`", fa: "`X-Cart-Session`" },
                  def: {
                    en: "Optional guest cart key for `/api/Cart`. Generate a UUID on first visit; keep it stable while the guest shops.",
                    fa: "کلید اختیاری سبد مهمان برای `/api/Cart`. در اولین بازدید UUID بسازید و تا خرید مهمان ثابت نگه دارید.",
                  },
                },
                {
                  term: { en: "`X-Api-Version`", fa: "`X-Api-Version`" },
                  def: {
                    en: "Selects the contract. Current value **1.0**. Omitted → 1.0 (old clients keep working). Details on the Versioning page.",
                    fa: "قرارداد را انتخاب می‌کند. مقدار فعلی **1.0**. نبودن → 1.0 (کلاینت قدیمی کار می‌کند). جزئیات در صفحه نسخه‌گذاری.",
                  },
                },
              ],
            },
            {
              title: { en: "Auth levels used in these docs", fa: "سطح‌های احراز در این مستندات" },
              items: [
                {
                  en: "**Website key only** — public read (products, menus, pages, stock availability).",
                  fa: "**فقط کلید وب‌سایت** — خواندن عمومی (محصول، منو، صفحه، موجودی قابل‌فروش).",
                },
                {
                  en: "**Guest or JWT** — cart works for both; if JWT present, cart is tied to the client.",
                  fa: "**مهمان یا JWT** — سبد برای هر دو کار می‌کند؛ اگر JWT باشد سبد به مشتری وصل است.",
                },
                {
                  en: "**Customer JWT** — profile, checkout, orders, payments, wallet, wishlist, reviews write.",
                  fa: "**JWT مشتری** — پروفایل، تسویه، سفارش، پرداخت، کیف پول، علاقه‌مندی، نوشتن نظر.",
                },
              ],
              callout: {
                tone: "warn",
                text: {
                  en: "Admin panel login is **not** this API. Admin uses its own cookie/session auth. This API is for **website customers** and public storefront data.",
                  fa: "لاگین پنل ادمین **این API نیست**. ادمین احراز خودش را دارد. این API برای **مشتریان وب‌سایت** و داده عمومی استورفرانت است.",
                },
              },
            },
            {
              title: { en: "Sessions and refresh tokens", fa: "نشست و توکن تمدید" },
              items: [
                {
                  en: "Login / verify-otp return **two** tokens: a short-lived `accessToken` (2 hours) you send as `Bearer`, and a long-lived `refreshToken` (30 days) you keep to get a new pair.",
                  fa: "login و verify-otp **دو** توکن برمی‌گردانند: `accessToken` کوتاه‌عمر (۲ ساعت) که به‌عنوان `Bearer` می‌فرستید، و `refreshToken` بلندعمر (۳۰ روز) که برای گرفتن جفت جدید نگه می‌دارید.",
                },
                {
                  en: "Refresh tokens **rotate**: every exchange revokes the one you presented and issues a replacement. Always store the new value and never reuse the old one.",
                  fa: "توکن تمدید **چرخشی** است: هر تعویض، توکن ارائه‌شده را باطل و یکی تازه صادر می‌کند. همیشه مقدار جدید را ذخیره کنید و قبلی را دوباره نفرستید.",
                },
                {
                  en: "Presenting an already-spent refresh token is treated as theft and revokes **every** session for that customer — so do not retry a failed refresh with the same value; sign in again instead.",
                  fa: "ارائه‌ی توکن مصرف‌شده به‌عنوان سرقت تلقی می‌شود و **همه** نشست‌های آن مشتری باطل می‌شود — پس refresh ناموفق را با همان مقدار retry نکنید؛ دوباره وارد شوید.",
                },
                {
                  en: "A password reset revokes all refresh tokens, so a session stolen before the reset cannot outlive it.",
                  fa: "بازنشانی رمز همه توکن‌های تمدید را باطل می‌کند، پس نشستی که پیش از بازنشانی دزدیده شده دوام نمی‌آورد.",
                },
              ],
            },
            {
              title: { en: "Limits, captcha and password rules", fa: "محدودیت‌ها، کپچا و قواعد رمز" },
              items: [
                {
                  en: "**Captcha.** register, login, resend-otp and forgot-password accept `captchaToken` / `captchaAnswer` (see the Captcha page). They are enforced only when the website has captcha configured in Admin, so a site that never enabled it keeps working unchanged.",
                  fa: "**کپچا.** اندپوینت‌های register، login، resend-otp و forgot-password فیلدهای `captchaToken` / `captchaAnswer` می‌گیرند (صفحه Captcha). فقط وقتی اجباری‌اند که سایت در ادمین کپچا را تنظیم کرده باشد.",
                },
                {
                  en: "**Rate limits (per IP).** All auth endpoints: 10 per minute. The code-sending ones (register, resend-otp, forgot-password): 5 per 15 minutes, because each one costs the site an email or an SMS. Over the limit → **429** with `Retry-After`.",
                  fa: "**محدودیت نرخ (به ازای IP).** همه اندپوینت‌های احراز: ۱۰ در دقیقه. آن‌هایی که کد می‌فرستند (register، resend-otp، forgot-password): ۵ در ۱۵ دقیقه، چون هرکدام برای سایت ایمیل یا پیامک خرج دارد. عبور از حد → **429** با `Retry-After`.",
                },
                {
                  en: "**Account limits.** A one-time code dies after 5 wrong attempts — **429**, request a new code. Repeated wrong passwords lock the account for 15 minutes — **423**, and the correct password is refused while the lock stands. A new code cannot be requested more than once a minute.",
                  fa: "**محدودیت حساب.** کد یک‌بارمصرف بعد از ۵ تلاش نادرست می‌میرد — **429**، کد جدید بگیرید. رمز اشتباه مکرر حساب را ۱۵ دقیقه قفل می‌کند — **423**، و تا پایان قفل حتی رمز درست هم رد می‌شود. کد جدید بیش از یک‌بار در دقیقه صادر نمی‌شود.",
                },
                {
                  en: "**Password policy.** At least 10 characters mixing three of: uppercase, lowercase, digits, symbols; not a common password; not containing the account's own email or mobile. A rejected password returns **400** with the reason.",
                  fa: "**سیاست رمز.** حداقل ۱۰ کاراکتر با ترکیب سه مورد از: بزرگ، کوچک، رقم، نماد؛ نه رمز رایج؛ و نه شامل ایمیل یا موبایل خود حساب. رمز ردشده **400** با دلیل برمی‌گرداند.",
                },
                {
                  en: "**Mobile-only sign-up** requires an SMS gateway configured for the website (Admin → SMS gateways). Without one, register returns **503** rather than reporting success for a code that would never arrive.",
                  fa: "**ثبت‌نام فقط با موبایل** به درگاه پیامک تنظیم‌شده برای سایت نیاز دارد (ادمین → درگاه‌های پیامک). بدون آن register کد **503** برمی‌گرداند، به‌جای گزارش موفقیت برای کدی که هرگز نمی‌رسد.",
                },
              ],
            },
          ],
          endpoints: [
            {
              title: { en: "Register", fa: "ثبت‌نام" },
              method: "POST",
              path: "/api/Auth/register",
              auth: "website",
              summary: {
                en: "Create a customer with email and/or mobile + password. Sends OTP.",
                fa: "ساخت مشتری با ایمیل و/یا موبایل + رمز. OTP می‌فرستد.",
              },
              headers: [
                { name: "X-Website-Key", desc: { en: "Website AuthCode GUID", fa: "GUID کلید وب‌سایت" } },
                { name: "X-Api-Version", desc: { en: "1.0", fa: "1.0" } },
                { name: "Content-Type", desc: { en: "application/json", fa: "application/json" } },
              ],
              request: {
                body: {
                  givenName: "Ali",
                  surname: "Rezaei",
                  email: "ali@example.com",
                  countryCode: "+98",
                  cellphone: "9123456789",
                  password: "secret1",
                },
              },
              response: {
                status: 200,
                body: {
                  success: true,
                  channel: "Email",
                  identifier: "ali@example.com",
                  message: "We sent a verification code to your email.",
                },
              },
            },
            {
              title: { en: "Verify OTP", fa: "تأیید OTP" },
              method: "POST",
              path: "/api/Auth/verify-otp",
              auth: "website",
              summary: {
                en: "Activate account and receive JWT in one step when OTP is valid.",
                fa: "با OTP معتبر حساب فعال می‌شود و JWT برمی‌گردد.",
              },
              headers: [
                { name: "X-Website-Key", desc: { en: "Website AuthCode GUID", fa: "GUID کلید وب‌سایت" } },
              ],
              request: {
                body: { identifier: "ali@example.com", code: "123456" },
              },
              response: {
                status: 200,
                body: {
                  accessToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                  expiresAt: "2026-08-03T12:00:00Z",
                  clientId: 42,
                },
                note: {
                  en: "Exact token payload shape comes from the token service; store `accessToken` as Bearer.",
                  fa: "شکل دقیق پاسخ توکن از سرویس JWT است؛ `accessToken` را به‌عنوان Bearer نگه دارید.",
                },
              },
            },
            {
              title: { en: "Login", fa: "ورود" },
              method: "POST",
              path: "/api/Auth/login",
              auth: "website",
              summary: {
                en: "Email or mobile + password → JWT. 403 if not activated yet.",
                fa: "ایمیل یا موبایل + رمز → JWT. اگر هنوز فعال نشده باشد 403.",
              },
              headers: [
                { name: "X-Website-Key", desc: { en: "Website AuthCode GUID", fa: "GUID کلید وب‌سایت" } },
              ],
              request: {
                body: { identifier: "ali@example.com", password: "secret1" },
              },
              response: {
                status: 200,
                body: {
                  accessToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                  expiresAt: "2026-08-03T12:00:00Z",
                  clientId: 42,
                },
              },
              notes: [
                {
                  en: "Invalid credentials → 401. Not activated → 403 with `status: NotActivated`.",
                  fa: "اعتبار اشتباه → 401. فعال‌نشده → 403 با `status: NotActivated`.",
                },
              ],
            },
            {
              title: { en: "Me", fa: "من کیستم" },
              method: "GET",
              path: "/api/Auth/me",
              auth: "jwt",
              summary: {
                en: "Returns claims from the current bearer token.",
                fa: "ادعاهای توکن فعلی را برمی‌گرداند.",
              },
              headers: [
                { name: "Authorization", desc: { en: "Bearer {token}", fa: "Bearer {token}" } },
              ],
              request: {
                body: "(no body)",
              },
              response: {
                status: 200,
                body: {
                  clientId: "42",
                  websiteId: "1",
                  level: "Standard",
                  name: "Ali Rezaei",
                },
              },
            },
            {
              title: { en: "Forgot / reset password", fa: "فراموشی / بازنشانی رمز" },
              method: "POST",
              path: "/api/Auth/forgot-password  ·  /api/Auth/reset-password",
              auth: "website",
              summary: {
                en: "forgot-password sends a code (does not reveal if account exists). reset-password sets a new password with that code.",
                fa: "forgot-password کد می‌فرستد (وجود حساب را لو نمی‌دهد). reset-password با همان کد رمز جدید می‌گذارد.",
              },
              request: {
                body: {
                  forgot: { identifier: "ali@example.com" },
                  reset: { identifier: "ali@example.com", code: "654321", newPassword: "newsecret1" },
                },
              },
              response: {
                status: 200,
                body: { success: true, message: "If the account exists, a reset code has been sent." },
              },
            },
            {
              title: { en: "Refresh session", fa: "تمدید نشست" },
              method: "POST",
              path: "/api/Auth/refresh",
              auth: "website",
              summary: {
                en: "Exchange a refresh token for a new access + refresh pair. Single use: the presented token is revoked here.",
                fa: "تعویض refresh token با یک جفت access + refresh تازه. یک‌بارمصرف: توکن ارائه‌شده همین‌جا باطل می‌شود.",
              },
              headers: [
                { name: "X-Website-Key", desc: { en: "Website AuthCode GUID", fa: "GUID کلید وب‌سایت" } },
              ],
              request: { body: { refreshToken: "Rk9vQmFyLi4u" } },
              response: {
                status: 200,
                body: {
                  accessToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                  expiresAtUtc: "2026-09-01T14:00:00Z",
                  tokenType: "Bearer",
                  refreshToken: "TmV3VG9rZW4uLi4=",
                  refreshExpiresAtUtc: "2026-10-01T12:00:00Z",
                },
                note: {
                  en: "Store the NEW refreshToken and discard the old one. Presenting a spent token is treated as theft and revokes every session for that customer, so never retry a refresh with the same value.",
                  fa: "refreshToken جدید را ذخیره و قبلی را دور بیندازید. ارائه‌ی توکن مصرف‌شده به‌عنوان سرقت تلقی می‌شود و همه نشست‌های آن مشتری باطل می‌شود؛ پس هرگز refresh را با همان مقدار retry نکنید.",
                },
              },
            },
            {
              title: { en: "Sign out", fa: "خروج" },
              method: "POST",
              path: "/api/Auth/logout  ·  /api/Auth/logout-all",
              auth: "website",
              summary: {
                en: "logout revokes the presented refresh token (this device). logout-all needs a JWT and revokes every session for the customer.",
                fa: "logout توکن ارائه‌شده را باطل می‌کند (همین دستگاه). logout-all به JWT نیاز دارد و همه نشست‌های مشتری را باطل می‌کند.",
              },
              request: { body: { refreshToken: "Rk9vQmFyLi4u" } },
              response: { status: 200, body: { success: true } },
            },
          ],
          related: ["versioning", "cart", "checkout", "orders"],
          relatedAdmin: ["website-api-key", "clients", "email-accounts"],
        },
      ],
    },

    /* ───────── Catalog ───────── */
    {
      id: "catalog",
      title: { en: "Catalog", fa: "کاتالوگ" },
      summary: {
        en: "Products, price lists, categories, brands, vendors, stock availability for the storefront.",
        fa: "محصول، لیست قیمت، دسته، برند، فروشنده، موجودی قابل‌فروش برای استورفرانت.",
      },
      pages: [
        {
          id: "products",
          title: { en: "Products", fa: "محصولات" },
          summary: {
            en: "List, detail, and related products for the website resolved by X-Website-Key.",
            fa: "فهرست، جزئیات و محصولات مرتبط برای وب‌سایت resolve‌شده با X-Website-Key.",
          },
          purpose: [
            {
              en: "This is what the shop UI calls to show the catalog. Admin creates/edits products; API only **reads published** data.",
              fa: "همان چیزی که UI فروشگاه برای نمایش کاتالوگ صدا می‌زند. ادمین محصول می‌سازد/ویرایش می‌کند؛ API فقط **منتشرشده** را می‌خواند.",
            },
          ],
          relatedAdmin: ["products", "price-lists", "catalog-categories", "brands"],
          related: ["price-lists", "product-categories", "inventory", "brands", "vendors"],
          endpoints: [
            {
              title: { en: "List products", fa: "فهرست محصولات" },
              method: "GET",
              path: "/api/Products",
              auth: "website",
              summary: {
                en: "Paged, filterable published products with display currency.",
                fa: "محصولات منتشرشده صفحه‌بندی‌شده با فیلتر و ارز نمایش.",
              },
              headers: [
                { name: "X-Website-Key", desc: { en: "Website AuthCode", fa: "کلید وب‌سایت" } },
              ],
              query: [
                { name: "categorySlug", desc: { en: "Filter by product category slug", fa: "فیلتر دسته محصول" } },
                { name: "brandSlug", desc: { en: "Filter by brand", fa: "فیلتر برند" } },
                { name: "search", desc: { en: "Text search", fa: "جستجوی متنی" } },
                { name: "minPrice / maxPrice", desc: { en: "Price range", fa: "بازه قیمت" } },
                { name: "page / pageSize", desc: { en: "Paging (default page=1, pageSize=20)", fa: "صفحه‌بندی" } },
                { name: "lang", desc: { en: "Content language code", fa: "کد زبان محتوا" } },
                { name: "currency", desc: { en: "Display currency code", fa: "کد ارز نمایش" } },
                { name: "inStock", desc: { en: "true/false to filter availability", fa: "true/false برای موجودی" } },
                { name: "attributeOptionIds", desc: { en: "Facet AND filter", fa: "فیلتر وجهی AND" } },
              ],
              request: {
                body: "GET /api/Products?categorySlug=shoes&page=1&pageSize=20&currency=IRR&lang=fa",
              },
              response: {
                status: 200,
                body: {
                  items: [
                    {
                      productId: 10,
                      title: "Running shoes",
                      slug: "running-shoes",
                      price: 2500000,
                      currency: "IRR",
                      inStock: true,
                    },
                  ],
                  totalCount: 1,
                },
                note: {
                  en: "Field names follow the service DTO; treat this as a realistic shape, not a contract freeze.",
                  fa: "نام فیلدها از DTO سرویس می‌آید؛ این یک شکل واقع‌گرایانه است نه قرارداد یخ‌زده.",
                },
              },
            },
            {
              title: { en: "Product by slug", fa: "محصول با اسلاگ" },
              method: "GET",
              path: "/api/Products/{slug}",
              auth: "website",
              pathParams: [
                { name: "slug", desc: { en: "Product slug", fa: "اسلاگ محصول" } },
              ],
              query: [
                { name: "lang", desc: { en: "Language", fa: "زبان" } },
                { name: "currency", desc: { en: "Display currency", fa: "ارز نمایش" } },
              ],
              request: { body: "GET /api/Products/running-shoes?lang=fa&currency=IRR" },
              response: {
                status: 200,
                body: {
                  productId: 10,
                  title: "Running shoes",
                  slug: "running-shoes",
                  variants: [{ variantId: 55, sku: "SHOE-42", price: 2500000 }],
                  description: "…",
                },
              },
            },
            {
              title: { en: "Related products", fa: "محصولات مرتبط" },
              method: "GET",
              path: "/api/Products/{slug}/related",
              auth: "website",
              query: [
                { name: "take", desc: { en: "Max items (default 8)", fa: "حداکثر تعداد (پیش‌فرض ۸)" } },
              ],
              request: { body: "GET /api/Products/running-shoes/related?take=8" },
              response: {
                status: 200,
                body: [{ productId: 11, title: "Sports socks", slug: "sports-socks" }],
              },
            },
          ],
        },
        {
          id: "price-lists",
          title: { en: "Price lists", fa: "لیست قیمت" },
          summary: {
            en: "Rate tables that do not need shop products (steel profiles/sheets). Optional USD follow or live catalog rows.",
            fa: "جدول نرخ بدون نیاز به محصول فروشگاه (پروفیل/ورق استیل). دنبال‌کردن دلار یا ردیف کاتالوگ اختیاری است.",
          },
          purpose: [
            {
              en: "Storefront list and detail. `followsUsd` is true only when the list opted into FX. Catalog-sourced rows include `productSlug`.",
              fa: "فهرست و جزئیات استورفرانت. `followsUsd` فقط وقتی true است که لیست دنبال دلار باشد. ردیف‌های کاتالوگ `productSlug` دارند.",
            },
          ],
          relatedAdmin: ["price-lists"],
          related: ["products"],
          endpoints: [
            {
              title: { en: "List price lists", fa: "فهرست لیست‌های قیمت" },
              method: "GET",
              path: "/api/PriceLists",
              auth: "website",
              headers: [
                { name: "X-Website-Key", desc: { en: "Website AuthCode", fa: "کلید وب‌سایت" } },
              ],
              request: { body: "GET /api/PriceLists" },
              response: {
                status: 200,
                body: [
                  {
                    priceListID: 1,
                    title: "Steel",
                    slug: "steel",
                    itemCount: 12,
                    lastUpdatedAt: "2026-09-05T08:00:00Z",
                    currencyCode: "IRR",
                  },
                ],
              },
            },
            {
              title: { en: "Price list by slug", fa: "لیست قیمت با اسلاگ" },
              method: "GET",
              path: "/api/PriceLists/{slug}",
              auth: "website",
              request: { body: "GET /api/PriceLists/steel" },
              response: {
                status: 200,
                body: {
                  title: "Steel",
                  slug: "steel",
                  lastUpdatedAt: "2026-09-05T08:00:00Z",
                  currencyCode: "IRR",
                  usdToCurrency: 1000000,
                  groups: [
                    {
                      name: "Profiles",
                      items: [
                        {
                          title: "40x40",
                          unit: "kg",
                          linkToUsd: true,
                          price: { amount: 85000000, currencyCode: "IRR", amountUsd: 85 },
                        },
                      ],
                    },
                  ],
                },
              },
            },
          ],
        },
        {
          id: "product-categories",
          title: { en: "Product categories", fa: "دسته‌های محصول" },
          summary: {
            en: "Category tree, category by slug, and facet filters for listing pages.",
            fa: "درخت دسته، دسته با اسلاگ، و فیلترهای facet برای صفحه لیست.",
          },
          purpose: [
            {
              en: "Admin path: Catalog → Categories. API path: `/api/ProductCategories/...`.",
              fa: "مسیر ادمین: Catalog → Categories. مسیر API: `/api/ProductCategories/...`.",
            },
          ],
          relatedAdmin: ["catalog-categories", "attributes"],
          related: ["products"],
          endpoints: [
            {
              title: { en: "Category tree", fa: "درخت دسته" },
              method: "GET",
              path: "/api/ProductCategories/tree",
              auth: "website",
              query: [{ name: "lang", desc: { en: "Language", fa: "زبان" } }],
              request: { body: "GET /api/ProductCategories/tree?lang=fa" },
              response: {
                status: 200,
                body: [{ slug: "shoes", title: "Shoes", children: [{ slug: "running", title: "Running" }] }],
              },
            },
            {
              title: { en: "Category by slug", fa: "دسته با اسلاگ" },
              method: "GET",
              path: "/api/ProductCategories/{slug}",
              auth: "website",
              request: { body: "GET /api/ProductCategories/shoes?lang=fa" },
              response: { status: 200, body: { slug: "shoes", title: "Shoes" } },
            },
            {
              title: { en: "Filters for a category", fa: "فیلترهای یک دسته" },
              method: "GET",
              path: "/api/ProductCategories/{slug}/filters",
              auth: "website",
              summary: {
                en: "Facet options (attributes) to build filter UI for that category.",
                fa: "گزینه‌های facet (ویژگی) برای ساخت UI فیلتر آن دسته.",
              },
              request: { body: "GET /api/ProductCategories/shoes/filters?lang=fa" },
              response: {
                status: 200,
                body: { attributes: [{ name: "Size", options: [{ id: 1, label: "42" }] }] },
              },
            },
          ],
        },
        {
          id: "brands",
          title: { en: "Brands", fa: "برندها" },
          summary: {
            en: "Public brand list for filters and brand pages.",
            fa: "فهرست عمومی برند برای فیلتر و صفحات برند.",
          },
          relatedAdmin: ["brands"],
          related: ["products"],
          endpoints: [
            {
              title: { en: "List brands", fa: "فهرست برندها" },
              method: "GET",
              path: "/api/Brands",
              auth: "website",
              query: [{ name: "lang", desc: { en: "Language", fa: "زبان" } }],
              request: { body: "GET /api/Brands?lang=fa" },
              response: {
                status: 200,
                body: [{ slug: "nike", title: "Nike" }],
              },
            },
          ],
        },
        {
          id: "vendors",
          title: { en: "Vendors", fa: "فروشندگان" },
          summary: {
            en: "Marketplace sellers visible on the storefront.",
            fa: "فروشندگان مارکت‌پلیس قابل‌نمایش در استورفرانت.",
          },
          relatedAdmin: ["vendors", "vendor-products"],
          related: ["products", "cart"],
          endpoints: [
            {
              title: { en: "List vendors", fa: "فهرست فروشندگان" },
              method: "GET",
              path: "/api/Vendors",
              auth: "website",
              query: [{ name: "lang", desc: { en: "Language", fa: "زبان" } }],
              request: { body: "GET /api/Vendors?lang=fa" },
              response: {
                status: 200,
                body: [{ vendorId: 3, name: "Official store" }],
              },
            },
          ],
        },
        {
          id: "inventory",
          title: { en: "Inventory availability", fa: "موجودی قابل‌فروش" },
          summary: {
            en: "Public stock check for PDP/cart badges. Does **not** expose raw On Hand / Reserved admin numbers.",
            fa: "چک عمومی موجودی برای نشان PDP/سبد. عدد خام On Hand / Reserved ادمین را **نشان نمی‌دهد**.",
          },
          purpose: [
            {
              en: "Admin **Stock** page shows On Hand, Reserved, Available for operators. This API only returns whether the customer can buy and the max quantity.",
              fa: "صفحه **Stock** ادمین On Hand، Reserved، Available را به اپراتور نشان می‌دهد. این API فقط می‌گوید مشتری می‌تواند بخرد یا نه و سقف تعداد چقدر است.",
            },
            {
              en: "**IsInStock** = Available > 0. **MaxPurchasable** = Available (never negative).",
              fa: "**IsInStock** = Available > 0. **MaxPurchasable** = Available (هرگز منفی نیست).",
            },
          ],
          relatedAdmin: ["inventory-stock", "inventory-movements", "products"],
          related: ["products", "cart"],
          endpoints: [
            {
              title: { en: "Bulk availability", fa: "موجودی چند واریانت" },
              method: "GET",
              path: "/api/Inventory/availability",
              auth: "website",
              query: [
                {
                  name: "variantIds",
                  desc: {
                    en: "Comma-separated variant ids, e.g. `1,2,3`",
                    fa: "شناسه واریانت‌ها با کاما، مثلاً `1,2,3`",
                  },
                },
              ],
              request: { body: "GET /api/Inventory/availability?variantIds=55,56" },
              response: {
                status: 200,
                body: {
                  "55": { isInStock: true, maxPurchasable: 17 },
                  "56": { isInStock: false, maxPurchasable: 0 },
                },
              },
              notes: [
                {
                  en: "Empty or invalid id list → `{}`.",
                  fa: "لیست خالی یا نامعتبر → `{}`.",
                },
              ],
            },
          ],
          tips: [
            {
              en: "If API says out of stock but Admin shows On Hand > 0, check **Reserved** and whether vendor listings exist.",
              fa: "اگر API ناموجود می‌گوید ولی ادمین On Hand > 0 دارد، **Reserved** و وجود لیستینگ فروشنده را چک کنید.",
            },
          ],
        },
      ],
    },

    /* ───────── Commerce ───────── */
    {
      id: "commerce",
      title: { en: "Cart & checkout", fa: "سبد و تسویه" },
      summary: {
        en: "Guest/member cart, coupons, shipping methods, placing an order.",
        fa: "سبد مهمان/عضو، کوپن، روش ارسال، ثبت سفارش.",
      },
      pages: [
        {
          id: "cart",
          title: { en: "Cart", fa: "سبد خرید" },
          summary: {
            en: "Add/update/remove lines, apply coupon, merge guest cart after login. No [Authorize] — guests allowed.",
            fa: "افزودن/ویرایش/حذف خط، اعمال کوپن، ادغام سبد مهمان بعد از لاگین. بدون [Authorize] — مهمان مجاز است.",
          },
          purpose: [
            {
              en: "Identify guest with `X-Cart-Session`. If `Authorization` is present, cart binds to that client.",
              fa: "مهمان با `X-Cart-Session` شناخته می‌شود. اگر `Authorization` باشد، سبد به همان مشتری وصل است.",
            },
          ],
          relatedAdmin: ["coupons", "products", "vendors"],
          related: ["auth", "checkout", "coupons", "inventory"],
          endpoints: [
            {
              title: { en: "Get cart", fa: "گرفتن سبد" },
              method: "GET",
              path: "/api/Cart",
              auth: "optional-jwt",
              headers: [
                { name: "X-Website-Key", desc: { en: "Required", fa: "لازم" } },
                { name: "X-Cart-Session", desc: { en: "Guest session id", fa: "شناسه نشست مهمان" } },
                { name: "Authorization", desc: { en: "Optional Bearer", fa: "Bearer اختیاری" } },
              ],
              query: [{ name: "currency", desc: { en: "Price currency", fa: "ارز قیمت" } }],
              request: { body: "GET /api/Cart?currency=IRR" },
              response: {
                status: 200,
                body: {
                  cartID: 1001,
                  sessionKey: "a1b2c3d4-…",
                  items: [{ cartItemId: 9, variantId: 55, quantity: 2, lineTotal: 5000000 }],
                  subTotal: 5000000,
                  couponCode: null,
                  discountAmount: 0,
                  totalWeightKg: 1.2,
                },
              },
            },
            {
              title: { en: "Add item", fa: "افزودن قلم" },
              method: "POST",
              path: "/api/Cart/items",
              auth: "optional-jwt",
              request: {
                body: { variantId: 55, quantity: 1, vendorProductId: null, vendorId: null },
              },
              response: {
                status: 200,
                body: { cartID: 1001, sessionKey: "a1b2c3d4-…", items: [], subTotal: 2500000 },
              },
            },
            {
              title: { en: "Update quantity", fa: "تغییر تعداد" },
              method: "PUT",
              path: "/api/Cart/items/{cartItemId}",
              auth: "optional-jwt",
              request: { body: { quantity: 3 } },
              response: { status: 200, body: "(empty OK)" },
            },
            {
              title: { en: "Remove item", fa: "حذف قلم" },
              method: "DELETE",
              path: "/api/Cart/items/{cartItemId}",
              auth: "optional-jwt",
              request: { body: "DELETE /api/Cart/items/9" },
              response: { status: 200, body: "(empty OK)" },
            },
            {
              title: { en: "Apply / remove coupon", fa: "اعمال / حذف کوپن" },
              method: "POST",
              path: "/api/Cart/coupon  ·  DELETE /api/Cart/coupon",
              auth: "optional-jwt",
              request: { body: { code: "WELCOME10" } },
              response: {
                status: 200,
                body: "(OK) or 400 { message: \"…\" }",
              },
            },
            {
              title: { en: "Merge guest cart after login", fa: "ادغام سبد مهمان بعد از ورود" },
              method: "POST",
              path: "/api/Cart/merge",
              auth: "jwt",
              summary: {
                en: "Requires JWT + previous X-Cart-Session. Folds guest lines into the account cart.",
                fa: "JWT + همان X-Cart-Session قبلی. خطوط مهمان را داخل سبد حساب می‌ریزد.",
              },
              request: { body: "POST /api/Cart/merge (no body)" },
              response: { status: 200, body: "(empty OK)" },
            },
          ],
        },
        {
          id: "coupons",
          title: { en: "Coupons (validate)", fa: "کوپن (اعتبارسنجی)" },
          summary: {
            en: "Validate a code without attaching it to a cart (cart attach is POST /api/Cart/coupon).",
            fa: "اعتبارسنجی کد بدون اتصال به سبد (اتصال با POST /api/Cart/coupon).",
          },
          relatedAdmin: ["coupons"],
          related: ["cart"],
          endpoints: [
            {
              title: { en: "Validate coupon", fa: "اعتبارسنجی کوپن" },
              method: "POST",
              path: "/api/Coupons/validate",
              auth: "website",
              request: { body: { code: "WELCOME10" } },
              response: {
                status: 200,
                body: { valid: true, discountType: "Percent", value: 10 },
              },
            },
          ],
        },
        {
          id: "shipping",
          title: { en: "Shipping methods", fa: "روش‌های ارسال" },
          summary: {
            en: "Methods/rates the storefront shows at checkout.",
            fa: "روش/نرخ‌هایی که استورفرانت در تسویه نشان می‌دهد.",
          },
          relatedAdmin: ["shipping"],
          related: ["checkout", "addresses"],
          endpoints: [
            {
              title: { en: "List methods", fa: "فهرست روش‌ها" },
              method: "GET",
              path: "/api/Shipping/methods",
              auth: "website",
              request: { body: "GET /api/Shipping/methods" },
              response: {
                status: 200,
                body: [{ shippingMethodId: 1, title: "Courier", price: 150000 }],
              },
            },
          ],
        },
        {
          id: "checkout",
          title: { en: "Checkout", fa: "ثبت سفارش (Checkout)" },
          summary: {
            en: "Turns the signed-in customer's cart into an order. Requires ClientPurchase policy.",
            fa: "سبد مشتری واردشده را به سفارش تبدیل می‌کند. سیاست ClientPurchase لازم است.",
          },
          purpose: [
            {
              en: "Prerequisites: JWT, cart with items, saved address, chosen shipping method. Stock reservation happens in the order flow.",
              fa: "پیش‌نیاز: JWT، سبد با قلم، آدرس ذخیره‌شده، روش ارسال. رزرو موجودی در جریان سفارش انجام می‌شود.",
            },
          ],
          relatedAdmin: ["orders", "inventory-stock", "shipping"],
          related: ["cart", "addresses", "shipping", "payments", "orders"],
          endpoints: [
            {
              title: { en: "Create order from cart", fa: "ساخت سفارش از سبد" },
              method: "POST",
              path: "/api/Checkout",
              auth: "jwt",
              headers: [
                { name: "X-Website-Key", desc: { en: "Required", fa: "لازم" } },
                { name: "Authorization", desc: { en: "Bearer customer JWT", fa: "Bearer JWT مشتری" } },
              ],
              request: {
                body: {
                  cartId: 1001,
                  addressId: 7,
                  shippingMethodId: 1,
                  currency: "IRR",
                  note: "Call before 5pm",
                },
              },
              response: {
                status: 200,
                body: { orderId: 500, orderNumber: "ORD-2026-000500" },
              },
              notes: [
                {
                  en: "Failure → 400 `{ message: \"…\" }` (empty cart, stock, validation, …).",
                  fa: "شکست → 400 `{ message: \"…\" }` (سبد خالی، موجودی، اعتبارسنجی، …).",
                },
              ],
            },
          ],
        },
      ],
    },

    /* ───────── Account ───────── */
    {
      id: "account",
      title: { en: "Customer account", fa: "حساب مشتری" },
      summary: {
        en: "Addresses, bank accounts, orders, digital library, wishlist, wallet.",
        fa: "آدرس، حساب بانکی، سفارش، کتابخانه دیجیتال، علاقه‌مندی، کیف پول.",
      },
      pages: [
        {
          id: "addresses",
          title: { en: "Addresses", fa: "آدرس‌ها" },
          summary: {
            en: "CRUD shipping addresses for the signed-in customer (ClientProfile).",
            fa: "CRUD آدرس ارسال مشتری واردشده (ClientProfile).",
          },
          relatedAdmin: ["clients", "initial-cities"],
          related: ["checkout", "locations"],
          endpoints: [
            {
              title: { en: "List / get / create / update / delete / set default", fa: "لیست / گرفتن / ساخت / ویرایش / حذف / پیش‌فرض" },
              method: "GET",
              path: "/api/Addresses  ·  /api/Addresses/{id}  ·  POST/PUT/DELETE  ·  POST …/default",
              auth: "jwt",
              request: {
                body: {
                  title: "Home",
                  recipientName: "Ali Rezaei",
                  phone: "09120000000",
                  countryId: 1,
                  cityId: 10,
                  addressLine: "Valiasr St.",
                  postalCode: "1234567890",
                },
              },
              response: {
                status: 200,
                body: [{ addressId: 7, title: "Home", isDefault: true }],
              },
            },
          ],
        },
        {
          id: "orders",
          title: { en: "Orders", fa: "سفارش‌های من" },
          summary: {
            en: "Customer’s own order history and detail (ClientPurchase). Detail includes preparation status, shipping status, and tracking code set by admin after purchase.",
            fa: "تاریخچه و جزئیات سفارش خود مشتری (ClientPurchase). جزئیات شامل وضعیت آماده‌سازی، وضعیت ارسال و کد پیگیری است که ادمین بعد از خرید ست می‌کند.",
          },
          relatedAdmin: ["orders", "customer-returns"],
          related: ["checkout", "payments", "digital", "returns", "support-tickets"],
          endpoints: [
            {
              title: { en: "List orders", fa: "فهرست سفارش‌ها" },
              method: "GET",
              path: "/api/Orders",
              auth: "jwt",
              query: [
                { name: "page", desc: { en: "Default 1", fa: "پیش‌فرض ۱" } },
                { name: "pageSize", desc: { en: "Default 10", fa: "پیش‌فرض ۱۰" } },
              ],
              request: { body: "GET /api/Orders?page=1&pageSize=10" },
              response: {
                status: 200,
                body: { items: [{ orderId: 500, orderNumber: "ORD-2026-000500", status: "PendingPayment" }], totalCount: 1 },
              },
            },
            {
              title: { en: "Order detail", fa: "جزئیات سفارش" },
              method: "GET",
              path: "/api/Orders/{id}",
              auth: "jwt",
              request: { body: "GET /api/Orders/500" },
              response: {
                status: 200,
                body: {
                  orderId: 500,
                  status: 3,
                  preparationStatus: 2,
                  shippingStatus: 2,
                  shippingTrackingCode: "1234567890123456789012345",
                  items: [],
                  grandTotal: 2650000,
                  currencyCode: "IRR",
                },
              },
            },
          ],
        },
        {
          id: "returns",
          title: { en: "Returns", fa: "برگشت کالا" },
          summary: {
            en: "Customer RMA (ClientPurchase). Pre-request with selected lines, reason, photos, and **shipping payer** (customer / 50-50 / seller / drop-off at center). After admin approval (refund amounts + shipping cost), submit tracking unless drop-off. Partial quantities allowed. Out-of-window returns need `acceptExpiredWindow`.",
            fa: "برگشت مشتری (ClientPurchase). پیش‌درخواست با خطوط، علت، عکس و **پرداخت‌کننده حمل** (مشتری / ۵۰-۵۰ / فروشنده / تحویل به مرکز). بعد از تأیید ادمین کد رهگیری می‌آید مگر تحویل حضوری. تعداد جزئی مجاز است. بعد از مهلت باید `acceptExpiredWindow` باشد.",
          },
          relatedAdmin: ["customer-returns", "orders"],
          related: ["orders", "payments", "support-tickets"],
          endpoints: [
            {
              title: { en: "List my returns", fa: "فهرست برگشت‌های من" },
              method: "GET",
              path: "/api/Returns",
              auth: "jwt",
              query: [
                { name: "page", desc: { en: "Default 1", fa: "پیش‌فرض ۱" } },
                { name: "pageSize", desc: { en: "Default 10", fa: "پیش‌فرض ۱۰" } },
              ],
              request: { body: "GET /api/Returns?page=1" },
              response: { status: 200, body: { items: [{ customerReturnRequestID: 1, status: 0, orderNumber: "ORD-80" }], totalCount: 1 } },
            },
            {
              title: { en: "Eligibility + remaining qty", fa: "مجاز بودن + تعداد باقی" },
              method: "GET",
              path: "/api/Returns/eligible/{orderId}",
              auth: "jwt",
              request: { body: "GET /api/Returns/eligible/80" },
              response: { status: 200, body: { eligible: true, returnWindowDays: 7, lines: [{ orderItemID: 801, remainingQty: 5, unitPricePaid: 30 }] } },
            },
            {
              title: { en: "Create pre-request", fa: "ثبت پیش‌درخواست" },
              method: "POST",
              path: "/api/Returns",
              auth: "jwt",
              request: {
                body: {
                  orderId: 80,
                  reason: 1,
                  description: "Box crushed",
                  shippingPayer: 1,
                  acceptExpiredWindow: false,
                  lines: [{ orderItemID: 801, quantity: 2, unitRefundRequested: 25 }],
                  photoFileIds: [],
                },
              },
              response: { status: 200, body: { customerReturnRequestID: 12, status: 0 } },
            },
            {
              title: { en: "Upload photo", fa: "آپلود عکس" },
              method: "POST",
              path: "/api/Returns/{id}/photos",
              auth: "jwt",
              request: { body: "multipart file" },
              response: { status: 200, body: { fileId: 900 } },
            },
            {
              title: { en: "Submit shipment", fa: "ثبت ارسال" },
              method: "POST",
              path: "/api/Returns/{id}/ship",
              auth: "jwt",
              request: { body: { trackingCode: "TRK-1", shipMethod: "Tipax" } },
              response: { status: 200, body: {} },
            },
            {
              title: { en: "Update tracking", fa: "به‌روزرسانی رهگیری" },
              method: "PUT",
              path: "/api/Returns/{id}/tracking",
              auth: "jwt",
              request: { body: { trackingCode: "TRK-2" } },
              response: { status: 200, body: {} },
            },
          ],
        },
        {
          id: "support-tickets",
          title: { en: "Support tickets", fa: "تیکت پشتیبانی" },
          summary: {
            en: "Signed-in customer opens a ticket about an order (ClientPurchase). Tickets appear in Admin **Support → Ticket queue** (channel Website). Internal agent notes are never returned.",
            fa: "مشتری واردشده درباره سفارش تیکت باز می‌کند (ClientPurchase). تیکت در ادمین **پشتیبانی → صف تیکت** با کانال Website دیده می‌شود. یادداشت داخلی پشتیبان برنمی‌گردد.",
          },
          relatedAdmin: ["support-desk", "support-tickets", "orders"],
          related: ["orders", "auth"],
          endpoints: [
            {
              title: { en: "List my tickets", fa: "فهرست تیکت‌های من" },
              method: "GET",
              path: "/api/Support",
              auth: "jwt",
              query: [
                { name: "page", desc: { en: "Default 1", fa: "پیش‌فرض ۱" } },
                { name: "pageSize", desc: { en: "Default 10", fa: "پیش‌فرض ۱۰" } },
              ],
              request: { body: "GET /api/Support?page=1" },
              response: { status: 200, body: { items: [{ supportSessionID: 1, sessionNumber: "SUP-20260823-00001", channel: 8, subject: "Where is my package?" }], totalCount: 1 } },
            },
            {
              title: { en: "Ticket + public timeline", fa: "تیکت + تایم‌لاین عمومی" },
              method: "GET",
              path: "/api/Support/{id}",
              auth: "jwt",
              request: { body: "GET /api/Support/1" },
              response: { status: 200, body: { ticket: { sessionNumber: "SUP-20260823-00001" }, interactions: [{ interactionType: 11, body: "Order still not here." }] } },
            },
            {
              title: { en: "Create ticket", fa: "ثبت تیکت" },
              method: "POST",
              path: "/api/Support",
              auth: "jwt",
              request: { body: { subject: "Where is my package?", body: "Shipped 5 days ago.", relatedOrderId: 500, category: 4 } },
              response: { status: 200, body: { supportSessionID: 1, sessionNumber: "SUP-20260823-00001" } },
            },
            {
              title: { en: "Reply", fa: "پاسخ" },
              method: "POST",
              path: "/api/Support/{id}/replies",
              auth: "jwt",
              request: { body: { body: "Still waiting." } },
              response: { status: 200, body: {} },
            },
          ],
        },
        {
          id: "payments",
          title: { en: "Payments", fa: "پرداخت" },
          summary: {
            en: "Online gateway redirect, wallet pay, offline bank accounts, upload receipt, payment status.",
            fa: "پرداخت آنلاین با درگاه، پرداخت با کیف پول، حساب‌های بانکی آفلاین، آپلود فیش، وضعیت پرداخت.",
          },
          relatedAdmin: ["payments", "bank-accounts", "wallets"],
          related: ["orders", "wallet", "checkout"],
          sections: [
            {
              title: { en: "How an online payment completes", fa: "روند تکمیل پرداخت آنلاین" },
              items: [
                {
                  en: "**1.** `GET /api/Payments/gateways` — the gateways this website has configured and can actually take money through. A gateway missing its credentials is never listed.",
                  fa: "**۱.** `GET /api/Payments/gateways` — درگاه‌هایی که این سایت تنظیم کرده و واقعاً می‌تواند با آن‌ها پول بگیرد. درگاه بدون اعتبارنامه هرگز فهرست نمی‌شود.",
                },
                {
                  en: "**2.** `POST /api/Payments/online/start` — creates a pending `Payment` and returns `redirectUrl`. Send the payer there. `returnUrl` must be an absolute URL on **this website's own host**, or the call is refused.",
                  fa: "**۲.** `POST /api/Payments/online/start` — یک `Payment` در وضعیت pending می‌سازد و `redirectUrl` برمی‌گرداند. پرداخت‌کننده را به آنجا بفرستید. `returnUrl` باید URL مطلق روی **هاست خود همین سایت** باشد وگرنه رد می‌شود.",
                },
                {
                  en: "**3.** The gateway sends the payer back to your `returnUrl`. Forward what it sent (query string and/or form) to `/api/Payments/online/callback`. The API then asks the gateway **server to server** whether the money actually moved, and only marks the order paid on that answer.",
                  fa: "**۳.** درگاه پرداخت‌کننده را به `returnUrl` شما برمی‌گرداند. آنچه فرستاده (query یا فرم) را به `/api/Payments/online/callback` بدهید. API سپس **سرور به سرور** از درگاه می‌پرسد پول واقعاً جابه‌جا شده یا نه و فقط بر اساس همان پاسخ سفارش را پرداخت‌شده می‌کند.",
                },
              ],
              callout: {
                tone: "warn",
                text: {
                  en: "Never mark an order paid from the callback's own query string. A return URL is something the payer's browser is redirected to, so its values can be forged; only the server-to-server verify in step 3 is evidence. Calling the callback twice is safe — the second call returns the first outcome instead of crediting the order again.",
                  fa: "هرگز سفارش را بر اساس query string خود callback پرداخت‌شده نکنید. return URL چیزی است که مرورگر پرداخت‌کننده به آن هدایت می‌شود، پس مقادیرش جعل‌پذیر است؛ فقط تأیید سرور-به-سرور مرحله ۳ سند است. صدا زدن دوباره‌ی callback امن است — بار دوم همان نتیجه‌ی اول را برمی‌گرداند و سفارش را دوباره شارژ نمی‌کند.",
                },
              },
            },
            {
              title: { en: "Supported gateways", fa: "درگاه‌های پشتیبانی‌شده" },
              items: [
                {
                  en: "**Iranian:** ZarinPal, Zibal, IDPay, NextPay, Pay.ir, PayPing.",
                  fa: "**ایرانی:** زرین‌پال، زیبال، آیدی‌پی، نکست‌پی، pay.ir، پی‌پینگ.",
                },
                {
                  en: "**International:** Stripe (hosted Checkout), PayPal (Orders v2).",
                  fa: "**بین‌المللی:** Stripe (Checkout میزبان‌شده)، PayPal (Orders v2).",
                },
                {
                  en: "**Custom HTTP gateway** — any PSP whose start/verify calls are plain HTTP can be wired up from Admin alone by filling in URLs, bodies and headers, with no new build.",
                  fa: "**درگاه HTTP سفارشی** — هر PSP که فراخوانی شروع/تأیید آن HTTP ساده باشد، فقط با پر کردن آدرس‌ها، بدنه و هدرها در ادمین اضافه می‌شود، بدون نیاز به بیلد جدید.",
                },
                {
                  en: "Amounts are converted per gateway: most Iranian aggregators bill in **Rial** while the shop prices in Toman, PayPing bills in Toman, Stripe in minor units. The storefront always sends the order total in the site currency and the gateway's own unit is applied server-side.",
                  fa: "مبلغ برای هر درگاه تبدیل می‌شود: بیشتر واسط‌های ایرانی به **ریال** صورتحساب می‌دهند در حالی که فروشگاه به تومان قیمت می‌گذارد، پی‌پینگ به تومان، و Stripe به واحد خرد. استورفرانت همیشه جمع سفارش را به ارز سایت می‌فرستد و واحد درگاه سمت سرور اعمال می‌شود.",
                },
              ],
            },
          ],
          endpoints: [
            {
              title: { en: "Available gateways", fa: "درگاه‌های در دسترس" },
              method: "GET",
              path: "/api/Payments/gateways",
              auth: "jwt",
              summary: {
                en: "Gateways this website can take payment through, for the checkout picker.",
                fa: "درگاه‌هایی که این سایت می‌تواند با آن‌ها پول بگیرد، برای انتخابگر تسویه.",
              },
              request: { body: "GET /api/Payments/gateways" },
              response: {
                status: 200,
                body: [
                  { paymentGatewayID: 3, name: "ZarinPal", provider: "Zarinpal", providerDisplayName: "ZarinPal", isSandbox: false },
                ],
              },
            },
            {
              title: { en: "Start an online payment", fa: "شروع پرداخت آنلاین" },
              method: "POST",
              path: "/api/Payments/online/start",
              auth: "jwt",
              summary: {
                en: "Creates a pending payment and returns the gateway URL to send the payer to.",
                fa: "یک پرداخت در انتظار می‌سازد و آدرس درگاه را برای هدایت پرداخت‌کننده برمی‌گرداند.",
              },
              request: {
                body: {
                  orderId: 1042,
                  gatewayId: 3,
                  returnUrl: "https://shop.example.com/checkout/callback",
                },
              },
              response: {
                status: 200,
                body: { redirectUrl: "https://payment.zarinpal.com/pg/StartPay/A0000000000000000000000000000123" },
                note: {
                  en: "The order is untouched here — it stays PendingPayment until the callback verifies. **400** when the order is not yours, not awaiting payment, or `returnUrl` is not on this website's host.",
                  fa: "سفارش اینجا دست‌نخورده می‌ماند و تا تأیید callback در PendingPayment است. **400** وقتی سفارش مال شما نیست، در انتظار پرداخت نیست، یا `returnUrl` روی هاست این سایت نیست.",
                },
              },
            },
            {
              title: { en: "Gateway callback", fa: "بازگشت از درگاه" },
              method: "POST",
              path: "/api/Payments/online/callback",
              auth: "website",
              summary: {
                en: "Verifies the payment server to server and marks the order paid. Also accepts GET. Anonymous, because the payer arrives via the PSP and may not carry their token.",
                fa: "پرداخت را سرور-به-سرور تأیید و سفارش را پرداخت‌شده می‌کند. GET هم می‌پذیرد. ناشناس است چون پرداخت‌کننده از مسیر PSP می‌آید و ممکن است توکن نداشته باشد.",
              },
              headers: [
                { name: "X-Website-Key", desc: { en: "Website AuthCode GUID", fa: "GUID کلید وب‌سایت" } },
              ],
              request: {
                body: { Authority: "A0000000000000000000000000000123", Status: "OK" },
                note: {
                  en: "Pass through whatever the gateway sent; field names differ per PSP and the matching provider picks the ones it needs.",
                  fa: "هرچه درگاه فرستاده را عیناً بدهید؛ نام فیلدها در هر PSP فرق دارد و provider مربوطه خودش موردنیازش را برمی‌دارد.",
                },
              },
              response: {
                status: 200,
                body: { paid: true, referenceNumber: "123456789" },
                note: {
                  en: "**400** with `paid: false` when the gateway did not confirm, when it was unreachable, or when the amount it confirmed does not match the order — in that last case the order is deliberately left unpaid.",
                  fa: "**400** با `paid: false` وقتی درگاه تأیید نکرده، در دسترس نبوده، یا مبلغ تأییدشده با سفارش نمی‌خواند — در حالت آخر سفارش عمداً پرداخت‌نشده می‌ماند.",
                },
              },
            },
            {
              title: { en: "Offline bank accounts", fa: "حساب‌های کارت‌به‌کارت" },
              method: "GET",
              path: "/api/Payments/bank-accounts",
              auth: "jwt",
              request: { body: "GET /api/Payments/bank-accounts" },
              response: {
                status: 200,
                body: [{ bankAccountID: 1, bankName: "Melli", title: "Sales", ownerName: "Shop", iban: "IR…", cardNumber: "6037…" }],
              },
            },
            {
              title: { en: "Upload receipt image", fa: "آپلود تصویر فیش" },
              method: "POST",
              path: "/api/Payments/receipt-upload",
              auth: "jwt",
              summary: {
                en: "multipart file → returns fileId for SubmitReceipt. Max ~10MB. Needs storage configured in Admin.",
                fa: "فایل multipart → fileId برای SubmitReceipt. حدود ۱۰MB. نیاز به Storage در ادمین.",
              },
              request: { body: "Content-Type: multipart/form-data\nfile: receipt.jpg" },
              response: { status: 200, body: { fileId: 9001 } },
            },
            {
              title: { en: "Submit bank receipt", fa: "ثبت فیش بانکی" },
              method: "POST",
              path: "/api/Payments/receipt",
              auth: "jwt",
              request: {
                body: { orderId: 500, bankAccountId: 1, receiptFileId: 9001 },
              },
              response: { status: 200, body: { paymentId: 70 } },
            },
            {
              title: { en: "Pay with wallet", fa: "پرداخت با کیف پول" },
              method: "POST",
              path: "/api/Payments/wallet",
              auth: "jwt",
              request: { body: { orderId: 500 } },
              response: { status: 200, body: { paymentId: 71 } },
            },
            {
              title: { en: "Payment status", fa: "وضعیت پرداخت" },
              method: "GET",
              path: "/api/Payments/{orderId}/status",
              auth: "jwt",
              request: { body: "GET /api/Payments/500/status" },
              response: {
                status: 200,
                body: { paymentID: 70, status: "PendingReview", method: "BankReceipt", paidAt: null },
              },
            },
          ],
        },
        {
          id: "wallet",
          title: { en: "Wallet", fa: "کیف پول" },
          summary: {
            en: "Balance, ledger, withdrawal requests for the signed-in customer.",
            fa: "مانده، دفتر، درخواست برداشت برای مشتری واردشده.",
          },
          relatedAdmin: ["withdrawals", "wallet-detail", "clients"],
          related: ["payments", "client-bank-accounts"],
          endpoints: [
            {
              title: { en: "Balance", fa: "مانده" },
              method: "GET",
              path: "/api/Wallet",
              auth: "jwt",
              request: { body: "GET /api/Wallet" },
              response: { status: 200, body: { balanceUsd: 12.5, isActive: true } },
            },
            {
              title: { en: "Transactions", fa: "تراکنش‌ها" },
              method: "GET",
              path: "/api/Wallet/transactions",
              auth: "jwt",
              query: [
                { name: "pageIndex", desc: { en: "Default 1", fa: "پیش‌فرض ۱" } },
                { name: "pageSize", desc: { en: "Default 20", fa: "پیش‌فرض ۲۰" } },
              ],
              request: { body: "GET /api/Wallet/transactions?pageIndex=1&pageSize=20" },
              response: {
                status: 200,
                body: { items: [{ amount: -5, type: "Payment", createdAt: "…" }], totalCount: 1 },
              },
            },
            {
              title: { en: "Request withdrawal", fa: "درخواست برداشت" },
              method: "POST",
              path: "/api/Wallet/withdrawals",
              auth: "jwt",
              request: { body: { clientBankAccountId: 3, amountUsd: 10 } },
              response: {
                status: 200,
                body: { withdrawalId: 15, status: "Pending", amountUsd: 10 },
              },
            },
            {
              title: { en: "List withdrawals", fa: "فهرست برداشت‌ها" },
              method: "GET",
              path: "/api/Wallet/withdrawals",
              auth: "jwt",
              request: { body: "GET /api/Wallet/withdrawals" },
              response: {
                status: 200,
                body: { items: [], totalCount: 0 },
              },
            },
          ],
        },
        {
          id: "client-bank-accounts",
          title: { en: "Client bank accounts", fa: "حساب بانکی مشتری" },
          summary: {
            en: "Customer’s own bank accounts for withdrawals.",
            fa: "حساب‌های بانکی خود مشتری برای برداشت.",
          },
          relatedAdmin: ["withdrawals"],
          related: ["wallet"],
          endpoints: [
            {
              title: { en: "CRUD + set default", fa: "CRUD + پیش‌فرض" },
              method: "GET",
              path: "/api/ClientBankAccounts",
              auth: "jwt",
              request: {
                body: { bankId: 1, accountHolder: "Ali", iban: "IR…", cardNumber: "6037…" },
              },
              response: {
                status: 200,
                body: [{ clientBankAccountId: 3, iban: "IR…", isDefault: true }],
              },
            },
          ],
        },
        {
          id: "wishlist",
          title: { en: "Wishlist", fa: "علاقه‌مندی" },
          summary: {
            en: "Saved variants for the signed-in customer.",
            fa: "واریانت‌های ذخیره‌شده مشتری واردشده.",
          },
          related: ["products"],
          endpoints: [
            {
              title: { en: "List / add / remove", fa: "لیست / افزودن / حذف" },
              method: "GET",
              path: "/api/Wishlist  ·  POST /api/Wishlist/items  ·  DELETE /api/Wishlist/items/{variantId}",
              auth: "jwt",
              request: { body: { variantId: 55 } },
              response: {
                status: 200,
                body: [{ variantId: 55, title: "Running shoes 42" }],
              },
            },
          ],
        },
        {
          id: "digital",
          title: { en: "Digital library", fa: "کتابخانه دیجیتال" },
          summary: {
            en: "Downloads/codes/services the customer purchased.",
            fa: "دانلود/کد/سرویسی که مشتری خریده.",
          },
          relatedAdmin: ["products", "orders"],
          related: ["orders"],
          endpoints: [
            {
              title: { en: "Library / by order / detail / access log / download", fa: "کتابخانه / سفارش / جزئیات / لاگ دسترسی / دانلود" },
              method: "GET",
              path: "/api/Digital  ·  /api/Digital/order/{orderId}  ·  /api/Digital/{id}  ·  POST …/access  ·  GET …/download",
              auth: "jwt",
              request: { body: "GET /api/Digital" },
              response: {
                status: 200,
                body: [{ id: 1, title: "E-book PDF", productType: "DigitalDownload" }],
              },
            },
          ],
        },
      ],
    },

    /* ───────── Content CMS ───────── */
    {
      id: "content",
      title: { en: "Content & site shell", fa: "محتوا و پوسته سایت" },
      summary: {
        en: "Pages, posts, menus, slideshows, advertisements, theme, site info, forms, contact, redirects, files.",
        fa: "صفحه، پست، منو، اسلایدشو، تبلیغات، تم، اطلاعات سایت، فرم، تماس، ریدایرکت، فایل.",
      },
      pages: [
        {
          id: "site-info",
          title: { en: "Site info", fa: "اطلاعات سایت" },
          summary: {
            en: "Brand, logo, contact, social, SEO defaults for the website key.",
            fa: "برند، لوگو، تماس، اجتماعی، پیش‌فرض SEO برای کلید وب‌سایت.",
          },
          relatedAdmin: ["websites", "website-seo", "website-social", "website-contact-info"],
          endpoints: [
            {
              title: { en: "Get site info", fa: "گرفتن اطلاعات سایت" },
              method: "GET",
              path: "/api/SiteInfo",
              auth: "website",
              query: [{ name: "lang", desc: { en: "Selects translated contact info fields (title/group/value); missing translations fall back to the default language.", fa: "انتخاب فیلدهای ترجمه‌شده‌ی اطلاعات تماس (عنوان/گروه/مقدار)؛ در نبود ترجمه، به زبان پیش‌فرض برمی‌گردد." } }],
              request: { body: "GET /api/SiteInfo?lang=fa" },
              response: {
                status: 200,
                body: {
                  brandName: "My Shop",
                  logoUrl: "/…",
                  socialLinks: [{ name: "Instagram", icon: "bi bi-instagram", url: "…" }],
                  contactInfos: [{ type: 1, groupTitle: "Sales Office", title: "Phone", value: "+98 21 …", icon: "bi bi-telephone" }],
                },
              },
            },
          ],
        },
        {
          id: "pages",
          title: { en: "Pages", fa: "صفحات" },
          summary: {
            en: "CMS pages tree, homepage, page by slug.",
            fa: "درخت صفحات CMS، صفحه خانه، صفحه با اسلاگ.",
          },
          relatedAdmin: ["pages", "menus"],
          endpoints: [
            {
              title: { en: "Tree / home / by slug", fa: "درخت / خانه / با اسلاگ" },
              method: "GET",
              path: "/api/Pages  ·  /api/Pages/home  ·  /api/Pages/{slug}",
              auth: "website",
              query: [{ name: "lang", desc: { en: "Language", fa: "زبان" } }],
              request: { body: "GET /api/Pages/about-us?lang=fa" },
              response: {
                status: 200,
                body: { slug: "about-us", title: "About us", bodyHtml: "<p>…</p>" },
              },
            },
          ],
        },
        {
          id: "posts",
          title: { en: "Posts", fa: "پست‌ها" },
          summary: {
            en: "Published blog/articles, featured list, post by slug.",
            fa: "مقالات منتشرشده، ویژه، پست با اسلاگ.",
          },
          relatedAdmin: ["posts", "content-categories", "content-tags"],
          endpoints: [
            {
              title: { en: "List / featured / by slug", fa: "لیست / ویژه / با اسلاگ" },
              method: "GET",
              path: "/api/Posts  ·  /api/Posts/featured  ·  /api/Posts/{slug}",
              auth: "website",
              request: { body: "GET /api/Posts?page=1&lang=fa" },
              response: {
                status: 200,
                body: { items: [{ slug: "hello", title: "Hello" }], totalCount: 1 },
              },
            },
          ],
        },
        {
          id: "content-categories",
          title: { en: "Content categories", fa: "دسته‌های محتوا" },
          summary: {
            en: "Blog/content category tree (not product categories).",
            fa: "درخت دسته بلاگ/محتوا (نه دسته محصول).",
          },
          relatedAdmin: ["content-categories"],
          related: ["posts"],
          endpoints: [
            {
              title: { en: "Tree / by slug", fa: "درخت / با اسلاگ" },
              method: "GET",
              path: "/api/Categories  ·  /api/Categories/{slug}",
              auth: "website",
              request: { body: "GET /api/Categories?lang=fa" },
              response: {
                status: 200,
                body: [{ slug: "news", title: "News" }],
              },
            },
          ],
        },
        {
          id: "menus",
          title: { en: "Menus", fa: "منوها" },
          summary: {
            en: "Navigation trees by location key for the theme.",
            fa: "درخت ناوبری بر اساس کلید location برای تم.",
          },
          relatedAdmin: ["menus"],
          endpoints: [
            {
              title: { en: "All menus / by location", fa: "همه منوها / با location" },
              method: "GET",
              path: "/api/Menu  ·  /api/Menu/{location}",
              auth: "website",
              request: { body: "GET /api/Menu/header?lang=fa" },
              response: {
                status: 200,
                body: [{ title: "Home", url: "/", children: [] }],
              },
            },
          ],
        },
        {
          id: "slideshows",
          title: { en: "Slideshows", fa: "اسلایدشوها" },
          summary: {
            en: "Hero/banner slides by placement key or id.",
            fa: "اسلاید هیرو/بنر با کلید placement یا id.",
          },
          relatedAdmin: ["slideshows", "media-library"],
          endpoints: [
            {
              title: { en: "By placement / by id", fa: "با placement / با id" },
              method: "GET",
              path: "/api/Slideshow/placement/{placementKey}  ·  /api/Slideshow/{id}",
              auth: "website",
              request: { body: "GET /api/Slideshow/placement/home-hero" },
              response: {
                status: 200,
                body: { slides: [{ title: "Sale", imageUrl: "/…", linkUrl: "/products" }] },
              },
            },
          ],
        },
        {
          id: "advertisements",
          title: { en: "Advertisements", fa: "تبلیغات" },
          summary: {
            en: "Active keyword ads for a location, localized by lang.",
            fa: "تبلیغ‌های کلمه کلیدی فعال برای یک محل نمایش، با زبان.",
          },
          relatedAdmin: ["advertisements"],
          endpoints: [
            {
              title: { en: "By location", fa: "با محل نمایش" },
              method: "GET",
              path: "/api/Advertisement/{location}",
              auth: "website",
              request: { body: "GET /api/Advertisement/Header?lang=fa" },
              response: {
                status: 200,
                body: [{ keyword: "Buy steel", url: "https://example.com", openInNewTab: true }],
              },
            },
          ],
        },
        {
          id: "theme",
          title: { en: "Theme", fa: "تم" },
          summary: {
            en: "Active theme for the website.",
            fa: "تم فعال وب‌سایت.",
          },
          relatedAdmin: ["themes"],
          endpoints: [
            {
              title: { en: "Active theme", fa: "تم فعال" },
              method: "GET",
              path: "/api/Theme/active",
              auth: "website",
              request: { body: "GET /api/Theme/active" },
              response: { status: 200, body: { key: "default", name: "Default" } },
            },
          ],
        },
        {
          id: "forms",
          title: { en: "Forms", fa: "فرم‌ها" },
          summary: {
            en: "Load form definition and submit answers; optional public results.",
            fa: "بارگذاری تعریف فرم و ارسال پاسخ؛ نتایج عمومی اختیاری.",
          },
          relatedAdmin: ["forms"],
          endpoints: [
            {
              title: { en: "Get / submit / results", fa: "گرفتن / ارسال / نتایج" },
              method: "GET",
              path: "/api/Forms/{slug}  ·  POST /api/Forms/{id}/submit  ·  GET /api/Forms/{id}/results",
              auth: "website",
              request: {
                body: { values: { email: "a@b.com", message: "Hello" } },
              },
              response: { status: 200, body: { success: true } },
            },
          ],
        },
        {
          id: "contact",
          title: { en: "Contact messages", fa: "پیام تماس" },
          summary: {
            en: "Simple contact form post → Admin Contact Messages.",
            fa: "ارسال فرم تماس ساده → پیام‌های تماس ادمین.",
          },
          relatedAdmin: ["contact-messages"],
          endpoints: [
            {
              title: { en: "Submit contact", fa: "ارسال تماس" },
              method: "POST",
              path: "/api/Contact",
              auth: "website",
              request: {
                body: { name: "Ali", email: "a@b.com", subject: "Hi", body: "…" },
              },
              response: { status: 200, body: { message: "Your message has been sent." } },
            },
          ],
        },
        {
          id: "subscribe",
          title: { en: "Newsletter subscribe", fa: "عضویت خبرنامه" },
          summary: {
            en: "Public email signup → EmailSubscribes for the resolved website. Honeypot field `website` must stay empty.",
            fa: "ثبت ایمیل عمومی → EmailSubscribes برای وب‌سایت resolveشده. فیلد honeypot با نام website باید خالی بماند.",
          },
          relatedAdmin: [],
          endpoints: [
            {
              title: { en: "Subscribe", fa: "عضویت" },
              method: "POST",
              path: "/api/Subscribe",
              auth: "website",
              request: {
                body: { email: "a@b.com", website: "" },
              },
              response: { status: 200, body: { message: "Thanks for subscribing." } },
            },
          ],
        },
        {
          id: "redirects",
          title: { en: "Redirects", fa: "ریدایرکت" },
          summary: {
            en: "Resolve an old path to a new target for the storefront router.",
            fa: "resolve مسیر قدیمی به مقصد جدید برای روتر استورفرانت.",
          },
          relatedAdmin: ["redirects"],
          endpoints: [
            {
              title: { en: "Resolve path", fa: "resolve مسیر" },
              method: "GET",
              path: "/api/Redirects/resolve",
              auth: "website",
              query: [{ name: "path", desc: { en: "Incoming path", fa: "مسیر ورودی" } }],
              request: { body: "GET /api/Redirects/resolve?path=/old-page" },
              response: {
                status: 200,
                body: { target: "/new-page", permanent: true },
              },
            },
          ],
        },
        {
          id: "files",
          title: { en: "Files", fa: "فایل‌ها" },
          summary: {
            en: "Serve a stored media file by website id + stored name.",
            fa: "سرو فایل مدیا با website id + نام ذخیره‌شده.",
          },
          relatedAdmin: ["media-library", "storage"],
          endpoints: [
            {
              title: { en: "Get file", fa: "گرفتن فایل" },
              method: "GET",
              path: "/api/files/{websiteId}/{*storedName}",
              auth: "none",
              request: { body: "GET /api/files/1/abc123.jpg" },
              response: {
                status: 200,
                body: "(binary file stream)",
              },
            },
          ],
        },
        {
          id: "feeds",
          title: { en: "Product feeds", fa: "فید محصولات" },
          summary: {
            en: "The crawlable product feed for one marketplace channel (Torob, Emalls, Google Shopping).",
            fa: "فید قابل کرال محصولات برای یک کانال مارکت‌پلیس (ترب، اِمالز، گوگل شاپینگ).",
          },
          relatedAdmin: ["marketplace", "products"],
          endpoints: [
            {
              title: { en: "Get feed", fa: "گرفتن فید" },
              method: "GET",
              path: "/feeds/{feedToken}",
              auth: "none",
              notes: {
                en: "The token is generated per channel and shown on its card in Admin → Catalog → Search Engines. It replaces a website id on purpose: a catalog should not be enumerable. An inactive channel or an unknown token returns 404. Content type follows the channel's provider — RSS 2.0 for Google Merchant Center, XML or JSON for a custom feed.",
                fa: "توکن برای هر کانال ساخته می‌شود و روی کارت همان کانال در ادمین ← کاتالوگ ← موتورهای جستجو نمایش داده می‌شود. عمداً جای شناسه سایت را گرفته است تا کاتالوگ قابل شمارش نباشد. کانال غیرفعال یا توکن ناشناس ۴۰۴ برمی‌گرداند. نوع محتوا تابع Provider کانال است — RSS 2.0 برای گوگل مرچنت سنتر و XML یا JSON برای فید سفارشی.",
              },
              request: { body: "GET /feeds/9f2c41ab7d0e4c1b8a35d6e2f0c71b94" },
              response: {
                status: 200,
                body: '<?xml version="1.0" encoding="utf-8"?>\n<rss xmlns:g="http://base.google.com/ns/1.0" version="2.0">\n  <channel>\n    <item>\n      <g:id>SKU-1</g:id>\n      <g:title>Blue Shirt</g:title>\n      <g:price>250000 IRR</g:price>\n      <g:availability>in stock</g:availability>\n    </item>\n  </channel>\n</rss>',
              },
            },
          ],
        },
      ],
    },

    /* ───────── Social commerce ───────── */
    {
      id: "social",
      title: { en: "Reviews & Q&A", fa: "نظرات و پرسش‌وپاسخ" },
      summary: {
        en: "Public approved lists; write actions need ClientReview JWT. Admin moderates.",
        fa: "لیست تأییدشده عمومی؛ نوشتن نیاز به JWT ClientReview. ادمین نظارت می‌کند.",
      },
      pages: [
        {
          id: "reviews",
          title: { en: "Product reviews", fa: "نظرات محصول" },
          relatedAdmin: ["moderation-reviews"],
          related: ["products", "auth"],
          endpoints: [
            {
              title: { en: "List approved", fa: "لیست تأییدشده" },
              method: "GET",
              path: "/api/products/{productId}/reviews",
              auth: "website",
              request: { body: "GET /api/products/10/reviews?page=1&pageSize=10" },
              response: {
                status: 200,
                body: { items: [{ rating: 5, body: "Great!" }], totalCount: 1 },
              },
            },
            {
              title: { en: "Submit review", fa: "ارسال نظر" },
              method: "POST",
              path: "/api/products/{productId}/reviews",
              auth: "jwt",
              request: { body: { rating: 5, title: "Great", body: "…" } },
              response: { status: 200, body: { success: true } },
            },
            {
              title: { en: "Like / dislike", fa: "لایک / دیسلایک" },
              method: "POST",
              path: "/api/reviews/{reviewId}/like  ·  /api/reviews/{reviewId}/dislike",
              auth: "jwt",
              request: { body: "POST /api/reviews/3/like" },
              response: { status: 200, body: "(OK)" },
            },
          ],
        },
        {
          id: "questions",
          title: { en: "Product Q&A", fa: "پرسش و پاسخ محصول" },
          relatedAdmin: ["moderation-questions"],
          related: ["products", "auth"],
          endpoints: [
            {
              title: { en: "List / ask / answer / like answer", fa: "لیست / پرسش / پاسخ / لایک پاسخ" },
              method: "GET",
              path: "/api/products/{productId}/questions  ·  POST …  ·  POST /api/questions/{id}/answers  ·  POST /api/answers/{id}/like",
              auth: "website",
              request: { body: { body: "Is this waterproof?" } },
              response: {
                status: 200,
                body: { items: [{ question: "…", answers: [] }], totalCount: 1 },
              },
              notes: [
                {
                  en: "Write endpoints (POST ask/answer/like) require JWT + ClientReview.",
                  fa: "اندپوینت‌های نوشتن (POST) به JWT + ClientReview نیاز دارند.",
                },
              ],
            },
          ],
        },
      ],
    },

    /* ───────── Platform ───────── */
    {
      id: "platform",
      title: { en: "Platform helpers", fa: "ابزارهای پلتفرم" },
      summary: {
        en: "Languages, localization, locations, captcha, cache invalidate.",
        fa: "زبان، بومی‌سازی، مکان، captcha، ابطال کش.",
      },
      pages: [
        {
          id: "languages",
          title: { en: "Languages", fa: "زبان‌ها" },
          relatedAdmin: ["website-languages", "languages-catalog"],
          endpoints: [
            {
              title: { en: "List languages", fa: "فهرست زبان‌ها" },
              method: "GET",
              path: "/api/Languages",
              auth: "website",
              request: { body: "GET /api/Languages" },
              response: {
                status: 200,
                body: [{ code: "fa", name: "Persian" }, { code: "en", name: "English" }],
              },
            },
          ],
        },
        {
          id: "localization",
          title: { en: "Localization strings", fa: "رشته‌های ترجمه" },
          relatedAdmin: ["website-translations", "admin-translations"],
          endpoints: [
            {
              title: { en: "Get all keys for a language", fa: "همه کلیدها برای یک زبان" },
              method: "GET",
              path: "/api/Localization/{languageCode}",
              auth: "website",
              request: { body: "GET /api/Localization/fa" },
              response: {
                status: 200,
                body: { "cart.title": "سبد خرید", "checkout.pay": "پرداخت" },
              },
            },
            {
              title: { en: "Set a key (authorized)", fa: "تنظیم یک کلید (مجاز)" },
              method: "PUT",
              path: "/api/Localization/{languageCode}/{key}",
              auth: "jwt",
              summary: {
                en: "Requires LocalizationEdit policy (not a normal customer).",
                fa: "سیاست LocalizationEdit لازم است (مشتری عادی نیست).",
              },
              request: { body: "\"متن جدید\"" },
              response: { status: 200, body: "(OK)" },
            },
          ],
        },
        {
          id: "locations",
          title: { en: "Locations", fa: "مکان‌ها" },
          summary: {
            en: "Countries and cities for address forms.",
            fa: "کشور و شهر برای فرم آدرس.",
          },
          relatedAdmin: ["initial-countries", "initial-cities"],
          related: ["addresses"],
          endpoints: [
            {
              title: { en: "Countries / cities", fa: "کشورها / شهرها" },
              method: "GET",
              path: "/api/Locations/countries  ·  /api/Locations/countries/{countryId}/cities",
              auth: "website",
              request: { body: "GET /api/Locations/countries/1/cities" },
              response: {
                status: 200,
                body: [{ cityId: 10, name: "Tehran" }],
              },
            },
          ],
        },
        {
          id: "captcha",
          title: { en: "Captcha challenge", fa: "چالش Captcha" },
          relatedAdmin: ["settings"],
          related: ["auth"],
          endpoints: [
            {
              title: { en: "Get challenge", fa: "گرفتن چالش" },
              method: "GET",
              path: "/api/Captcha/challenge",
              auth: "website",
              request: { body: "GET /api/Captcha/challenge" },
              response: {
                status: 200,
                body: { mode: "Math", challengeId: "…", question: "3 + 4 = ?" },
              },
            },
          ],
        },
        {
          id: "cache",
          title: { en: "Cache invalidate", fa: "ابطال کش" },
          summary: {
            en: "Internal/admin-style cache busting endpoint (not for storefront UI).",
            fa: "اندپوینت ابطال کش داخلی/ادمین‌گونه (برای UI استورفرانت نیست).",
          },
          endpoints: [
            {
              title: { en: "Invalidate", fa: "ابطال" },
              method: "POST",
              path: "/api/cache/invalidate",
              auth: "none",
              summary: {
                en: "Protected by configuration/shared secret patterns in the controller — do not expose casually.",
                fa: "با الگوهای پیکربندی/راز مشترک در کنترلر محافظت می‌شود — سرسری در معرض نگذارید.",
              },
              request: { body: { keys: ["product:10"] } },
              response: { status: 200, body: { success: true } },
            },
          ],
        },
      ],
    },
  ],
};
