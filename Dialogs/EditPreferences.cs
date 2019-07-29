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
    public class EditPreferences : ComponentDialog
    {
        UserState userState;
        public EditPreferences(UserState userState) : base(nameof(EditPreferences))
        {
            InitialDialogId = "waterfall";
            this.userState = userState;
            AddDialog(new WaterfallDialog("waterfall", new WaterfallStep[]
            {
                StartAsync,
                AwaitPreference
            }));
            AddDialog(new WaterfallDialog("EditPrefLang", new WaterfallStep[]
            {
                EditPrefLang
            }));
            AddDialog(new WaterfallDialog("EditCourseLang", new WaterfallStep[]
            {
                EditCourseLang
            }));
            AddDialog(new WaterfallDialog("RequestData", new WaterfallStep[]
            {
                RequestData
            }));
            AddDialog(new WaterfallDialog("EditPace", new WaterfallStep[]
            {
                EditPace
            }));
            AddDialog(new WaterfallDialog("EditInterests", new WaterfallStep[]
            {
                EditInterests
            }));
            AddDialog(new WaterfallDialog("EditAccred", new WaterfallStep[]
            {
                EditAccred
            }));
            AddDialog(new WaterfallDialog("EditEducation", new WaterfallStep[]
            {
                EditEducation
            }));
            
            AddDialog(new TextPrompt("text"));
        }

        private async Task<DialogTurnResult> StartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string language = "en";
            
            var reply = stepContext.Context.Activity.CreateReply();
            if (language == "en")
            {
                reply.Text = "You can change your preferences now by selecting one of the options below \n Alternatively, select 'Continue' to begin learning";

                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "Edit Conversation Language", Type=ActionTypes.ImBack, Value="Edit Conversation Language" },
                        new CardAction(){ Title = "Edit Course Delivery Language", Type=ActionTypes.ImBack, Value="Edit Course Language" },
                        //new CardAction(){ Title = "Edit Privacy Consent", Type=ActionTypes.ImBack, Value="Edit Privacy Consent" },
                        new CardAction() { Title = "Edit Name", Type=ActionTypes.ImBack, Value= "Edit Name" },
                        new CardAction() { Title = "Edit Interests", Type = ActionTypes.ImBack, Value = "Edit Interests" },
                        new CardAction() { Title = "Edit Gender preferences", Type = ActionTypes.ImBack, Value = "Edit Gender" },
                        new CardAction() { Title = "Edit Accreditation preferences", Type = ActionTypes.ImBack, Value = "Edit Accreditation" },
                        new CardAction() { Title = "Edit Delivery preferences", Type = ActionTypes.ImBack, Value = "Edit Delivery" },
                        new CardAction() { Title = "Edit Education preferences", Type = ActionTypes.ImBack, Value = "Edit Education" },
                        new CardAction(){ Title = "Request Data", Type=ActionTypes.ImBack, Value="Request Data" },
                        new CardAction(){ Title = "Continue", Type=ActionTypes.ImBack, Value="Continue" },
                        new CardAction(){ Title = "Delete User Data", Type=ActionTypes.ImBack, Value="Delete User" }
                    }
                };
            }
            else
            {
                reply.Text = "هل ترغب في تغيير تفضيلاتك الآن؟";

                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "تحرير لغة المحادثة", Type=ActionTypes.ImBack, Value="تحرير لغة المحادثة" },
                        new CardAction(){ Title = "تحرير لغة تسليم الدورة", Type=ActionTypes.ImBack, Value="تحرير لغة الدورة" },
                        //new CardAction(){ Title = "تحرير موافقة الخصوصية", Type=ActionTypes.ImBack, Value="تحرير موافقة الخصوصية" },
                        new CardAction() { Title = "تعديل الاسم", Type = ActionTypes.ImBack, Value = "تعديل الاسم" },
                        new CardAction() { Title = "تحرير الاهتمامات", Type = ActionTypes.ImBack, Value = "تحرير الاهتمامات" },
                        new CardAction() { Title = "تحرير تفضيل الجنس", Type = ActionTypes.ImBack, Value = "تحرير تفضيل الجنس" },
                        new CardAction() { Title = "تحرير تفضيلات الاعتماد", Type = ActionTypes.ImBack, Value = "تحرير الاعتماد" },
                        new CardAction() { Title = "تحرير تفضيلات التسليم", Type = ActionTypes.ImBack, Value = "تحرير التسليم" },
                        new CardAction() { Title = "تحرير تفضيلات التعليم", Type = ActionTypes.ImBack, Value = "تحرير التعليم" },
                        new CardAction(){ Title = "طلب البيانات", Type=ActionTypes.ImBack, Value="طلب البيانات" },
                        new CardAction(){ Title = "استمر", Type=ActionTypes.ImBack, Value="استمر" },
                        new CardAction(){ Title = "حذف بيانات المستخدم", Type=ActionTypes.ImBack, Value="حذف بيانات المستخدم" }
                    }
                };
            }
            Debug.WriteLine("in edit pref");
            await stepContext.Context.SendActivityAsync(reply);

            return await stepContext.PromptAsync("text", new PromptOptions { });
        }

        private async Task<DialogTurnResult> AwaitPreference(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string choice = (string)stepContext.Result;
            string language = "en";
            UserDataCollection user = await SaveConversationData.GetUserDataCollection(stepContext.Context.Activity.From.Id);
            var reply = stepContext.Context.Activity.CreateReply();
            switch (choice)
            {
                case "Edit Conversation Language":
                    /* Change a user's preferred language */
                    
                    if (language.ToLower() == "ar" || language.ToLower() == "arabic")
                    {
                        reply.Text = "نحن نتحدث حاليا باللغة العربية. يرجى النقر على \"التبديل \" للتحدث باللغة الإنجليزية من هنا على";
                        reply.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="التبديل إلى اللغة الإنجليزية", Type=ActionTypes.ImBack, Value="الإنجليزية" },
                                new CardAction(){ Title = "الى الخلف", Type=ActionTypes.ImBack, Value="الى الخلف" },
                            }
                        };
                    }
                    else
                    {
                        reply.Text = "We currently converse in English. Please click \"Switch\" to converse in Arabic from here on";
                        reply.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="Switch to Arabic", Type=ActionTypes.ImBack, Value="Arabic" },
                                new CardAction(){ Title = "Go Back", Type=ActionTypes.ImBack, Value="Go back" },
                            }
                        };
                    }
                    await stepContext.Context.SendActivityAsync(reply);
                    await stepContext.PromptAsync("text", new PromptOptions {  });
                    return await stepContext.BeginDialogAsync("EditPrefLang");
                    
                case "تحرير لغة المحادثة":
                    
                    if (language.ToLower() == "ar" || language.ToLower() == "arabic")
                    {
                        reply.Text = "نحن نتحدث حاليا باللغة العربية. يرجى النقر على \"التبديل \" للتحدث باللغة الإنجليزية من هنا على";
                        reply.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="التبديل إلى اللغة الإنجليزية", Type=ActionTypes.ImBack, Value="الإنجليزية" },
                                new CardAction(){ Title = "الى الخلف", Type=ActionTypes.ImBack, Value="الى الخلف" },
                            }
                        };
                    }
                    else
                    {
                        reply.Text = "We currently converse in English. Please click \"Switch\" to converse in Arabic from here on";
                        reply.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="Switch to Arabic", Type=ActionTypes.ImBack, Value="Arabic" },
                                new CardAction(){ Title = "Go Back", Type=ActionTypes.ImBack, Value="Go back" },
                            }
                        };
                    }
                    await stepContext.Context.SendActivityAsync(reply);
                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditPrefLang");

                case "Edit Course Language":
                    /* Change a user's preferred language */
                    
                    if (language.ToLower() == "ar" || language.ToLower() == "arabic")
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("تم ضبط تفضيلات اللغة الخاصة بك على" + " {0}", user.language));
                        reply.Text = "يرجى اختيار اللغة التي تريد استخدامها";
                        reply.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="الإنجليزية", Type=ActionTypes.ImBack, Value="الإنجليزية" },
                                new CardAction(){ Title = "عربى", Type=ActionTypes.ImBack, Value= "عربى" },
                                new CardAction(){ Title = "على حد سواء", Type=ActionTypes.ImBack, Value="على حد سواء" },
                                new CardAction(){ Title = "الى الخلف", Type=ActionTypes.ImBack, Value="الى الخلف" },
                            }
                        };
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("Your course language preference is set to {0}", user.language));
                        reply.Text = "Please select your course language preference";
                        reply.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="English", Type=ActionTypes.ImBack, Value="English" },
                                new CardAction(){ Title = "Arabic", Type=ActionTypes.ImBack, Value= "Arabic" },
                                new CardAction(){ Title = "Both", Type=ActionTypes.ImBack, Value="Both" },
                                new CardAction(){ Title = "Go Back", Type=ActionTypes.ImBack, Value="Go Back" },
                            }
                        };
                    }
                    await stepContext.Context.SendActivityAsync(reply);

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditCourseLang");

                case "تحرير لغة الدورة":
                    
                    if (language.ToLower() == "ar" || language.ToLower() == "arabic")
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("تم ضبط تفضيلات اللغة الخاصة بك على" + " {0}", user.language));
                        reply.Text = "يرجى اختيار اللغة التي تريد استخدامها";
                        reply.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="الإنجليزية", Type=ActionTypes.ImBack, Value="الإنجليزية" },
                                new CardAction(){ Title = "عربى", Type=ActionTypes.ImBack, Value= "عربى" },
                                new CardAction(){ Title = "على حد سواء", Type=ActionTypes.ImBack, Value="على حد سواء" },
                                new CardAction(){ Title = "الى الخلف", Type=ActionTypes.ImBack, Value="الى الخلف" },
                            }
                        };
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("Your course language preference is set to {0}", user.language));
                        reply.Text = "Please select your course language preference";
                        reply.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="English", Type=ActionTypes.ImBack, Value="English" },
                                new CardAction(){ Title = "Arabic", Type=ActionTypes.ImBack, Value= "Arabic" },
                                new CardAction(){ Title = "Both", Type=ActionTypes.ImBack, Value="Both" },
                                new CardAction(){ Title = "Go Back", Type=ActionTypes.ImBack, Value="Go back" },
                            }
                        };
                    }
                    await stepContext.Context.SendActivityAsync(reply);

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditCourseLang");

                case "Edit Interests":
                    /* Change a user's interests */
                    //UserDataCollection user = UserDataCollection.Find(x => x.User_id == "default-user" && x.Name.ToLower() == context.UserData.GetValue<string>("userName").ToLower()).FirstOrDefault();
                    //UserDataCollection user = UserDataCollection.Find(x => x.User_id == activity.From.Id).FirstOrDefault(); // used to store changes in session only
                    string interest = string.Join(", ", user.interests.ToArray()); // used to store changes in session only
                                                                              //string subjects = string.Join(", ", user.PreferedSub.ToArray());
                    var changeInterests = stepContext.Context.Activity.CreateReply();
                    changeInterests.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    await stepContext.Context.SendActivityAsync(String.Format("Your interest preferences are set to {0}", interest));

                    var subject_choice = stepContext.Context.Activity.CreateReply();
                    subject_choice.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    List<CardAction> childSuggestions = new List<CardAction>();
                    if (language.ToLower() == "ar")
                    {
                        await stepContext.Context.SendActivityAsync("هل ترغب في تحديث اهتماماتك؟");
                        childSuggestions.Add(new CardAction() { Title = "نعم فعلا", Type = ActionTypes.ImBack, Value = "نعم فعلا" });
                        childSuggestions.Add(new CardAction() { Title = "الى الخلف", Type = ActionTypes.ImBack, Value = "الى الخلف" });

                        changeInterests.SuggestedActions = new SuggestedActions() { Actions = childSuggestions };
                        await stepContext.Context.SendActivityAsync(changeInterests);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync("Would you like update your interests?");
                        childSuggestions.Add(new CardAction() { Title = "Yes", Type = ActionTypes.ImBack, Value = "Yes" });
                        childSuggestions.Add(new CardAction() { Title = "No, go back", Type = ActionTypes.ImBack, Value = "Go back" });
                        changeInterests.SuggestedActions = new SuggestedActions() { Actions = childSuggestions };
                        await stepContext.Context.SendActivityAsync(changeInterests);
                    }

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditInterests");

                case "تحرير الاهتمامات":
                    string ints = string.Join(", ", user.interests.ToArray()); // used to store changes in session only
                                                                          //string subjects = string.Join(", ", user.PreferedSub.ToArray());
                    var changeInterest = stepContext.Context.Activity.CreateReply();
                    changeInterest.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    await stepContext.Context.SendActivityAsync(String.Format("{0} يتم تعيين تفضيلات اهتمامك على", ints));

                    var subjectChoice = stepContext.Context.Activity.CreateReply();
                    subjectChoice.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    List<CardAction> Suggestions = new List<CardAction>();
                    if (language.ToLower() == "ar")
                    {
                        await stepContext.Context.SendActivityAsync("هل ترغب في تحديث اهتماماتك؟");
                        Suggestions.Add(new CardAction() { Title = "نعم فعلا", Type = ActionTypes.ImBack, Value = "نعم فعلا" });
                        Suggestions.Add(new CardAction() { Title = "الى الخلف", Type = ActionTypes.ImBack, Value = "الى الخلف" });

                        changeInterest.SuggestedActions = new SuggestedActions() { Actions = Suggestions };
                        await stepContext.Context.SendActivityAsync(changeInterest);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync("Would you like update your interests?");
                        Suggestions.Add(new CardAction() { Title = "Yes", Type = ActionTypes.ImBack, Value = "Yes" });
                        Suggestions.Add(new CardAction() { Title = "No, go back", Type = ActionTypes.ImBack, Value = "Go back" });
                        changeInterest.SuggestedActions = new SuggestedActions() { Actions = Suggestions };
                        await stepContext.Context.SendActivityAsync(changeInterest);
                    }

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditInterests");



                case "Edit Gender":
                    /* Change a user's preferred gender to identify as */
                    var selectGender = stepContext.Context.Activity.CreateReply();
                    if (language.ToLower() == "en")
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("Your gender preferences are currently set to {0}", user.gender));
                        selectGender.Text = "Please choose which gender you wish to identify as";
                        selectGender.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="Male", Type=ActionTypes.ImBack, Value="Male" },
                                new CardAction(){ Title = "Female", Type=ActionTypes.ImBack, Value= "Female" },
                                new CardAction(){ Title = "Prefer not to say", Type=ActionTypes.ImBack, Value="Prefer not to say" },
                                new CardAction(){ Title = "Go back", Type=ActionTypes.ImBack, Value="Go back" },
                            },
                        };
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("تم ضبط تفضيلات النوع الاجتماعي حاليًا على {0}", user.gender));
                        selectGender.Text = "الرجاء اختيار نوع الجنس الذي ترغب في تحديده";
                        selectGender.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="الذكر", Type=ActionTypes.ImBack, Value="Male" },
                                new CardAction(){ Title = "إناثا", Type=ActionTypes.ImBack, Value= "Female" },
                                new CardAction(){ Title = "افضل عدم القول", Type=ActionTypes.ImBack, Value="Prefer not to say" },
                                new CardAction(){ Title = "الى الخلف", Type=ActionTypes.ImBack, Value="الى الخلف" },
                            },
                        };
                    }
                    await stepContext.Context.SendActivityAsync(selectGender);

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditGender");

                case "تحرير تفضيل الجنس":
                    var select_gender = stepContext.Context.Activity.CreateReply();
                    if (language.ToLower() == "en")
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("Your gender preferences are currently set to {0}", user.gender));
                        select_gender.Text = "Please choose which gender you wish to identify as";
                        select_gender.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="Male", Type=ActionTypes.ImBack, Value="Male" },
                                new CardAction(){ Title = "Female", Type=ActionTypes.ImBack, Value= "Female" },
                                new CardAction(){ Title = "Prefer not to say", Type=ActionTypes.ImBack, Value="Prefer not to say" },
                                new CardAction(){ Title = "Go back", Type=ActionTypes.ImBack, Value="Go back" },
                            },
                        };
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("{تم ضبط تفضيلات النوع الاجتماعي حاليًا على {0", user.gender));
                        select_gender.Text = "الرجاء اختيار نوع الجنس الذي ترغب في تحديده";
                        select_gender.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="الذكر", Type=ActionTypes.ImBack, Value="الذكر" },
                                new CardAction(){ Title = "إناثا", Type=ActionTypes.ImBack, Value= "إناثا" },
                                new CardAction(){ Title = "افضل عدم القول", Type=ActionTypes.ImBack, Value="افضل عدم القول" },
                                new CardAction(){ Title = "الى الخلف", Type=ActionTypes.ImBack, Value="الى الخلف"},
                            },
                        };
                    }
                    await stepContext.Context.SendActivityAsync(select_gender);

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditGender");

                case "Edit Accreditation":
                    /* Change a user's preferred course accreditation setting */
                    var selectAccred = stepContext.Context.Activity.CreateReply();

                    if (language.ToLower() == "en")
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("Your accreditation preferences are cuurently set to {0}", user.accreditation));
                        selectAccred.Text = "Please select whether you want to study accredited or non-accredited courses";
                        selectAccred.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="Accredited", Type=ActionTypes.ImBack, Value="Accredited" },
                                new CardAction(){ Title = "Non-Accredited", Type=ActionTypes.ImBack, Value= "Non-Accredited" },
                                new CardAction(){ Title = "Either", Type=ActionTypes.ImBack, Value="Either" },
                                new CardAction(){ Title = "Go back", Type=ActionTypes.ImBack, Value="Go back" },
                            },
                        };
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("{0} تم ضبط تفضيلات الاعتماد الخاصة بك على", user.accreditation));
                        selectAccred.Text = "يرجى تحديد ما إذا كنت تريد دراسة الدورات المعتمدة أو غير المعتمدة";
                        selectAccred.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="معتمدة", Type=ActionTypes.ImBack, Value="معتمدة" },
                                new CardAction(){ Title = "غير معتمد", Type=ActionTypes.ImBack, Value= "غير معتمد" },
                                new CardAction(){ Title = "إما او", Type=ActionTypes.ImBack, Value="إما او" },
                                new CardAction(){ Title = "الى الخلف", Type=ActionTypes.ImBack, Value="الى الخلف" },
                            },
                        };
                    }
                    await stepContext.Context.SendActivityAsync(selectAccred);

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditAccred");

                case "تحرير الاعتماد":
                    var selectAccr = stepContext.Context.Activity.CreateReply();

                    if (language.ToLower() == "en")
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("Your accreditation preferences are cuurently set to {0}", user.accreditation));
                        selectAccr.Text = "Please select whether you want to study accredited or non-accredited courses";
                        selectAccr.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="Accredited", Type=ActionTypes.ImBack, Value="Accredited" },
                                new CardAction(){ Title = "Non-Accredited", Type=ActionTypes.ImBack, Value= "Non-Accredited" },
                                new CardAction(){ Title = "Either", Type=ActionTypes.ImBack, Value="Either" },
                                new CardAction(){ Title = "Go back", Type=ActionTypes.ImBack, Value="Go back" },
                            },
                        };
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("{0} تم ضبط تفضيلات الاعتماد الخاصة بك على", user.accreditation));
                        selectAccr.Text = "يرجى تحديد ما إذا كنت تريد دراسة الدورات المعتمدة أو غير المعتمدة";
                        selectAccr.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="معتمدة", Type=ActionTypes.ImBack, Value="معتمدة" },
                                new CardAction(){ Title = "غير معتمد", Type=ActionTypes.ImBack, Value= "غير معتمد" },
                                new CardAction(){ Title = "إما او", Type=ActionTypes.ImBack, Value="إما او" },
                                new CardAction(){ Title = "الى الخلف", Type=ActionTypes.ImBack, Value="الى الخلف" },
                            },
                        };
                    }
                    await stepContext.Context.SendActivityAsync(selectAccr);

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditAccred");

                case "Edit Delivery":
                    /* Change a user's preferred course delivery setting */
                    var selectDelivery = stepContext.Context.Activity.CreateReply();

                    if (language.ToLower() == "en")
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("Your current delivery preferences are {0}", user.delivery));
                        selectDelivery.Text = "Please select how you want the course content to be delivered";
                        selectDelivery.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="Self-Paced", Type=ActionTypes.ImBack, Value="Self-Paced" },
                                new CardAction(){ Title = "Interval", Type=ActionTypes.ImBack, Value= "Interval" },
                                new CardAction(){ Title = "Either", Type=ActionTypes.ImBack, Value="Either" },
                                new CardAction(){ Title = "Go back", Type=ActionTypes.ImBack, Value="Go back" },
                            },
                        };
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("{0} تفضيلات التسليم الحالية الخاصة بك هي", user.delivery));
                        selectDelivery.Text = "الرجاء تحديد الطريقة التي تريد تسليم محتوى الدورة التدريبية بها";
                        selectDelivery.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="ذاتية السرعة", Type=ActionTypes.ImBack, Value="ذاتية السرعة" },
                                new CardAction(){ Title = "فترات منتظمة", Type=ActionTypes.ImBack, Value= "فترات منتظمة" },
                                new CardAction(){ Title ="إما او", Type=ActionTypes.ImBack, Value="إما او" },
                                new CardAction(){ Title = "الى الخلف", Type=ActionTypes.ImBack, Value="الى الخلف" },
                            },
                        };
                    }
                    await stepContext.Context.SendActivityAsync(selectDelivery);

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditPace");

                case "تحرير التسليم":
                    /* Change a user's preferred course delivery setting */
                    var selectDel = stepContext.Context.Activity.CreateReply();

                    if (language.ToLower() == "en")
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("Your current delivery preferences are {0}", user.delivery));
                        selectDel.Text = "Please select how you want the course content to be delivered";
                        selectDel.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="Self-Paced", Type=ActionTypes.ImBack, Value="Self-Paced" },
                                new CardAction(){ Title = "Interval", Type=ActionTypes.ImBack, Value= "Interval" },
                                new CardAction(){ Title = "Either", Type=ActionTypes.ImBack, Value="Either" },
                                new CardAction(){ Title = "Go back", Type=ActionTypes.ImBack, Value="Go back" },
                            },
                        };
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("{0} تفضيلات التسليم الحالية الخاصة بك هي", user.delivery));
                        selectDel.Text = "الرجاء تحديد الطريقة التي تريد تسليم محتوى الدورة التدريبية بها";
                        selectDel.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="ذاتية السرعة", Type=ActionTypes.ImBack, Value="ذاتية السرعة" },
                                new CardAction(){ Title = "فترات منتظمة", Type=ActionTypes.ImBack, Value= "فترات منتظمة" },
                                new CardAction(){ Title ="إما او", Type=ActionTypes.ImBack, Value="إما او" },
                                new CardAction(){ Title = "الى الخلف", Type=ActionTypes.ImBack, Value="الى الخلف" },
                            },
                        };
                    }
                    await stepContext.Context.SendActivityAsync(selectDel);

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditPace");

                case "Edit Education":
                    /* Change a user's preferred course delivery setting */
                    var selectEducation = stepContext.Context.Activity.CreateReply();
                    if (language.ToLower() == "en")
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("Your current education level is set to {0}", user.education));
                        selectEducation.Text = "Please select you new education level";
                        selectEducation.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="Primary School", Type=ActionTypes.ImBack, Value="Primary School"},
                                new CardAction(){ Title = "Early High School", Type=ActionTypes.ImBack, Value= "Early High School"},
                                new CardAction(){ Title ="Late High School", Type=ActionTypes.ImBack, Value="Late High School"},
                                new CardAction(){ Title = "University", Type = ActionTypes.ImBack, Value="University"},
                                new CardAction(){ Title = "Go back", Type=ActionTypes.ImBack, Value="Go back" },
                            },
                        };
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("{0} تم تعيين مستوى التعليم الحالي الخاص بك إلى", user.education));
                        selectEducation.Text = "يرجى تحديد مستوى تعليمي جديد لك";
                        selectEducation.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="المدرسة الابتدائية", Type=ActionTypes.ImBack, Value="المدرسة الابتدائية"},
                                new CardAction(){ Title = "المدرسة الثانوية المبكرة", Type=ActionTypes.ImBack, Value= "المدرسة الثانوية المبكرة"},
                                new CardAction(){ Title ="المدرسة الثانوية المتأخرة", Type=ActionTypes.ImBack, Value="المدرسة الثانوية المتأخرة"},
                                new CardAction(){ Title = "جامعة",  Type = ActionTypes.ImBack, Value="جامعة"},
                                new CardAction(){ Title = "الى الخلف", Type=ActionTypes.ImBack, Value="الى الخلف" },
                            },
                        };
                    }
                    await stepContext.Context.SendActivityAsync(selectEducation);

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditEducation");

                case "تحرير التعليم":
                    var selectEd = stepContext.Context.Activity.CreateReply();
                    if (language.ToLower() == "en")
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("Your current education level is set to {0}", user.education));
                        selectEd.Text = "Please select you new education level";
                        selectEd.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="Primary School", Type=ActionTypes.ImBack, Value="Primary School"},
                                new CardAction(){ Title = "Early High School", Type=ActionTypes.ImBack, Value= "Early High School"},
                                new CardAction(){ Title ="Late High School", Type=ActionTypes.ImBack, Value="Late High School"},
                                new CardAction(){ Title = "University", Type = ActionTypes.ImBack, Value="University"},
                                new CardAction(){ Title = "Go back", Type=ActionTypes.ImBack, Value="Go back" },
                            },
                        };
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("{0} تم تعيين مستوى التعليم الحالي الخاص بك إلى", user.education));
                        selectEd.Text = "يرجى تحديد مستوى تعليمي جديد لك";
                        selectEd.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="المدرسة الابتدائية", Type=ActionTypes.ImBack, Value="المدرسة الابتدائية"},
                                new CardAction(){ Title = "المدرسة الثانوية المبكرة", Type=ActionTypes.ImBack, Value= "المدرسة الثانوية المبكرة"},
                                new CardAction(){ Title ="المدرسة الثانوية المتأخرة", Type=ActionTypes.ImBack, Value="المدرسة الثانوية المتأخرة"},
                                new CardAction(){ Title = "جامعة",  Type = ActionTypes.ImBack, Value="جامعة"},
                                new CardAction(){ Title = "الى الخلف", Type=ActionTypes.ImBack, Value="الى الخلف" },
                            },
                        };
                    }
                    await stepContext.Context.SendActivityAsync(selectEd);

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditEducation");

                case "Edit Name":
                    /* Change a user's preferred course delivery setting */

                    if (language.ToLower() == "en")
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("I currently know you as {0}", user.Name));
                        await stepContext.Context.SendActivityAsync("How would you like to be addressed?");
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("{0} أنا أعرفك حاليًا", user.Name));
                        await stepContext.Context.SendActivityAsync("كيف تريد أن تكون موجهة؟");
                    }

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditName");

                case "تعديل الاسم":
                    if (language.ToLower() == "en")
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("I currently know you as {0}", user.Name));
                        await stepContext.Context.SendActivityAsync("How would you like to be addressed?");
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(String.Format("{0} أنا أعرفك حاليًا", user.Name));
                        await stepContext.Context.SendActivityAsync("كيف تريد أن تكون موجهة؟");
                    }

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("EditName");

                case "Continue":
                    /* Bring the user back to the main dialog */
                    AddDialog(new LearningDialog(userState, null));
                    return await stepContext.ReplaceDialogAsync(nameof(LearningDialog));
                    
                    
                case "استمر":
                    return await stepContext.EndDialogAsync();
                   

                case "Request Data":
                    /*Allows user to request a portable copy of the data we hold on them */
                    var getData = stepContext.Context.Activity.CreateReply();

                    if (language.ToLower() == "en" || language.ToLower() == "english")
                    {
                        getData.Text = "No problem.Please specify if you would like me to share with I know about you or send you a copy to download.";
                        getData.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                                {
                                    new CardAction(){ Title ="View Data", Type=ActionTypes.ImBack, Value="View Data"},
                                    new CardAction(){ Title ="Get a Copy", Type=ActionTypes.ImBack, Value="Get a Copy"},
                                    new CardAction(){ Title = "Go back", Type=ActionTypes.ImBack, Value="Go back" },
                                },
                        };
                    }
                    else
                    {
                        getData.Text = "ليس هناك أى مشكلة. يرجى تحديد ما إذا كنت ترغب في مشاركتي مع أعلم بك أو أرسل نسخة لتنزيلها.";
                        getData.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="عرض البيانات", Type=ActionTypes.ImBack, Value="عرض البيانات"},
                                new CardAction(){ Title ="الحصول على نسخة", Type=ActionTypes.ImBack, Value="الحصول على نسخة"},
                                new CardAction(){ Title = "الى الخلف", Type=ActionTypes.ImBack, Value="الى الخلف" },
                            },
                        };
                    }
                    await stepContext.Context.SendActivityAsync(getData);

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("RequestData");
                case "طلب البيانات":
                    var getDat = stepContext.Context.Activity.CreateReply();

                    if (language.ToLower() == "en" || language.ToLower() == "english")
                    {
                        getDat.Text = "No problem.Please specify if you would like me to share with I know about you or send you a copy to download.";
                        getDat.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                                {
                                    new CardAction(){ Title ="View Data", Type=ActionTypes.ImBack, Value="View Data"},
                                    new CardAction(){ Title ="Get a Copy", Type=ActionTypes.ImBack, Value="Get a Copy"},
                                    new CardAction(){ Title = "Go back", Type=ActionTypes.ImBack, Value="Go back" },
                                },
                        };
                    }
                    else
                    {
                        getDat.Text = "ليس هناك أى مشكلة. يرجى تحديد ما إذا كنت ترغب في مشاركتي مع أعلم بك أو أرسل نسخة لتنزيلها.";
                        getDat.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction(){ Title ="عرض البيانات", Type=ActionTypes.ImBack, Value="عرض البيانات"},
                                new CardAction(){ Title ="الحصول على نسخة", Type=ActionTypes.ImBack, Value="الحصول على نسخة"},
                                new CardAction(){ Title = "الى الخلف", Type=ActionTypes.ImBack, Value="الى الخلف" },
                            },
                        };
                    }
                    await stepContext.Context.SendActivityAsync(getDat);

                    await stepContext.PromptAsync("text", new PromptOptions { });
                    return await stepContext.BeginDialogAsync("RequestData");

                default:
                    /* If none of the options provided are chosen we use LUIS to see what the user wants to do */

                    if (language == "ar" || language == "arabic")
                    {
                        stepContext.Context.Activity.Text = await Translate.Translator((string)stepContext.Result, "en");
                    }
                    return await stepContext.ReplaceDialogAsync(nameof(LuisDialog));
                    
            }
        }

        private async Task<DialogTurnResult> EditPrefLang(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Debug.WriteLine("edit pref lang " + (string)stepContext.Result);
            stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 3;
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> EditCourseLang(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 3;
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> EditInterests(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 3;
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> RequestData(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 3;
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> EditEducation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 3;
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> EditAccred(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 3;
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> EditPace(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 3;
            return await stepContext.NextAsync();
        }


    }
}