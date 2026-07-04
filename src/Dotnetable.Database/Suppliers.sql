CREATE TABLE [dbo].[Suppliers] (
    [SupplierID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]  INT            NOT NULL,
    [Name]       NVARCHAR (200) NOT NULL,
    [Phone]      NVARCHAR (20)  NULL,
    [IsActive]   BIT            CONSTRAINT [DF_Suppliers_IsActive_1] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Suppliers] PRIMARY KEY CLUSTERED ([SupplierID] ASC),
    CONSTRAINT [FK_Suppliers_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

