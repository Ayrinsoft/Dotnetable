CREATE TABLE [dbo].[ProductWarnings] (
    [ProductWarningID] INT             IDENTITY (1, 1) NOT NULL,
    [ProductID]        INT             NOT NULL,
    [Severity]         NVARCHAR (20)   CONSTRAINT [DF_ProductWarnings_Severity] DEFAULT ('info') NOT NULL,
    [Text]             NVARCHAR (1000) NOT NULL,
    [IsActive]         BIT             CONSTRAINT [DF_ProductWarnings_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_ProductWarnings] PRIMARY KEY CLUSTERED ([ProductWarningID] ASC),
    CONSTRAINT [FK_ProductWarnings_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID])
);

