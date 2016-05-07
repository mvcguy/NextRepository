IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'NextDataLayer')
	BEGIN
		CREATE DATABASE [NextDataLayer]
	END

