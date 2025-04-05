CREATE TABLE [dbo].[TB_Post_Category] (
    [PostCategoryID]  INT              IDENTITY (1, 1) NOT NULL,
    [ParentID]        INT              NULL,
    [MenuView]        BIT              NOT NULL,
    [Title]           NVARCHAR (128)   NOT NULL,
    [Tags]            NVARCHAR (2000)  NULL,
    [MetaKeywords]    NVARCHAR (256)   NULL,
    [MetaDescription] NVARCHAR (256)   NULL,
    [Priority]        SMALLINT         NOT NULL,
    [FooterView]      BIT              NOT NULL,
    [Active]          BIT              NOT NULL,
    [Description]     NVARCHAR (512)   NOT NULL,
    [FileCode]        UNIQUEIDENTIFIER NULL,
    [LanguageCode]    CHAR (2)         NULL,
    CONSTRAINT [PK_TB_Post_Category] PRIMARY KEY CLUSTERED ([PostCategoryID] ASC)
);

