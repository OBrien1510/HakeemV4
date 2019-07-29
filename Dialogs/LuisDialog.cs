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

namespace HakeemTestV4.Dialogs
{
    public class LuisDialog : ComponentDialog
    {

        protected readonly LuisApplication luisApplication;
        private UserState userState;

        public LuisDialog(UserState userState) : base(nameof(LuisDialog))
        {
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
                    Debug.WriteLine("entity " + entity);
                    AddDialog(new LearningDialog(userState, entity));
                    return await stepContext.ReplaceDialogAsync(nameof(LearningDialog));
                case "suggestion":
                    AddDialog(new RecommendDialog(userState));
                    return await stepContext.ReplaceDialogAsync(nameof(RecommendDialog));
                case "Preferences":
                    AddDialog(new EditPreferences(userState));
                    return await stepContext.ReplaceDialogAsync(nameof(EditPreferences));
                default:
                    AddDialog(new LearningDialog(userState, null));
                    return await stepContext.ReplaceDialogAsync(nameof(LearningDialog));

            }
            AddDialog(new LearningDialog(userState, null));
            return await stepContext.ReplaceDialogAsync(nameof(LearningDialog));
        }

        private async Task<DialogTurnResult> LearningIntent(WaterfallStepContext stepContext, RecognizerResult result)
        {
            string entity = result.Entities["subject"]?.First.ToString();
            
            return await stepContext.ReplaceDialogAsync(nameof(LearningDialog), entity);
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
