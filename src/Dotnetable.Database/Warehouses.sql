CREATE TABLE [dbo].[Warehouses] (
    [WarehouseID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]   INT            NOT NULL,
    [Code]        NVARCHAR (20)  NOT NULL,
    [Name]        NVARCHAR (200) NOT NULL,
    [Address]     NVARCHAR (500) NULL,
    [IsDefault]   BIT            DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [IsActive]    BIT            DEFAULT (CONVERT([bit],(1))) NOT NULL,
    [CreatedAt]   DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_Warehouses] PRIMARY KEY CLUSTERED ([WarehouseID] ASC),
    CONSTRAINT [FK_Warehouses_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Warehouses_Website_Code] ON [dbo].[Warehouses] ([WebsiteID] ASC, [Code] ASC);
GO
