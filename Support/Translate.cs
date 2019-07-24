using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HakeemTestV4.SupportClasses
{
    class Translate
    {
        //Global Variables for each function
        static string host = "https://api.cognitive.microsofttranslator.com";
        static string key = "4e4825d422c24aeaae56ff17833bdaec";

        //translate
        static string transPath = "/translate?api-version=3.0";
        //static string params_ = "&to=ar";
        static string transUri = host + transPath; // + params_;

        //detect
        static string detectPath = "/detect?api-version=3.0";
        static string detectUri = host + detectPath;

        /*Function responsible for translating any messages sent in any language other than English             
         Currently only used to translate to Classic Arabic but could be expanded to include other languages
         */
        public async static Task<string> Translator(string text, string language)
        {
            // Takes in the language we want the text translated to and then calls detect to figure out which language we need translating from
            System.Object[] body = new System.Object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(transUri + "&to=" + language);//combine URI with the context language for translation

                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                //var result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(responseBody), Formatting.Indented);

                var result = JArray.Parse(responseBody)[0]["translations"][0]["text"];//returns JSON text, relevant value is extracted here

                return result.ToString();
            }
        }

        /*Function to detect the language the message was sent in
         Uses same key and api as translator function above
         Useful for when user changes language mid conversation- Hakeem can then respond in same language
         Almost identical to translator function in terms of structure*/
        public async static Task<string> Detect(string text)
        {
            System.Object[] body = new System.Object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(detectUri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                var result = JArray.Parse(responseBody)[0]["language"];
                return result.ToString();
            }
        }
    }
}
