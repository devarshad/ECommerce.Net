DECLARE @sql NVARCHAR(max)=''

SELECT @sql += ' Drop table ' + QUOTENAME(TABLE_SCHEMA) + '.'+ QUOTENAME(TABLE_NAME) + '; '
FROM   INFORMATION_SCHEMA.TABLES
WHERE  TABLE_TYPE = 'BASE TABLE'

Exec Sp_executesql @sql

GO
Drop table [dbo].[Clients]
GO
drop table [dbo].[AspNetRoles]

GO
drop table [dbo].[AspNetUsers]
Go
drop table [dbo].[ApiResources]
