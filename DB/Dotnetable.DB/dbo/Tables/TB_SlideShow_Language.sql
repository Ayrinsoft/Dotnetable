CREATE TABLE [dbo].[TB_SlideShow_Language] (
    [SlideShowLanguageID] INT            IDENTITY (1, 1) NOT NULL,
    [SlideShowID]         INT            NOT NULL,
    [Title]               NVARCHAR (64)  NOT NULL,
    [LanguageCode]        CHAR (2)       NOT NULL,
    [SettingArray]        NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_TB_SlideShow_Language] PRIMARY KEY CLUSTERED ([SlideShowLanguageID] ASC),
    CONSTRAINT [FK_TB_SlideShow_Language_TB_SlideShow] FOREIGN KEY ([SlideShowID]) REFERENCES [dbo].[TB_SlideShow] ([SlideShowID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_SlideShow_Language_SlideShowID]
    ON [dbo].[TB_SlideShow_Language]([SlideShowID] ASC);

