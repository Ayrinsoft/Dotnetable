CREATE TABLE [dbo].[TB_Login_Try] (
    [LoginTryID] INT          IDENTITY (1, 1) NOT NULL,
    [Username]   VARCHAR (64) NOT NULL,
    [LogTime]    DATETIME     NOT NULL,
    [IsSuccess]  BIT          NOT NULL,
    [TryIP]      VARCHAR (15) NOT NULL,
    CONSTRAINT [PK_TB_Login_Try] PRIMARY KEY CLUSTERED ([LoginTryID] ASC)
);

