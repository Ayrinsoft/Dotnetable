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
    CONSTRAINT [PK_Members] PRIMARY KEY CLUSTERED ([MemberID] ASC),
    CONSTRAINT [FK_Members_FileRecords] FOREIGN KEY ([AvatarID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_Members_Policies] FOREIGN KEY ([PolicyID]) REFERENCES [dbo].[Policies] ([PolicyID]),
    CONSTRAINT [FK_Members_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

