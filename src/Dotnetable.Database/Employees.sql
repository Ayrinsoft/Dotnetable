CREATE TABLE [dbo].[Employees] (
    [EmployeeID]      INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT            NOT NULL,
    [EmployeeCode]    NVARCHAR (30)  NOT NULL,
    [GivenName]       NVARCHAR (100) NOT NULL,
    [Surname]         NVARCHAR (100) NOT NULL,
    [NationalId]      NVARCHAR (MAX) NULL,
    [Email]           NVARCHAR (MAX) NULL,
    [Phone]           NVARCHAR (MAX) NULL,
    [OrgUnitID]       INT            NULL,
    [JobTitle]        NVARCHAR (MAX) NULL,
    [MemberID]        INT            NULL,
    [Status]          TINYINT        NOT NULL,
    [HireDate]        DATE           NOT NULL,
    [TerminationDate] DATE           NULL,
    [CreatedAt]       DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED ([EmployeeID] ASC),
    CONSTRAINT [FK_Employees_Members_MemberID] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_Employees_OrgUnits_OrgUnitID] FOREIGN KEY ([OrgUnitID]) REFERENCES [dbo].[OrgUnits] ([OrgUnitID]),
    CONSTRAINT [FK_Employees_Websites_WebsiteID] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Employees_Website_Code] ON [dbo].[Employees] ([WebsiteID] ASC, [EmployeeCode] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_Employees_OrgUnitID]
    ON [dbo].[Employees]([OrgUnitID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Employees_MemberID]
    ON [dbo].[Employees]([MemberID] ASC);

