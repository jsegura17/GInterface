using RestSharp;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace GInterface.Core
{
    /*
     * How to used this Class
     * 
     * var apiClient = new ApiClient<MyObject>("https://api.example.com/endpoint", bearerToken: "my_token");
     * var myData = new { Name = "John", Age = 30 };
     * var response = await apiClient.PostAsync(myData);
     * 
     */
    public class ApiRestClient<T>
    {
        //Get Global Core Settings
        private Core.AppCore _appCore = Core.AppCore.Instance;

        private HttpClient _httpClient;
        public string _endPoint;

        public ApiRestClient()
        {
            _httpClient = _appCore.Global_HttpClient;
            _endPoint = string.Empty;
        }

        public ApiRestClient(string endPoint, string username = null, string password = null, string bearerToken = null)
        {
            _httpClient = new HttpClient();
            _endPoint = endPoint;

            if (!string.IsNullOrEmpty(bearerToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            }
            else if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
        }

        public async Task<RestResponse> GetAsync(string uriRequest)
        {
            RestResponse response = null;
            try
            {
                // send GET request with RestSharp
                var options = new RestClientOptions()
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };
                var client = new RestSharp.RestClient(options);
                var request = new RestRequest(uriRequest,Method.Get);
                
                //string paramToken = "Bearer " + _appCore.Global_Token.Token;
                //request.AddHeader("Authorization", paramToken);
                response = client.Execute(request);
            }
            catch (Exception ex)
            {
                _appCore.GlobalMsg = ex.Message;
            }
            return response;
        }

        public async Task<HttpResponseMessage> PostAsync(object data)
        {

            HttpResponseMessage response = null; // = await _httpClient.PostAsJsonAsync(_endPoint, data);

            //We accept the Certificate into the comunication channel
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(_appCore.AcceptAllCertifications);
                        
            try
            {
                response = await _httpClient.PostAsync(_endPoint, (HttpContent?)data);
            }
            catch (Exception ex)
            {
                _appCore.GlobalMsg = ex.Message;
            }

            return response;
        }

        /*
        * Call the API Rest using RestClient - POST method
        * How used:
        * RestResponse response = await apiRestClient.PostAsync(URI_Request, json);
        */
        public async Task<RestResponse> PostAsync(string uriRequest, object data = null)
        {
            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"DEBUG - Init ApiRestClient->PostAsync");
            Console.WriteLine($"DEBUG -> URL [{uriRequest}]");

            RestResponse response = null;
            try
            {
                // send GET request with RestSharp
                var options = new RestClientOptions()
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };
                var client = new RestSharp.RestClient(options);
                var request = new RestRequest(uriRequest, Method.Post);
                
                request.AddHeader("Content-Type", "application/json");
                if (data != null)
                {
                    request.AddParameter("application/json", data, ParameterType.RequestBody);
                    Console.WriteLine($"DEBUG -> DATA [{data}]");
                }                

                //string paramToken = "Bearer " + _appCore.Global_Token.Token;
                //request.AddHeader("Authorization", paramToken);
                response = client.Execute(request);
            }
            catch (Exception ex)
            {
                _appCore.GlobalMsg = ex.Message;
            }
            //Log
            Console.WriteLine("DEBUG - End ApiRestClient->PostAsync");
            Console.WriteLine("------------------------------------ ");
            return response;
        }

        public async Task<HttpResponseMessage> PutAsync(object data)
        {
            var response = await _httpClient.PutAsJsonAsync(_endPoint, data);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error {response.StatusCode}: {response.ReasonPhrase}");
            }

            return response;
        }

        public async Task<HttpResponseMessage> DeleteAsync()
        {
            var response = await _httpClient.DeleteAsync(_endPoint);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error {response.StatusCode}: {response.ReasonPhrase}");
            }

            return response;
        }
    }
}
