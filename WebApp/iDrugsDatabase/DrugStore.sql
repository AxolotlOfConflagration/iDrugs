﻿CREATE TABLE [dbo].[DrugStore]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[City] NVARCHAR(50) NOT NULL,
	[PostalCode] NCHAR(6) NOT NULL,
	[StreetName] NVARCHAR(50) NOT NULL,
	[StreetNumber] NVARCHAR(10) NOT NULL,
	[FlatNumber] NVARCHAR(10)
)
