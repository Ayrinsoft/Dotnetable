CREATE TABLE [dbo].[WebsiteClients] (
    [WebsiteClientID] INT              IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT              NOT NULL,
    [AvatarID]        INT              NULL,
    [Email]           VARCHAR (60)     NULL,
    [Cellphone]       VARCHAR (16)     NULL,
    [CountryCode]     VARCHAR (3)      NULL,
    [Password]        VARCHAR (256)    NULL,
    [Active]          BIT              NOT NULL,
    [RegisterDate]    DATE             NOT NULL,
    [Gender]          BIT              NULL,
    [Givenname]       NVARCHAR (42)    NULL,
    [Surname]         NVARCHAR (42)    NULL,
    [HashKey]         UNIQUEIDENTIFIER NOT NULL,
    [ClientLevel]     TINYINT          NOT NULL,
    CONSTRAINT [PK_WebsiteClients] PRIMARY KEY CLUSTERED ([WebsiteClientID] ASC),
    CONSTRAINT [FK_WebsiteClients_FileRecords] FOREIGN KEY ([AvatarID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_WebsiteClients_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


GO

-- A customer's email / mobile must be unique per website. Filtered so multiple customers who
-- registered with only the other identifier (NULL here) don't collide under SQL Server's
-- "NULLs are equal" unique-index rule.

GO

CREATE NONCLUSTERED INDEX [IX_WebsiteClients_AvatarID]
    ON [dbo].[WebsiteClients] ([AvatarID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteClients_WebsiteID]
    ON [dbo].[WebsiteClients] ([WebsiteID] ASC);
