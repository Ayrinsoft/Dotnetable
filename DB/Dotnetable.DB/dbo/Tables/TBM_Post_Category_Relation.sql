CREATE TABLE [dbo].[TBM_Post_Category_Relation] (
    [PostCategoryRelationID] INT IDENTITY (1, 1) NOT NULL,
    [PostID]                 INT NOT NULL,
    [PostCategoryID]         INT NOT NULL,
    [ShowInList]             BIT NOT NULL,
    CONSTRAINT [PK_TBM_Post_Category_Relation] PRIMARY KEY CLUSTERED ([PostCategoryRelationID] ASC),
    CONSTRAINT [FK_TBM_Post_Category_Relation_TB_Post] FOREIGN KEY ([PostID]) REFERENCES [dbo].[TB_Post] ([PostID]),
    CONSTRAINT [FK_TBM_Post_Category_Relation_TB_Post_Category] FOREIGN KEY ([PostCategoryID]) REFERENCES [dbo].[TB_Post_Category] ([PostCategoryID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TBM_Post_Category_Relation_PostID]
    ON [dbo].[TBM_Post_Category_Relation]([PostID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TBM_Post_Category_Relation_PostCategoryID]
    ON [dbo].[TBM_Post_Category_Relation]([PostCategoryID] ASC);

