CREATE TABLE [dbo].[EmailSubscribe] (
    [EmailSubscribeID] INT          IDENTITY (1, 1) NOT NULL,
    [Email]            VARCHAR (64) NOT NULL,
    [LogTime]          DATETIME     NOT NULL,
    [Active]           BIT          NOT NULL,
    [MemberID]         INT          NULL,
    [Approved]         BIT          NOT NULL,
    CONSTRAINT [PK_EmailSubscribe] PRIMARY KEY CLUSTERED ([EmailSubscribeID] ASC),
    CONSTRAINT [FK_EmailSubscribe_Member] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[Member] ([MemberID])
);

