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
    public class CommandDialog : ComponentDialog
    {
        private IStatePropertyAccessor<UserDataCollection> _userStateAccessor;
        public CommandDialog(UserState userState) : base(nameof(CommandDialog))
        {
            _userStateAccessor = userState.CreateProperty<UserDataCollection>(nameof(UserDataCollection));

            AddDialog(new WaterfallDialog("waterfall", new WaterfallStep[]
            {
                DisplayCommands,
                ProcessCommand,
            }));
            AddDialog(new TextPrompt("textPrompt"));
            AddDialog(new LearningDialog(userState, null));
            AddDialog(new LuisDialog(userState));

            InitialDialogId = "waterfall";
        }

        private async Task<DialogTurnResult> DisplayCommands(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("You can switch between English and Arabic at any time by simply sending your message in the language you want to use."));
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Before we get started, take a second to learn about what I can do. You can also type “Commands” at any time to view other commands you can use."));
            List<Choice> suggested = new List<Choice>();
            suggested.Add(new Choice { Value = "About" });
            suggested.Add(new Choice { Value = "Commands" });
            suggested.Add(new Choice { Value = "Preferences" });
            var reply = MessageFactory.SuggestedActions(new List<CardAction>
            {
                new CardAction() { Title = "About Hakeem", Type = ActionTypes.ImBack, Value = "About" },
                new CardAction() { Title = "Commands", Type = ActionTypes.ImBack, Value = "Commands" },
                new CardAction() { Title = "My Preferences", Type = ActionTypes.ImBack, Value = "Preferences" },
                new CardAction() { Title = "Begin Learning", Type = ActionTypes.ImBack, Value = "Continue" },
            });
            await stepContext.Context.SendActivityAsync(reply);
            PromptOptions promptOptions = new PromptOptions { };
            return await stepContext.PromptAsync("text", promptOptions);
        }

        private async Task<DialogTurnResult> ProcessCommand(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string choice = (string)stepContext.Result;

            switch (choice)
            {
                case "Continue":
                    return await stepContext.ReplaceDialogAsync(nameof(LearningDialog));
                default:
                    return await stepContext.ReplaceDialogAsync(nameof(LuisDialog));
                    
            }

        }
    }
}
