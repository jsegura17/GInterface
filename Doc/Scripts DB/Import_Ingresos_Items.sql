USE [GInterface]
GO
/****** Object:  StoredProcedure [dbo].[SP_GInterface_Import_Income]    Script Date: 10/17/2024 12:54:29 PM ******/
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