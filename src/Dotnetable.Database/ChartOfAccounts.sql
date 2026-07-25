CREATE TABLE [dbo].[ChartOfAccounts] (
    [ChartOfAccountID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT            NOT NULL,
    [ParentAccountID]  INT            NULL,
    [Code]             NVARCHAR (20)  NOT NULL,
    [Name]             NVARCHAR (200) NOT NULL,
    [AccountType]      TINYINT        NOT NULL,
    [IsActive]         BIT            CONSTRAINT [DF_ChartOfAccounts_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_ChartOfAccounts] PRIMARY KEY CLUSTERED ([ChartOfAccountID] ASC),
    CONSTRAINT [FK_ChartOfAccounts_ChartOfAccounts] FOREIGN KEY ([ParentAccountID]) REFERENCES [dbo].[ChartOfAccounts] ([ChartOfAccountID]),
    CONSTRAINT [FK_ChartOfAccounts_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ChartOfAccounts_ParentAccountID]
    ON [dbo].[ChartOfAccounts] ([ParentAccountID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ChartOfAccounts_WebsiteID]
    ON [dbo].[ChartOfAccounts] ([WebsiteID] ASC);
