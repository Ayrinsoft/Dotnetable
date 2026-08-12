CREATE TABLE [dbo].[PayrollRateBrackets] (
    [PayrollRateBracketID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]            INT             NOT NULL,
    [Kind]                 TINYINT         NOT NULL,
    [FromAmount]           DECIMAL (18, 4) NOT NULL,
    [ToAmount]             DECIMAL (18, 4) NULL,
    [Rate]                 DECIMAL (9, 6)  NOT NULL,
    [SortOrder]            INT             NOT NULL,
    [IsActive]             BIT             NOT NULL CONSTRAINT [DF_PayrollRateBrackets_IsActive] DEFAULT ((1)),
    [CreatedAt]            DATETIME2 (7)   NOT NULL,
    CONSTRAINT [PK_PayrollRateBrackets] PRIMARY KEY CLUSTERED ([PayrollRateBracketID] ASC),
    CONSTRAINT [FK_PayrollRateBrackets_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO
CREATE NONCLUSTERED INDEX [IX_PayrollRateBrackets_Website_Kind]
    ON [dbo].[PayrollRateBrackets] ([WebsiteID] ASC, [Kind] ASC, [SortOrder] ASC);
GO
