IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Clients] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [Email] nvarchar(100) NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
);

CREATE TABLE [Services] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Price] decimal(18,2) NOT NULL,
    [DurationMinutes] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Services] PRIMARY KEY ([Id])
);

CREATE TABLE [Appointments] (
    [Id] int NOT NULL IDENTITY,
    [DateStart] datetime2 NOT NULL,
    [DateEnd] datetime2 NOT NULL,
    [Notes] nvarchar(max) NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ClientId] int NOT NULL,
    [ServiceId] int NOT NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Appointments_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Appointments_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Appointments_ClientId] ON [Appointments] ([ClientId]);

CREATE INDEX [IX_Appointments_DateStart] ON [Appointments] ([DateStart]);

CREATE INDEX [IX_Appointments_ServiceId] ON [Appointments] ([ServiceId]);

CREATE INDEX [IX_Services_IsActive] ON [Services] ([IsActive]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260211140316_InitialCreate', N'9.0.12');

CREATE TABLE [Reviews] (
    [Id] int NOT NULL IDENTITY,
    [ClientId] int NOT NULL,
    [Rating] int NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    [IsApproved] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Reviews] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Reviews_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Reviews_ClientId] ON [Reviews] ([ClientId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260217130431_AddReviews', N'9.0.12');

ALTER TABLE [Clients] ADD [Password] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260226013135_AddPasswordToClient', N'9.0.12');

ALTER TABLE [Reviews] DROP CONSTRAINT [FK_Reviews_Clients_ClientId];

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Reviews]') AND [c].[name] = N'Text');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Reviews] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Reviews] ALTER COLUMN [Text] nvarchar(1000) NOT NULL;

ALTER TABLE [Reviews] ADD CONSTRAINT [FK_Reviews_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260310044410_FixPriceDecimal', N'9.0.12');

COMMIT;
GO

