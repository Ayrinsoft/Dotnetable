CREATE TABLE [dbo].[LoginTry] (
    [LoginTryID] INT          IDENTITY (1, 1) NOT NULL,
    [Username]   VARCHAR (64) NOT NULL,
    [LogTime]    DATETIME     NOT NULL,
    [IsSuccess]  BIT          NOT NULL,
    [TryIP]      VARCHAR (15) NOT NULL,
    CONSTRAINT [PK_LoginTry] PRIMARY KEY CLUSTERED ([LoginTryID] ASC)
);

