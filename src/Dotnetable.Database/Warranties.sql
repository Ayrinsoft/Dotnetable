CREATE TABLE [dbo].[Warranties] (
    [WarrantyID]     INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]      INT            NOT NULL,
    [Title]          NVARCHAR (200) NOT NULL,
    [Description]    NVARCHAR (2000) NULL,
    [ProviderName]   NVARCHAR (200) NULL,
    [DurationMonths] INT            NULL,
    [IsActive]       BIT            CONSTRAINT [DF_Warranties_IsActive] DEFAULT ((1)) NOT NULL,
    [SortOrder]      INT            CONSTRAINT [DF_Warranties_SortOrder] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Warranties] PRIMARY KEY CLUSTERED ([WarrantyID] ASC),
    CONSTRAINT [FK_Warranties_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_Warranties_WebsiteID]
    ON [dbo].[Warranties] ([WebsiteID] ASC);
