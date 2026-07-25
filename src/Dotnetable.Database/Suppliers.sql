CREATE TABLE [dbo].[Suppliers] (
    [SupplierID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]  INT            NOT NULL,
    [Name]       NVARCHAR (200) NOT NULL,
    [Phone]      NVARCHAR (20)  NULL,
    [IsActive]   BIT NOT NULL,
    CONSTRAINT [PK_Suppliers] PRIMARY KEY CLUSTERED ([SupplierID] ASC),
    CONSTRAINT [FK_Suppliers_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_Suppliers_WebsiteID]
    ON [dbo].[Suppliers] ([WebsiteID] ASC);
