CREATE TABLE [dbo].[Slideshows] (
    [SlideshowID]      INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT            NOT NULL,
    [Name]             NVARCHAR (150) NOT NULL,
    [PlacementKey]     NVARCHAR (100) NULL,
    [TransitionEffect] TINYINT NOT NULL,
    [AutoPlay]         BIT NOT NULL,
    [IntervalMs]       INT NOT NULL,
    [ShowArrows]       BIT NOT NULL,
    [ShowDots]         BIT NOT NULL,
    [EnableLightbox]   BIT NOT NULL,
    [AspectRatio]      NVARCHAR (20)  NULL,
    [IsActive]         BIT NOT NULL,
    [CreatedAt]        DATETIME NOT NULL,
    CONSTRAINT [PK_Slideshows] PRIMARY KEY CLUSTERED ([SlideshowID] ASC),
    CONSTRAINT [FK_Slideshows_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_Slideshows_WebsiteID]
    ON [dbo].[Slideshows] ([WebsiteID] ASC);
