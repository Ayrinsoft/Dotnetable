namespace Dotnetable.Admin.Localization;

public record AuthStrings(
    string BrandTitle,
    string LoginTitle,
    string LoginSubtitle,
    string UsernameLabel,
    string PasswordLabel,
    string SignIn,
    string ForgotPasswordLink,
    string InvalidCredentials,
    string ForgotTitle,
    string ForgotSubtitle,
    string EmailOrUsernameLabel,
    string SendResetLink,
    string BackToSignIn,
    string GenericSent,
    string EmailNotConfigured,
    string TryAgainLater
);

public static class AuthL10n
{
    public static AuthStrings Get(string lang) => lang switch
    {
        "de" => De,
        "fr" => Fr,
        "ru" => Ru,
        "zh" => Zh,
        "fa" => Fa,
        "ar" => Ar,
        _ => En,
    };

    private static readonly AuthStrings En = new(
        "Dotnetable Admin", "Login", "Let's get you signed in. Enter your credentials to continue.",
        "Username", "Password", "Sign in", "Forgot password?", "Invalid username or password.",
        "Forgot password", "Enter your username or email and we'll send you a reset link.",
        "Username or email", "Send reset link", "Back to sign in",
        "If an account matches, a password reset link has been sent to its email address.",
        "Email sending is not configured. Please contact your administrator.",
        "We couldn't send the email right now. Please try again later.");

    private static readonly AuthStrings De = new(
        "Dotnetable Admin", "Anmelden", "Melden Sie sich mit Ihren Zugangsdaten an, um fortzufahren.",
        "Benutzername", "Passwort", "Anmelden", "Passwort vergessen?", "Ungültiger Benutzername oder Passwort.",
        "Passwort vergessen", "Geben Sie Ihren Benutzernamen oder Ihre E-Mail-Adresse ein, wir senden Ihnen einen Link.",
        "Benutzername oder E-Mail", "Link senden", "Zurück zur Anmeldung",
        "Falls ein Konto übereinstimmt, wurde ein Link zum Zurücksetzen des Passworts an dessen E-Mail-Adresse gesendet.",
        "Der E-Mail-Versand ist nicht konfiguriert. Bitte wenden Sie sich an Ihren Administrator.",
        "Die E-Mail konnte derzeit nicht gesendet werden. Bitte versuchen Sie es später erneut.");

    private static readonly AuthStrings Fr = new(
        "Dotnetable Admin", "Connexion", "Connectez-vous avec vos identifiants pour continuer.",
        "Nom d'utilisateur", "Mot de passe", "Se connecter", "Mot de passe oublié ?", "Nom d'utilisateur ou mot de passe invalide.",
        "Mot de passe oublié", "Entrez votre nom d'utilisateur ou votre e-mail, nous vous enverrons un lien.",
        "Nom d'utilisateur ou e-mail", "Envoyer le lien", "Retour à la connexion",
        "Si un compte correspond, un lien de réinitialisation a été envoyé à son adresse e-mail.",
        "L'envoi d'e-mails n'est pas configuré. Veuillez contacter votre administrateur.",
        "Nous n'avons pas pu envoyer l'e-mail pour le moment. Veuillez réessayer plus tard.");

    private static readonly AuthStrings Ru = new(
        "Dotnetable Admin", "Вход", "Введите учётные данные, чтобы продолжить.",
        "Имя пользователя", "Пароль", "Войти", "Забыли пароль?", "Неверное имя пользователя или пароль.",
        "Восстановление пароля", "Введите имя пользователя или e-mail, и мы отправим вам ссылку.",
        "Имя пользователя или e-mail", "Отправить ссылку", "Назад ко входу",
        "Если аккаунт найден, ссылка для сброса пароля отправлена на его e-mail.",
        "Отправка почты не настроена. Обратитесь к администратору.",
        "Не удалось отправить письмо. Попробуйте позже.");

    private static readonly AuthStrings Zh = new(
        "Dotnetable Admin", "登录", "请输入您的账户信息以继续。",
        "用户名", "密码", "登录", "忘记密码？", "用户名或密码无效。",
        "忘记密码", "输入您的用户名或电子邮箱，我们会发送重置链接。",
        "用户名或电子邮箱", "发送重置链接", "返回登录",
        "如果账户匹配，重置密码的链接已发送至其电子邮箱。",
        "邮件发送未配置，请联系管理员。",
        "暂时无法发送邮件，请稍后重试。");

    private static readonly AuthStrings Fa = new(
        "پنل مدیریت Dotnetable", "ورود", "برای ادامه، اطلاعات ورود خود را وارد کنید.",
        "نام کاربری", "رمز عبور", "ورود", "رمز عبور را فراموش کرده‌اید؟", "نام کاربری یا رمز عبور اشتباه است.",
        "فراموشی رمز عبور", "نام کاربری یا ایمیل خود را وارد کنید تا لینک بازیابی برایتان ارسال شود.",
        "نام کاربری یا ایمیل", "ارسال لینک بازیابی", "بازگشت به ورود",
        "در صورت وجود حساب کاربری مطابق، لینک بازیابی رمز عبور به ایمیل آن ارسال شد.",
        "ارسال ایمیل پیکربندی نشده است. لطفاً با مدیر سیستم تماس بگیرید.",
        "در حال حاضر امکان ارسال ایمیل نیست. لطفاً بعداً دوباره تلاش کنید.");

    private static readonly AuthStrings Ar = new(
        "لوحة تحكم Dotnetable", "تسجيل الدخول", "أدخل بيانات الدخول الخاصة بك للمتابعة.",
        "اسم المستخدم", "كلمة المرور", "تسجيل الدخول", "هل نسيت كلمة المرور؟", "اسم المستخدم أو كلمة المرور غير صحيحة.",
        "نسيت كلمة المرور", "أدخل اسم المستخدم أو البريد الإلكتروني وسنرسل لك رابط إعادة التعيين.",
        "اسم المستخدم أو البريد الإلكتروني", "إرسال رابط إعادة التعيين", "العودة لتسجيل الدخول",
        "إذا كان هناك حساب مطابق، فقد تم إرسال رابط إعادة تعيين كلمة المرور إلى بريده الإلكتروني.",
        "إرسال البريد الإلكتروني غير مُهيَّأ. يرجى الاتصال بالمسؤول.",
        "تعذر إرسال البريد الإلكتروني الآن. يرجى المحاولة لاحقًا.");
}
