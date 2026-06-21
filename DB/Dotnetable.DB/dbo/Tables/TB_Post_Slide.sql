CREATE TABLE [dbo].[TB_Post_Slide] (
    [PostSlideID] INT              IDENTITY (1, 1) NOT NULL,
    [PostID]      INT              NOT NULL,
    [Slug]        VARCHAR (16)     NOT NULL,
    [FileCode]    UNIQUEIDENTIFIER NOT NULL,
    [Description] NVARCHAR (120)   NULL,
    CONSTRAINT [PK_TB_Post_Slide] PRIMARY KEY CLUSTERED ([PostSlideID] ASC),
    CONSTRAINT [FK_TB_Post_Slide_TB_Post] FOREIGN KEY ([PostID]) REFERENCES [dbo].[TB_Post] ([PostID])
);

