CREATE TABLE [dbo].[ProductContentSections] (
    [ProductContentSectionID] INT            IDENTITY (1, 1) NOT NULL,
    [ProductID]               INT            NOT NULL,
    [SectionType]             TINYINT        NOT NULL,
    [HtmlContent]             NVARCHAR (MAX) NULL,
    [FileID]                  INT            NULL,
    [MediaSetID]              INT            NULL,
    [SortOrder]               INT            CONSTRAINT [DF_ProductContentSections_SortOrder] DEFAULT ((0)) NOT NULL,
    [IsActive]                BIT            CONSTRAINT [DF_ProductContentSections_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_ProductContentSections] PRIMARY KEY CLUSTERED ([ProductContentSectionID] ASC),
    CONSTRAINT [FK_ProductContentSections_FileRecords] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_ProductContentSections_MediaSets] FOREIGN KEY ([MediaSetID]) REFERENCES [dbo].[MediaSets] ([MediaSetID]),
    CONSTRAINT [FK_ProductContentSections_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID])
);

