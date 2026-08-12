CREATE TABLE [dbo].[RecordAttachments] (
    [RecordAttachmentID] BIGINT         IDENTITY (1, 1) NOT NULL,
    [WebsiteID]          INT            NOT NULL,
    [EntityType]         NVARCHAR (64)  NOT NULL,
    [EntityID]           BIGINT         NOT NULL,
    [FileRecordID]       INT            NOT NULL,
    [Title]              NVARCHAR (200) NULL,
    [Note]               NVARCHAR (500) NULL,
    [CreatedByMemberID]  INT            NULL,
    [CreatedAt]          DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_RecordAttachments] PRIMARY KEY CLUSTERED ([RecordAttachmentID] ASC),
    CONSTRAINT [FK_RecordAttachments_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_RecordAttachments_Files] FOREIGN KEY ([FileRecordID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_RecordAttachments_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID])
);
GO

CREATE NONCLUSTERED INDEX [IX_RecordAttachments_Entity]
    ON [dbo].[RecordAttachments] ([WebsiteID] ASC, [EntityType] ASC, [EntityID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_RecordAttachments_File]
    ON [dbo].[RecordAttachments] ([FileRecordID] ASC);
GO
