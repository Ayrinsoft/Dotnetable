CREATE TABLE [dbo].[TB_Policy] (
    [PolicyID] INT          IDENTITY (1, 1) NOT NULL,
    [Title]    VARCHAR (64) NOT NULL,
    [Active]   BIT          NOT NULL,
    CONSTRAINT [PK_TB_Policy] PRIMARY KEY CLUSTERED ([PolicyID] ASC)
);

