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
	public class CheckCourseDialog : ComponentDialog
    {
        private IStatePropertyAccessor<UserDataCollection> _userStateAccessor;
        public CheckCourseDialog(UserState userState) : base(nameof(CheckCourseDialog))
        {
            _userStateAccessor = userState.CreateProperty<UserDataCollection>(nameof(UserDataCollection));

            InitialDialogId = "waterfall";

            AddDialog(new WaterfallDialog("waterfall", new WaterfallStep[]
            {
                CheckCourse
            }));
            // AddDialog(new LearningDialog(userState));

        }

		private async Task<DialogTurnResult> CheckCourse(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync();
        }
    }
}