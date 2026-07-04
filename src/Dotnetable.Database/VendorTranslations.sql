CREATE TABLE [dbo].[VendorTranslations] (
    [VendorTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [VendorID]            INT            NOT NULL,
    [LanguageCode]        CHAR (2)       NOT NULL,
    [Name]                NVARCHAR (200) NOT NULL,
    CONSTRAINT [PK_VendorTranslations] PRIMARY KEY CLUSTERED ([VendorTranslationID] ASC),
    CONSTRAINT [FK_VendorTranslations_Vendors] FOREIGN KEY ([VendorID]) REFERENCES [dbo].[Vendors] ([VendorID])
);

