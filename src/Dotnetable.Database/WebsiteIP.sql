CREATE TABLE [dbo].[WebsiteIP] (
    [WebsiteIPID] INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]   INT           NOT NULL,
    [StartIP]     VARCHAR (45)  NOT NULL,
    [EndIP]       VARCHAR (45)  NULL,
    [CidrPrefix]  INT           NULL,
    [Label]       NVARCHAR (30) NOT NULL,
    [Active]      BIT           NOT NULL,
    CONSTRAINT [PK_WebsiteIP] PRIMARY KEY CLUSTERED ([WebsiteIPID] ASC),
    CONSTRAINT [FK_WebsiteIP_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

