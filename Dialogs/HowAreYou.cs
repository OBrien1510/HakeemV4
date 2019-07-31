using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.AI.Luis;
using HakeemTestV4.Database;

namespace HakeemTestV4.Dialogs
{
    public class HowAreYou : ComponentDialog
    {
        private IStatePropertyAccessor<UserDataCollection> _userStateAccessor;
        private UserState userState;
        protected readonly string endpoint = "https://northeurope.api.cognitive.microsoft.com/luis/v2.0/apps/285d2e50-ca52-41a6-9325-19c10303a0b9?verbose=true&timezoneOffset=60&subscription-key=ea767ffa5069422b9f377427b35c412e&q=";

        public HowAreYou(UserState userState) : base(nameof(HowAreYou))
        {
            this.userState = userState;
            _userStateAccessor = userState.CreateProperty<UserDataCollection>(nameof(UserDataCollection));

            AddDialog(new WaterfallDialog("waterfall", new WaterfallStep[]
            {
                HowAreYouQuestion,
                HowAreYouResponse
            }));
        }

        private async Task<DialogTurnResult> HowAreYouQuestion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserDataCollection userProfile = await _userStateAccessor.GetAsync(stepContext.Context, () => new UserDataCollection());
            // var userStateAccessors = userState.CreateProperty<UserDataCollection>(nameof(UserDataCollection));
            // var userProfile = await userStateAccessors.GetAsync(stepContext.Context, () => new UserDataCollection());
            PromptOptions promptOption = new PromptOptions { Prompt = MessageFactory.Text("Hello " + userProfile.Name + ", how are you today?") };
            return await stepContext.PromptAsync("text", promptOption);
        }

        private async Task<DialogTurnResult> HowAreYouResponse(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string query = endpoint + (string)stepContext.Result;
            string sentiment;
            using (WebClient client = new WebClient())
            {
                string response = client.DownloadString(query);

                QueryResponse output = new QueryResponse(response);
                sentiment = output.sentimentAnalysis.sentiment;
            }


            switch (sentiment.ToLower())
            {
                case "positive":
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Glad to hear it!"));
                    break;
                case "negative":
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("I'm sorry to hear that! Maybe some of my learning resources can make you feel a better."));
                    break;
                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Let's get to learning then!"));
                    break;
            }
            AddDialog(new CommandDialog(userState));
            return await stepContext.ReplaceDialogAsync(nameof(CommandDialog), cancellationToken);
        }
    }
}