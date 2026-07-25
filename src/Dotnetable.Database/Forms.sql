CREATE TABLE [dbo].[Forms] (
    [FormID]                   INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]                INT            NOT NULL,
    [Title]                    NVARCHAR (200) NOT NULL,
    [Slug]                     NVARCHAR (200) NOT NULL,
    [Description]              NVARCHAR (1000) NULL,
    [FormType]                 TINYINT NOT NULL,
    [SubmitButtonText]         NVARCHAR (100) NULL,
    [SuccessMessage]           NVARCHAR (500) NULL,
    [RequireLogin]             BIT NOT NULL,
    [AllowMultipleSubmissions] BIT NOT NULL,
    [ShowResults]              BIT NOT NULL,
    [NotifyEmail]              NVARCHAR (200) NULL,
    [StartAt]                  DATETIME       NULL,
    [EndAt]                    DATETIME       NULL,
    [IsActive]                 BIT NOT NULL,
    [CreatedAt]                DATETIME NOT NULL,
    CONSTRAINT [PK_Forms] PRIMARY KEY CLUSTERED ([FormID] ASC),
    CONSTRAINT [FK_Forms_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_Forms_WebsiteID]
    ON [dbo].[Forms] ([WebsiteID] ASC);
