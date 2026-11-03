CREATE TABLE [dbo].[PostXPublications] (
    [PostID]      INT            NOT NULL,
    [TweetId]     NVARCHAR (40)  NULL,
    [PublishedAt] DATETIME       NOT NULL,
    CONSTRAINT [PK_PostXPublications] PRIMARY KEY CLUSTERED ([PostID] ASC),
    CONSTRAINT [FK_PostXPublications_Posts] FOREIGN KEY ([PostID]) REFERENCES [dbo].[Posts] ([PostID])
);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Present when the post has been sent to X at least once.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PostXPublications';
GO
