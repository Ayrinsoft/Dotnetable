CREATE TABLE [dbo].[WebsiteCaptchaSettings] (
    [WebsiteCaptchaSettingID] INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]               INT           NOT NULL,
    [Provider]                TINYINT NOT NULL,
    [TurnstileSiteKey]        NVARCHAR (200) NULL,
    [TurnstileSecretKey]      NVARCHAR (200) NULL,
    CONSTRAINT [PK_WebsiteCaptchaSettings] PRIMARY KEY CLUSTERED ([WebsiteCaptchaSettingID] ASC),
    CONSTRAINT [UQ_WebsiteCaptchaSettings_WebsiteID] UNIQUE ([WebsiteID]),
    CONSTRAINT [FK_WebsiteCaptchaSettings_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

-- Provider maps to Dotnetable.Domain.Enums.CaptchaProvider. No row, or Provider=Turnstile with an
-- empty key pair, both mean "use the built-in math captcha" (see IHumanVerificationService).
