CREATE TABLE [dbo].[TB_Email_Subscribe] (
    [EmailSubscribeID] INT          IDENTITY (1, 1) NOT NULL,
    [Email]            VARCHAR (64) NOT NULL,
    [LogTime]          DATETIME     NOT NULL,
    [Active]           BIT          NOT NULL,
    [MemberID]         INT          NULL,
    [Approved]         BIT          NOT NULL,
    CONSTRAINT [PK_TB_Email_Subscribe] PRIMARY KEY CLUSTERED ([EmailSubscribeID] ASC),
    CONSTRAINT [FK_TB_Email_Subscribe_TB_Member] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[TB_Member] ([MemberID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Email_Subscribe_MemberID]
    ON [dbo].[TB_Email_Subscribe]([MemberID] ASC);

