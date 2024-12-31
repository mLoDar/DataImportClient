


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
            id INT IDENTITY(1,1) PRIMARY KEY,
            importDate DATE DEFAULT CAST(GETDATE() AS DATE),
            importTime TIME(3) DEFAULT CAST(GETDATE() AS TIME(3)),
            longitude DECIMAL(6, 4),
            latitude DECIMAL(6, 4),
            weatherType NVARCHAR(255) NULL,
            sunriseUnixSeconds BIGINT NULL,
            sunsetUnixSeconds BIGINT NULL,
            humidity DECIMAL(8, 2),
            windSpeed DECIMAL(8, 2),
            temperature DECIMAL(8, 2),
        );

        PRINT 'Successfully created a new table named "Weather".';

    END
ELSE
    BEGIN

        PRINT 'Skipping the creation of the table "Weather" as it already exists. Please check if the table contains all needed columns.';

    END

GO