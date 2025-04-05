CREATE TABLE [dbo].[TB_Member_Activate_Log] (
    [MemberActivateLogID] INT              IDENTITY (1, 1) NOT NULL,
    [MemberID]            INT              NOT NULL,
    [ActivateCode]        UNIQUEIDENTIFIER NOT NULL,
    [ExpireDate]          DATETIME         NOT NULL,
    CONSTRAINT [PK_TB_Member_Activate_Log] PRIMARY KEY CLUSTERED ([MemberActivateLogID] ASC),
    CONSTRAINT [FK_TB_Member_Activate_Log_TB_Member_Activate_Log] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[TB_Member] ([MemberID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Member_Activate_Log_MemberID]
    ON [dbo].[TB_Member_Activate_Log]([MemberID] ASC);

