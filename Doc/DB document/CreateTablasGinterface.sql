--esta tabla servira para identificar que tipo de cliente esta usando la app y asignarle sus propios archivos
-- Crear tabla I_CLIENT
CREATE TABLE I_CLIENT (
    ID INT IDENTITY(1,1) PRIMARY KEY,  -- Columna ID con autoincremento
    CLIENT_NAME NVARCHAR(100) NOT NULL  -- Nombre del cliente con longitud de 100
);

--Se utiliza para ver que sistema esta usando la base de datos DMS1, SOFTLAND, ETC

-- Crear tabla I_SYSTEM
CREATE TABLE I_SYSTEM (
    ID INT IDENTITY(1,1) PRIMARY KEY,           -- Columna ID con autoincremento
    ID_CLIENT INT NOT NULL,                     -- Columna para la relación con I_CLIENT
    I_SYSTEM_NAME NVARCHAR(100) NOT NULL,       -- Nombre del sistema con longitud de 100
    CONSTRAINT FK_I_SYSTEM_ID_CLIENT FOREIGN KEY (ID_CLIENT) REFERENCES I_CLIENT(ID) -- Clave foránea
);


-- Si existe la tabla i_DocumentType, se elimina.
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[i_DocumentType]') AND type in (N'U'))
DROP TABLE [dbo].[i_DocumentType]
GO
/****** Object:  Table [dbo].[i_DocumentType]    Script Date: 10/21/2024 8:05:47 AM ******/
/****** Document type se encarga de darle Al documento que typo de documento es picklistm ingresos, egresos, etc ******/
-- Crear la tabla i_DocumentType manteniendo Tipo_Documento y agregando las llaves foráneas
CREATE TABLE [dbo].[i_DocumentType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Tipo_Documento] [nvarchar](200) NOT NULL,
	[Numero_Documento] [int] NOT NULL,
    [ID_CLIENT] [int] NOT NULL,                 -- Llave foránea a I_CLIENT
    [ID_SYSTEM] [int] NOT NULL,                 -- Llave foránea a I_SYSTEM
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
) WITH (
	PAD_INDEX = OFF, 
	STATISTICS_NORECOMPUTE = OFF, 
	IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, 
	ALLOW_PAGE_LOCKS = ON, 
	OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF
) ON [PRIMARY],
CONSTRAINT FK_i_DocumentType_ID_CLIENT FOREIGN KEY (ID_CLIENT) REFERENCES [dbo].[I_CLIENT](ID),  -- Llave foránea a I_CLIENT
CONSTRAINT FK_i_DocumentType_ID_SYSTEM FOREIGN KEY (ID_SYSTEM) REFERENCES [dbo].[I_SYSTEM](ID)   -- Llave foránea a I_SYSTEM
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[i_STATUS]    Script Date: 10/21/2024 8:17:27 AM ******/
/****** Esto dice en que estado esta el documento leido con anterioridad ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[i_STATUS](
	[id] [int] NOT NULL,
	[description] [nchar](50) NOT NULL,
	[detail] [nchar](100) NULL,
 CONSTRAINT [PK_iStatus] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO






/****** Object:  Table [dbo].[i_FILE_CSV]    Script Date: 10/21/2024 8:12:49 AM ******/
/****** File csv va a contener el encabezado de un documento, contiene el nombre del archivo, la fecha, en el estado que esta el doc, cuantos campos tiene, si es de ingreso o de egresos ******/
/****** Y el json de como fue construido ese documento ******/
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
	[FileInbound] [varchar](30) NULL,
	[FileJsonObj] [varchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[i_FILE_CSV]  WITH CHECK ADD FOREIGN KEY([FileStatus])
REFERENCES [dbo].[i_STATUS] ([id])
GO

/****** Object:  Table [dbo].[i_TEMP_CSV_GLOBAL]    Script Date: 10/21/2024 8:18:40 AM ******/

/****** Este tiene una llave foranea e filecsv para que en esta tabla se guarde la informacion del documento y en el otro el "reporte" ******/
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
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


/****** Object:  Table [dbo].[i_Template_Files]    Script Date: 10/21/2024 8:20:39 AM ******/
/****** Esta tabla es igual que file csv, pero esta exclusivamente se encarga de guardar en que forma se construye el documentos para el facil de los usuarios normales
  ******/
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
	[FileInbound] [varchar](30) NULL,
	[FileJsonObj] [varchar](max) NULL,
UNIQUE NONCLUSTERED 
(
	[ID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


/****** Object:  Table [dbo].[i_User]    Script Date: 10/21/2024 8:24:37 AM ******/
/****** Este es el usuario, para entrar a la web se necesita de uno y además se utiliza para dms1 para identificar que tipo de archivo necesita el codigo del site ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[i_User](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[full_Name] [varchar](50) NOT NULL,
	[email] [varchar](50) NOT NULL,
	[user_Pass] [varchar](50) NOT NULL,
	[type_User] [varchar](10) NOT NULL,
	[site] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  UserDefinedTableType [dbo].[TempGlobalType]    Script Date: 10/21/2024 8:33:25 AM ******/
/****** Esta se encarga que el Sp pueda recibir un bloque de datos en lugar uno en uno, de esta manera traer toda la informacion de un documento ******/
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

-- Crear tabla I_TRANSACTION
CREATE TABLE [dbo].[I_TRANSACTION](
    [ID] INT IDENTITY(1,1) PRIMARY KEY,              -- ID autoincremental
    [I_ID_CLIENT] INT NOT NULL,                      -- Llave foránea de I_CLIENT
    [I_ID_SYSTEM] INT NOT NULL,                      -- Llave foránea de I_SYSTEM
    [I_ID_TYPEDOC] INT NOT NULL,                     -- Llave foránea de i_DocumentType
    [I_JSONTEMPLATE] NVARCHAR(MAX) NULL,             -- Plantilla en formato JSON
    [I_JSONDATA] NVARCHAR(MAX) NULL,                 -- Datos en formato JSON
    [I_ID_STATUS] INT NOT NULL,                      -- Llave foránea de I_STATUS
    [I_CREATED_DTM] DATE NOT NULL,                   -- Fecha de creación
CONSTRAINT FK_I_TRANSACTION_ID_CLIENT FOREIGN KEY (I_ID_CLIENT) REFERENCES [dbo].[I_CLIENT](ID),    -- Relación con I_CLIENT
CONSTRAINT FK_I_TRANSACTION_ID_SYSTEM FOREIGN KEY (I_ID_SYSTEM) REFERENCES [dbo].[I_SYSTEM](ID),    -- Relación con I_SYSTEM
CONSTRAINT FK_I_TRANSACTION_ID_TYPEDOC FOREIGN KEY (I_ID_TYPEDOC) REFERENCES [dbo].[i_DocumentType](Id), -- Relación con i_DocumentType
CONSTRAINT FK_I_TRANSACTION_ID_STATUS FOREIGN KEY (I_ID_STATUS) REFERENCES [dbo].[I_STATUS](ID)    -- Relación con I_STATUS
) ON [PRIMARY]
GO






/****** Object:  Table [dbo].[i_Products_Size]    Script Date: 10/21/2024 8:15:40 AM ******/
/****** Esta tabla product esa basado en los productos de dms1, entonces esta tabla se utiliza para hacer la conversion de datos de cs a ea ******/
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








