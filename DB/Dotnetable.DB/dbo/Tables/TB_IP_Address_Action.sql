CREATE TABLE [dbo].[TB_IP_Address_Action] (
    [IPAddressActionID] INT          IDENTITY (1, 1) NOT NULL,
    [TryIP]             VARCHAR (15) NOT NULL,
    [LogTime]           DATETIME     NOT NULL,
    CONSTRAINT [PK_TB_IP_Address_Action] PRIMARY KEY CLUSTERED ([IPAddressActionID] ASC)
);

