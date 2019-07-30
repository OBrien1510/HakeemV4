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
using HakeemTestV4.SupportClasses;

namespace HakeemTestV4.Dialogs
{
	public class LearningDialog : ComponentDialog
    {
        private IStatePropertyAccessor<UserDataCollection> _userStateAccessor;
        private UserState userState;
        private string start;
        public LearningDialog(UserState userState, string start) : base(nameof(LearningDialog))
        {
            this.userState = userState;
            this.start = start;
            Debug.WriteLine("start: " + start);
            _userStateAccessor = userState.CreateProperty<UserDataCollection>(nameof(UserDataCollection));

            AddDialog(new WaterfallDialog("waterfall", new WaterfallStep[]
            {
				CheckCourses,
                DisplayTopics,
				DisplaySubTopics,
				DisplayCourses,
				DisplayFinalCourse
            }));
            AddDialog(new WaterfallDialog("waterfall2", new WaterfallStep[]
           {
                DisplaySubTopics,
                DisplayCourses,
                DisplayFinalCourse
           }));
            AddDialog(new CheckCourseDialog(userState));
            
            AddDialog(new TextPrompt("text"));
            if (this.start == null)
            {
                Debug.WriteLine("here2");
                InitialDialogId = "waterfall";
            }
            else
            {
                Debug.WriteLine("Here");
                InitialDialogId = "waterfall2";
            }
        }

		private async Task<DialogTurnResult> CheckCourses(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
			
            UserDataCollection user = await SaveConversationData.GetUserDataCollection(stepContext.Context.Activity.From.Id);
            
            List<UserCourse> courses = user.PastCourses;
            UserCourse current_course = new UserCourse();
            Random rnd = new Random();
            int start = user.PastCourses.Count - 1;
            int random = 0;
            bool found = false;
            while (start >= 0)
            {
                
                random = rnd.Next(0, start);
                current_course = courses[random];
                courses.RemoveAt(random);
                courses.Add(current_course);
                start--;
                DateTime now = DateTime.Now;
                TimeSpan diff = now - current_course.Date;

                if (!current_course.InProgress && !current_course.Taken && !current_course.Queried && diff.TotalDays >= 700)
                {
                    found = true;
                    
                    break;
                }
            }
            
            if (found)
            {
                // return await stepContext.BeginDialogAsync(nameof(CheckCourseDialog));
                // stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                // return await stepContext.NextAsync();
            }
           
            return await stepContext.NextAsync();

            
        }

		private async Task<DialogTurnResult> DisplayTopics(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserDataCollection userProfile = await _userStateAccessor.GetAsync(stepContext.Context, () => new UserDataCollection());
            stepContext.Values["SessionLang"] = userProfile.language;
            var reply = MessageFactory.Text("");
            if (userProfile.language.ToLower() == "english")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Let's get to learning then!"));
                reply.Text = "Please select the subject that you want to explore.";
                List<dynamic> unique_topics = SaveConversationData.GetUniqueTopics();
                List<CardAction> childSuggestions = new List<CardAction>();
                foreach (string topic in unique_topics)
                {
                    childSuggestions.Add(new CardAction() { Title = topic, Type = ActionTypes.ImBack, Value = topic });
                }
                childSuggestions.Add(new CardAction() { Title = "Go Back", Type = ActionTypes.ImBack, Value = "Go Back" });
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = childSuggestions
                };
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(await Translate.Translator("Let's get to learning then!", "ar")));
                reply.Text = await Translate.Translator("Please select the subject that you want to explore.", "ar");
                List<dynamic> unique_topics = SaveConversationData.GetUniqueTopics();
                List<CardAction> childSuggestions = new List<CardAction>();
                foreach (string topic in unique_topics)
                {
                    childSuggestions.Add(new CardAction() { Title = topic, Type = ActionTypes.ImBack, Value = topic });
                }
                childSuggestions.Add(new CardAction() { Title = await Translate.Translator("Go Back", "ar"), Type = ActionTypes.ImBack, Value = await Translate.Translator("Go Back", "ar") });
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = childSuggestions
                };
            }

            await stepContext.Context.SendActivityAsync(reply);

            return await stepContext.PromptAsync("text", new PromptOptions{ }, cancellationToken);
        }

        private async Task<DialogTurnResult> DisplaySubTopics(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserDataCollection userProfile = await _userStateAccessor.GetAsync(stepContext.Context, () => new UserDataCollection());
            stepContext.Values["SessionLang"] = userProfile.language;
            
            string choice = (string)stepContext.Result;
            
            if (String.IsNullOrEmpty(choice) && stepContext.Values.ContainsKey("CurrentTopic"))
            {
                choice = (string)stepContext.Values["currentTopic"];
            }
            else if (String.IsNullOrEmpty(choice))
            {
                choice = start;
            }
            else
            {
                stepContext.Values["currentTopic"] = choice;
            }
            if (choice.ToLower() == "go back")
            {
                stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 3;
                return await stepContext.NextAsync();
            }
            List<dynamic> unique_subtopics = SaveConversationData.GetUniqueSubTopics(choice);
            
            if (unique_subtopics.Count == 0)
            {
                AddDialog(new LuisDialog(userState));
                return await stepContext.ReplaceDialogAsync(nameof(LuisDialog));
            }
            var reply = MessageFactory.Text("");
            if (userProfile.language.ToLower() == "english")
            {
                reply.Text = "Selected the sub-topic you wish to explore";

                List<CardAction> childSuggestions = new List<CardAction>();
                foreach (string topic in unique_subtopics)
                {
                    childSuggestions.Add(new CardAction() { Title = topic, Type = ActionTypes.ImBack, Value = topic });
                }
                childSuggestions.Add(new CardAction() { Title = "Go Back", Type = ActionTypes.ImBack, Value = "Go Back" });
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = childSuggestions
                };
            }
            else
            {
                reply.Text = await Translate.Translator("Selected the sub-topic you wish to explore", "ar");

                List<CardAction> childSuggestions = new List<CardAction>();
                foreach (string topic in unique_subtopics)
                {
                    childSuggestions.Add(new CardAction() { Title = await Translate.Translator(topic, "ar"), Type = ActionTypes.ImBack, Value = await Translate.Translator(topic, "ar") });
                }
                childSuggestions.Add(new CardAction() { Title = await Translate.Translator("Go Back", "ar"), Type = ActionTypes.ImBack, Value = await Translate.Translator("Go Back", "ar") });
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = childSuggestions
                };
            }
            await stepContext.Context.SendActivityAsync(reply);

            return await stepContext.PromptAsync("text", new PromptOptions());
        }

		private async Task<DialogTurnResult> DisplayCourses(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string choice = (string)stepContext.Result;
            Debug.WriteLine($"Choice: {choice}");
            string language = (string)stepContext.Values["SessionLang"];
            if (String.IsNullOrEmpty(choice))
            {
                choice = (string)stepContext.Values["currentSubTopic"];
            }
            else
            {
                stepContext.Values["currentSubTopic"] = choice;
            }
            if (choice.ToLower() == "go back")
            {
                stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 3;
                return await stepContext.NextAsync();
            }
            List<CourseList> course_by_sub = SaveConversationData.MatchCourseBySubTopic(choice);
            if (course_by_sub.Count == 0)
            {
                AddDialog(new LuisDialog(userState));
                return await stepContext.ReplaceDialogAsync(nameof(LuisDialog));
            }

            var reply = MessageFactory.Text("");
            if (language.ToLower() == "english")
            {
                reply.Text = $"Here are all the courses I know on {choice}";

                List<CardAction> childSuggestions = new List<CardAction>();
                foreach (CourseList c in course_by_sub)
                {
                    childSuggestions.Add(new CardAction() { Title = c.courseName, Type = ActionTypes.ImBack, Value = c.courseName });
                }
                childSuggestions.Add(new CardAction() { Title = "Go Back", Type = ActionTypes.ImBack, Value = "Go Back" });
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = childSuggestions
                };
            }
            else
            {
                reply.Text = await Translate.Translator($"Here are all the courses I know on {choice}", "ar");

                List<CardAction> childSuggestions = new List<CardAction>();
                foreach (CourseList c in course_by_sub)
                {
                    childSuggestions.Add(new CardAction() { Title = await Translate.Translator(c.courseName, "ar"), Type = ActionTypes.ImBack, Value = await Translate.Translator(c.courseName, "ar") });
                }
                childSuggestions.Add(new CardAction() { Title = await Translate.Translator("Go Back", "ar"), Type = ActionTypes.ImBack, Value = await Translate.Translator("Go Back", "ar") });
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = childSuggestions
                };
            }
            await stepContext.Context.SendActivityAsync(reply);

            return await stepContext.PromptAsync("text", new PromptOptions());
        }

		private async Task<DialogTurnResult> DisplayFinalCourse(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            string choice = (string)stepContext.Result;
            string language = (string)stepContext.Values["SessionLang"];
            if (String.IsNullOrEmpty(choice))
            {
                choice = (string)stepContext.Values["currentCourse"];
            }
            else
            {
                stepContext.Values["currentCourse"] = choice;
            }
            if (choice.ToLower() == "go back")
            {
                stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 3;
                return await stepContext.NextAsync();
            }
            CourseList course = SaveConversationData.GetCourseByName(choice);
            if (course == null)
            {
                return await stepContext.ReplaceDialogAsync(nameof(LuisDialog));
            }
            string courseInfo = "";
            var options = MessageFactory.Text("");
            if (language == "english")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Good choice! This is what I know about the course {course.courseName}"));

                if (course.selfPaced)
                {
                    courseInfo += "This is a self-paced course, taking approximately " + course.approxDuration.ToString() + " hours\n\n";
                }
                else
                {
                    courseInfo += "This is a scheduled-learning course, taking approximately " + course.approxDuration.ToString() + " hours\n\n";
                }
                if (course.accreditationOption)
                {
                    courseInfo += "There is an option for accreditation available for this course, although a charge may apply\n\n";
                }
                else
                {
                    courseInfo += "There is no option for accreditation available\n\n";
                }
                if (course.financialAid)
                {
                    courseInfo += "Financial aid is available and can be applied for\n\n";
                }
                else
                {
                    courseInfo += "There is no financial aid available\n\n";
                }
                courseInfo += "The course description is here:\n";
                courseInfo += course.description;
                await stepContext.Context.SendActivityAsync(courseInfo);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(await Translate.Translator($"Good choice! This is what I know about the course {course.courseName}", "ar")));
                courseInfo = "";
                if (course.selfPaced)
                {
                    courseInfo += await Translate.Translator("This is a self-paced course, taking approximately " + course.approxDuration.ToString() + " hours\n\n", "ar");
                }
                else
                {
                    courseInfo += await Translate.Translator("This is a scheduled-learning course, taking approximately " + course.approxDuration.ToString() + " hours\n\n", "ar");
                }
                if (course.accreditationOption)
                {
                    courseInfo += await Translate.Translator("There is an option for accreditation available for this course, although a charge may apply\n\n", "ar");
                }
                else
                {
                    courseInfo += await Translate.Translator("There is no option for accreditation available\n\n", "ar");
                }
                if (course.financialAid)
                {
                    courseInfo += await Translate.Translator("Financial aid is available and can be applied for\n\n", "ar");
                }
                else
                {
                    courseInfo += await Translate.Translator("There is no financial aid available\n\n", "ar");
                }
                courseInfo += await Translate.Translator("The course description is here:\n", "ar");
                courseInfo += course.description;
                
                await stepContext.Context.SendActivityAsync(courseInfo);
                options.Text = await Translate.Translator("Would you like to take this course?", "ar");
                options.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                        {
                            new CardAction(){ Title = await Translate.Translator("Take Course", "ar"), Type = ActionTypes.ImBack, Value = await Translate.Translator("Take Course", "ar") },
                            new CardAction(){ Title = await Translate.Translator("Go Back", "ar"), Type = ActionTypes.ImBack, Value = await Translate.Translator("Go Back", "ar")}
                        }
                };

            }
            await stepContext.Context.SendActivityAsync(courseInfo);
            options.Text = "Would you like to take this course?";
            
            options.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                        {
                            new CardAction(){ Title = "Take Course", Type = ActionTypes.ImBack, Value = "Take Course" },
                            new CardAction(){ Title = "Go Back", Type = ActionTypes.ImBack, Value = "Go Back"}
                        }
            };

            await stepContext.Context.SendActivityAsync(options);

            return await stepContext.EndDialogAsync();
        }

       
    }
}
