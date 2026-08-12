CREATE TABLE [dbo].[PayrollLines] (
    [PayrollLineID]       INT             IDENTITY (1, 1) NOT NULL,
    [PayrollRunID]        INT             NOT NULL,
    [EmployeeID]          INT             NOT NULL,
    [Gross]               DECIMAL (18, 4) NOT NULL,
    [EmployeeInsurance]   DECIMAL (18, 4) NOT NULL,
    [EmployerInsurance]   DECIMAL (18, 4) NOT NULL,
    [IncomeTax]           DECIMAL (18, 4) NOT NULL,
    [Net]                 DECIMAL (18, 4) NOT NULL,
    [EmployerCost]        DECIMAL (18, 4) NOT NULL,
    [Note]                NVARCHAR (300)  NULL,
    CONSTRAINT [PK_PayrollLines] PRIMARY KEY CLUSTERED ([PayrollLineID] ASC),
    CONSTRAINT [FK_PayrollLines_Runs] FOREIGN KEY ([PayrollRunID]) REFERENCES [dbo].[PayrollRuns] ([PayrollRunID]),
    CONSTRAINT [FK_PayrollLines_Employees] FOREIGN KEY ([EmployeeID]) REFERENCES [dbo].[Employees] ([EmployeeID])
);
GO
