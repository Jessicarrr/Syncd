using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Video_Syncer.api.receiver.manager;

namespace Video_Syncer.api.receiver
{
    public class YoutubeApiHandler
    {
        //https://www.youtube.com/oembed?url=http://www.youtube.com/watch?v=ojCkgU5XGdg&format=json
        protected readonly string apiName = "youtube";
        protected readonly string baseAddress = "https://youtube.com/";
        protected readonly string resourceAddressStart = "oembed?url=http://www.youtube.com/watch?v=";
        protected readonly string resourceAddressEnd = "&format=json";

        private ApiManager apiManager;

        public YoutubeApiHandler()
        {
            apiManager = ApiManager.GetSingletonInstance();

            if (!apiManager.IsNameRegistered(apiName))
            {
                apiManager.RegisterNewApi(apiName, baseAddress);
            }
        }

        public async Task<JObject> GetYoutubeData(string videoId, CancellationTokenSource source)
        {
            string fullResourceAddress = resourceAddressStart + videoId + resourceAddressEnd;

            try
            {
                JObject jObj = await apiManager.CallApi(apiName, fullResourceAddress, source).ContinueWith<JObject>(result =>
                {
                    JObject json = JObject.Parse(result.Result);
                    return json;
                });

                return jObj;
            }
            catch (ApiException e)
            {
                //logger.LogError("[VSY] ApiException in NoEmbedHandler.GetYoutubeData, statusCode =  " + e.statusCode + " with message " + e.Message + ", e = " + e);
                return null;
            }
        }
    }
}
