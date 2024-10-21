USE [GInterface]
GO
/****** Object:  User [WMS-SRV\Reintec]    Script Date: 10/17/2024 2:02:28 PM ******/
CREATE USER [WMS-SRV\Reintec] FOR LOGIN [WMS-SRV\Reintec] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  UserDefinedTableType [dbo].[TempGlobalType]    Script Date: 10/17/2024 2:02:28 PM ******/
CREATE TYPE [dbo].[TempGlobalType] AS TABLE(
	[Campo1] [bigint] NULL,
	[Campo2] [bigint] NULL,
	[Campo3] [bigint] NULL,
	[Campo4] [bigint] NULL,
	[Campo5] [bigint] NULL,
	[Campo6] [bigint] NULL,
	[Campo7] [bigint] NULL,
	[Campo8] [bigint] NULL,
	[Campo9] [bigint] NULL,
	[Campo10] [bigint] NULL,
	[Campo11] [bigint] NULL,
	[Campo12] [bigint] NULL,
	[Campo13] [bigint] NULL,
	[Campo14] [bigint] NULL,
	[Campo15] [bigint] NULL,
	[Campo16] [varchar](max) NULL,
	[Campo17] [varchar](max) NULL,
	[Campo18] [varchar](max) NULL,
	[Campo19] [varchar](max) NULL,
	[Campo20] [varchar](max) NULL,
	[Campo21] [varchar](max) NULL,
	[Campo22] [varchar](max) NULL,
	[Campo23] [varchar](max) NULL,
	[Campo24] [varchar](max) NULL,
	[Campo25] [varchar](max) NULL,
	[Campo26] [varchar](max) NULL,
	[Campo27] [varchar](max) NULL,
	[Campo28] [varchar](max) NULL,
	[Campo29] [varchar](max) NULL,
	[Campo30] [varchar](max) NULL,
	[Campo31] [varchar](max) NULL,
	[Campo32] [varchar](max) NULL,
	[Campo33] [varchar](max) NULL,
	[Campo34] [varchar](max) NULL,
	[Campo35] [varchar](max) NULL,
	[Campo36] [varchar](max) NULL,
	[Campo37] [varchar](max) NULL,
	[Campo38] [varchar](max) NULL,
	[Campo39] [varchar](max) NULL,
	[Campo40] [varchar](max) NULL,
	[Campo41] [varchar](max) NULL,
	[Campo42] [varchar](max) NULL,
	[Campo43] [varchar](max) NULL,
	[Campo44] [varchar](max) NULL,
	[Campo45] [varchar](max) NULL
)
GO
/****** Object:  UserDefinedFunction [dbo].[fn_GInterface_Get_EA]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[fn_GInterface_Get_EA]
(
	@CodigoProducto INT,
    @CantidadCS INT,
	@CantidadZ96 INT,
    @CantidadEA INT
)
RETURNS INT
AS
BEGIN
    DECLARE @CantidadCS_Producto INT, 
            @CantidadZ96_Producto INT,
            @UnidadesPorCS_Z96 INT = 0, -- Inicializamos con 0 para evitar problemas de NULL
            @UnidadesPorZ96_Z96 INT = 0, -- También inicializamos en 0
            @Cantidad_Calculado_EA INT = 0; -- Inicializamos el resultado

    -- Obtener las cantidades de presentación por CS y Z96
    SELECT 
        @CantidadCS_Producto = MAX(CASE WHEN codigo_presentaciones = 'CS' THEN cantidad_presentacion END),
        @CantidadZ96_Producto = MAX(CASE WHEN codigo_presentaciones = 'Z96' THEN cantidad_presentacion END)
    FROM i_Products_Size
    WHERE codigo_productos = @CodigoProducto;

    -- Calcular unidades por CS si Z96 es mayor a 0
    IF @CantidadCS IS NOT NULL AND @CantidadZ96_Producto > 0
    BEGIN
        SET @UnidadesPorCS_Z96 = @CantidadCS * @CantidadCS_Producto * @CantidadZ96_Producto;
    END
    ELSE IF @CantidadCS IS NOT NULL
    BEGIN
        SET @UnidadesPorCS_Z96 = @CantidadCS * @CantidadCS_Producto;
    END;

    -- Calcular unidades por Z96 si aplica
    IF @CantidadZ96 IS NOT NULL AND @CantidadZ96_Producto > 0
    BEGIN
        SET @UnidadesPorZ96_Z96 = @CantidadZ96 * @CantidadZ96_Producto;
    END;

    -- Sumar las cantidades calculadas
    SET @Cantidad_Calculado_EA = ISNULL(@UnidadesPorCS_Z96, 0) + ISNULL(@UnidadesPorZ96_Z96, 0) + ISNULL(@CantidadEA, 0);

    RETURN @Cantidad_Calculado_EA;
END;
GO
/****** Object:  Table [dbo].[i_DocumentType]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[i_DocumentType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Tipo_Documento] [nvarchar](200) NOT NULL,
	[Numero_Documento] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[i_FILE_CSV]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[i_FILE_CSV](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FileNames] [varchar](200) NULL,
	[FileDate] [datetime] NULL,
	[FileStatus] [int] NULL,
	[FileFields] [int] NULL,
	[FileType] [int] NULL,
	[FileInbound] [nvarchar](30) NULL,
	[FileJsonObj] [varchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[i_Products_Size]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[i_Products_Size](
	[codigo_productos_presentaciones] [varchar](50) NULL,
	[codigo_productos] [int] NOT NULL,
	[codigo_presentaciones] [varchar](4) NOT NULL,
	[codigo_ean] [varchar](22) NULL,
	[cantidad_presentacion] [int] NULL,
	[UV] [int] NULL,
	[cantidad_Unidades] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[i_status]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[i_status](
	[id] [int] NOT NULL,
	[description] [nchar](50) NOT NULL,
	[detail] [nchar](100) NULL,
 CONSTRAINT [PK_iStatus] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[i_TEMP_CSV_GLOBAL]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[i_TEMP_CSV_GLOBAL](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Campo1] [bigint] NULL,
	[Campo2] [bigint] NULL,
	[Campo3] [bigint] NULL,
	[Campo4] [bigint] NULL,
	[Campo5] [bigint] NULL,
	[Campo6] [bigint] NULL,
	[Campo7] [bigint] NULL,
	[Campo8] [bigint] NULL,
	[Campo9] [bigint] NULL,
	[Campo10] [bigint] NULL,
	[Campo11] [bigint] NULL,
	[Campo12] [bigint] NULL,
	[Campo13] [bigint] NULL,
	[Campo14] [bigint] NULL,
	[Campo15] [bigint] NULL,
	[Campo16] [varchar](max) NULL,
	[Campo17] [varchar](max) NULL,
	[Campo18] [varchar](max) NULL,
	[Campo19] [varchar](max) NULL,
	[Campo20] [varchar](max) NULL,
	[Campo21] [varchar](max) NULL,
	[Campo22] [varchar](max) NULL,
	[Campo23] [varchar](max) NULL,
	[Campo24] [varchar](max) NULL,
	[Campo25] [varchar](max) NULL,
	[Campo26] [varchar](max) NULL,
	[Campo27] [varchar](max) NULL,
	[Campo28] [varchar](max) NULL,
	[Campo29] [varchar](max) NULL,
	[Campo30] [varchar](max) NULL,
	[Campo31] [varchar](max) NULL,
	[Campo32] [varchar](max) NULL,
	[Campo33] [varchar](max) NULL,
	[Campo34] [varchar](max) NULL,
	[Campo35] [varchar](max) NULL,
	[Campo36] [varchar](max) NULL,
	[Campo37] [varchar](max) NULL,
	[Campo38] [varchar](max) NULL,
	[Campo39] [varchar](max) NULL,
	[Campo40] [varchar](max) NULL,
	[Campo41] [varchar](max) NULL,
	[Campo42] [varchar](max) NULL,
	[Campo43] [varchar](max) NULL,
	[Campo44] [varchar](max) NULL,
	[Campo45] [varchar](max) NULL,
	[ID_FILE_CSV] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[i_Template_Files]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[i_Template_Files](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FileNames] [varchar](200) NULL,
	[FileDate] [datetime] NULL,
	[FileStatus] [int] NULL,
	[FileType] [int] NULL,
	[FileFields] [int] NULL,
	[FileInbound] [nvarchar](30) NULL,
	[FileJsonObj] [varchar](max) NULL,
UNIQUE NONCLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[i_User]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[i_User](
	[id] [int] NOT NULL,
	[full_Name] [varchar](50) NULL,
	[email] [varchar](50) NULL,
	[user_Pass] [varchar](50) NULL,
	[type_User] [varchar](10) NULL,
	[site] [varchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[SP_GetBaseFileTemplate]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_GetBaseFileTemplate]
AS
BEGIN
    SELECT * FROM i_Template_Files
END
GO
/****** Object:  StoredProcedure [dbo].[SP_GetFileCsv]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_GetFileCsv]
AS
BEGIN
    SELECT * FROM i_FILE_CSV
END
GO
/****** Object:  StoredProcedure [dbo].[SP_GInterface_DOC_PICKLIST]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_GInterface_DOC_PICKLIST]
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
/****** Object:  StoredProcedure [dbo].[SP_GInterface_GetDocumentType]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


 CREATE PROCEDURE [dbo].[SP_GInterface_GetDocumentType]
AS
BEGIN
    
    SELECT  Tipo_Documento,[Numero_Documento] FROM i_DocumentType ;
END;
GO
/****** Object:  StoredProcedure [dbo].[SP_GInterface_Import_Income]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_GInterface_Import_Income]
	@CodigoERP NVARCHAR(MAX)='000123',
    @TipoDocumento NVARCHAR(MAX)='IN_PT_PROV_MAO',
    @TipoEntidad NVARCHAR(MAX)='PRO',
	@Entidad NVARCHAR(MAX)='ARVACO',
    @FechaOrden DATE= '17/10/2024',
	@ABM CHAR='A',
	
	-- Test control.  Rolls back the transaction if in test mode.  1 to test; 0 to persist changes.
	@testMode bit = 0, -- IMPORTANT live this value in 1 for testing, set it to 0 to apply the changes,
	@MSG NVARCHAR(MAX) OUTPUT,
	@Status BIT OUTPUT
AS
BEGIN

	PRINT 'Proceso iniciado'
	PRINT 'Valores a insertar:'
	PRINT 'Codigo ERP: ' + @CodigoERP
	PRINT 'Tipo Documento: ' + @TipoDocumento
	PRINT 'Tipo Entidad: ' + @TipoEntidad
	PRINT 'Entidad: ' + @Entidad
	PRINT 'ABM: ' + @ABM

	PRINT 'Proceso iniciado'

	BEGIN TRAN T1
	BEGIN TRY
	
		
		
		-- Insertar el nuevo usuario
	INSERT INTO [sys_block_MorelDistribucion].[dbo].[i_Ordenes_Ingreso](
	codigo_ordenes_erp,
	codigo_ordenes_documento,
	codigo_entidades_tipo,
	codigo_entidades,
	fecha_orden,
	abm) VALUES(
	@CodigoERP,
	@TipoDocumento,
	@TipoEntidad,
	@Entidad,
	@FechaOrden,
	@ABM); 


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


	PRINT @MSG
	PRINT 'Proceso Detenido'

END
GO
/****** Object:  StoredProcedure [dbo].[SP_GInterface_Import_Income_Items]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_GInterface_Import_Income_Items]
	@CodigoERP NVARCHAR(MAX)='000123',
    @Producto NVARCHAR(MAX)='NIDO',
    @Presentacion NVARCHAR(MAX)='700',
	@Cantidad DECIMAL,
	
	-- Test control.  Rolls back the transaction if in test mode.  1 to test; 0 to persist changes.
	@testMode bit = 0, -- IMPORTANT live this value in 1 for testing, set it to 0 to apply the changes,
	@MSG NVARCHAR(MAX) OUTPUT,
	@Status BIT OUTPUT
AS
BEGIN

	

	PRINT 'Proceso iniciado'

	BEGIN TRAN T1
	BEGIN TRY
	
		
		
		-- Insertar el nuevo usuario
	INSERT INTO [sys_block_MorelDistribucion].[dbo].[i_Ordenes_Ingreso_Items](
	[codigo_ordenes_erp],
	[codigo_productos],
	[codigo_presentaciones],
	[cantidad]
	) VALUES(
	@CodigoERP,
	@Producto,
	@Presentacion,
	@Cantidad); 


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


	PRINT @MSG
	PRINT 'Proceso Detenido'

END
GO
/****** Object:  StoredProcedure [dbo].[SP_GInterface_INGRESO]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[SP_GInterface_INGRESO]
    @IdFileCSV INT  -- ID de la tabla i_FILE_CSV que contiene el JSON
AS
BEGIN
    -- Variables para almacenar los datos extraídos del JSON
    DECLARE @JsonData NVARCHAR(MAX),
            @codigo_ordenes_erp NVARCHAR(MAX),
            @codigo_ordenes_documento NVARCHAR(MAX),
            @codigo_entidades_tipo NVARCHAR(MAX),
            @codigo_entidades NVARCHAR(MAX),
            @fecha_orden DATETIME,
            @abm NVARCHAR(MAX)
            

    -- Crear tabla temporal para almacenar los resultados
    CREATE TABLE #Resultados (
        codigo_ordenes_erp NVARCHAR(MAX),
        codigo_ordenes_documento NVARCHAR(MAX),
        codigo_entidades_tipo NVARCHAR(MAX),
        codigo_entidades NVARCHAR(MAX),
        fecha_orden DATETIME,
        abm NVARCHAR(MAX),
        CodigoProducto INT,
        CantidadCS INT,
        CantidadEA INT
    );
	DECLARE @Erp_Reporte INT;
    -- Obtener el JSON de la tabla i_FILE_CSV
    SELECT @JsonData = f.FileJsonObj 
    FROM i_FILE_CSV AS f
    WHERE ID = @IdFileCSV; 

    -- Extraer DataRequireFields del JSON
    DECLARE @DataRequireFields TABLE (Pos INT, Campo NVARCHAR(MAX));

    INSERT INTO @DataRequireFields (Pos, Campo)
    SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Pos, value
    FROM OPENJSON(@JsonData, '$.DataRequireFields');

	SELECT @Erp_Reporte = COUNT(*) + 1
		FROM i_FILE_CSV F
		WHERE F.FileStatus = 2;
		

    -- Generar el código ERP una sola vez
    SET @codigo_ordenes_erp = CONCAT('ing-', @Erp_Reporte);

    -- Ajustar los valores de DataRequireFields con la posición correcta
    SELECT 
        @codigo_ordenes_documento = (SELECT Campo FROM @DataRequireFields WHERE Pos = 5), -- Tercer elemento
        @codigo_entidades = (SELECT Campo FROM @DataRequireFields WHERE Pos = 4), -- Cuarto elemento
        @codigo_entidades_tipo = (SELECT Campo FROM @DataRequireFields WHERE Pos = 3), -- Quinto elemento
        @fecha_orden = GETDATE(), -- Asignar la fecha actual
        @abm = (SELECT Campo FROM @DataRequireFields WHERE Pos = 1); -- Sexto elemento (vacío o no definido)

    -- Declarar un cursor para iterar sobre los registros de i_TEMP_CSV_GLOBAL
    DECLARE cur CURSOR FOR
    SELECT 
        Campo1,  -- CodigoProducto
        Campo2   -- CantidadCS
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
            @fecha_orden, 
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

    -- Devolver los resultados
    SELECT * FROM #Resultados;

    -- Eliminar la tabla temporal
    DROP TABLE #Resultados;
END;
GO
/****** Object:  StoredProcedure [dbo].[SP_GInterface_INSERT_BASE_FILE_CSV]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[SP_GInterface_INSERT_BASE_FILE_CSV]
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
/****** Object:  StoredProcedure [dbo].[SP_GInterface_INSERT_FILE_CSV]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE  PROCEDURE [dbo].[SP_GInterface_INSERT_FILE_CSV]
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
/****** Object:  StoredProcedure [dbo].[sp_i_ValidateUserCredentials]    Script Date: 10/17/2024 2:02:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[sp_i_ValidateUserCredentials]
    @Email VARCHAR(50),
    @Password VARCHAR(50),
    @IsValid BIT OUTPUT,
    @IsAdmin BIT OUTPUT
AS
BEGIN
    SET @IsValid = 0;
    SET @IsAdmin = 0; -- Asumimos que el usuario no es admin por defecto

    -- Declaramos una variable para almacenar el tipo de usuario
    DECLARE @UserType VARCHAR(10);

    -- Buscamos el usuario con las credenciales proporcionadas y obtenemos su tipo de usuario
    SELECT @UserType = [type_User]
    FROM dbo.i_User AS us
    WHERE us.email = @Email AND us.user_Pass = @Password;

    -- Si encontramos un usuario, entonces las credenciales son válidas
    IF @UserType IS NOT NULL
    BEGIN
        SET @IsValid = 1;
        -- Comprobamos si el tipo de usuario es 'admin' y establecemos @IsAdmin en consecuencia
        IF @UserType = 'admin'
        BEGIN
            SET @IsAdmin = 1;
        END
    END
END;
GO
