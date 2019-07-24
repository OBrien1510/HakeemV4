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
    public class RootDialog : ComponentDialog
    {

        private IStatePropertyAccessor<UserDataCollection> _userStateAccessor;
        protected readonly string endpoint = "https://northeurope.api.cognitive.microsoft.com/luis/v2.0/apps/285d2e50-ca52-41a6-9325-19c10303a0b9?verbose=true&timezoneOffset=60&subscription-key=ea767ffa5069422b9f377427b35c412e&q=";
        private UserState userState;
        public RootDialog(UserState userState)
            : base("root")
        {

            // add check here to see if a new user has been added (send to welcome)
            
            this.userState = userState;
            _userStateAccessor = userState.CreateProperty<UserDataCollection>(nameof(UserDataCollection));
            AddDialog(new WaterfallDialog("waterfall", new WaterfallStep[] {
                StartDialogAsync,
                HowAreYou,
                HowAreYouResponse,
            }));
            AddDialog(new TextPrompt("text"));
            AddDialog(new LuisDialog(userState));
            
            
            
            InitialDialogId = "waterfall";

        }

        private async Task<DialogTurnResult> StartDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            bool userExists = SaveConversationData.CheckUserExists(stepContext.Context.Activity.From.Id);
            // check if user is new (if so, send welcome message)
            if (!userExists)
            {
                AddDialog(new WelcomeDialog(userState));
                return await stepContext.ReplaceDialogAsync(nameof(WelcomeDialog));
            }
            
            UserDataCollection user = SaveConversationData.GetUserDataCollection(stepContext.Context.Activity.From.Id).Result;
            await _userStateAccessor.SetAsync(stepContext.Context, user);
            return await stepContext.NextAsync(user);
            
        }



        private async Task<DialogTurnResult> HowAreYou(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserDataCollection user = (UserDataCollection)stepContext.Result;
            // var userStateAccessors = userState.CreateProperty<UserDataCollection>(nameof(UserDataCollection));
            // var userProfile = await userStateAccessors.GetAsync(stepContext.Context, () => new UserDataCollection());
            PromptOptions promptOption = new PromptOptions { Prompt = MessageFactory.Text("Hello " + user.Name + ", how are you today?") };
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