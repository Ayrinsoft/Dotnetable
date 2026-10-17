CREATE TABLE [dbo].[QuizAttempts] (
    [QuizAttemptID]        INT              IDENTITY (1, 1) NOT NULL,
    [QuizID]               INT              NOT NULL,
    [WebsiteID]            INT              NOT NULL,
    [WebsiteClientID]      INT              NULL,
    [PublicToken]          UNIQUEIDENTIFIER NOT NULL,
    [StartedAt]            DATETIME         NOT NULL,
    [ExpiresAt]            DATETIME         NOT NULL,
    [SubmittedAt]          DATETIME         NULL,
    [Status]               TINYINT          NOT NULL,
    [Score]                DECIMAL (18, 4)  NOT NULL,
    [MaxScore]             DECIMAL (18, 4)  NOT NULL,
    [Percent]              DECIMAL (5, 2)   NOT NULL,
    [BandTitle]            NVARCHAR (200)   NULL,
    [BandBody]             NVARCHAR (MAX)   NULL,
    [WantsPrivateReview]   BIT              NOT NULL,
    [ContactName]          NVARCHAR (120)   NULL,
    [Phone]                NVARCHAR (30)    NULL,
    [Email]                NVARCHAR (200)   NULL,
    [PrivateReviewStatus]  TINYINT          NOT NULL,
    [PrivateReviewFee]     DECIMAL (18, 4)  NOT NULL,
    [CurrencyCode]         NVARCHAR (3)     NULL,
    [PaymentID]            INT              NULL,
    [PaymentGatewayID]     INT              NULL,
    [GatewayAuthority]     NVARCHAR (200)   NULL,
    [GatewayRef]           NVARCHAR (100)   NULL,
    [PaidAt]               DATETIME         NULL,
    [PrivateAnswer]        NVARCHAR (MAX)   NULL,
    [PrivateAnswerAt]      DATETIME         NULL,
    [LanguageCode]         CHAR (2)         NULL,
    CONSTRAINT [PK_QuizAttempts] PRIMARY KEY CLUSTERED ([QuizAttemptID] ASC),
    CONSTRAINT [FK_QuizAttempts_Quizzes] FOREIGN KEY ([QuizID]) REFERENCES [dbo].[Quizzes] ([QuizID]),
    CONSTRAINT [FK_QuizAttempts_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_QuizAttempts_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]),
    CONSTRAINT [FK_QuizAttempts_Payments] FOREIGN KEY ([PaymentID]) REFERENCES [dbo].[Payments] ([PaymentID]),
    CONSTRAINT [FK_QuizAttempts_PaymentGateways] FOREIGN KEY ([PaymentGatewayID]) REFERENCES [dbo].[PaymentGateways] ([PaymentGatewayID])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_QuizAttempts_PublicToken]
    ON [dbo].[QuizAttempts] ([PublicToken] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_QuizAttempts_QuizID]
    ON [dbo].[QuizAttempts] ([QuizID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_QuizAttempts_GatewayAuthority]
    ON [dbo].[QuizAttempts] ([GatewayAuthority] ASC);
