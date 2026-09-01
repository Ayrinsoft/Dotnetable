CREATE TABLE [dbo].[CustomerReturnRequestHistories] (
    [CustomerReturnRequestHistoryID] INT            IDENTITY (1, 1) NOT NULL,
    [CustomerReturnRequestID]        INT            NOT NULL,
    [FromStatus]                     TINYINT        NOT NULL,
    [ToStatus]                       TINYINT        NOT NULL,
    [Note]                           NVARCHAR (500) NULL,
    [CreatedByMemberID]              INT            NULL,
    [CreatedByClientID]              INT            NULL,
    [CreatedAt]                      DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_CustomerReturnRequestHistories] PRIMARY KEY CLUSTERED ([CustomerReturnRequestHistoryID] ASC),
    CONSTRAINT [FK_CustomerReturnRequestHistories_Requests] FOREIGN KEY ([CustomerReturnRequestID]) REFERENCES [dbo].[CustomerReturnRequests] ([CustomerReturnRequestID]),
    CONSTRAINT [FK_CustomerReturnRequestHistories_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID])
);

GO
CREATE NONCLUSTERED INDEX [IX_CustomerReturnRequestHistories_Request]
    ON [dbo].[CustomerReturnRequestHistories] ([CustomerReturnRequestID] ASC, [CreatedAt] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_CustomerReturnRequestHistories_CreatedByMemberID]
    ON [dbo].[CustomerReturnRequestHistories] ([CreatedByMemberID] ASC);
