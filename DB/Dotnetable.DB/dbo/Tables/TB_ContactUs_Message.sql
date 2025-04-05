CREATE TABLE [dbo].[TB_ContactUs_Message] (
    [ContactUsMessagesID] INT             IDENTITY (1, 1) NOT NULL,
    [SenderName]          NVARCHAR (64)   NOT NULL,
    [EmailAddress]        VARCHAR (64)    NOT NULL,
    [CellphoneNumber]     VARCHAR (15)    NOT NULL,
    [MessageSubject]      NVARCHAR (512)  NOT NULL,
    [MessageBody]         NVARCHAR (4000) NOT NULL,
    [Archive]             BIT             NOT NULL,
    [LogTime]             DATETIME        NOT NULL,
    [SenderIPAddress]     VARCHAR (15)    NOT NULL,
    CONSTRAINT [PK_TB_ContactUs_Message] PRIMARY KEY CLUSTERED ([ContactUsMessagesID] ASC)
);

