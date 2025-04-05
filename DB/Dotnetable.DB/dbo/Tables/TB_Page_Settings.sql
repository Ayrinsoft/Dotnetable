CREATE TABLE [dbo].[TB_Page_Settings] (
    [PageSettingsID] INT             IDENTITY (1, 1) NOT NULL,
    [PagePositionID] TINYINT         NOT NULL,
    [ItemTag]        VARCHAR (32)    NOT NULL,
    [ItemTypeID]     TINYINT         NOT NULL,
    [ItemBody]       NVARCHAR (4000) NOT NULL,
    CONSTRAINT [PK_TB_Page_Settings] PRIMARY KEY CLUSTERED ([PageSettingsID] ASC)
);

