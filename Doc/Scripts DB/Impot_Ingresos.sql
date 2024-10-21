USE [GInterface]
GO
/****** Object:  StoredProcedure [dbo].[SP_GInterface_Import_Income]    Script Date: 10/17/2024 12:54:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[SP_GInterface_Import_Income]
	@CodigoERP NVARCHAR(MAX),
    @TipoDocumento NVARCHAR(MAX),
    @TipoEntidad NVARCHAR(MAX),
	@Entidad NVARCHAR(MAX),
    @FechaOrden DATE,
	@ABM CHAR(1),
	@Producto NVARCHAR(MAX),
    @Presentacion NVARCHAR(MAX),
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

	--Validacion de datos necesarios
	DECLARE @Documento NVARCHAR(200);
	DECLARE @CodigoEntidad NVARCHAR(50);
	DECLARE @CodigoTipoEntidad NVARCHAR(50);

	
	SELECT @Documento = [Tipo_Documento] 
	FROM [dbo].[i_DocumentType]
	WHERE [Tipo_Documento] = @TipoDocumento;  -- Comparación de TipoDocumento

	SELECT @CodigoEntidad = [codigo_entidades] 
	FROM [sys_block_MorelDistribucion].[dbo].[Entidades]
	WHERE [codigo_entidades] = @Entidad;  -- Comparación de Entidad

	SELECT @CodigoTipoEntidad = [codigo_entidades_tipos] 
	FROM [sys_block_MorelDistribucion].[dbo].[i_Entidades_Categorias]
	WHERE [codigo_entidades_tipos] = @TipoEntidad;  -- Comparación de TipoEntidad
	
	-- Comparar los resultados de las validaciones
	IF (@CodigoEntidad IS NOT NULL AND @CodigoTipoEntidad IS NOT NULL AND @Documento IS NOT NULL)
	BEGIN
		-- Insertar data de orden
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
	--Insertar data items
	INSERT INTO [sys_block_MorelDistribucion].[dbo].[i_Ordenes_Ingreso_Items](
	[codigo_ordenes_erp],
	[codigo_productos],
	[codigo_presentaciones],
	[cantidad]
	)VALUES(
	@CodigoERP,
	@Producto,
	@Presentacion,
	@Cantidad
	);
	END
	ELSE
	BEGIN
		-- Si falla la validacion
		PRINT 'Validacion Fallida';
		SET @MSG = 'La validacion de los datos fallo';
		SET @Status = 0;
	END


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
			  PRINT N'------- TEST MODE OFF ------- Changes was commited.';
			  SET @MSG = 'Insertion successful.';
			  SET @Status = 1; -- True (1) for success

			  SELECT * FROM [sys_block_MorelDistribucion].[dbo].[i_Ordenes_Ingreso] 
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