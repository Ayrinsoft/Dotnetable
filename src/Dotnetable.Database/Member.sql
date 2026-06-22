CREATE TABLE [dbo].[Member] (
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
    [AvatarID]        UNIQUEIDENTIFIER NULL,
    [HashKey]         UNIQUEIDENTIFIER NOT NULL,
    [PolicyID]        INT              NOT NULL,
    [Gender]          BIT              NULL,
    [WebsiteID]       INT              NOT NULL,
    CONSTRAINT [PK_Member] PRIMARY KEY CLUSTERED ([MemberID] ASC),
    CONSTRAINT [FK_Member_Policy] FOREIGN KEY ([PolicyID]) REFERENCES [dbo].[Policy] ([PolicyID]),
    CONSTRAINT [FK_Member_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

