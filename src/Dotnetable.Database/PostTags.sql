CREATE TABLE [dbo].[PostTags] (
    [PosID] INT NOT NULL,
    [TagID] INT NOT NULL,
    CONSTRAINT [PK_PostTags] PRIMARY KEY CLUSTERED ([PosID] ASC, [TagID] ASC),
    CONSTRAINT [FK_PostTags_Posts] FOREIGN KEY ([PosID]) REFERENCES [dbo].[Posts] ([PostID]),
    CONSTRAINT [FK_PostTags_Tags] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([TagID])
);

