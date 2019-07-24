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
using HakeemTestV4.Database;

namespace HakeemTestV4.Dialogs
{
    public class WelcomeDialog : ComponentDialog
    {
        private readonly string UserInfo = "UserInfo";
        private IStatePropertyAccessor<UserDataCollection> userStateAccessor;
        public WelcomeDialog(UserState userState) : base("WelcomeDialog")
        {
            userStateAccessor = userState.CreateProperty<UserDataCollection>(nameof(UserDataCollection));
            
            AddDialog(new TextPrompt("text"));
            AddDialog(new WaterfallDialog("waterfall", new WaterfallStep[]
            {
                StartDialogAsync,
                ProcessLanguageChoice,
                NamePrompt,
                ProcessName,
            }));
            InitialDialogId = "waterfall";
        }

        private async Task<DialogTurnResult> StartDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create an object in which to collect the user's information within the dialog.
            UserDataCollection userProfile = await userStateAccessor.GetAsync(stepContext.Context, () => new UserDataCollection());
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
            UserDataCollection userProfile = await userStateAccessor.GetAsync(stepContext.Context, () => new UserDataCollection());

            if (choice.ToLower() == "english")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Great! I'll use English from now on"));
                userProfile.language = "en";
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("عظيم سأستخدم اللغة العربية"));
                userProfile.language = "ar";
            }

            return await stepContext.NextAsync();
            
        }

        private async Task<DialogTurnResult> NamePrompt(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var prompt_options = new PromptOptions { Prompt = MessageFactory.Text("I see we haven't met before. Before we get started, what should I call you?") };
            return await stepContext.PromptAsync("text", prompt_options, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserDataCollection userProfile = await userStateAccessor.GetAsync(stepContext.Context, () => new UserDataCollection());
            string answer = (string)stepContext.Result;

            userProfile.Name = answer;
            await SaveConversationData.SaveNewUser(stepContext.Context.Activity.From.Id, answer);
            return await stepContext.EndDialogAsync(userProfile);
        }
    }
}