CREATE TABLE [dbo].[TB_Email_Type] (
    [EmailTypeID] TINYINT     NOT NULL,
    [Title]       VARCHAR (8) NOT NULL,
    CONSTRAINT [PK_TB_Email_Type] PRIMARY KEY CLUSTERED ([EmailTypeID] ASC)
);

