/****** Object:  UserDefinedFunction [dbo].[fn_GInterface_Get_EA]    Script Date: 10/21/2024 9:55:01 AM ******/
/****** Funcion que se encarga de leer los datos de cs, ea y o z96 para sacar solo de respuesta ea ******/
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


