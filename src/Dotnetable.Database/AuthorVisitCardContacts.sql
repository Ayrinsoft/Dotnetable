CREATE TABLE [dbo].[AuthorVisitCardContacts] (
    [ContactID]         INT             IDENTITY (1, 1) NOT NULL,
    [AuthorVisitCardID] INT             NOT NULL,
    [Name]              NVARCHAR (70)   NOT NULL,
    [Mobile]            VARCHAR (20)    NULL,
    [Email]             NVARCHAR (120)  NULL,
    [Reason]            NVARCHAR (2000) NOT NULL,
    [LogTime]           DATETIME2 (0)   NOT NULL,
    [DisplayName]       NVARCHAR (70)   NULL,
    [Note]              NVARCHAR (200)  NULL,
    [VisitorKey]        CHAR (32)       NULL,
    [SavedContact]      BIT             CONSTRAINT [DF_AuthorVisitCardContacts_SavedContact] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_AuthorVisitCardContacts] PRIMARY KEY CLUSTERED ([ContactID] ASC),
    CONSTRAINT [FK_AuthorVisitCardContacts_AuthorVisitCards] FOREIGN KEY ([AuthorVisitCardID]) REFERENCES [dbo].[AuthorVisitCards] ([AuthorVisitCardID]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_AuthorVisitCardContacts_AuthorVisitCardID]
    ON [dbo].[AuthorVisitCardContacts] ([AuthorVisitCardID] ASC);
GO
