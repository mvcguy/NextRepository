USE [NextDatalayerWeb]

IF  NOT EXISTS (SELECT * from products )
	BEGIN

	INSERT INTO Products ([Name],[Description]) values
		('Galaxy S6','3GB RAM 32 Internal Storage'), 
		('Lenovo L430','Pentium I5 960GB SSD'),
		('Logitech Headset','wireless, USB recharege')

	END

	IF  NOT EXISTS (SELECT * from productslog )
	BEGIN

	INSERT INTO ProductsLog ([Message],[LastUpdated]) values
		('Text-Message1',getdate()), 
		('Text-Message2',getdate()),
		('Text-Message3',getdate())

	END

