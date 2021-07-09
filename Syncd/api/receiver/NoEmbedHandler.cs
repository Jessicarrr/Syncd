using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Syncd.api.receiver.manager;
using Syncd.logging;
using Syncd.Models.Playlist;

namespace Syncd.api.receiver
{
    public class NoEmbedHandler
    {
        
        protected readonly string apiName = "noembed";
        protected readonly string baseAddress = "https://noembed.com/";
        protected readonly string resourceAddressStart = "embed?url=http://www.youtube.com/watch?v=";
        private ILogger logger;

        private ApiManager apiManager;

        public NoEmbedHandler()
        {
            apiManager = ApiManager.GetSingletonInstance();

            if(!apiManager.IsNameRegistered(apiName))
            {
                apiManager.RegisterNewApi(apiName, baseAddress);
            }

            logger = LoggingHandler.CreateLogger<NoEmbedHandler>();
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
                logger.LogError("[VSY] ApiException in NoEmbedHandler.GetYoutubeData, statusCode =  " + e.statusCode + " with message " + e.Message + ", e = " + e);
                return null;
            }
        }
    }
}
