CREATE TABLE [dbo].[LoginTry] (
    [LoginTryID] INT          IDENTITY (1, 1) NOT NULL,
    [Username]   VARCHAR (64) NOT NULL,
    [LogTime]    DATETIME     NOT NULL,
    [IsSuccess]  BIT          NOT NULL,
    [TryIP]      VARCHAR (15) NOT NULL,
    [WebsiteID]  INT          DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_LoginTry] PRIMARY KEY CLUSTERED ([LoginTryID] ASC),
    CONSTRAINT [FK_LoginTry_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

