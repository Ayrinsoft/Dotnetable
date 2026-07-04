CREATE TABLE [dbo].[EmailSetting] (
    [EmailSettingID] INT            IDENTITY (1, 1) NOT NULL,
    [EmailAddress]   VARCHAR (64)   NOT NULL,
    [Password]       NVARCHAR (256) NOT NULL,
    [SMTPPort]       INT            NOT NULL,
    [EnableSSL]      BIT            NOT NULL,
    [MailName]       NVARCHAR (64)  NOT NULL,
    [MailServer]     VARCHAR (64)   NOT NULL,
    [EmailTypeID]    TINYINT        NOT NULL,
    [DefaultEMail]   BIT            NOT NULL,
    [Active]         BIT            NOT NULL,
    [WebsiteID]      INT            NOT NULL,
    CONSTRAINT [PK_EmailSetting] PRIMARY KEY CLUSTERED ([EmailSettingID] ASC),
    CONSTRAINT [FK_EmailSetting_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

