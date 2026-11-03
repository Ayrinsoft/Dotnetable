CREATE TABLE [dbo].[PostInstagramPublications] (
    [PostID]      INT            NOT NULL,
    [IgMediaId]   NVARCHAR (64)  NOT NULL,
    [Permalink]   NVARCHAR (300) NULL,
    [PublishedAt] DATETIME       NOT NULL,
    CONSTRAINT [PK_PostInstagramPublications] PRIMARY KEY CLUSTERED ([PostID] ASC),
    CONSTRAINT [FK_PostInstagramPublications_Posts] FOREIGN KEY ([PostID]) REFERENCES [dbo].[Posts] ([PostID])
);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Instagram media id after a successful publish. One row blocks a second send.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PostInstagramPublications';
GO
