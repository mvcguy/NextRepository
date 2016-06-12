	
	DROP DATABASE IF EXISTS NextDatalayerWeb;

	CREATE DATABASE NextDatalayerWeb; 

	USE NextDatalayerWeb;

	CREATE TABLE  Products (
			Id int(11)   NOT NULL auto_increment,
			Name varchar(180) NULL,
			Description varchar(250) NULL,
			PRIMARY KEY (Id)
	);
		


	CREATE TABLE  ProductsLog (
			Id int(11)  NOT NULL auto_increment,
			Message varchar(500) NULL,
			LastUpdated datetime NULL,
			PRIMARY KEY (Id)
	);

	INSERT INTO Products (Name,Description) values	
	('S7','3GB RAM 128GB Internal Storage'),
	('T430 Lenovo','Pentium I5 960GB SSD'),
	('Keyboard','wireless, USB recharege'),
	('Milk','1 Litre pack'), 
	('Apples','6 stick in pack'),
	('Motorcycle','Kawasaki KW6679');


	INSERT INTO ProductsLog (Message,LastUpdated) values
	('Test-Message1',now()),
	('Test-Message2',now());


CREATE PROCEDURE GetProducts (IN PName nvarchar(180))
BEGIN
	SELECT * FROM products  where products.Name like PName;
END 
