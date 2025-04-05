CREATE TABLE [dbo].[TB_Member] (
    [MemberID]        INT              IDENTITY (1, 1) NOT NULL,
    [Active]          BIT              NOT NULL,
    [Username]        VARCHAR (64)     NOT NULL,
    [Password]        VARCHAR (256)    NOT NULL,
    [Email]           VARCHAR (64)     NOT NULL,
    [CellphoneNumber] VARCHAR (12)     NOT NULL,
    [CountryCode]     VARCHAR (3)      NOT NULL,
    [Activate]        BIT              NOT NULL,
    [RegisterDate]    DATETIME         NOT NULL,
    [Givenname]       NVARCHAR (64)    NOT NULL,
    [Surname]         NVARCHAR (64)    NOT NULL,
    [AvatarID]        UNIQUEIDENTIFIER NULL,
    [HashKey]         UNIQUEIDENTIFIER NOT NULL,
    [PolicyID]        INT              NOT NULL,
    [Gender]          BIT              NULL,
    [PostalCode]      VARCHAR (10)     NULL,
    [CityID]          INT              NOT NULL,
    CONSTRAINT [PK_TB_Member] PRIMARY KEY CLUSTERED ([MemberID] ASC),
    CONSTRAINT [FK_TB_Member_TB_City] FOREIGN KEY ([CityID]) REFERENCES [dbo].[TB_City] ([CityID]),
    CONSTRAINT [FK_TB_Member_TB_Policy] FOREIGN KEY ([PolicyID]) REFERENCES [dbo].[TB_Policy] ([PolicyID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Member_PolicyID]
    ON [dbo].[TB_Member]([PolicyID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Member_CityID]
    ON [dbo].[TB_Member]([CityID] ASC);

