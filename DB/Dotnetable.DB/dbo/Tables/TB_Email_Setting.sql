CREATE TABLE [dbo].[TB_Email_Setting] (
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
    CONSTRAINT [PK_TB_Email_Setting] PRIMARY KEY CLUSTERED ([EmailSettingID] ASC),
    CONSTRAINT [FK_TB_Email_Setting_TB_Email_Type] FOREIGN KEY ([EmailTypeID]) REFERENCES [dbo].[TB_Email_Type] ([EmailTypeID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Email_Setting_EmailTypeID]
    ON [dbo].[TB_Email_Setting]([EmailTypeID] ASC);

