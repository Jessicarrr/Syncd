using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Syncd.api.receiver.manager
{
    public class ApiManager
    {
        private List<ApiCaller> apiList;

        private static readonly ApiManager apiManager = new ApiManager();

        public static ApiManager GetSingletonInstance()
        {
            return apiManager;
        }

        private ApiManager()
        {
            apiList = new List<ApiCaller>();
        }

        public void RegisterNewApi(string name, string baseAddress)
        {
            string normalizedBaseAddress = NormalizeBaseAddress(baseAddress);

            if (ListContainsBaseAddress(normalizedBaseAddress) || ListContainsName(name))
            {
                return;
            }

            ApiCaller newCaller = new ApiCaller(name, normalizedBaseAddress);
            apiList.Add(newCaller);
        }

        public bool IsNameRegistered(string name)
        {
            return ListContainsName(NormalizeBaseAddress(name));
        }

        public bool IsBaseAddressRegistered(string baseAddress)
        {
            return ListContainsBaseAddress(NormalizeBaseAddress(baseAddress));
        }

        protected string NormalizeBaseAddress(string baseAddress)
        {
            string newBaseAddress = baseAddress.ToLower();

            if (!newBaseAddress.EndsWith("/"))
            {
                newBaseAddress = newBaseAddress + "/";
            }

            return newBaseAddress;
        }

        public async Task<string> CallApi(string name, string resourceAddress, CancellationTokenSource cancelTokenSource)
        {
            ApiCaller relevantApiCaller = null;

            foreach (ApiCaller caller in apiList)
            {
                if (name.Equals(caller.name))
                {
                    relevantApiCaller = caller;
                }
            }

            if (relevantApiCaller == null)
            {
                throw new Exception("Unregistered api caller - please use RegisterNewApi(string baseAddress) to register an api for tracking");
            }

            return await relevantApiCaller.MakeCall(resourceAddress, cancelTokenSource.Token);
        }

        protected bool ListContainsBaseAddress(string baseAddress)
        {
            foreach (ApiCaller caller in apiList)
            {
                if (baseAddress.Equals(caller.baseAddress, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        protected bool ListContainsName(string name)
        {
            foreach (ApiCaller caller in apiList)
            {
                if (name.Equals(caller.name))
                {
                    return true;
                }
            }
            return false;
        }

        private class ApiCaller
        {
            public string name { get; set; }
            public string baseAddress { get; set; }
            public List<RequestHistoryItem> requestHistory { get; }
            public HttpClient client { get; }

            public ApiCaller(string name, string baseAddress)
            {
                this.name = name;
                this.baseAddress = baseAddress;
                client = new HttpClient();
                requestHistory = new List<RequestHistoryItem>();
            }

            public async Task<string> MakeCall(string resourceAddress, CancellationToken cancellationToken)
            {
                string url = BuildUrl(resourceAddress);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

                RequestHistoryItem newHistoryItem
                   = new RequestHistoryItem(this.baseAddress, resourceAddress, DateTimeOffset.Now.ToUnixTimeMilliseconds());

                using (var response = await client.SendAsync(request, cancellationToken))
                {
                    string content = await response.Content.ReadAsStringAsync();

                    if(!response.IsSuccessStatusCode)
                    {
                        newHistoryItem.wasSuccessful = false;
                        newHistoryItem.statusCode = (int)response.StatusCode;
                        requestHistory.Add(newHistoryItem);

                        throw new ApiException((int) response.StatusCode, content);
                    }
                    else
                    {
                        newHistoryItem.wasSuccessful = true;
                        newHistoryItem.statusCode = (int)response.StatusCode;
                        requestHistory.Add(newHistoryItem);

                        return content;
                    }
                }
            }

            private string BuildUrl(string resourceAddress)
            {
                string finalResourceAddress = resourceAddress;
                string finalBaseAddress = this.baseAddress;

                if (resourceAddress.StartsWith("/")) 
                {
                    finalResourceAddress = finalResourceAddress.Remove(0);
                }

                if(!finalBaseAddress.EndsWith("/"))
                {
                    finalBaseAddress = finalBaseAddress + "/";
                }

                return finalBaseAddress + finalResourceAddress;
            }

            public int GetNumRequestsInTimeSpan(TimeSpan timeSpan)
            {
                return -1;
            }

            public class RequestHistoryItem
            {
                public long time { get; }
                public string baseAddress { get; }
                public string resourceAddress { get; }
                public bool wasSuccessful { get; set; }
                public int statusCode { get; set; }

                public RequestHistoryItem(string baseAddress, string resourceAddress, long time)
                {
                    this.baseAddress = baseAddress;
                    this.resourceAddress = resourceAddress;
                    this.time = time;
                }

                public RequestHistoryItem(string baseAddress, string resourceAddress, long time, bool wasSuccessful)
                {
                    this.baseAddress = baseAddress;
                    this.resourceAddress = resourceAddress;
                    this.time = time;
                    this.wasSuccessful = wasSuccessful;
                }
            }
        }
    }
}