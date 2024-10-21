/****** Object:  StoredProcedure [dbo].[SP_GetBaseFileTemplate]    Script Date: 10/21/2024 8:48:38 AM ******/
/****** Se encarga de traer los Templates del los archivos para la web ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_GINTERFACE_GetBaseFileTemplate]
AS
BEGIN
    SELECT * FROM i_Template_Files
END
GO


/****** Object:  StoredProcedure [dbo].[SP_GetFileCsv]    Script Date: 10/21/2024 8:51:41 AM ******/
/******  Se encarga de traer los archivos pendientes para subir a block del los archivos para la web ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_GINTERFACE_GetFileCsv]
AS
BEGIN
    SELECT * FROM i_FILE_CSV
END
GO

/****** Object:  StoredProcedure [dbo].[SP_GInterface_GetDocumentType]    Script Date: 10/21/2024 8:54:00 AM ******/
/******  Traer los diferentes tipos de documentos para la configuracion y la manera de como tratarlo ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


 CREATE PROCEDURE [dbo].[SP_GINTERFACE_GetDocumentType]
AS
BEGIN
    
    SELECT  Tipo_Documento,[Numero_Documento] FROM i_DocumentType ;
END;
GO


/****** Object:  StoredProcedure [dbo].[SP_GInterface_DOC_PICKLIST]    Script Date: 10/21/2024 8:56:18 AM ******/
/****** Se encarga de cargar los datos como requierre block con conversion, sin hacer todavia el envio ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_GINTERFACE_DOC_PICKLIST]
    @IdFileCSV INT  -- ID de la tabla i_FILE_CSV que contiene el JSON
AS
BEGIN
    -- Variables para almacenar los datos de i_TEMP_CSV_GLOBAL
    DECLARE @CodigoProducto INT,
            @CantidadCS INT,
            @CantidadEA INT,
            @CantidadZ96 INT,
            @Result INT,
            @JsonData NVARCHAR(MAX), -- Variable para almacenar el JSON
            @Picklist NVARCHAR(MAX), -- Para almacenar el valor de "Picklist No. :"
            @PrintLayout NVARCHAR(MAX), -- Para almacenar el valor de "Print Layout:"
            @DeliveryRepresentative NVARCHAR(MAX), -- Para almacenar el valor de "Delivery Representative:"
            @Ruta NVARCHAR(MAX), -- Para almacenar el valor de "RUTA"
            @Product NVARCHAR(MAX), -- Para almacenar el valor de "Product"
            @Pl2407034024 NVARCHAR(MAX); -- Para almacenar el valor de "PL2407034024"

    -- Crear una tabla temporal para almacenar los resultados
    CREATE TABLE #Resultados (
        CodigoProducto INT,
        CantidadCS INT,
        CantidadZ96 INT,
        CantidadEA INT,
        Resultado INT,
        Picklist NVARCHAR(MAX),
        PrintLayout NVARCHAR(MAX),
        DeliveryRepresentative NVARCHAR(MAX)
    );

    -- Obtener el JSON de la tabla i_FILE_CSV (ajusta según tu esquema)
    SELECT @JsonData = f.FileJsonObj 
    FROM i_FILE_CSV as f
    WHERE ID = @IdFileCSV; 

    -- Extraer DataRequireFields del JSON
    DECLARE @DataRequireFields NVARCHAR(MAX);
    
    SELECT @DataRequireFields = STRING_AGG(value, ', ') 
    FROM OPENJSON(@JsonData, '$.DataRequireFields');

    -- Asignar valores a las variables
    -- Suponiendo que el orden es: RUTA, Product, PL2407034024
    SELECT 
        @Ruta = PARSENAME(REPLACE(@DataRequireFields, ', ', '.'), 3),
        @Product = PARSENAME(REPLACE(@DataRequireFields, ', ', '.'), 2),
        @Pl2407034024 = PARSENAME(REPLACE(@DataRequireFields, ', ', '.'), 1);

    -- Asignar los datos de DataRequireFields a RequireFields
    SET @Picklist = @Pl2407034024; -- Asignar PL2407034024 a Picklist No.
    SET @PrintLayout = @Product; -- Asignar Product a Print Layout
    SET @DeliveryRepresentative = @Ruta; -- Asignar RUTA a Delivery Representative

    -- Declarar un cursor para iterar sobre los registros de i_TEMP_CSV_GLOBAL
    DECLARE cur CURSOR FOR
    SELECT 
        Campo2,  -- CodigoProducto
        Campo3,  -- CantidadCS
        Campo4,  -- CantidadZ96
        Campo5   -- CantidadEA
    FROM i_TEMP_CSV_GLOBAL AS temp
    WHERE temp.ID_FILE_CSV = @IdFileCSV;

    -- Abrir el cursor
    OPEN cur;

    -- Obtener el primer registro
    FETCH NEXT FROM cur INTO @CodigoProducto, @CantidadCS, @CantidadZ96, @CantidadEA;

    -- Iterar sobre los registros
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Llamar a la función y almacenar el resultado
        SET @Result = [dbo].[fn_GInterface_Get_EA](@CodigoProducto, @CantidadCS, @CantidadZ96, @CantidadEA);

        -- Insertar el resultado en la tabla temporal
        INSERT INTO #Resultados (CodigoProducto, CantidadCS, CantidadZ96, CantidadEA, Resultado, Picklist, PrintLayout, DeliveryRepresentative)
        VALUES (@CodigoProducto, @CantidadCS, @CantidadZ96, @CantidadEA, @Result, @Picklist, @PrintLayout, @DeliveryRepresentative);

        -- Obtener el siguiente registro
        FETCH NEXT FROM cur INTO @CodigoProducto, @CantidadCS, @CantidadZ96, @CantidadEA;
    END;

    -- Cerrar y liberar el cursor
    CLOSE cur;
    DEALLOCATE cur;

    -- Devolver los resultados
    SELECT * FROM #Resultados;

    -- Eliminar la tabla temporal
    DROP TABLE #Resultados;
END;
GO







SET QUOTED_IDENTIFIER ON
GO


/****** Object:  StoredProcedure [dbo].[SP_GInterface_INSERT_FILE_CSV]    Script Date: 10/21/2024 8:58:17 AM ******/
/****** Este Sp se utiliza para guardar los docs Tanto reporte como data en las tablas i_FILE_CSV y i_TEMP_CSV_GLOBAL******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE  PROCEDURE [dbo].[SP_GINTERFACE_INSERT_FILE_CSV]
	@FileNames NVARCHAR(MAX),
    @FileStatus INT,
    @FileFields INT,
	@Inbound nvarchar (30),
    @FileJsonObj NVARCHAR(MAX),
	----Temp
	@CsvData [dbo].[TempGlobalType] READONLY ,
	-- Test control.  Rolls back the transaction if in test mode.  1 to test; 0 to persist changes.
	@testMode bit = 1, -- IMPORTANT live this value in 1 for testing, set it to 0 to apply the changes,
	@MSG NVARCHAR(MAX) OUTPUT,
	@Status BIT OUTPUT
 
AS
BEGIN
--Variables a 

DECLARE @RECORD_EXIST AS BIT = 'false'
DECLARE @FILE_CSV_ID AS INT = 0
DECLARE @TEMP_CSV_GLOBAL_ID AS INT = 0

  
PRINT '--Validar si eiste un dato.'
IF EXISTS
(
	SELECT * FROM [dbo].[i_FILE_CSV] WHERE[FileNames]=@FileNames
)
BEGIN
	SET @MSG='Ya existe ese CSV en la DB...!'
	SET @testMode = 1;
    RAISERROR(@MSG, 16, 1);
            	
	SELECT @MSG [Error Msg]
	
	SET @RECORD_EXIST='true'
END;

IF @RECORD_EXIST = 0
BEGIN
	GOTO PROCESS_RUN
END
ELSE
BEGIN
	GOTO PROCESS_STOP
END


PROCESS_RUN:
	PRINT 'Proceso iniciado'

	BEGIN TRAN T1
	BEGIN TRY
		PRINT '------------'
		
		--LOGIC INSERT	
		-- Insertar el nuevo registro
		INSERT INTO i_FILE_CSV (FileNames, FileDate, FileStatus, FileFields,FileInbound, FileJsonObj)
			VALUES (@FileNames, GETDATE(), @FileStatus, @FileFields,@Inbound, @FileJsonObj);

		--Obtener el ID de FILE_CSV, supones que el valor del WHERE es UNICO (Solo existe un Registro)
		SELECT @FILE_CSV_ID = id
			FROM i_FILE_CSV
			WHERE FileNames	= @FileNames


		INSERT INTO i_TEMP_CSV_GLOBAL(
		    Campo1, Campo2, Campo3, Campo4, Campo5, Campo6, Campo7, Campo8, Campo9, Campo10, 
			Campo11, Campo12, Campo13, Campo14, Campo15, 
			Campo16, Campo17, Campo18, Campo19, Campo20, Campo21, Campo22, Campo23, Campo24, Campo25, 
			Campo26, Campo27, Campo28, Campo29, Campo30, Campo31, Campo32, Campo33, Campo34, Campo35, 
			Campo36, Campo37, Campo38, Campo39, Campo40, Campo41, Campo42, Campo43, Campo44, Campo45, 
			ID_FILE_CSV
		)
			SELECT    Campo1, Campo2, Campo3, Campo4, Campo5, Campo6, Campo7, Campo8, Campo9, Campo10, 
					  Campo11, Campo12, Campo13, Campo14, Campo15, 
					  Campo16, Campo17, Campo18, Campo19, Campo20, Campo21, Campo22, Campo23, Campo24, Campo25, 
                      Campo26, Campo27, Campo28, Campo29, Campo30, Campo31, Campo32, Campo33, Campo34, Campo35, 
                      Campo36, Campo37, Campo38, Campo39, Campo40, Campo41, Campo42, Campo43, Campo44, Campo45, 
                      @FILE_CSV_ID 

				FROM @CsvData;
				
		
		SELECT @TEMP_CSV_GLOBAL_ID = MAX(id)
			FROM i_TEMP_CSV_GLOBAL

			/*
		INSERT INTO [dbo].[i_CROSS_FILE_CSV] ([ID_File_CSV], [ID_Temp_CSV])
			SELECT ID, @FILE_CSV_ID FROM i_TEMP_CSV_GLOBAL
				WHERE 
			VALUES (@FILE_CSV_ID, @TEMP_CSV_GLOBAL_ID);	
			*/

	 IF (@testMode = 1)
       BEGIN
              ROLLBACK TRAN T1;
              PRINT N'------- TEST MODE ON ------- Changes rolled back.';
			  SET @MSG = 'Testing Mode ON - No se Cargaron Datos por ser pruebas';
			  SET @Status = 0; 
			  -- Solo ejecuta DBCC CHECKIDENT poara revertir el INDENTITY Value al valor anterior
			  SELECT @FILE_CSV_ID = MAX(ID) FROM i_FILE_CSV
			  DBCC CHECKIDENT (i_FILE_CSV, RESEED, @FILE_CSV_ID);
			  -- Solo ejecuta DBCC CHECKIDENT poara revertir el INDENTITY Value al valor anterior
			  SELECT @TEMP_CSV_GLOBAL_ID = MAX(ID) FROM i_TEMP_CSV_GLOBAL
			  DBCC CHECKIDENT (i_TEMP_CSV_GLOBAL, RESEED, @TEMP_CSV_GLOBAL_ID);
       END
     ELSE
	   BEGIN
			  COMMIT TRAN T1;
			  PRINT N'------- TEST MODE ON ------- Changes was commited.';
			  SET @MSG = 'Insertion successful.';
			  SET @Status = 1; -- True (1) for success
	   END
	END TRY
	BEGIN CATCH
		SET @MSG = ERROR_MESSAGE();
        SET @Status = 0; -- False (0) for failure
		PRINT N'Error: ' + ERROR_MESSAGE()
		ROLLBACK TRAN T1;
	END CATCH

PROCESS_STOP:
	PRINT @MSG
	PRINT 'Proceso Detenido'

END
GO

/****** Este SP se encarga exclusivamente para convertir los datos de ingresos guardados en datos de valor para block con conversion y seleccion de datos ******/

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_GINTERFACE_INGRESO]
    @IdFileCSV INT,  -- ID de la tabla i_FILE_CSV que contiene el JSON
    @testMode BIT = 1, -- Test mode
    @MSG NVARCHAR(MAX) OUTPUT,
    @Status BIT OUTPUT
AS
BEGIN
    -- Variables para almacenar los datos extraídos del JSON
    DECLARE @JsonData NVARCHAR(MAX),
            @codigo_ordenes_erp NVARCHAR(MAX),
            @codigo_ordenes_documento NVARCHAR(MAX),
            @codigo_entidades_tipo NVARCHAR(MAX),
            @codigo_entidades NVARCHAR(MAX),
            @fecha_orden DATE,
            @abm NVARCHAR(MAX),
            @RECORD_EXIST AS BIT = 'false',
            @Erp_Reporte INT

    -- Verificar si ya existe el archivo CSV
    PRINT '--Validar si existe un dato.'
    IF EXISTS (SELECT * FROM [dbo].[i_FILE_CSV] WHERE ID = @IdFileCSV)
    BEGIN
        SET @MSG = 'Ya existe ese CSV en la DB...!';
        SET @testMode = 1;
        RAISERROR(@MSG, 16, 1);
        SELECT @MSG [Error Msg];
        SET @RECORD_EXIST = 'true';
    END;

    IF @RECORD_EXIST = 0
    BEGIN
        GOTO PROCESS_RUN
    END
    ELSE
    BEGIN
        GOTO PROCESS_STOP
    END

PROCESS_RUN:
    PRINT 'Proceso iniciado';

    -- Variables para tabla temporal
    CREATE TABLE #Resultados (
        codigo_ordenes_erp NVARCHAR(MAX),
        codigo_ordenes_documento NVARCHAR(MAX),
        codigo_entidades_tipo NVARCHAR(MAX),
        codigo_entidades NVARCHAR(MAX),
        fecha_orden DATE,
        abm NVARCHAR(MAX),
        CodigoProducto INT,
        CantidadCS INT,
        CantidadEA INT
    );

    BEGIN TRAN T1;
    BEGIN TRY
        -- Obtener el JSON de la tabla i_FILE_CSV
        SELECT @JsonData = f.FileJsonObj 
        FROM i_FILE_CSV AS f
        WHERE ID = @IdFileCSV; 

        -- Extraer DataRequireFields del JSON
        DECLARE @DataRequireFields TABLE (Pos INT, Campo NVARCHAR(MAX));
        INSERT INTO @DataRequireFields (Pos, Campo)
        SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Pos, value
        FROM OPENJSON(@JsonData, '$.DataRequireFields');

        -- Obtener reporte ERP
        SELECT @Erp_Reporte = COUNT(*) + 1
        FROM i_FILE_CSV F
        WHERE F.FileStatus = 2;

        -- Generar el código ERP
        SET @codigo_ordenes_erp = CONCAT('ing-', @Erp_Reporte);

        -- Asignar los valores de DataRequireFields
        SELECT 
            @codigo_ordenes_documento = (SELECT Campo FROM @DataRequireFields WHERE Pos = 5),
            @codigo_entidades = (SELECT Campo FROM @DataRequireFields WHERE Pos = 4),
            @codigo_entidades_tipo = (SELECT Campo FROM @DataRequireFields WHERE Pos = 3),
            @fecha_orden = GETDATE(),
            @abm = (SELECT Campo FROM @DataRequireFields WHERE Pos = 1);

        -- Declarar un cursor para iterar sobre los registros de i_TEMP_CSV_GLOBAL
        DECLARE cur CURSOR FOR
        SELECT Campo1, Campo2
        FROM i_TEMP_CSV_GLOBAL AS temp
        WHERE temp.ID_FILE_CSV = @IdFileCSV;

        -- Abrir el cursor
        OPEN cur;

        -- Variables para almacenar los datos obtenidos del cursor
        DECLARE @CodigoProducto INT, @CantidadCS INT, @CantidadEA INT;

        -- Obtener el primer registro
        FETCH NEXT FROM cur INTO @CodigoProducto, @CantidadCS;

        -- Iterar sobre los registros
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Calcular CantidadEA usando la función que convierte CantidadCS a EA
            SET @CantidadEA = [dbo].[fn_GInterface_Get_EA](@CodigoProducto, @CantidadCS, 0, 0);

            -- Insertar los datos en la tabla temporal
            INSERT INTO #Resultados (
                codigo_ordenes_erp, 
                codigo_ordenes_documento, 
                codigo_entidades_tipo, 
                codigo_entidades, 
                fecha_orden, 
                abm,
                CodigoProducto,
                CantidadCS,
                CantidadEA
            )
            VALUES (
                @codigo_ordenes_erp, 
                @codigo_ordenes_documento, 
                @codigo_entidades_tipo, 
                @codigo_entidades, 
                CONVERT(DATE, @fecha_orden, 103), 
                @abm,
                @CodigoProducto,
                @CantidadCS,
                @CantidadEA
            );

            -- Obtener el siguiente registro
            FETCH NEXT FROM cur INTO @CodigoProducto, @CantidadCS;
        END;

        -- Cerrar y liberar el cursor
        CLOSE cur;
        DEALLOCATE cur;

        -- Modo de prueba
        IF (@testMode = 1)
        BEGIN
            ROLLBACK TRAN T1;
            PRINT N'------- TEST MODE ON ------- Changes rolled back.';
            SET @MSG = 'Testing Mode ON - No se cargaron datos por ser pruebas';
            SET @Status = 0;
        END
        ELSE
        BEGIN
            COMMIT TRAN T1;
            PRINT N'------- TEST MODE ON ------- Changes were committed.';
            SET @MSG = 'Insertion successful.';
            SET @Status = 1;
        END
    END TRY
    BEGIN CATCH
        SET @MSG = ERROR_MESSAGE();
        SET @Status = 0;
        PRINT N'Error: ' + ERROR_MESSAGE();
        ROLLBACK TRAN T1;
    END CATCH

PROCESS_STOP:
    PRINT @MSG;
    PRINT 'Proceso Detenido';

    -- Devolver los resultados
    SELECT 
        codigo_ordenes_erp, 
        codigo_ordenes_documento, 
        codigo_entidades_tipo, 
        codigo_entidades, 
        fecha_orden,  
        abm,
        CodigoProducto,
        CantidadCS,
        CantidadEA
    FROM #Resultados;

    -- Eliminar la tabla temporal
    DROP TABLE #Resultados;
END
GO


/****** Este SP se encarga exclusivamente para poder generar el template de los archivos que se van a utilizar en un futuro ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SP_GINTERFACE_INSERT_BASE_FILE_CSV]
    @FileNames NVARCHAR(MAX),
    @FileStatus INT,
    @FileFields INT,
    @FileType INT,
	@Inbound nvarchar (30),
    @FileJsonObj NVARCHAR(MAX),
    
    -- Test control. Rolls back the transaction if in test mode. 1 to test; 0 to persist changes.
    @testMode BIT = 1, -- IMPORTANT: Leave this value as 1 for testing; set to 0 to apply changes
    @MSG NVARCHAR(MAX) OUTPUT,
    @Status BIT OUTPUT
AS
BEGIN
    -- Variables
    DECLARE @FILE_CSV_ID AS INT = 0

    PRINT '--Iniciando operación MERGE...'

    BEGIN TRAN T1
    BEGIN TRY
        -- Usar MERGE para insertar o actualizar
        MERGE INTO i_Template_Files AS target
        USING (SELECT @FileType AS FileType) AS source
        ON target.FileType = source.FileType
        WHEN MATCHED THEN 
            UPDATE SET 
                FileStatus = @FileStatus,
                FileFields = @FileFields,
				[FileInbound] = @Inbound,
                FileJsonObj = @FileJsonObj,
                FileDate = GETDATE()
        WHEN NOT MATCHED THEN
            INSERT (FileNames, FileDate, FileStatus, FileFields, FileType,[FileInbound], FileJsonObj)
            VALUES (@FileNames, GETDATE(), @FileStatus, @FileFields, @FileType,@Inbound, @FileJsonObj);
     

        IF (@testMode = 1)
        BEGIN
            ROLLBACK TRAN T1;
            PRINT N'------- TEST MODE ON ------- Changes rolled back.';
            SET @MSG = 'Testing Mode ON - No se cargaron datos porque es una prueba';
            SET @Status = 0; -- Indica que es solo una prueba
        END
        ELSE
        BEGIN
            COMMIT TRAN T1;
            PRINT N'------- TEST MODE OFF ------- Changes committed.';
            SET @MSG = 'Operación completada con éxito.';
            SET @Status = 1; -- Éxito
        END
    END TRY
    BEGIN CATCH
        SET @MSG = ERROR_MESSAGE();
        SET @Status = 0; -- Error
        PRINT N'Error: ' + @MSG;
        ROLLBACK TRAN T1;
    END CATCH

    PRINT @MSG
END
GO

/****** Object:  StoredProcedure [dbo].[SP_GInterface_Insert_User]    Script Date: 10/21/2024 9:41:42 AM ******/
/****** SP Para insertar el usuario ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE  PROCEDURE [dbo].[SP_GINTERFACE_Insert_User]
	@Name NVARCHAR(MAX),
    @Email NVARCHAR(MAX),
    @Password NVARCHAR(MAX),
    @UserType NVARCHAR(MAX),
	@Site NVARCHAR(MAX),
	
	-- Test control.  Rolls back the transaction if in test mode.  1 to test; 0 to persist changes.
	@testMode bit = 1, -- IMPORTANT live this value in 1 for testing, set it to 0 to apply the changes,
	@MSG NVARCHAR(MAX) OUTPUT,
	@Status BIT OUTPUT
AS
BEGIN
--Validar si el usuario existe
DECLARE @USER_EXIST AS BIT=0
IF EXISTS
(
	SELECT * FROM [dbo].[i_User] WHERE [email]=@Email
)
BEGIN
SET @MSG='Ya existe el usuario!'
            	
	SELECT @MSG [Error Msg]
	
	SET @USER_EXIST=1
END

IF @USER_EXIST = 0
BEGIN
    GOTO REGISTER_RUN;
END
ELSE
BEGIN
    GOTO REGISTER_STOP;
END


REGISTER_RUN:

	PRINT 'Proceso iniciado'

	BEGIN TRAN T1
	BEGIN TRY
		PRINT '------------'
		
		
		-- Insertar el nuevo usuario
		INSERT INTO i_USER ( full_Name, email, user_Pass,type_User, site) 
VALUES ( @Name, @Email, @Password, 'user', @Site);


	 IF (@testMode = 1)
       BEGIN
              ROLLBACK TRAN T1;
              PRINT N'------- TEST MODE ON ------- Changes rolled back.';
			  SET @MSG = 'Testing Mode ON - No se Cargaron Datos por ser pruebas';
			  SET @Status = 0; 
       END
     ELSE
	   BEGIN
			  COMMIT TRAN T1;
			  PRINT N'------- TEST MODE ON ------- Changes was commited.';
			  SET @MSG = 'Insertion successful.';
			  SET @Status = 1; -- True (1) for success
	   END
	END TRY
	BEGIN CATCH
		SET @MSG = ERROR_MESSAGE();
        SET @Status = 0; -- False (0) for failure
		PRINT N'Error: ' + ERROR_MESSAGE()
		ROLLBACK TRAN T1;
	END CATCH

REGISTER_STOP:
	PRINT @MSG
	PRINT 'Proceso Detenido'

END
GO










