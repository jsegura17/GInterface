using System;
using Microsoft.JSInterop;
using System;
using System.Text;
using System.Net;
using RestSharp;
using GInterface.Models;
using Newtonsoft.Json;
using GInterface.Core.Utils;

namespace GInterface.Core
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
        public bool IsOnline { get; set; }
        public bool IsInEmulator { get; set; } = false;
        public bool IsLoginUser { get; set; } = false;
        public EnumTypes.TransactionTask LastTransactionTask { get; set; }
        public HttpClient Global_HttpClient;
        JsonSerializerSettings settings;

        //Global EventManager for Class
        public EventManager<TransactionEvent> Global_EventManager { get; set; }
        //public NavigationManager _navigationManager { get; set; }
        
        //Global for Blazor Components
        public Shared.MainLayout MainLayoutCore { get; set; }

        //Global Variables
        public string GlobalMsg { get; set; } = string.Empty;
       
        
        public int SecondsPullSync { get; set; } = 15;
        public int SecondsPushSync { get; set; } = 10;
        public TimeSpan timePull { get; set; }
        public TimeSpan timePush { get; set; }
        public System.Timers.Timer SyncPullTimeObj { get; set; }
        public System.Timers.Timer SyncPushTimeObj { get; set; }        
        public DateTime LastSyncPullDateTime { get; set; }
        public DateTime LastSyncPushDateTime { get; set; }
        public List<TransactionEvent> lstTransactionEvents { get; set; }
                
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
                temp.settings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                temp.Global_EventManager = new EventManager<TransactionEvent>();
                temp.lstTransactionEvents = new List<TransactionEvent>();

                temp.LastSyncPullDateTime = DateTime.Now;
                temp.LastSyncPushDateTime = DateTime.Now;
                temp.PullProcessTime = true;
                temp.PushProcessTime = true;
            }
            catch (Exception ex)
            {
                temp.GlobalMsg = ex.Message;

            }
        }

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
