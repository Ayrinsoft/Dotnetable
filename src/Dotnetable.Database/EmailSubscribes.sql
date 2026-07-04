CREATE TABLE [dbo].[EmailSubscribes] (
    [EmailSubscribeID] INT          IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT          NOT NULL,
    [Email]            VARCHAR (64) NOT NULL,
    [LogTime]          DATETIME     NOT NULL,
    [Active]           BIT          NOT NULL,
    [MemberID]         INT          NULL,
    [Approved]         BIT          NOT NULL,
    CONSTRAINT [PK_EmailSubscribes] PRIMARY KEY CLUSTERED ([EmailSubscribeID] ASC),
    CONSTRAINT [FK_EmailSubscribes_Members] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_EmailSubscribes_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

