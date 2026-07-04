CREATE TABLE [dbo].[Slideshows] (
    [SlideshowID]      INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT            NOT NULL,
    [Name]             NVARCHAR (150) NOT NULL,
    [PlacementKey]     NVARCHAR (100) NULL,
    [TransitionEffect] TINYINT        CONSTRAINT [DF_Slideshows_TransitionEffect] DEFAULT ((1)) NOT NULL,
    [AutoPlay]         BIT            CONSTRAINT [DF_Slideshows_AutoPlay] DEFAULT ((1)) NOT NULL,
    [IntervalMs]       INT            CONSTRAINT [DF_Slideshows_IntervalMs] DEFAULT ((5000)) NOT NULL,
    [ShowArrows]       BIT            CONSTRAINT [DF_Slideshows_ShowArrows] DEFAULT ((1)) NOT NULL,
    [ShowDots]         BIT            CONSTRAINT [DF_Slideshows_ShowDots] DEFAULT ((1)) NOT NULL,
    [EnableLightbox]   BIT            CONSTRAINT [DF_Slideshows_EnableLightbox] DEFAULT ((1)) NOT NULL,
    [AspectRatio]      NVARCHAR (20)  NULL,
    [IsActive]         BIT            CONSTRAINT [DF_Slideshows_IsActive] DEFAULT ((1)) NOT NULL,
    [CreatedAt]        DATETIME       CONSTRAINT [DF_Slideshows_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_Slideshows] PRIMARY KEY CLUSTERED ([SlideshowID] ASC),
    CONSTRAINT [FK_Slideshows_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
