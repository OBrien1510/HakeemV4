using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using HakeemTestV4.SupportClasses;
using HakeemTestV4.Database;


namespace HakeemTestV4.Dialogs
{
    public class RecommendDialog : ComponentDialog
    {
        private IStatePropertyAccessor<UserDataCollection> _userStateAccessor;
        private UserState userState;
        UserDataCollection userProfile;
        public RecommendDialog(UserState userState) : base(nameof(RecommendDialog))
        {
            _userStateAccessor = userState.CreateProperty<UserDataCollection>(nameof(UserDataCollection));
            
            InitialDialogId = "waterfall";
            AddDialog(new WaterfallDialog("waterfall", new WaterfallStep[]
            {
                StartAsync,
                SelectCourse
            }));
        }

        private async Task<DialogTurnResult> StartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            UserDataCollection user = await SaveConversationData.GetUserDataCollection(stepContext.Context.Activity.From.Id);
            UserDataCollection userProfile = await _userStateAccessor.GetAsync(stepContext.Context, () => new UserDataCollection());
            string language = userProfile.language;
            List<CourseList> course = new List<CourseList>();
            List<string> interest = user.interests;
            List<UserCourse> past_courses = user.PastCourses;
            
            if (past_courses.Count > 0)
            {
                
                string[] course_to_sub = CourseToInterest(past_courses);
                
                interest.AddRange(course_to_sub);
            }

            if (interest.Count == 0)
            {
                // If no interest present, then recommend the most populaar course (or random course)
                // RecommendMostPopular();
                // return;
            }

            
            Random rnd = new Random();

            int restrictive = 0;
            List<CourseList> course_current = new List<CourseList>();
            // make sure all interests have a chance to be checked for courses
            // we will cull the list down later at random so it doesn't get too long for the user
            for (int i = 0; i < interest.Count; i++)
            {

                // only select element from all the 'live' elements
                int index = rnd.Next(i, interest.Count);

                string current_interest = interest[index];
                // if an interest has been 'used' move to the front of list, element is now dead and will not be considered by the rng
                interest.Insert(0, interest[index]);
                interest.RemoveAt(index + 1);

                course_current = SaveConversationData.TryMatchCourse(current_interest, user.accreditation, user.delivery, true, user.PreferedLang, user.education, restrictive);
               
                course.AddRange(course_current);

                if (i == interest.Count - 1 && course.Count == 0)
                {
                    if (restrictive == 3)
                    {
                        // we've had 3 failed passes, give up without finding any courses
                        // later we should instead recommend the most popular course (collabartive filtering)

                        break;
                    }
                    // first 'pass' has failed to find any courses, start a new 'pass' with less restrictive parameters
                    i = 0;
                    i--; // cancel out the i++
                    restrictive++;
                }
            }

            course = ReduceLength(course);
            
            // string language = context.UserData.GetValue<string>("inputLanguage");
            if (course.Count == 0)
            {
                //RecommendMostPopular();
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Sorry, I couldn't find any courses for you, but you can look through all the topics and subtopics I know about if you like."));
                //context.Done(true);
            }
            else
            {
                
                string text = "Based on some of the preferences you have shared with me, here are all the courses I think you might like";
                if (restrictive != 0)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("I couldn't find any courses that exactly matched you preferences so I had to leave out the following preferences during my search: "));
                    switch (restrictive)
                    {
                        case 1:
                            await stepContext.Context.SendActivityAsync("\u2022 Your education level");
                            goto case 2;
                        case 2:
                            await stepContext.Context.SendActivityAsync("\u2022 Your prefered language");
                            goto case 3;
                        case 3:
                            await stepContext.Context.SendActivityAsync("\u2022 Your self paced preference");
                            break;
                        default:
                            break;

                    }
                }
                // var reply = context.MakeMessage();
                
                var reply = stepContext.Context.Activity.CreateReply();
                List<CardAction> course_suggestions = new List<CardAction>();
                for (int i = 0; i < course.Count; i++)
                {
                    if (course[i] == null)
                    {
                        break;
                    }

                    if (language.ToLower() == "english")
                    {
                        course_suggestions.Add(new CardAction() { Title = course[i].courseName, Type = ActionTypes.ImBack, Value = course[i].courseName });
                    }
                    else
                    {
                        course_suggestions.Add(new CardAction() { Title = course[i].courseNameArabic, Type = ActionTypes.ImBack, Value = course[i].courseNameArabic });
                    }
                }
                if (language.ToLower() == "english")
                {
                    reply.Text = "Based on some of the preferences you have shared with me, here are all the courses I think you might like";
                    course_suggestions.Add(new CardAction() { Title = "Start Over", Type = ActionTypes.ImBack, Value = "Start Over" });
                }
                else
                {
                    reply.Text = "استنادًا إلى بعض التفضيلات التي قمت بمشاركتها معي ، إليك جميع الدورات التدريبية التي قد تعجبك";
                    course_suggestions.Add(new CardAction() { Title = "ابدأ من جديد", Type = ActionTypes.ImBack, Value = "ابدأ من جديد" });
                }
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = course_suggestions
                };
                await stepContext.Context.SendActivityAsync(reply);
            }
            
            return await stepContext.EndDialogAsync();

        }

        private async Task<DialogTurnResult> SelectCourse(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string choice = (string)stepContext.Result;
            UserDataCollection userProfile = await _userStateAccessor.GetAsync(stepContext.Context, () => new UserDataCollection());
            string language = userProfile.language;
            CourseList course = SaveConversationData.GetCourseByName(choice);
            stepContext.Values["CurrentCourse"] = course;
            if (course != null)
            {
                await DisplayFinalCourse(stepContext, course);
                return await stepContext.NextAsync();
            }
            else
            {
                AddDialog(new LuisDialog(userState));
                return await stepContext.ReplaceDialogAsync(nameof(LuisDialog));
            }
        }

        private async Task<DialogTurnResult> ConfirmCourse(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string choice = (string)stepContext.Result;
            UserDataCollection userProfile = await _userStateAccessor.GetAsync(stepContext.Context, () => new UserDataCollection());
            string language = userProfile.language;
            CourseList course = (CourseList)stepContext.Values["CurrentCourse"];
            if (choice.ToLower() == "take course")
            {
                UserCourse user_course = new UserCourse()
                {
                    Name = course.courseName,
                    NameArabic = course.courseNameArabic,
                    Complete = false,
                    Date = DateTime.Now,
                    InProgress = false,
                    Queried = false,
                    Rating = 0,
                    Taken = false
                };
                
                await SaveConversationData.SaveUserCourse(stepContext.Context.Activity.From.Id, user_course);
                AddDialog(new LearningDialog(userState, null));
                return await stepContext.ReplaceDialogAsync(nameof(LearningDialog));
            }
            else if (choice.ToLower() == "go back")
            {
                stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 3;
                return await stepContext.NextAsync();
            }
            else
            {
                AddDialog(new LuisDialog(userState));
                return await stepContext.ReplaceDialogAsync(nameof(LuisDialog));
            }
        }

        private string[] CourseToInterest(List<UserCourse> past_courses)
        {
            
            string[] subjects = new string[past_courses.Count];
            dynamic[] time = new dynamic[past_courses.Count];
            for (int i = 0; i < past_courses.Count; i++)
            {
                string sub = SaveConversationData.CourseToSubject(past_courses[i].Name);
                subjects[i] = sub;
                time[i] = past_courses[i].Date;
            }
            
            bool sorted = false;
            bool swap = false;
            // bubble sort to sort the topics by most recent course taken (ie, most recent course topic at start of array)
            while (!sorted)
            {
                swap = false;
                dynamic last = time[0];
                for (int i = 1; i < subjects.Length; i++)
                {
                    if (time[i] < time[i - 1])
                    {
                        dynamic tmp = time[i];
                        time[i] = time[i - 1];
                        time[i - 1] = tmp;
                        tmp = subjects[i];
                        subjects[i] = subjects[i - 1];
                        subjects[i - 1] = tmp;
                        swap = true;
                        break;
                    }
                }

                if (!swap) { sorted = true; }
            }

            return subjects;
        }

        private List<CourseList> ReduceLength(List<CourseList> courses)
        {
            // reduce list of courses so it's not too long
            if (courses.Count <= 5)
            {
                return courses;
            }

            Random rnd = new Random();
            while (courses.Count > 5)
            {
                int rand = rnd.Next(0, courses.Count);
                courses.RemoveAt(rand);
            }

            return courses;
        }

        private async Task<DialogTurnResult> DisplayFinalCourse(WaterfallStepContext stepContext, CourseList course)
        {

            UserDataCollection userProfile = await _userStateAccessor.GetAsync(stepContext.Context, () => new UserDataCollection());
            string language = userProfile.language;
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