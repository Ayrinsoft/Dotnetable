CREATE TABLE [dbo].[LoginTries] (
    [LoginTryID] INT          IDENTITY (1, 1) NOT NULL,
    [Username]   VARCHAR (64) NOT NULL,
    [LogTime]    DATETIME     NOT NULL,
    [IsSuccess]  BIT          NOT NULL,
    [TryIP]      VARCHAR (15) NOT NULL,
    [WebsiteID]  INT          CONSTRAINT [DF_LoginTries_WebsiteID] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_LoginTries] PRIMARY KEY CLUSTERED ([LoginTryID] ASC),
    CONSTRAINT [FK_LoginTries_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

