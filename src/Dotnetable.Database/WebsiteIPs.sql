CREATE TABLE [dbo].[WebsiteIPs] (
    [WebsiteIPID] INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]   INT           NOT NULL,
    [StartIP]     VARCHAR (45)  NOT NULL,
    [EndIP]       VARCHAR (45)  NULL,
    [CidrPrefix]  INT           NULL,
    [Label]       NVARCHAR (30) NOT NULL,
    [Active]      BIT           NOT NULL,
    CONSTRAINT [PK_WebsiteIPs] PRIMARY KEY CLUSTERED ([WebsiteIPID] ASC),
    CONSTRAINT [FK_WebsiteIPs_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteIPs_WebsiteID]
    ON [dbo].[WebsiteIPs] ([WebsiteID] ASC);
