CREATE TABLE [dbo].[TB_Login_Token] (
    [LoginTokenID] INT              IDENTITY (1, 1) NOT NULL,
    [MemberID]     INT              NOT NULL,
    [RefreshToken] UNIQUEIDENTIFIER NOT NULL,
    [ExpireTime]   DATETIME         NOT NULL,
    CONSTRAINT [PK_TB_L_Login_Token] PRIMARY KEY CLUSTERED ([LoginTokenID] ASC),
    CONSTRAINT [FK_TB_L_Login_Token_TB_Member] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[TB_Member] ([MemberID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Login_Token_MemberID]
    ON [dbo].[TB_Login_Token]([MemberID] ASC);

