CREATE TABLE [dbo].[Employees] (
    [EmployeeID]       INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT            NOT NULL,
    [EmployeeCode]     NVARCHAR (30)  NOT NULL,
    [GivenName]        NVARCHAR (100) NOT NULL,
    [Surname]          NVARCHAR (100) NOT NULL,
    [NationalId]       NVARCHAR (50)  NULL,
    [Email]            NVARCHAR (200) NULL,
    [Phone]            NVARCHAR (50)  NULL,
    [OrgUnitID]        INT            NULL,
    [JobTitle]         NVARCHAR (150) NULL,
    [MemberID]         INT            NULL,
    [Status]           TINYINT        DEFAULT (CONVERT([tinyint],(1))) NOT NULL,
    [HireDate]         DATE           NOT NULL,
    [TerminationDate]  DATE           NULL,
    [CreatedAt]        DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED ([EmployeeID] ASC),
    CONSTRAINT [FK_Employees_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_Employees_OrgUnits] FOREIGN KEY ([OrgUnitID]) REFERENCES [dbo].[OrgUnits] ([OrgUnitID]),
    CONSTRAINT [FK_Employees_Members] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[Members] ([MemberID])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Employees_Website_Code] ON [dbo].[Employees] ([WebsiteID] ASC, [EmployeeCode] ASC);
GO
