CREATE TABLE [dbo].[Members] (
    [MemberID]        INT              IDENTITY (1, 1) NOT NULL,
    [Active]          BIT              NOT NULL,
    [Username]        VARCHAR (64)     NOT NULL,
    [Password]        VARCHAR (256)    NOT NULL,
    [Email]           VARCHAR (64)     NOT NULL,
    [CellphoneNumber] VARCHAR (12)     NOT NULL,
    [CountryCode]     VARCHAR (3)      NOT NULL,
    [RegisterDate]    DATE             NOT NULL,
    [Givenname]       NVARCHAR (64)    NOT NULL,
    [Surname]         NVARCHAR (64)    NOT NULL,
    [AvatarID]        INT              NULL,
    [HashKey]         UNIQUEIDENTIFIER NOT NULL,
    [PolicyID]        INT              NOT NULL,
    [Gender]          BIT              NULL,
    [WebsiteID]       INT              NOT NULL,
    [AdminUIMode]     TINYINT          CONSTRAINT [DF_Members_AdminUIMode] DEFAULT ((1)) NOT NULL,
    [IsSiteAdmin]     BIT              CONSTRAINT [DF_Members_IsSiteAdmin] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Members] PRIMARY KEY CLUSTERED ([MemberID] ASC),
    CONSTRAINT [FK_Members_FileRecords] FOREIGN KEY ([AvatarID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_Members_Policies] FOREIGN KEY ([PolicyID]) REFERENCES [dbo].[Policies] ([PolicyID]),
    CONSTRAINT [FK_Members_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 = Basic (admin surface reduced to what this member''s Website.WebsiteType needs), 1 = General (same admin surface regardless of website type), 2 = Advanced (full surface, e.g. extra-language pages/buttons on an otherwise single-language site)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Members', @level2type = N'COLUMN', @level2name = N'AdminUIMode';
GO

CREATE NONCLUSTERED INDEX [IX_Members_AvatarID]
    ON [dbo].[Members] ([AvatarID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Members_PolicyID]
    ON [dbo].[Members] ([PolicyID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Members_WebsiteID]
    ON [dbo].[Members] ([WebsiteID] ASC);
