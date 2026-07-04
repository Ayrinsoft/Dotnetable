CREATE TABLE [dbo].[WebsiteSocialLink] (
    [WebsiteSocialLinkID] INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]           INT           NOT NULL,
    [SocialType]          TINYINT       NOT NULL,
    [SocialName]          NVARCHAR (64) NULL,
    [SocialIcon]          VARCHAR (64)  NULL,
    [UrlAddress]          NVARCHAR (80) NOT NULL,
    CONSTRAINT [PK_WebsiteSocialLink] PRIMARY KEY CLUSTERED ([WebsiteSocialLinkID] ASC),
    CONSTRAINT [FK_WebsiteSocialLink_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

