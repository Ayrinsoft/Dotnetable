CREATE TABLE [dbo].[EmployeeContracts] (
    [EmployeeContractID]    INT             IDENTITY (1, 1) NOT NULL,
    [EmployeeID]            INT             NOT NULL,
    [EffectiveFrom]         DATE            NOT NULL,
    [EffectiveTo]           DATE            NULL,
    [BaseSalary]            DECIMAL (18, 4) NOT NULL,
    [CurrencyCode]          CHAR (3)        NOT NULL,
    [PayFrequency]          TINYINT         NOT NULL,
    [EmployeeInsuranceRate] DECIMAL (9, 6)  NOT NULL,
    [EmployerInsuranceRate] DECIMAL (9, 6)  NOT NULL,
    [IncomeTaxRate]         DECIMAL (9, 6)  NOT NULL,
    [UseFlatRates]          BIT             DEFAULT (CONVERT([bit],(1))) NOT NULL,
    [IsActive]              BIT             NOT NULL,
    [CreatedAt]             DATETIME2 (7)   NOT NULL,
    CONSTRAINT [PK_EmployeeContracts] PRIMARY KEY CLUSTERED ([EmployeeContractID] ASC),
    CONSTRAINT [FK_EmployeeContracts_Currencies_CurrencyCode] FOREIGN KEY ([CurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_EmployeeContracts_Employees_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [dbo].[Employees] ([EmployeeID])
);


GO
CREATE NONCLUSTERED INDEX [IX_EmployeeContracts_EmployeeID]
    ON [dbo].[EmployeeContracts]([EmployeeID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_EmployeeContracts_CurrencyCode]
    ON [dbo].[EmployeeContracts]([CurrencyCode] ASC);

