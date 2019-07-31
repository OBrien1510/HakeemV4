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
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using HakeemTestV4.Database;

namespace HakeemTestV4.Dialogs
{
    public class WelcomeDialog : ComponentDialog
    {
        private readonly string UserInfo = "UserInfo";
        private IStatePropertyAccessor<UserDataCollection> userStateAccessor;
        UserState userState;
        public WelcomeDialog(UserState userState) : base("WelcomeDialog")
        {
            userStateAccessor = userState.CreateProperty<UserDataCollection>(nameof(UserDataCollection));
            this.userState = userState;
            AddDialog(new TextPrompt("text"));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog("waterfall", new WaterfallStep[]
            {
                StartDialogAsync,
                ProcessLanguageChoice,
                GenderPrompt,
                ProcessGender,
                NamePrompt,
                ProcessName,
                
            }));
            InitialDialogId = "waterfall";
        }

        private async Task<DialogTurnResult> StartDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create an object in which to collect the user's information within the dialog.
            UserDataCollection userProfile = await userStateAccessor.GetAsync(stepContext.Context, () => new UserDataCollection());
            stepContext.Values["User"] = userProfile;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("مرحبا! أنا حكيم ، رفيق التعليم الافتراضي الخاص بك. من المثير أن تساعدك على اكتشاف وتعلم أشياء جديدة."));
            var reply = MessageFactory.Text("Hi!I'm Hakeem, your virtual Learning Companion. It's exciting to help you discover and learn new things.");
            
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Continue in English", Type = ActionTypes.ImBack, Value = "English" },
                    new CardAction() { Title = "تواصل باللغة العربية ى", Type = ActionTypes.ImBack, Value = "عربى" },
                }
            };
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            var promptOptions = new PromptOptions {  };

            return await stepContext.PromptAsync("text", promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessLanguageChoice(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string choice = (string)stepContext.Result;
            UserDataCollection userProfile = (UserDataCollection)stepContext.Values["User"];

            if (choice.ToLower() == "english")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Great! I'll use English from now on"));
                userProfile.language = "english";
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("عظيم سأستخدم اللغة العربية"));
                userProfile.language = "arabic";
            }
            stepContext.Values["User"] = userProfile;
            return await stepContext.NextAsync();
            
        }

       

        private async Task<DialogTurnResult> GenderPrompt(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserDataCollection user = (UserDataCollection)stepContext.Values["User"];
            string language = user.language;
            List<string> options = new List<string>();
            options.Add("Male");
            options.Add("Female");
            options.Add("Prefer not say");
            PromptOptions prompt = new PromptOptions
            {
                Prompt = MessageFactory.Text("For the Arabic language it is important I talk to you in the correct gender/nSo what gender would you like me to address you as?"),
                Choices = ChoiceFactory.ToChoices(options),
                RetryPrompt = MessageFactory.Text("Sorry that wasn't one of the options, please try again")
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), prompt);
            
        }

        private async Task<DialogTurnResult> ProcessGender(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var result = (FoundChoice)stepContext.Result;
            string gender = result.Value;
            
            UserDataCollection userProfile = (UserDataCollection)stepContext.Values["User"];
            userProfile.gender = gender;
            
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> NamePrompt(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var prompt_options = new PromptOptions { Prompt = MessageFactory.Text("Before we get started, what should I call you?") };
            return await stepContext.PromptAsync("text", prompt_options, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Ok thanks! Now the boring stuff is out of the way we can get on to the good stuff!"));

            UserDataCollection userProfile = (UserDataCollection)stepContext.Values["User"];
            string answer = (string)stepContext.Result;

            userProfile.Name = answer;
            await SaveConversationData.SaveNewUser(stepContext.Context.Activity.From.Id, userProfile);
            await userStateAccessor.SetAsync(stepContext.Context, userProfile);
            AddDialog(new HowAreYou(userState));
            Debug.WriteLine("Sending to how");
            return await stepContext.ReplaceDialogAsync(nameof(HowAreYou));
        }
    }
}