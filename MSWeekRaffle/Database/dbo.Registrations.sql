CREATE TABLE [dbo].[Registrations] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [EmailAddress]   NVARCHAR (200) NOT NULL,
    [ValidationCode] NVARCHAR (200) NOT NULL,
    [IsValidated]    BIT            NULL,
    [CreatedDate]    DATETIME2 (7)  NOT NULL,
    [ActivatedDate]  DATETIME2 (7)  NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

