CREATE TABLE [dbo].[TB_Setting] (
    [SettingID]    INT             IDENTITY (1, 1) NOT NULL,
    [SettingKey]   VARCHAR (64)    NOT NULL,
    [SettingValue] NVARCHAR (4000) NOT NULL,
    CONSTRAINT [PK_TB_Setting] PRIMARY KEY CLUSTERED ([SettingID] ASC)
);

