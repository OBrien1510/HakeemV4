// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;

namespace HakeemTestV4.Bots
{
    public class HakeemTestV4<T> : ActivityHandler where T : Dialog
    {

    protected readonly BotState _conversationState;
    protected readonly Dialog Dialog;
    protected readonly ILogger Logger;
    protected readonly BotState _userState;

    public HakeemTestV4(ConversationState conversationState, UserState userState, T dialog, ILogger<HakeemTestV4<T>> logger)
    {
        _conversationState = conversationState;
        _userState = userState;
        Dialog = dialog;
        Logger = logger;
    }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationProfile>(nameof(ConversationProfile));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationProfile());

            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());
            await Dialog.Run(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
            //await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

    }
}
