	
	DROP DATABASE IF EXISTS NextDatalayer;

	CREATE DATABASE NextDataLayer; 

	USE NextDataLayer;

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
	('Galaxy S6','3GB RAM 32 Internal Storage'),
		('Lenovo L430','Pentium I5 960GB SSD'),
		('Logitech Headset','wireless, USB recharege');




CREATE PROCEDURE GetProducts (IN PName nvarchar(180))
BEGIN
	SELECT * FROM products  where products.Name like PName;
END 
