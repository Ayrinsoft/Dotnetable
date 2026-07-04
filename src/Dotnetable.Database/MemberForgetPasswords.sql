CREATE TABLE [dbo].[MemberForgetPassword] (
    [MemberForgetPasswordID] INT         IDENTITY (1, 1) NOT NULL,
    [ForgetKey]              VARCHAR (8) NOT NULL,
    [MemberID]               INT         NOT NULL,
    [LogTime]                DATETIME    NOT NULL,
    CONSTRAINT [PK_MemberForgetPassword] PRIMARY KEY CLUSTERED ([MemberForgetPasswordID] ASC),
    CONSTRAINT [FK_MemberForgetPassword_Member] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[Member] ([MemberID])
);

