using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HakeemTestV4.Database;

namespace HakeemTestV4.Dialogs
{
    public class LuisDialog : ComponentDialog
    {

        protected readonly LuisApplication luisApplication;
        private UserState userState;
        private IStatePropertyAccessor<UserDataCollection> userStateAccessor;
        public LuisDialog(UserState userState) : base(nameof(LuisDialog))
        {
            userStateAccessor = userState.CreateProperty<UserDataCollection>(nameof(UserDataCollection));
            this.userState = userState;
            luisApplication = new LuisApplication(
                "285d2e50-ca52-41a6-9325-19c10303a0b9",
                "ea767ffa5069422b9f377427b35c412e",
                "https://northeurope.api.cognitive.microsoft.com"
            );
            
            AddDialog(new WaterfallDialog("waterfall", new WaterfallStep[]
            {
                
                GetIntent
            }));
            
            
            
            
            InitialDialogId = "waterfall";
        }

        private async Task<DialogTurnResult> GetIntent(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            var recognizer = new LuisRecognizer(luisApplication);
            
            var recognizerResult = await recognizer.RecognizeAsync(stepContext.Context, cancellationToken);
            var (intent, score) = recognizerResult.GetTopScoringIntent();
            Debug.WriteLine($"intent {intent}");
            switch (intent)
            {
                case "command":
                    await CommandIntent(stepContext);
                    break;
                case "Question":
                    await QuestionIntent(stepContext);
                    break;
                case "learning":
                    
                    string entity = recognizerResult.Entities["subject"]?.First.ToString();
                    
                    AddDialog(new LearningDialog(userState, entity));
                    return await stepContext.ReplaceDialogAsync(nameof(LearningDialog));
                case "suggestion":
                    AddDialog(new RecommendDialog(userState));
                    return await stepContext.ReplaceDialogAsync(nameof(RecommendDialog));
                case "Preferences":
                    AddDialog(new EditPreferences(userState));
                    return await stepContext.ReplaceDialogAsync(nameof(EditPreferences));
                case "Restart":
                    AddDialog(new LearningDialog(userState, null));
                    return await stepContext.ReplaceDialogAsync(nameof(LearningDialog));
                case "ChangeLanguage":
                    await ChangeLanguage(stepContext, recognizerResult);
                    break;
                case "None":
                case "Gibberish":
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Sorry I understand that!"));
                    break;
                default:
                    AddDialog(new LearningDialog(userState, null));
                    return await stepContext.ReplaceDialogAsync(nameof(LearningDialog));

            }
            AddDialog(new LearningDialog(userState, null));
            UserDataCollection userProfile = await userStateAccessor.GetAsync(stepContext.Context, () => new UserDataCollection());
            Debug.WriteLine($"Language after {userProfile.language}");
            return await stepContext.ReplaceDialogAsync(nameof(LearningDialog));
        }

        private async Task ChangeLanguage(WaterfallStepContext stepContext, RecognizerResult result)
        {
            string entity = result.Entities["Language"]?.First.ToString();
            UserDataCollection userProfile = await userStateAccessor.GetAsync(stepContext.Context, () => new UserDataCollection());
            Debug.WriteLine($"Language before {userProfile.language}");
            Debug.WriteLine("entity " + entity);
            // user has specified which language they want to change to
            if (entity != null && entity.ToLower() == "arabic" || entity.ToLower() == "english")
            {
                userProfile.language = entity.ToLower();
            }
            // user has not specified the language but intends to change language
            else
            {
                if (userProfile.language.ToLower() == "english")
                {
                    userProfile.language = "arabic";
                }
                else
                {
                    userProfile.language = "english";
                }
            }

            Debug.WriteLine("Saving");
            await userStateAccessor.SetAsync(stepContext.Context, userProfile);
        }

        
        private async Task CommandIntent(WaterfallStepContext stepContext)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("A list of the commands you can type or say:"));
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("\"Start over\" anytime you want to restart the conversation\n\n\"Subjects\" to view the current subject options\n\n\"My Preferences\" to update your current preferences\n\n\"About Hakeem\" to learn more about me and the value I offer\n\n\"Back\" to return to the previous message\n\n\"Feedback\" if you run into a problem and want to tell us"));
        }

        private async Task QuestionIntent(WaterfallStepContext stepContext)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("About message"));
        }
        
    }

    public class QueryResponse
    {
        public QueryResponse(string json)
        {
            JObject jobject = JObject.Parse(json);
            query = jobject["query"].ToString();
            TopScoreIntent = new Entity(jobject["topScoringIntent"]["intent"].ToString(), jobject["topScoringIntent"]["score"].ToObject<float>());
            entities = jobject["entities"].ToObject<List<string>>();
            var sentiment_an = jobject["sentimentAnalysis"];
            sentimentAnalysis = new SentimentAnalysis(sentiment_an["label"].ToString(), sentiment_an["score"].ToObject<float>());
        }
        public string query { get; set; }
        public Entity TopScoreIntent { get; set; }
        public List<string> entities { get; set; }
        public SentimentAnalysis sentimentAnalysis { get; set; }
    }

    public class SentimentAnalysis
    {
        public SentimentAnalysis(string sentimentType, float score)
        {
            sentiment = sentimentType;
            sentimentScore = score;
        }
        public string sentiment { get; set; }
        public float sentimentScore { get; set; }
    }

    public class Entity
    {
        public Entity(string type, float score)
        {
            entityType = type;
            entityScore = score;
        }
        public string entityType { get; set; }
        public float entityScore { get; set; }
    }
}
