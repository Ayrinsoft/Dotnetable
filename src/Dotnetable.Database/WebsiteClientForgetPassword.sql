CREATE TABLE [dbo].[WebsiteClientForgetPassword] (
    [WebsiteClientForgetPasswordID] INT         IDENTITY (1, 1) NOT NULL,
    [ForgetKey]                     VARCHAR (8) NOT NULL,
    [WebsiteClientID]               INT         NOT NULL,
    [LogTime]                       DATETIME    NOT NULL,
    CONSTRAINT [PK_WebsiteClientForgetPassword] PRIMARY KEY CLUSTERED ([WebsiteClientForgetPasswordID] ASC),
    CONSTRAINT [FK_WebsiteClientForgetPassword_WebsiteClient] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClient] ([WebsiteClientID])
);

