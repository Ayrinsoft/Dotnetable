CREATE TABLE [dbo].[PostTelegramPublications] (
    [PostID]      INT            NOT NULL,
    [MessageId]   NVARCHAR (40)  NULL,
    [PublishedAt] DATETIME       NOT NULL,
    CONSTRAINT [PK_PostTelegramPublications] PRIMARY KEY CLUSTERED ([PostID] ASC),
    CONSTRAINT [FK_PostTelegramPublications_Posts] FOREIGN KEY ([PostID]) REFERENCES [dbo].[Posts] ([PostID])
);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Present when the post has been sent to Telegram at least once.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PostTelegramPublications';
GO
