CREATE TABLE [dbo].[TB_SlideShow] (
    [SlideShowID]  INT              IDENTITY (1, 1) NOT NULL,
    [FileCode]     UNIQUEIDENTIFIER NOT NULL,
    [Title]        NVARCHAR (64)    NOT NULL,
    [SettingArray] NVARCHAR (MAX)   NOT NULL,
    [Active]       BIT              NOT NULL,
    [Priority]     TINYINT          NOT NULL,
    [PageCode]     VARCHAR (16)     NOT NULL,
    [LanguageCode] CHAR (2)         NULL,
    CONSTRAINT [PK_TB_SlideShow] PRIMARY KEY CLUSTERED ([SlideShowID] ASC)
);

