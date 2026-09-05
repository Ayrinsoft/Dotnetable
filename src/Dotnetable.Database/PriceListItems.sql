CREATE TABLE [dbo].[PriceListItems] (
    [PriceListItemID] INT             IDENTITY (1, 1) NOT NULL,
    [PriceListID]     INT             NOT NULL,
    [Title]           NVARCHAR (300)  NOT NULL,
    [GroupName]       NVARCHAR (150)  NULL,
    [Specification]   NVARCHAR (300)  NULL,
    [Unit]            NVARCHAR (50)   NULL,
    [Sku]             NVARCHAR (100)  NULL,
    [BasePriceUsd]    DECIMAL (18, 4) NOT NULL CONSTRAINT [DF_PriceListItems_BasePriceUsd] DEFAULT ((0)),
    [LinkToUsd]       BIT             NOT NULL CONSTRAINT [DF_PriceListItems_LinkToUsd] DEFAULT ((0)),
    [FixedPrice]      DECIMAL (18, 4) NULL,
    [Notes]           NVARCHAR (1000) NULL,
    [SortOrder]       INT             NOT NULL CONSTRAINT [DF_PriceListItems_SortOrder] DEFAULT ((0)),
    [IsActive]        BIT             NOT NULL CONSTRAINT [DF_PriceListItems_IsActive] DEFAULT ((1)),
    [UpdatedAt]       DATETIME        NOT NULL,
    CONSTRAINT [PK_PriceListItems] PRIMARY KEY CLUSTERED ([PriceListItemID] ASC),
    CONSTRAINT [FK_PriceListItems_PriceLists] FOREIGN KEY ([PriceListID]) REFERENCES [dbo].[PriceLists] ([PriceListID]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_PriceListItems_PriceListID]
    ON [dbo].[PriceListItems] ([PriceListID] ASC);
GO
