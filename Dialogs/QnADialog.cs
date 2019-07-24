using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Schema;
using HakeemTestV4.Dialogs;
using HakeemTestV4.Database;

namespace HakeemTestV4.Dialogs
{
    public class QnADialog : ComponentDialog
    {
        static string host = "https://westus.api.cognitive.microsoft.com";
        static string service = "/qnamaker/v4.0";
        static string baseRoute = "/knowledgebases";
        static string endpointService = "/qnamaker";
        static string key = "e7a4ba95553b4d00989df79682fdf319";
        static string kbid = "bdb1f742-4a70-403f-8d62-8e3b8378b5d8";
        static string endpoint_host = "https://hakeemqnav2.azurewebsites.net";
        static string endpoint_key = "ef4dc03b-4086-4a06-9181-8b9e2b56f20f";
        public string question = "";
        
        public QnADialog(UserState userState) : base(nameof(QnADialog))
        {
            InitialDialogId = "waterfall";

        }
    }
}