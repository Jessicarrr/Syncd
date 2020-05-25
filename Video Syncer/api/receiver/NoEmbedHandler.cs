using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Video_Syncer.api.receiver.manager;
using Video_Syncer.logging;
using Video_Syncer.Models.Playlist;

namespace Video_Syncer.api.receiver
{
    public class NoEmbedHandler
    {
        protected readonly string apiName = "noembed";
        protected readonly string baseAddress = "https://noembed.com/";
        protected readonly string resourceAddressStart = "embed?url=http://www.youtube.com/watch?v=";

        private ApiManager apiManager;

        public NoEmbedHandler()
        {
            apiManager = ApiManager.GetSingletonInstance();

            if(!apiManager.IsNameRegistered(apiName))
            {
                apiManager.RegisterNewApi(apiName, baseAddress);
            }
        }

        public async Task<JObject> GetYoutubeData(string videoId, CancellationTokenSource source)
        {
            string fullResourceAddress = resourceAddressStart + videoId;

            try
            {
                JObject jObj = await apiManager.CallApi(apiName, fullResourceAddress, source).ContinueWith<JObject>(result =>
                {
                    JObject json = JObject.Parse(result.Result);
                    return json;
                });

                return jObj;
            }
            catch(ApiException e)
            {
                CTrace.TraceError("ApiException in NoEmbedHandler.GetYoutubeData, statusCode =  " + e.statusCode + " with message " + e.Message);
                return null;
            }
        }
    }
}
