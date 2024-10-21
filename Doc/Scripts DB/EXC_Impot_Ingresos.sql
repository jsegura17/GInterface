USE [GInterface]
GO

DECLARE @RC int
DECLARE @CodigoERP nvarchar(max)
DECLARE @TipoDocumento nvarchar(max)
DECLARE @TipoEntidad nvarchar(max)
DECLARE @Entidad nvarchar(max)
DECLARE @FechaOrden date
DECLARE @ABM char(1)
DECLARE @Producto nvarchar(max)
DECLARE @Presentacion nvarchar(max)
DECLARE @Cantidad decimal(18,0)
DECLARE @testMode bit
DECLARE @MSG nvarchar(max)
DECLARE @Status bit

-- TODO: Set parameter values here.

SELECT 
        @CodigoERP = CodigoERP,
        @TipoDocumento = TipoDocumento,
        @TipoEntidad = TipoEntidad,
        @Entidad = Entidad,
        @FechaOrden = FechaOrden,
        @ABM = ABM,
        @Producto = Producto,
        @Presentacion = Presentacion,
        @Cantidad = Cantidad
    FROM OPENJSON()
    WITH (
        CodigoERP NVARCHAR(MAX) '$.CodigoERP',
        TipoDocumento NVARCHAR(MAX) '$.TipoDocumento',
        TipoEntidad NVARCHAR(MAX) '$.TipoEntidad',
        Entidad NVARCHAR(MAX) '$.Entidad',
        FechaOrden DATE '$.FechaOrden',
        ABM CHAR(1) '$.ABM',
        Producto NVARCHAR(MAX) '$.Producto',
        Presentacion NVARCHAR(MAX) '$.Presentacion',
        Cantidad DECIMAL '$.Cantidad'
    );

EXECUTE @RC = [dbo].[SP_GInterface_Import_Income] 
   @CodigoERP
  ,@TipoDocumento
  ,@TipoEntidad
  ,@Entidad
  ,@FechaOrden
  ,@ABM
  ,@Producto
  ,@Presentacion
  ,@Cantidad
  ,@testMode
  ,@MSG OUTPUT
  ,@Status OUTPUT
GO


