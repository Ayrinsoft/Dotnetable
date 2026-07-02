CREATE TABLE [dbo].[WebsiteClient] (
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
    CONSTRAINT [PK_WebsiteClient] PRIMARY KEY CLUSTERED ([WebsiteClientID] ASC),
    CONSTRAINT [FK_WebsiteClient_FileRecord] FOREIGN KEY ([AvatarID]) REFERENCES [dbo].[FileRecord] ([FileRecordID]),
    CONSTRAINT [FK_WebsiteClient_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

