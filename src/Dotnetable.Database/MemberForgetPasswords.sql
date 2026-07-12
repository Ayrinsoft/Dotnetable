CREATE TABLE [dbo].[MemberForgetPasswords] (
    [MemberForgetPasswordID] INT         IDENTITY (1, 1) NOT NULL,
    [ForgetKey]              VARCHAR (8) NOT NULL,
    [MemberID]               INT         NOT NULL,
    [LogTime]                DATETIME    NOT NULL,
    CONSTRAINT [PK_MemberForgetPasswords] PRIMARY KEY CLUSTERED ([MemberForgetPasswordID] ASC),
    CONSTRAINT [FK_MemberForgetPasswords_Members] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[Members] ([MemberID])
);
GO

CREATE NONCLUSTERED INDEX [IX_MemberForgetPasswords_MemberID]
    ON [dbo].[MemberForgetPasswords] ([MemberID] ASC);
