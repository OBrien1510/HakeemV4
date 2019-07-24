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
        public RecommendDialog(UserState userState) : base(nameof(RecommendDialog))
        {
            InitialDialogId = "waterfall";
            AddDialog(new WaterfallDialog("waterfall", new WaterfallStep[]
            {
                StartAsync,
            }));
        }

        private async Task<DialogTurnResult> StartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserDataCollection user = await SaveConversationData.GetUserDataCollection(stepContext.Context.Activity.From.Id);
            List<CourseList> course = new List<CourseList>();
            List<string> interest = user.interests;
            List<UserCourse> past_courses = user.PastCourses;
            if (past_courses.Count > 0)
            {
                Debug.WriteLine("here");
                string[] course_to_sub = CourseToInterest(past_courses);
                Debug.WriteLine("here2");
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
            Debug.WriteLine("count " + course.Count);
            // string language = context.UserData.GetValue<string>("inputLanguage");
            if (course.Count == 0)
            {
                //RecommendMostPopular();
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Sorry, I couldn't find any courses for you, but you can look through all the topics and subtopics I know about if you like."));
                //context.Done(true);
            }
            else
            {
                Debug.WriteLine(restrictive);
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
                string language = "en";
                var reply = stepContext.Context.Activity.CreateReply();
                List<CardAction> course_suggestions = new List<CardAction>();
                for (int i = 0; i < course.Count; i++)
                {
                    if (course[i] == null)
                    {
                        break;
                    }

                    if (language == "en")
                    {
                        course_suggestions.Add(new CardAction() { Title = course[i].courseName, Type = ActionTypes.ImBack, Value = course[i].courseName });
                    }
                    else
                    {
                        course_suggestions.Add(new CardAction() { Title = course[i].courseNameArabic, Type = ActionTypes.ImBack, Value = course[i].courseNameArabic });
                    }
                }
                if (language == "en")
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
    }


}