CREATE TABLE [dbo].[AdminNotifications] (
    [AdminNotificationID] INT             IDENTITY (1, 1) NOT NULL,
    [MemberID]            INT             NOT NULL,
    [WebsiteID]           INT             NOT NULL,
    [NotificationType]    TINYINT         NOT NULL,
    [Title]               NVARCHAR (200)  NOT NULL,
    [Message]             NVARCHAR (1000) NOT NULL,
    [ActionUrl]           VARCHAR (256)   NULL,
    [RelatedEntityID]     INT             NULL,
    [IsRead]              BIT             CONSTRAINT [DF_AdminNotifications_IsRead] DEFAULT ((0)) NOT NULL,
    [CreatedAt]           DATETIME        NOT NULL,
    CONSTRAINT [PK_AdminNotifications] PRIMARY KEY CLUSTERED ([AdminNotificationID] ASC),
    CONSTRAINT [FK_AdminNotifications_Members] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_AdminNotifications_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_AdminNotifications_MemberID_IsRead_CreatedAt]
    ON [dbo].[AdminNotifications] ([MemberID] ASC, [IsRead] ASC, [CreatedAt] DESC);
GO

CREATE NONCLUSTERED INDEX [IX_AdminNotifications_WebsiteID]
    ON [dbo].[AdminNotifications] ([WebsiteID] ASC);
