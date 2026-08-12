CREATE TABLE [dbo].[StockDocumentHistories] (
    [StockDocumentHistoryID] INT            IDENTITY (1, 1) NOT NULL,
    [StockDocumentID]        INT            NOT NULL,
    [FromStatus]             TINYINT        NOT NULL,
    [ToStatus]               TINYINT        NOT NULL,
    [Note]                   NVARCHAR (500) NULL,
    [CreatedByMemberID]      INT            NULL,
    [CreatedAt]              DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_StockDocumentHistories] PRIMARY KEY CLUSTERED ([StockDocumentHistoryID] ASC),
    CONSTRAINT [FK_StockDocumentHistories_Docs] FOREIGN KEY ([StockDocumentID]) REFERENCES [dbo].[StockDocuments] ([StockDocumentID]),
    CONSTRAINT [FK_StockDocumentHistories_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID])
);
GO
