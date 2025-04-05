CREATE TABLE [dbo].[TB_Member_Forget_Password] (
    [MemberForgetPasswordID] INT         IDENTITY (1, 1) NOT NULL,
    [ForgetKey]              VARCHAR (8) NOT NULL,
    [MemberID]               INT         NOT NULL,
    [LogTime]                DATETIME    NOT NULL,
    CONSTRAINT [PK_TB_Member_Forget_Password] PRIMARY KEY CLUSTERED ([MemberForgetPasswordID] ASC),
    CONSTRAINT [FK_TB_Member_Forget_Password_TB_Member] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[TB_Member] ([MemberID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Member_Forget_Password_MemberID]
    ON [dbo].[TB_Member_Forget_Password]([MemberID] ASC);

