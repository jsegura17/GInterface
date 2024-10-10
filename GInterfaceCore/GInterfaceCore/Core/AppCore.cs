using Microsoft.JSInterop;
using System.Data;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text;
using GInterfaceCore.Models;
using GInterfaceCore.Core.Utils;
using static GInterfaceCore.Models.EnumTypes;
using System.Data.SqlClient;
using GInterfaceCore.Properties;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Components;
using GInterfaceCore.Components.Layout;
using DocumentFormat.OpenXml.Wordprocessing;

namespace GInterfaceCore.Core
{
    public class AppCore
    {
        //Singleton Variables
        private static readonly Object s_lock = new Object();
        private static AppCore instance = null;

        //Format DateTime
        public string GLOBAL_DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
#if WINDOWS
        public string URLBASE_FILE_CSV = @"C:\Temp\CSV";
#endif
#if ANDROID
        public string URLBASE_DOWNLOAD_FOLDER = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
#endif
        //App Info
        public string App_name { get; set; } = string.Empty;
        public string App_package { get; set; } = string.Empty;
        public string App_version { get; set; } = string.Empty;
        public string App_build { get; set; } = string.Empty;
        public string App_deviceName { get; set; } = string.Empty;

        //Global Settings
        public IJSRuntime JS { get; set; }
        
        public bool SyncProcessRunning { get; set; } = false;
        public bool LoginProcessRunning { get; set; } = false;
        public bool PullProcessTime { get; set; } = false;
        public bool PushProcessTime { get; set; } = false;
        public bool Global_App_Started { get; set; } = false;
        public bool Global_ScanInfo_Running { get; set; } = false;
        public bool IsOnline { get; set; } = false;
        public bool IsInEmulator { get; set; } = false;
        public bool IsLoginUser { get; set; } = true;
        public bool IsAdmin { get; set; } = true;
        public EnumTypes.TransactionTask LastTransactionTask { get; set; }
        public HttpClient Global_HttpClient;
        
        //Global EventManager for Class
        public EventManager<TransactionEvent> Global_EventManager { get; set; }
        public NavigationManager _navigationManager { get; set; }

        //Global for Blazor Components
        public MainLayout MainLayoutCore { get; set; }

        //Global Variables
        public string GlobalMsg { get; set; } = string.Empty;

        public ExcelParse _excelParse = new ExcelParse();

        public int SecondsPullSync { get; set; } = 15;
        public int SecondsPushSync { get; set; } = 10;
        public TimeSpan timePull { get; set; }
        public TimeSpan timePush { get; set; }
        public System.Timers.Timer SyncPullTimeObj { get; set; }
        public System.Timers.Timer SyncPushTimeObj { get; set; }
        public DateTime LastSyncPullDateTime { get; set; }
        public DateTime LastSyncPushDateTime { get; set; }
        public List<TransactionEvent> lstTransactionEvents { get; set; }

        public Dictionary<int, string> DocumentType = new Dictionary<int, string>();

        /// <summary>
        /// Para esta parte va hacer para compartir los datos generados para guardar el csv
        /// </summary>
        public string CFileName = "";
        public TransactionStatus fileStatus;
        public string ObjJason = "";
        public DataTable InfotTempo;
        public int head;
        public string[] headers;
        public int documentType;
        public string inbound;

        //Lista de Tipos de Documentos
        public List<EnumTypes.DocumentType> GlobalDocType { get; set; }

        /*
        * Set initial Data for Singleton Pattern Class
        * How used:
        * Make the reference into the CS code
        *   private Core.AppCore _appCore = Core.AppCore.Instance;
        *   
        *   _appCore.JS = JS; -> If you need acces to specific variable
        */
        public static AppCore Instance
        {
            get
            {
                if (instance != null) return instance;
                Monitor.Enter(s_lock);
                AppCore temp = new AppCore();
                setDataInit(temp);

                Interlocked.Exchange(ref instance, temp);
                Monitor.Exit(s_lock);
                return instance;
            }
        }

        /*
        * Set initial Data for Singleton CORE Class
        */
        public static void setDataInit(AppCore temp)
        {
            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                temp.Global_HttpClient = new HttpClient(clientHandler);
                
                temp.Global_EventManager = new EventManager<TransactionEvent>();
                temp.lstTransactionEvents = new List<TransactionEvent>();

                temp.LastSyncPullDateTime = DateTime.Now;
                temp.LastSyncPushDateTime = DateTime.Now;
                temp.PullProcessTime = true;
                temp.PushProcessTime = true;
                temp.IsLoginUser = false;
                temp.IsAdmin = false;

                temp.GlobalDocType = EnumHelpers<EnumTypes.DocumentType>.GetValues().ToList();

            }
            catch (Exception ex)
            {
                temp.GlobalMsg = ex.Message;

            }
        }
       
            // Simulamos una llamada a un servicio o la carga de datos
    
        

        /*
         * For Generic certificate
         * How used:
         * See the reference into callAPI METHOD
         */
        public async Task<bool> SyncProcess()
        {
            bool _return = false;
            //await Task.Delay(100);
            //Set the Global SyncProcess to TRUE
            instance.SyncProcessRunning = true;
            Console.WriteLine("-------------------------------- ");
            Console.WriteLine($"DEBUG - Init SyncProcess with [{instance.lstTransactionEvents.Count}] Transactions ");
            //Check if there are pendient transactions on List
            foreach (TransactionEvent itemTransactionEvent in instance.lstTransactionEvents)
            {

                //The event is trigger
                switch (itemTransactionEvent.TransactionTaskProcess)
                {
                    case EnumTypes.TransactionTask.GET_PULL:
                        GetPullAsync();
                        break;
                    case EnumTypes.TransactionTask.GET_PUSH:
                        GetPushAsync();
                        break;
                    default:
                        break;
                }
                _return = true;
            }
            //Clean transactions from Queue of Transactions
            instance.lstTransactionEvents.Clear();

            //Set the Global SyncProcess to FALSE
            instance.SyncProcessRunning = false;

            //Log
            Console.WriteLine("DEBUG - End SyncProcess");
            Console.WriteLine("-------------------------------- ");

            return true;
        }

        public async Task Focus(string elementId, IJSRuntime _js)
        {
            await _js.InvokeVoidAsync("invokeTabKey");
        }

        /*
        * For Generic certificate
        * How used:
        * See the reference into callAPI METHOD
        */
        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /*
        * Call the API Rest 
        * How used:
        * callAPI("https://youtube.googleapis.com/youtube/v3/","search?part=snippet&q=STRING-TO-SEARCH&key=YOUTUBE-API-KEY")
        */
        public async Task<string> callAPI(string Baseurl, string MethodCall, string username = "", string password = "")
        {
            string _result = "";

            //We accept the Certificate into the comunication channel
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Baseurl);
                client.DefaultRequestHeaders.Clear();
                //Basic Authentication if need
                if (username.Length > 0)
                {
                    var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                }

                //Sending request to find web api REST service resource get_facturas using HttpClient  
                HttpResponseMessage Res = await client.GetAsync(MethodCall);

                //Checking the response is successful or not which is sent using HttpClient  
                if (Res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api   
                    _result = Res.Content.ReadAsStringAsync().Result;
                }
            }
            return _result;
        }
       
        public SqlConnection GetDBConnection()
        {
#if DEBUG
            string connString = Resources.ConnectionString;
#else
            string connString = Resources.ConnectionStringProd;
#endif

            SqlConnection connection = new SqlConnection(connString);

            return connection;
        }

        public bool SqlVerificationUser(string email, string password)
        {
            bool _return = false;


            using (SqlConnection connection = GetDBConnection())
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("sp_i_ValidateUserCredentials", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Agregar parámetros de entrada
                        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.VarChar, 50)).Value = email;
                        command.Parameters.Add(new SqlParameter("@Password", SqlDbType.VarChar, 50)).Value = password;

                        // Agregar parámetro de salida
                        SqlParameter isValidUser = new SqlParameter("@IsValid", SqlDbType.Bit)
                        {
                            Direction = ParameterDirection.Output
                        };
                        SqlParameter isAdminParam = new SqlParameter("@IsAdmin", SqlDbType.Bit)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(isValidUser);
                        command.Parameters.Add(isAdminParam);

                        command.ExecuteNonQuery();

                        instance.IsLoginUser = (bool)isValidUser.Value;
                        instance.IsAdmin = (bool)isAdminParam.Value;
                        instance.IsOnline = (bool)isValidUser.Value;

                        _return = IsLoginUser;
                    }
                }
                catch (SqlException ex)
                {
                    instance.GlobalMsg = "Error:" + ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            return _return;
        }
        public static string AppendDateTimeToName(string baseName)
        {
            DateTime now = DateTime.Now;
            string dateTimeSuffix = now.ToString("dd/MM/yyyy-HHmm");
            string newName = $"{baseName}_{dateTimeSuffix}";
            return newName;
        }
        public void sortData(string[] csvFile, string fileName)
        {
            fileName = AppendDateTimeToName(fileName);


            try
            {
                var Longer = 45;

                if (csvFile.Length > 0)
                {
                    var processedData = new List<TempCSVGlobal>();
                    var fieldNames = new List<string>();
                    var count = 0;

                    ///Datos a subir
                    var fileFields = 0;
                    var objjson = string.Empty;
                    var items = string.Empty;
                    List<TempCSVGlobal> itemTemp = new List<TempCSVGlobal>();
                    // Itera a través de las filas del archivo CSV (omitimos el encabezado)
                    for (int i = 1; i < csvFile.Length; i++)
                    {

                        var row = csvFile[i].Split(',');


                        if (row.Length > 0)
                        {
                            var item = new TempCSVGlobal();
                            var assignedFields = new HashSet<string>();


                            for (int j = 0; j < row.Length; j++)
                            {
                                if (long.TryParse(row[j], out long number))
                                {
                                    if (j < Longer && item.Campo1 == 0) { item.Campo1 = number; assignedFields.Add("Campo1"); }
                                    else if (j < Longer && item.Campo2 == 0) { item.Campo2 = number; assignedFields.Add("Campo2"); }
                                    else if (j < Longer && item.Campo3 == 0) { item.Campo3 = number; assignedFields.Add("Campo3"); }
                                    else if (j < Longer && item.Campo4 == 0) { item.Campo4 = number; assignedFields.Add("Campo4"); }
                                    else if (j < Longer && item.Campo5 == 0) { item.Campo5 = number; assignedFields.Add("Campo5"); }
                                    else if (j < Longer && item.Campo6 == 0) { item.Campo6 = number; assignedFields.Add("Campo6"); }
                                    else if (j < Longer && item.Campo7 == 0) { item.Campo7 = number; assignedFields.Add("Campo7"); }
                                    else if (j < Longer && item.Campo8 == 0) { item.Campo8 = number; assignedFields.Add("Campo8"); }
                                    else if (j < Longer && item.Campo9 == 0) { item.Campo9 = number; assignedFields.Add("Campo9"); }
                                    else if (j < Longer && item.Campo10 == 0) { item.Campo10 = number; assignedFields.Add("Campo10"); }
                                    else if (j < Longer && item.Campo11 == 0) { item.Campo11 = number; assignedFields.Add("Campo11"); }
                                    else if (j < Longer && item.Campo12 == 0) { item.Campo12 = number; assignedFields.Add("Campo12"); }
                                    else if (j < Longer && item.Campo13 == 0) { item.Campo13 = number; assignedFields.Add("Campo13"); }
                                    else if (j < Longer && item.Campo14 == 0) { item.Campo14 = number; assignedFields.Add("Campo14"); }
                                    else if (j < Longer && item.Campo15 == 0) { item.Campo15 = number; assignedFields.Add("Campo15"); }
                                }
                                else
                                {
                                    if (j < Longer && item.Campo16 == null) { item.Campo16 = row[j]; assignedFields.Add("Campo16"); }
                                    else if (j < Longer && item.Campo17 == null) { item.Campo17 = row[j]; assignedFields.Add("Campo17"); }
                                    else if (j < Longer && item.Campo18 == null) { item.Campo18 = row[j]; assignedFields.Add("Campo18"); }
                                    else if (j < Longer && item.Campo19 == null) { item.Campo19 = row[j]; assignedFields.Add("Campo19"); }
                                    else if (j < Longer && item.Campo20 == null) { item.Campo20 = row[j]; assignedFields.Add("Campo20"); }
                                    else if (j < Longer && item.Campo21 == null) { item.Campo21 = row[j]; assignedFields.Add("Campo21"); }
                                    else if (j < Longer && item.Campo22 == null) { item.Campo22 = row[j]; assignedFields.Add("Campo22"); }
                                    else if (j < Longer && item.Campo23 == null) { item.Campo23 = row[j]; assignedFields.Add("Campo23"); }
                                    else if (j < Longer && item.Campo24 == null) { item.Campo24 = row[j]; assignedFields.Add("Campo24"); }
                                    else if (j < Longer && item.Campo25 == null) { item.Campo25 = row[j]; assignedFields.Add("Campo25"); }
                                    else if (j < Longer && item.Campo26 == null) { item.Campo26 = row[j]; assignedFields.Add("Campo26"); }
                                    else if (j < Longer && item.Campo27 == null) { item.Campo27 = row[j]; assignedFields.Add("Campo27"); }
                                    else if (j < Longer && item.Campo28 == null) { item.Campo28 = row[j]; assignedFields.Add("Campo28"); }
                                    else if (j < Longer && item.Campo29 == null) { item.Campo29 = row[j]; assignedFields.Add("Campo29"); }
                                    else if (j < Longer && item.Campo30 == null) { item.Campo30 = row[j]; assignedFields.Add("Campo30"); }
                                    else if (j < Longer && item.Campo31 == null) { item.Campo31 = row[j]; assignedFields.Add("Campo31"); }
                                    else if (j < Longer && item.Campo32 == null) { item.Campo32 = row[j]; assignedFields.Add("Campo32"); }
                                    else if (j < Longer && item.Campo33 == null) { item.Campo33 = row[j]; assignedFields.Add("Campo33"); }
                                    else if (j < Longer && item.Campo34 == null) { item.Campo34 = row[j]; assignedFields.Add("Campo34"); }
                                    else if (j < Longer && item.Campo35 == null) { item.Campo35 = row[j]; assignedFields.Add("Campo35"); }
                                    else if (j < Longer && item.Campo36 == null) { item.Campo36 = row[j]; assignedFields.Add("Campo36"); }
                                    else if (j < Longer && item.Campo37 == null) { item.Campo37 = row[j]; assignedFields.Add("Campo37"); }
                                    else if (j < Longer && item.Campo38 == null) { item.Campo38 = row[j]; assignedFields.Add("Campo38"); }
                                    else if (j < Longer && item.Campo39 == null) { item.Campo39 = row[j]; assignedFields.Add("Campo39"); }
                                    else if (j < Longer && item.Campo40 == null) { item.Campo40 = row[j]; assignedFields.Add("Campo40"); }
                                    else if (j < Longer && item.Campo41 == null) { item.Campo41 = row[j]; assignedFields.Add("Campo41"); }
                                    else if (j < Longer && item.Campo42 == null) { item.Campo42 = row[j]; assignedFields.Add("Campo42"); }
                                    else if (j < Longer && item.Campo43 == null) { item.Campo43 = row[j]; assignedFields.Add("Campo43"); }
                                    else if (j < Longer && item.Campo44 == null) { item.Campo44 = row[j]; assignedFields.Add("Campo44"); }
                                    else if (j < Longer && item.Campo45 == null) { item.Campo45 = row[j]; assignedFields.Add("Campo45"); }
                                }
                            }


                            processedData.Add(item);
                            headers = csvFile[0].Split(',');

                            fieldNames.AddRange(assignedFields);

                        }
                        //processedData.Add(item); este va a tener todos los datos del csv en cuestion de datos
                        //headers va a tener la cabeza del titulo de ese csv
                        objjson = convertJson(headers.ToList(), fieldNames);
                        fileFields = headers.Length;
                        ///objson va hacer uno de los elementos para contruir filecsv
                        //// file name tambien se tiene que va hacer el nombre del archivo
                        ///fileDate se rellena automaticamente en la base de datos
                        ///hacer headers.lenght para ver los campos

                        //InsertFileCsv(fileName, TransactionStatus.Pending, fileFields, objjson);
                        //InsertTempCsvGlobal(processedData[count]);
                        itemTemp.Add(processedData[count]);

                        count++;
                    }
                    DataTable dataTable = LoadCsvData(itemTemp);
                    instance.CFileName = fileName;
                    instance.fileStatus = TransactionStatus.Pending;
                    instance.ObjJason = objjson;
                    instance.InfotTempo = dataTable;
                    instance.head = headers.Length;

                }

            }
            catch (Exception ex)
            {
                // Manejo de errores
                Console.WriteLine($"Error procesando el archivo: {ex.Message}");
            }

        }
        public void InsertFileCsv(string fileNames, TransactionStatus fileStatus, int fileFields, string fileJsonObj, DataTable csvData, string inbound)
        {

            string message = string.Empty;
            bool status = false;

            using (SqlConnection connection = GetDBConnection())
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SP_GInterface_INSERT_FILE_CSV", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Agregar parámetros de entrada
                        command.Parameters.Add(new SqlParameter("@FileNames", SqlDbType.NVarChar, -1)
                        {
                            Value = fileNames
                        });
                        command.Parameters.Add(new SqlParameter("@FileStatus", SqlDbType.Int)
                        {
                            Value = fileStatus
                        });
                        command.Parameters.Add(new SqlParameter("@FileFields", SqlDbType.Int)
                        {
                            Value = fileFields
                        });
                        command.Parameters.Add(new SqlParameter("@Inbound", SqlDbType.NVarChar, -1)
                        {
                            Value = inbound
                        });
                        command.Parameters.Add(new SqlParameter("@FileJsonObj", SqlDbType.NVarChar, -1)
                        {
                            Value = fileJsonObj
                        });
                        command.Parameters.Add(new SqlParameter("@testMode", SqlDbType.Int)
                        {
                            Value = 0
                        });

                        // Tabla Temp

                        SqlParameter tvpParam = new SqlParameter("@CsvData", SqlDbType.Structured)
                        {
                            TypeName = "dbo.TempGlobalType", // Tipo de datos de tabla definido en SQL Server
                            Value = csvData
                        };
                        command.Parameters.Add(tvpParam);



                        // Agregar parámetro de salida

                        SqlParameter messageParam = new SqlParameter("@MSG", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(messageParam);

                        SqlParameter statusParam = new SqlParameter("@Status", SqlDbType.Bit)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(statusParam);

                        command.ExecuteNonQuery();

                        // Obtener el mensaje y status del nuevo registro
                        message = (string)messageParam.Value;
                        status = (bool)statusParam.Value;
                    }
                }
                catch (SqlException ex)
                {
                    // Manejar excepciones, por ejemplo:
                    Console.WriteLine("SQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    // Manejar otras excepciones
                    Console.WriteLine("Error: " + ex.Message);
                }
            }


        }

        public void InsertBaseFileCsv(string fileNames, TransactionStatus fileStatus, int fileFields, int fileType, string fileJsonObj, string inbound)
        {

            string message = string.Empty;
            bool status = false;

            using (SqlConnection connection = GetDBConnection())
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SP_GInterface_INSERT_BASE_FILE_CSV", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Agregar parámetros de entrada
                        command.Parameters.Add(new SqlParameter("@FileNames", SqlDbType.NVarChar, -1)
                        {
                            Value = fileNames
                        });
                        command.Parameters.Add(new SqlParameter("@FileStatus", SqlDbType.Int)
                        {
                            Value = fileStatus
                        });
                        command.Parameters.Add(new SqlParameter("@FileFields", SqlDbType.Int)
                        {
                            Value = fileFields
                        });
                        command.Parameters.Add(new SqlParameter("@FileType", SqlDbType.Int)
                        {
                            Value = fileType
                        });
                        command.Parameters.Add(new SqlParameter("@Inbound", SqlDbType.NVarChar, -1)
                        {
                            Value = inbound
                        });
                        command.Parameters.Add(new SqlParameter("@FileJsonObj", SqlDbType.NVarChar, -1)
                        {
                            Value = fileJsonObj
                        });
                        command.Parameters.Add(new SqlParameter("@testMode", SqlDbType.Int)
                        {
                            Value = 0
                        });

                        // Tabla Temp
                        // Agregar parámetro de salida

                        SqlParameter messageParam = new SqlParameter("@MSG", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(messageParam);

                        SqlParameter statusParam = new SqlParameter("@Status", SqlDbType.Bit)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(statusParam);

                        command.ExecuteNonQuery();

                        // Obtener el mensaje y status del nuevo registro
                        message = (string)messageParam.Value;
                        status = (bool)statusParam.Value;
                    }
                }
                catch (SqlException ex)
                {
                    // Manejar excepciones, por ejemplo:
                    Console.WriteLine("SQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    // Manejar otras excepciones
                    Console.WriteLine("Error: " + ex.Message);
                }
            }


        }
        public List<FileCSV> GetTemplateFiles()
        {
            List<FileCSV> templateFiles = new List<FileCSV>();

            using (SqlConnection connection = GetDBConnection())
            {
                SqlCommand cmd = new SqlCommand("SP_GetBaseFileTemplate", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                Console.WriteLine(connection);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    FileCSV file = new FileCSV
                    {
                        ID = Convert.ToInt32(reader["ID"]),
                        FileNames = reader["FileNames"].ToString(),
                        FileDate = Convert.ToDateTime(reader["FileDate"]),
                        FileStatus = (TransactionStatus)Convert.ToInt32(reader["FileStatus"]), // Asegúrate de que FileStatus sea un int en la base de datos y se pueda mapear a TransactionStatus
                        FileFields = Convert.ToInt32(reader["FileFields"]),
                        FileType = GetDocumentType(Convert.ToInt32(reader["FileType"])),
                        InboundOutbound = reader["FileInbound"].ToString(),
                        FileJsonObj = reader["FileJsonObj"].ToString()
                    };


                    templateFiles.Add(file);
                }
            }

            return templateFiles;
        }
        private Dictionary<int, string> GetDocumentType(int fileTypeId)
        {
             documentTypeAsync();
            
             return instance.DocumentType; // Retorna la descripción del tipo de documento
           
        }
        public List<FileCSV> GetPendingFiles()
        {
            List<FileCSV> templateFiles = new List<FileCSV>();

            using (SqlConnection connection = GetDBConnection())
            {
                SqlCommand cmd = new SqlCommand("SP_GetFileCsv", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    FileCSV file = new FileCSV
                    {
                        ID = Convert.ToInt32(reader["ID"]),
                        FileNames = reader["FileNames"].ToString(),
                        FileDate = Convert.ToDateTime(reader["FileDate"]),
                        FileStatus = (TransactionStatus)Convert.ToInt32(reader["FileStatus"]), // Asegúrate de que FileStatus sea un int en la base de datos y se pueda mapear a TransactionStatus
                        FileFields = Convert.ToInt32(reader["FileFields"]),
                        InboundOutbound = reader["FileInbound"].ToString(),
                        FileJsonObj = reader["FileJsonObj"].ToString()
                    };


                    templateFiles.Add(file);
                }
            }

            return templateFiles;
        }
        public async Task<Dictionary<int, string>> documentTypeAsync()
        {
            // Llamar al método que obtiene los datos de la base de datos
            instance.DocumentType = await instance.GetDocumentTypeAsync();
            return instance.DocumentType;
        }
        public async Task<Dictionary<int, string>> GetDocumentTypeAsync()
        {
            var result = new Dictionary<int, string>();

            using (SqlConnection connection = GetDBConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GInterface_GetDocumentType", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int tipoDocumentoIndex = reader.GetOrdinal("Tipo_Documento");
                            int numeroDocumentoIndex = reader.GetOrdinal("Numero_Documento");

                            string Tipo_Documento = reader.GetString(tipoDocumentoIndex);
                            int Numero_Documento = reader.GetInt32(numeroDocumentoIndex);

                            // Agregar los valores al diccionario
                            result.Add(Numero_Documento, Tipo_Documento);


                            
                        }
                    }
                }
            }
            instance.DocumentType = result;
            return result;
        }


        private static DataTable LoadCsvData(List<TempCSVGlobal> temp)
        {
            // Cargar datos del CSV en una DataTable
            DataTable table = new DataTable();
            table.Columns.Add("Campo1", typeof(long));
            table.Columns.Add("Campo2", typeof(long));
            table.Columns.Add("Campo3", typeof(long));
            table.Columns.Add("Campo4", typeof(long));
            table.Columns.Add("Campo5", typeof(long));
            table.Columns.Add("Campo6", typeof(long));
            table.Columns.Add("Campo7", typeof(long));
            table.Columns.Add("Campo8", typeof(long));
            table.Columns.Add("Campo9", typeof(long));
            table.Columns.Add("Campo10", typeof(long));
            table.Columns.Add("Campo11", typeof(long));
            table.Columns.Add("Campo12", typeof(long));
            table.Columns.Add("Campo13", typeof(long));
            table.Columns.Add("Campo14", typeof(long));
            table.Columns.Add("Campo15", typeof(long));
            //Text
            table.Columns.Add("Campo16", typeof(string));
            table.Columns.Add("Campo17", typeof(string));
            table.Columns.Add("Campo18", typeof(string));
            table.Columns.Add("Campo19", typeof(string));
            table.Columns.Add("Campo20", typeof(string));
            table.Columns.Add("Campo21", typeof(string));
            table.Columns.Add("Campo22", typeof(string));
            table.Columns.Add("Campo23", typeof(string));
            table.Columns.Add("Campo24", typeof(string));
            table.Columns.Add("Campo25", typeof(string));
            table.Columns.Add("Campo26", typeof(string));
            table.Columns.Add("Campo27", typeof(string));
            table.Columns.Add("Campo28", typeof(string));
            table.Columns.Add("Campo29", typeof(string));
            table.Columns.Add("Campo30", typeof(string));
            table.Columns.Add("Campo31", typeof(string));
            table.Columns.Add("Campo32", typeof(string));
            table.Columns.Add("Campo33", typeof(string));
            table.Columns.Add("Campo34", typeof(string));
            table.Columns.Add("Campo35", typeof(string));
            table.Columns.Add("Campo36", typeof(string));
            table.Columns.Add("Campo37", typeof(string));
            table.Columns.Add("Campo38", typeof(string));
            table.Columns.Add("Campo39", typeof(string));
            table.Columns.Add("Campo40", typeof(string));
            table.Columns.Add("Campo41", typeof(string));
            table.Columns.Add("Campo42", typeof(string));
            table.Columns.Add("Campo43", typeof(string));
            table.Columns.Add("Campo44", typeof(string));
            table.Columns.Add("Campo45", typeof(string));



            // Aquí deberías cargar datos desde tu archivo CSV a la DataTable
            // Ejemplo de adición de filas
            foreach (var item in temp)
            {
                table.Rows.Add(
                    item.Campo1,
                    item.Campo2,
                    item.Campo3,
                    item.Campo4,
                    item.Campo5,
                    item.Campo6,
                    item.Campo7,
                    item.Campo8,
                    item.Campo9,
                    item.Campo10,
                    item.Campo11,
                    item.Campo12,
                    item.Campo13,
                    item.Campo14,
                    item.Campo15,
                    item.Campo16 ?? (object)DBNull.Value,
                    item.Campo17 ?? (object)DBNull.Value,
                    item.Campo18 ?? (object)DBNull.Value,
                    item.Campo19 ?? (object)DBNull.Value,
                    item.Campo20 ?? (object)DBNull.Value,
                    item.Campo21 ?? (object)DBNull.Value,
                    item.Campo22 ?? (object)DBNull.Value,
                    item.Campo23 ?? (object)DBNull.Value,
                    item.Campo24 ?? (object)DBNull.Value,
                    item.Campo25 ?? (object)DBNull.Value,
                    item.Campo26 ?? (object)DBNull.Value,
                    item.Campo27 ?? (object)DBNull.Value,
                    item.Campo28 ?? (object)DBNull.Value,
                    item.Campo29 ?? (object)DBNull.Value,
                    item.Campo30 ?? (object)DBNull.Value,
                    item.Campo31 ?? (object)DBNull.Value,
                    item.Campo32 ?? (object)DBNull.Value,
                    item.Campo33 ?? (object)DBNull.Value,
                    item.Campo34 ?? (object)DBNull.Value,
                    item.Campo35 ?? (object)DBNull.Value,
                    item.Campo36 ?? (object)DBNull.Value,
                    item.Campo37 ?? (object)DBNull.Value,
                    item.Campo38 ?? (object)DBNull.Value,
                    item.Campo39 ?? (object)DBNull.Value,
                    item.Campo40 ?? (object)DBNull.Value,
                    item.Campo41 ?? (object)DBNull.Value,
                    item.Campo42 ?? (object)DBNull.Value,
                    item.Campo43 ?? (object)DBNull.Value,
                    item.Campo44 ?? (object)DBNull.Value,
                    item.Campo45 ?? (object)DBNull.Value
                );
            }
            return table;
        }
        public List<string> RequireHeaders() 
        {
            List<string> requireHeaderlist = new List<string> { };
              
            

            switch (instance.documentType)
            {
                case 1:
                    // Código a ejecutar si variable es igual a valor1
                    break;

                case 2:
                    // Código a ejecutar si variable es igual a valor2
                    break;

                case 3:
                    // Código a ejecutar si variable es igual a valor3
                    break;
                case 4:
                    // Código a ejecutar si variable es igual a valor3
                    break;
                case 5:
                    // Código a ejecutar si variable es igual a valor3
                    break;
                case 6:
                    // Código a ejecutar si variable es igual a valor3
                    break;
                case 7:
                    // Código a ejecutar si variable es igual a valor3
                    break;

                default:
                    // Código a ejecutar si ninguno de los casos anteriores coincide
                    break;
            }


            return requireHeaderlist;
        }


        public void sortDataExcel(List<List<string>> excel, string fileName, List<string> dataArray, string endStartInfo)
        {
            fileName = AppendDateTimeToName(fileName);
            var requireCount = 0;
            var requireData = dataArray;
            requireCount = requireData.Count;
            

            bool[] usable = new bool[15] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true};

            try
            {
                var Longer = 45;

                if (excel.Count > 0)
                {
                    var processedData = new List<TempCSVGlobal>();
                    var fieldNames = new List<string>();
                    var count = 0;

                    // Datos a subir
                    var fileFields = 0;
                    var objjson = string.Empty;
                    var items = string.Empty;
                    List<TempCSVGlobal> itemTemp = new List<TempCSVGlobal>();

                    // Asignamos la fila de headers
                    if (requireData[0].Equals("No_contiene_encabezado")) 
                    { 
                        requireCount =0; 
                    }
                    var headers = excel[requireCount];
                    var datanot = 0;
                   
                    // Iteramos a través de las filas del archivo EXCEL (omitimos el encabezado)
                    for (int i = requireCount + 1; i < excel.Count; i++)
                    {
                        var row = excel[i]; // Ya es una lista de strings, no necesitas Split

                        if (row.Count > 0)
                        {
                            var item = new TempCSVGlobal();
                            var assignedFields = new HashSet<string>();

                            for (int j = 0; j < row.Count; j++)
                            {
                                if (long.TryParse(row[j], out long number))
                                {
                                    if (j < Longer && item.Campo1 == datanot && usable[0]) { item.Campo1 = number; assignedFields.Add("Campo1"); usable[0] = false; }
                                    else if (j < Longer && item.Campo2 == datanot && usable[1]) { item.Campo2 = number; assignedFields.Add("Campo2"); usable[1] = false; }
                                    else if (j < Longer && item.Campo3 == datanot && usable[2]) { item.Campo3 = number; assignedFields.Add("Campo3"); usable[2] = false; }
                                    else if (j < Longer && item.Campo4 == datanot && usable[3]) { item.Campo4 = number; assignedFields.Add("Campo4"); usable[3] = false; }
                                    else if (j < Longer && item.Campo5 == datanot && usable[4]) { item.Campo5 = number; assignedFields.Add("Campo5"); usable[4] = false; }
                                    else if (j < Longer && item.Campo6 == datanot && usable[5]) { item.Campo6 = number; assignedFields.Add("Campo6"); usable[5] = false; }
                                    else if (j < Longer && item.Campo7 == datanot && usable[6]) { item.Campo7 = number; assignedFields.Add("Campo7"); usable[6] = false; }
                                    else if (j < Longer && item.Campo8 == datanot && usable[7]) { item.Campo8 = number; assignedFields.Add("Campo8"); usable[7] = false; }
                                    else if (j < Longer && item.Campo9 == datanot && usable[8]) { item.Campo9 = number; assignedFields.Add("Campo9"); usable[8] = false; }
                                    else if (j < Longer && item.Campo10 == datanot && usable[9]) { item.Campo10 = number; assignedFields.Add("Campo10"); usable[9] = false; }
                                    else if (j < Longer && item.Campo11 == datanot && usable[10]) { item.Campo11 = number; assignedFields.Add("Campo11"); usable[10] = false; }
                                    else if (j < Longer && item.Campo12 == datanot && usable[11]) { item.Campo12 = number; assignedFields.Add("Campo12"); usable[11] = false; }
                                    else if (j < Longer && item.Campo13 == datanot && usable[12]) { item.Campo13 = number; assignedFields.Add("Campo13"); usable[12] = false; }
                                    else if (j < Longer && item.Campo14 == datanot && usable[13]) { item.Campo14 = number; assignedFields.Add("Campo14"); usable[13] = false; }
                                    else if (j < Longer && item.Campo15 == datanot && usable[14]) { item.Campo15 = number; assignedFields.Add("Campo15"); usable[14] = false; }



                                }
                                else
                                {
                                    if (j < Longer && item.Campo16 == null) { item.Campo16 = row[j]; assignedFields.Add("Campo16"); }
                                    else if (j < Longer && item.Campo17 == null) { item.Campo17 = row[j]; assignedFields.Add("Campo17"); }
                                    else if (j < Longer && item.Campo18 == null) { item.Campo18 = row[j]; assignedFields.Add("Campo18"); }
                                    else if (j < Longer && item.Campo19 == null) { item.Campo19 = row[j]; assignedFields.Add("Campo19"); }
                                    else if (j < Longer && item.Campo20 == null) { item.Campo20 = row[j]; assignedFields.Add("Campo20"); }
                                    else if (j < Longer && item.Campo21 == null) { item.Campo21 = row[j]; assignedFields.Add("Campo21"); }
                                    else if (j < Longer && item.Campo22 == null) { item.Campo22 = row[j]; assignedFields.Add("Campo22"); }
                                    else if (j < Longer && item.Campo23 == null) { item.Campo23 = row[j]; assignedFields.Add("Campo23"); }
                                    else if (j < Longer && item.Campo24 == null) { item.Campo24 = row[j]; assignedFields.Add("Campo24"); }
                                    else if (j < Longer && item.Campo25 == null) { item.Campo25 = row[j]; assignedFields.Add("Campo25"); }
                                    else if (j < Longer && item.Campo26 == null) { item.Campo26 = row[j]; assignedFields.Add("Campo26"); }
                                    else if (j < Longer && item.Campo27 == null) { item.Campo27 = row[j]; assignedFields.Add("Campo27"); }
                                    else if (j < Longer && item.Campo28 == null) { item.Campo28 = row[j]; assignedFields.Add("Campo28"); }
                                    else if (j < Longer && item.Campo29 == null) { item.Campo29 = row[j]; assignedFields.Add("Campo29"); }
                                    else if (j < Longer && item.Campo30 == null) { item.Campo30 = row[j]; assignedFields.Add("Campo30"); }
                                }
                            }

                            processedData.Add(item);
                            fieldNames.AddRange(assignedFields);
                            instance.headers = excel[requireCount + 1].ToArray();
                            usable = new bool[15] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true };
                        }
                    }

                    // Procesar datos CSV

                    fileFields = headers.Count;
                    objjson = convertJsonExcel(headers, fieldNames, requireData, endStartInfo, fileFields, excel);
                    // Insertar en base de datos
                    DataTable dataTable = LoadCsvData(processedData);
                    instance.CFileName = fileName;
                    instance.fileStatus = TransactionStatus.Pending;
                    instance.ObjJason = objjson;
                    instance.InfotTempo = dataTable;
                    instance.head = headers.Count;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando el archivo: {ex.Message}");
            }
        }




        public string convertJson(List<string> header, List<string> fieldNames)
        {
            var node = new JsonArray();
            var colums = new JsonArray();
            foreach (var item in header)
            {
                int index = header.IndexOf(item);
                node.Add(item);
                colums.Add(fieldNames[index]);
            }
            var jsonObject = new Dictionary<string, JsonArray>
            {
                { "FileColumName", node },
                { "FileColumNamePosition", colums }
            };
            string jsonString = System.Text.Json.JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true });
            return jsonString;
        }
        public string convertJsonExcel(List<string> header, List<string> fieldNames, List<string> require, string endStartInfo, int fielfields, List<List<string>> excel)
        {
            var node = new JsonArray();
            var columns = new JsonArray();
            var requireNode = new JsonArray();
            var dataRequireNode = new JsonArray();
            var startEndHead = new JsonArray();
            startEndHead = new JsonArray();

            startEndHead.Add(fielfields);


            var row = endStartInfo.Split(',');
            foreach (var item in row)
            {
                startEndHead.Add(item);
            }
            foreach (var item in header)
            {
                int index = header.IndexOf(item);
                node.Add(item);
                columns.Add(fieldNames[index]);
            }
            List<string> onlySeconds = excel.Select(sublista => sublista[1])
                                       .Take(require.Count)
                                       .ToList();
            foreach (var req in require)
            { requireNode.Add(req); }
            foreach (var dataReq in onlySeconds)
            { dataRequireNode.Add(dataReq); }



            var jsonObject = new Dictionary<string, JsonArray>
    {
        { "FileColumName", node },
        { "FileColumNamePosition", columns },
        { "RequireFields", requireNode },
        { "DataRequireFields", dataRequireNode },
        { "StartEndHead", startEndHead }
    };
            string jsonString = System.Text.Json.JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true });
            return jsonString;
        }

        public JsonTemplate TemplateJson(FileCSV data)
        {
            JsonTemplate fileJson = JsonSerializer.Deserialize<JsonTemplate>(data.FileJsonObj);
            return fileJson;
        }



    public void LogOut()
        {
            instance.IsLoginUser = false;
            instance.IsAdmin = false;
            instance.IsOnline = false;
        }

        public void GoToUrl(string _uri)
        {
            
        }

        #region SYNC_PROCESS
        /*
        * Call the API Rest 
        * How used:
        * Execute Pull Process
        */
        public async Task GetPullAsync()
        {
            Console.WriteLine("DEBUG - Init GetPullAsync ");
            instance.PullProcessTime = true;
            try
            {
                instance.LastSyncPullDateTime = DateTime.Now;
                // Simulate background task
                await Task.Delay(1500);
            }
            catch (Exception ex)
            {
                instance.GlobalMsg = ex.Message;
                Console.WriteLine("DEBUG - Exceptiom -> " + instance.GlobalMsg);
            }
            Console.WriteLine("DEBUG - End GetPullAsync ");
            instance.PullProcessTime = false;
        }

        /*
        * Call the API Rest 
        * How used:
        * Execute Pull Process
        */
        public async Task GetPushAsync()
        {
            Console.WriteLine("DEBUG - Init GetPushAsync ");
            instance.PushProcessTime = true;
            try
            {
                instance.LastSyncPushDateTime = DateTime.Now;
                // Simulate background task
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                instance.GlobalMsg = ex.Message;
                Console.WriteLine("DEBUG - Exceptiom -> " + instance.GlobalMsg);
            }
            Console.WriteLine("DEBUG - End GetPushAsync ");
            instance.PushProcessTime = false;
        }
        #endregion SYNC_PROCESS
    }

}
