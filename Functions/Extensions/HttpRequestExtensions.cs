using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace Functions.Extensions
{
    public static class HttpRequestExtensions
    {
        public static M ParseQueryString<M>(this HttpRequest req)
        {
            var dict = req.GetQueryParameterDictionary();
            var json = JsonConvert.SerializeObject(dict, Formatting.Indented);

            return JsonConvert.DeserializeObject<M>(json);
        }
    }
}
