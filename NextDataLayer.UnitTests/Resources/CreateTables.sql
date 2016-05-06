USE [NextDataLayer]

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Products' AND xtype='U')
	BEGIN

		CREATE TABLE Products (
			[Id] int IDENTITY(1,1) NOT NULL,
			[Name] nvarchar(180) NULL,
			[Description] nvarchar(250) NULL,
			CONSTRAINT [PK_Products_ID] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] 
		
	END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProductsLog' AND xtype='U')
	BEGIN

		CREATE TABLE ProductsLog (
			[Id] int IDENTITY(1,1) NOT NULL,
			[Message] nvarchar(MAX) NULL,
			[LastUpdated] datetime NULL,
			CONSTRAINT [PK_ProductsLog_ID] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] 
		
	END


if exists ( select * from sys.objects where name='GetProducts' and objectproperty(object_id,'IsProcedure')=1 )
exec('drop proc GetProducts')

exec('create proc GetProducts @Name nvarchar(180)
as begin
  SELECT * FROM PRODUCTS where Name like @Name
end')