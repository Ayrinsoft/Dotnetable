CREATE TABLE [dbo].[StockDocumentHistories] (
    [StockDocumentHistoryID] INT            IDENTITY (1, 1) NOT NULL,
    [StockDocumentID]        INT            NOT NULL,
    [FromStatus]             TINYINT        NOT NULL,
    [ToStatus]               TINYINT        NOT NULL,
    [Note]                   NVARCHAR (500) NULL,
    [CreatedByMemberID]      INT            NULL,
    [CreatedAt]              DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_StockDocumentHistories] PRIMARY KEY CLUSTERED ([StockDocumentHistoryID] ASC),
    CONSTRAINT [FK_StockDocumentHistories_Members_CreatedByMemberID] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_StockDocumentHistories_StockDocuments_StockDocumentID] FOREIGN KEY ([StockDocumentID]) REFERENCES [dbo].[StockDocuments] ([StockDocumentID])
);


GO
CREATE NONCLUSTERED INDEX [IX_StockDocumentHistories_StockDocumentID]
    ON [dbo].[StockDocumentHistories]([StockDocumentID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_StockDocumentHistories_CreatedByMemberID]
    ON [dbo].[StockDocumentHistories]([CreatedByMemberID] ASC);

