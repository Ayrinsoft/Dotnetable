CREATE TABLE [dbo].[SlideshowSlides] (
    [SlideshowSlideID] INT            IDENTITY (1, 1) NOT NULL,
    [SlideshowID]       INT            NOT NULL,
    [FileID]            INT            NOT NULL,
    [MobileFileID]      INT            NULL,
    [Title]             NVARCHAR (200) NULL,
    [Caption]           NVARCHAR (500) NULL,
    [ButtonText]        NVARCHAR (100) NULL,
    [LinkUrl]           NVARCHAR (500) NULL,
    [OpenInNewTab]      BIT            CONSTRAINT [DF_SlideshowSlides_OpenInNewTab] DEFAULT ((0)) NOT NULL,
    [SortOrder]         INT            CONSTRAINT [DF_SlideshowSlides_SortOrder] DEFAULT ((0)) NOT NULL,
    [IsActive]          BIT            CONSTRAINT [DF_SlideshowSlides_IsActive] DEFAULT ((1)) NOT NULL,
    [StartAt]           DATETIME2 (7)  NULL,
    [EndAt]             DATETIME2 (7)  NULL,
    CONSTRAINT [PK_SlideshowSlides] PRIMARY KEY CLUSTERED ([SlideshowSlideID] ASC),
    CONSTRAINT [FK_SlideshowSlides_Slideshows] FOREIGN KEY ([SlideshowID]) REFERENCES [dbo].[Slideshows] ([SlideshowID]),
    CONSTRAINT [FK_SlideshowSlides_FileRecords] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_SlideshowSlides_FileRecord1] FOREIGN KEY ([MobileFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID])
);
