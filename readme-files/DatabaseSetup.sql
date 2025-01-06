


-- Please check the 'README.md' file on how to use this setup.



PRINT 'Starting the SQL setup for:';
PRINT 'DataImportClient (C) made in Austria';
PRINT '';

GO



IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'DataImportClient')
    BEGIN

        CREATE DATABASE DataImportClient;
        PRINT 'Successfully created a new database named "DataImportClient".';

    END
ELSE
    BEGIN

        PRINT 'Skipping the creation of the database "DataImportClient" as it already exists.';

    END

GO



USE DataImportClient;

GO



IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Weather' AND xtype = 'U')
    BEGIN

        CREATE TABLE Weather (
            weather_id INT IDENTITY(1,1) PRIMARY KEY,
            weather_date DATE DEFAULT CAST(GETDATE() AS DATE),
            weather_time TIME(3) DEFAULT CAST(GETDATE() AS TIME(3)),
			weather_type NVARCHAR(100) NULL,
            longitude DECIMAL(9, 6),
            latitude DECIMAL(9, 6),
            sunrise_unix_seconds BIGINT NULL,
            sunset_unix_seconds BIGINT NULL,
            humidity DECIMAL(8, 2),
            windspeed DECIMAL(8, 2),
            temperature DECIMAL(8, 2),
        );

        PRINT 'Successfully created a new table named "Weather".';

    END
ELSE
    BEGIN

        PRINT 'Skipping the creation of the table "Weather" as it already exists. Please check if the table contains all needed columns.';

    END

GO



IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'ElectricityPower' AND xtype = 'U')
    BEGIN

        CREATE TABLE ElectricityPower (
            power_id INT IDENTITY(1,1) PRIMARY KEY,
			power_date DATE DEFAULT CAST(GETDATE() AS DATE),
			power_time TIME(3) DEFAULT CAST(GETDATE() AS TIME(3)),
			keller_L1 DECIMAL(14, 2),
			keller_L2 DECIMAL(14, 2),
			keller_L3 DECIMAL(14, 2),
			theorie_L1 DECIMAL(14, 2),
			theorie_L2 DECIMAL(14, 2),
			theorie_L3 DECIMAL(14, 2),
			tischlerei_L1 DECIMAL(14, 2),
			tischlerei_L2 DECIMAL(14, 2),
			tischlerei_L3 DECIMAL(14, 2),
			einspeisung_L1 DECIMAL(14, 2),
			einspeisung_L2 DECIMAL(14, 2),
			einspeisung_L3 DECIMAL(14, 2),
			flur_zimmerei_L1 DECIMAL(14, 2),
			flur_zimmerei_L2 DECIMAL(14, 2),
			flur_zimmerei_L3 DECIMAL(14, 2),
			flur_tischlerei_L1 DECIMAL(14, 2),
			flur_tischlerei_L2 DECIMAL(14, 2),
			flur_tischlerei_L3 DECIMAL(14, 2),
			absaugung_steinmetz_L1 DECIMAL(14, 2),
			absaugung_steinmetz_L2 DECIMAL(14, 2),
			absaugung_steinmetz_L3 DECIMAL(14, 2),
			absaugung_tischlerei_L1 DECIMAL(14, 2),
			absaugung_tischlerei_L2 DECIMAL(14, 2),
			absaugung_tischlerei_L3 DECIMAL(14, 2),
			werkstatterweiterung_L1 DECIMAL(14, 2),
			werkstatterweiterung_L2 DECIMAL(14, 2),
			werkstatterweiterung_L3 DECIMAL(14, 2),
        );

        PRINT 'Successfully created a new table named "ElectricityPower".';

    END
ELSE
    BEGIN

        PRINT 'Skipping the creation of the table "ElectricityPower" as it already exists. Please check if the table contains all needed columns.';

    END

GO



IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'ElectricityPowerfactor' AND xtype = 'U')
    BEGIN
	
        CREATE TABLE ElectricityPowerfactor (
            powerfactor_id INT IDENTITY(1,1) PRIMARY KEY,
			powerfactor_date DATE DEFAULT CAST(GETDATE() AS DATE),
			powerfactor_time TIME(3) DEFAULT CAST(GETDATE() AS TIME(3)),
			power_id INT NOT NULL, 
			FOREIGN KEY (power_id) REFERENCES ElectricityPower(power_id),
			keller_L1 DECIMAL(3, 2),
			keller_L2 DECIMAL(3, 2),
			keller_L3 DECIMAL(3, 2),
			theorie_L1 DECIMAL(3, 2),
			theorie_L2 DECIMAL(3, 2),
			theorie_L3 DECIMAL(3, 2),
			tischlerei_L1 DECIMAL(3, 2),
			tischlerei_L2 DECIMAL(3, 2),
			tischlerei_L3 DECIMAL(3, 2),
			einspeisung_L1 DECIMAL(3, 2),
			einspeisung_L2 DECIMAL(3, 2),
			einspeisung_L3 DECIMAL(3, 2),
			flur_zimmerei_L1 DECIMAL(3, 2),
			flur_zimmerei_L2 DECIMAL(3, 2),
			flur_zimmerei_L3 DECIMAL(3, 2),
			flur_tischlerei_L1 DECIMAL(3, 2),
			flur_tischlerei_L2 DECIMAL(3, 2),
			flur_tischlerei_L3 DECIMAL(3, 2),
			absaugung_steinmetz_L1 DECIMAL(3, 2),
			absaugung_steinmetz_L2 DECIMAL(3, 2),
			absaugung_steinmetz_L3 DECIMAL(3, 2),
			absaugung_tischlerei_L1 DECIMAL(3, 2),
			absaugung_tischlerei_L2 DECIMAL(3, 2),
			absaugung_tischlerei_L3 DECIMAL(3, 2),
			werkstatterweiterung_L1 DECIMAL(3, 2),
			werkstatterweiterung_L2 DECIMAL(3, 2),
			werkstatterweiterung_L3 DECIMAL(3, 2),
        );

        PRINT 'Successfully created a new table named "ElectricityPowerfactor".';

    END
ELSE
    BEGIN

        PRINT 'Skipping the creation of the table "ElectricityPowerfactor" as it already exists. Please check if the table contains all needed columns.';

    END

GO



-- TODO: Create "DistrictHeat" table

-- TODO: Create "Photovoltaic" table



PRINT '';
PRINT 'Finished the SQL setup.';