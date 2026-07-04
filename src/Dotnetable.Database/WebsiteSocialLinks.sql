CREATE TABLE [dbo].[WebsiteSocialLinks] (
    [WebsiteSocialLinkID] INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]           INT           NOT NULL,
    [SocialType]          TINYINT       NOT NULL,
    [SocialName]          NVARCHAR (64) NULL,
    [SocialIcon]          VARCHAR (64)  NULL,
    [UrlAddress]          NVARCHAR (80) NOT NULL,
    CONSTRAINT [PK_WebsiteSocialLinks] PRIMARY KEY CLUSTERED ([WebsiteSocialLinkID] ASC),
    CONSTRAINT [FK_WebsiteSocialLinks_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

