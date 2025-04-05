CREATE TABLE [dbo].[TB_Member_Contact] (
    [MemberContactID] INT            IDENTITY (1, 1) NOT NULL,
    [MemberID]        INT            NOT NULL,
    [Address]         NVARCHAR (800) NULL,
    [LanguageCode]    CHAR (2)       NOT NULL,
    [PhoneNumber]     VARCHAR (16)   NULL,
    [CellphoneNumber] VARCHAR (14)   NULL,
    [HomeOwnerName]   NVARCHAR (70)  NULL,
    [PointLatitude]   VARCHAR (16)   NULL,
    [PointLongitude]  VARCHAR (16)   NULL,
    [Active]          BIT            NOT NULL,
    [DefaultContact]  BIT            NOT NULL,
    [PostalCode]      VARCHAR (10)   NULL,
    [CityID]          INT            NOT NULL,
    CONSTRAINT [PK_TB_Member_Contact] PRIMARY KEY CLUSTERED ([MemberContactID] ASC),
    CONSTRAINT [FK_TB_Member_Contact_TB_City] FOREIGN KEY ([CityID]) REFERENCES [dbo].[TB_City] ([CityID]),
    CONSTRAINT [FK_TB_Member_Contact_TB_Member] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[TB_Member] ([MemberID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Member_Contact_MemberID]
    ON [dbo].[TB_Member_Contact]([MemberID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Member_Contact_CityID]
    ON [dbo].[TB_Member_Contact]([CityID] ASC);

