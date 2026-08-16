CREATE TABLE [dbo].[PayrollLines] (
    [PayrollLineID]     INT             IDENTITY (1, 1) NOT NULL,
    [PayrollRunID]      INT             NOT NULL,
    [EmployeeID]        INT             NOT NULL,
    [Gross]             DECIMAL (18, 4) NOT NULL,
    [EmployeeInsurance] DECIMAL (18, 4) NOT NULL,
    [EmployerInsurance] DECIMAL (18, 4) NOT NULL,
    [IncomeTax]         DECIMAL (18, 4) NOT NULL,
    [Net]               DECIMAL (18, 4) NOT NULL,
    [EmployerCost]      DECIMAL (18, 4) NOT NULL,
    [Note]              NVARCHAR (MAX)  NULL,
    CONSTRAINT [PK_PayrollLines] PRIMARY KEY CLUSTERED ([PayrollLineID] ASC),
    CONSTRAINT [FK_PayrollLines_Employees_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [dbo].[Employees] ([EmployeeID]),
    CONSTRAINT [FK_PayrollLines_PayrollRuns_PayrollRunID] FOREIGN KEY ([PayrollRunID]) REFERENCES [dbo].[PayrollRuns] ([PayrollRunID])
);


GO
CREATE NONCLUSTERED INDEX [IX_PayrollLines_PayrollRunID]
    ON [dbo].[PayrollLines]([PayrollRunID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PayrollLines_EmployeeID]
    ON [dbo].[PayrollLines]([EmployeeID] ASC);

