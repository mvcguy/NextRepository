IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'NextDatalayerWeb')
	BEGIN
		CREATE DATABASE [NextDatalayerWeb]
	END

