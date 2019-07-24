using Microsoft.Bot.Connector;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Authentication;
using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;
using HakeemTestV4.SupportClasses;

namespace HakeemTestV4.Database
{
    public class SaveConversationData
    {
        //private static readonly IMongoCollection<ConversationObject> UserCollection = GetReferenceToCollection<ConversationObject>("ResumeConversationObjectDeploy");
        private static readonly IMongoCollection<ConversationObject> UserCollection = GetReferenceToCollection<ConversationObject>("ResumeConversationObjectDeploy");
        private static readonly IMongoCollection<MessageCollection> saveMessagesCollection = GetReferenceToCollection<MessageCollection>("savedMessages");
        private static readonly IMongoCollection<UserDataCollection> UserDataCollection = GetReferenceToCollection<UserDataCollection>("users");
        private static readonly IMongoCollection<CourseList> CoursesCollection = GetReferenceToCollection<CourseList>("hakeem_course_list");

        //Get language stored in conversation object using conversation id, if null default to English
        internal static string GetLanguage(string conversationId)
        {
            var filter = Builders<ConversationObject>.Filter.Eq(s => s.ConversationId, conversationId);
            ConversationObject results = UserCollection.Find(filter).FirstOrDefault();
            return results.Language != null && results.Language != "" ? results.Language : "En";
        }

        //API needs language code for speech to text, obtained from conversation, gets from Conversation obj using ID
        internal static string GetLanguageForAPI(string conversationId)
        {
            switch (GetLanguage(conversationId))//switch for language input which defaults to English
            {
                case ("En"):
                    return "en-US";
                case ("Ar"):
                    return "ar-EG";
                case ("Arabic"):
                    return "ar-EG";
                case ("عربى"):
                    return ("ar-EG");
                default:
                    return "en-US";
            }
        }

        internal static string GetLanguageForString(string conversationId)
        {
            switch (GetLanguage(conversationId))
            {
                case ("En"):
                    return "English";
                case ("Ar"):
                    return "عربى";
                default:
                    return "English";
            }
        }

        internal async static Task<UserDataCollection> GetUserDataCollection(string user_id)
        {
            //change after upcoming demo
            UserDataCollection user = UserDataCollection.Find(x => x.User_id == user_id).FirstAsync().Result;
            //UserDataCollection user = UserDataCollection.Find(x => x.User_id == user_id && x.Name == user_name.ToLower()).FirstAsync().Result;
            //if (user == null)
            //{
            //    await SaveNewUser(user_id);
            //    user = UserDataCollection.Find(x => x.User_id == user_id).FirstAsync().Result;
            //}
            return user;
        }

        internal async static Task<ConversationObject> GetConversationObject(string conversationId)
        {
            try
            {
                return await UserCollection.Find(_ => _.ConversationId == conversationId).SingleAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        //Updates the language stored in the conversation object with user selection
        //Should eventually be changed to take/ update from database instead
        internal static async Task UpdateLanguage(string conversationId, string language)
        {
            var filter = Builders<ConversationObject>.Filter.Eq(s => s.ConversationId, conversationId);
            ConversationObject results = UserCollection.Find(filter).FirstOrDefault();
            results.Language = language;

            var x = await UserCollection.ReplaceOneAsync(filter, results);
        }

        /* Called by root dialog to save new conversation IDs*/
        internal static async Task<bool> SaveBasicConversationAsync(string fromId, string serviceUrl, string channelId, string conversationId, bool status)
        {
            var exists = UserCollection.AsQueryable().FirstOrDefault(x => x.ConversationId == conversationId) != null;

            if (!exists)
            {

                await UserCollection.InsertOneAsync(
                    new ConversationObject
                    {
                        FromId = fromId,
                        ServiceUrl = serviceUrl,
                        ConversationId = conversationId,
                        Date = DateTime.Now,
                        Status = status,
                        FromName = "",
                        ToId = "",
                        ToName = "",
                        ChannelId = "",
                        Language = ""
                    });

                return true;
            }
            else // updating the time of already existing attribute
            {
                var filter = Builders<ConversationObject>.Filter.Eq(s => s.ConversationId, conversationId);
                ConversationObject results = UserCollection.Find(filter).FirstOrDefault();
                DateTime previousDate = results.Date;
                results.Date = DateTime.Now;


                var result = await UserCollection.ReplaceOneAsync(filter, results);

            }
            return false;
        }
        internal static async Task<bool> SaveConversationAsync(string fromName, string toId, string toName, string serviceUrl, string channelId, string conversationId)
        {
            if (channelId != "directline") // updating the time of already existing attribute
            {
                var filter = Builders<ConversationObject>.Filter.Eq(s => s.ConversationId, conversationId);
                ConversationObject results = UserCollection.Find(filter).FirstOrDefault();
                results.FromName = fromName;
                results.ToId = toId;
                results.ToName = toName;
                results.ChannelId = channelId;

                var x = await UserCollection.ReplaceOneAsync(filter, results);

                return true;
            }
            return false;
        }

        internal static async Task<bool> SaveMessagesAsync(string input, string conversationId, bool status, string channelId)
        {
            var exists = saveMessagesCollection.AsQueryable().Any(x => x.ConversationId == conversationId);

            if (!exists && channelId != "directline")
            {
                List<string> newText = new List<string>(new string[] { input });

                await saveMessagesCollection.InsertOneAsync(new MessageCollection
                {
                    ConversationId = conversationId,
                    Message = newText,
                    Status = status
                });

                return true;
            }

            else if (channelId != "directline") // updating the time of already existing attribute
            {
                var filter = Builders<MessageCollection>.Filter.Eq(s => s.ConversationId, conversationId);
                MessageCollection results = saveMessagesCollection.Find(filter).FirstOrDefault();
                List<string> text = results.Message;
                text.Add(input);
                results.Message = text;

                var result = await saveMessagesCollection.ReplaceOneAsync(filter, results);

                //if ((previousdate - datetime.now).totaldays < 1)
                //    return results.language;
            }
            return false;

        }

        internal static async Task<bool> AddBotResponseAsync(string input, string conversationId)
        {
            var filter = Builders<MessageCollection>.Filter.Eq(s => s.ConversationId, conversationId);
            MessageCollection results = saveMessagesCollection.Find(filter).FirstOrDefault();
            List<string> text = results.Message;
            text.Add(input);
            results.Message = text;

            var result = await saveMessagesCollection.ReplaceOneAsync(filter, results);

            return true;
        }

        internal async static Task EndConversation(string convoId) //sets conversation as finished in DB
        {

            var update = Builders<ConversationObject>.Update.Set("Status", false);

            await UserCollection.FindOneAndUpdateAsync(x => x.ConversationId == convoId, update);

        }

        internal async static Task<List<string>> GetAllUniqueSubTopics(string topic)
        {
            // there is a functionality to get all distinct values of a field from a query but I couldn't get it to work
            var result = CoursesCollection.Find(y => y.topic.ToLower() == topic.ToLower()).ToList();

            HashSet<string> distinct = new HashSet<string>();
            for (int i = 0; i < result.Count; i++)
            {
                distinct.Add(result[i].subTopic);
            }

            return distinct.ToList();

        }

        internal async static Task RestartConversation(string convoId)
        {
            var update = Builders<ConversationObject>.Update.Set("Status", true);

            await UserCollection.FindOneAndUpdateAsync(x => x.ConversationId == convoId, update);
        }

        internal static bool CheckConvoStatus(string convoId)
        {
            return UserCollection.Find(x => x.ConversationId == convoId).FirstOrDefault().Status;
        }

        internal static DateTime GetLastMessage(string conversationId)
        {
            var filter = Builders<ConversationObject>.Filter.Eq(s => s.ConversationId, conversationId);
            ConversationObject results = UserCollection.Find(filter).FirstOrDefault();
            return results.Date != null ? results.Date : DateTime.Now;
        }

        /* Keep track of the time since the last message the user sent*/
        internal async static Task UpdateConvTime(string conversationId)
        {
            var filter = Builders<ConversationObject>.Filter.Eq(s => s.ConversationId, conversationId);
            ConversationObject results = UserCollection.Find(filter).FirstOrDefault();
            DateTime previousDate = results.Date;
            results.Date = DateTime.Now;

            var result = await UserCollection.ReplaceOneAsync(filter, results);
        }

        internal static bool CheckConvoExist(string conversationId)
        {
            if (UserCollection.Find(x => x.ConversationId == conversationId).FirstOrDefault() == null)
            {

                return false;
            }
            else
            {

                return true;
            }
        }

        /*internal static async Task<bool> SaveNewContact()
        {
            CollectionNamespace = GetReferenceToCollection<ConversationObject>("contact");
        }*/

        /* Method that takes the name of a collection as a parameter and returns a reference to that collection */
        public static IMongoCollection<T> GetReferenceToCollection<T>(string collectionName)
        {
            //MongoClient client = new MongoClient(
            //new MongoClientSettings
            //{
            //    Credential = MongoCredential.CreateCredential("mydb", "jake", "running")
            // ,
            //    Server = new MongoServerAddress("13.81.172.78", 27017),

            //});



            //IMongoDatabase _database = client.GetDatabase("mydb");
            String connectionString = @"mongodb://hakeemdb:ESmBikpcBDGdwAJ3NEpRhmOIlyVhJ1TKzbllvXm8GdDWYyBYrAGsa0Tp19MB0YoWVQAyW3522vGAcN3C72N2Rg==@hakeemdb.documents.azure.com:10255/?ssl=true&replicaSet=globaldb";
            MongoClientSettings settings = MongoClientSettings.FromUrl(
              new MongoUrl(connectionString)
            );
            settings.SslSettings =
              new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            var mongoClient = new MongoClient(settings);

            IMongoDatabase _database = mongoClient.GetDatabase("hakeemdb");
            return _database.GetCollection<T>(collectionName);
        }

        public static async Task DeleteUserData(string id)
        {
            /* Delete the current user's data from the database */

            UserDataCollection.DeleteMany(x => x.User_id == id);
        }

        public static async Task DeleteConvoData(String conversation_id)
        {
            /* Delete data about the current conversation from the database */

            UserCollection.FindOneAndDelete(x => x.ConversationId == conversation_id);
        }

        public static async Task DeleteSavedMessagesData(String conversation_id)
        {
            /* Delete data about the current conversation from the database */

            saveMessagesCollection.FindOneAndDelete(x => x.ConversationId == conversation_id);
        }

        public static async Task SaveUserCourse(string user_id, UserCourse course)
        {
            UserDataCollection user = UserDataCollection.Find(x => x.User_id == user_id).FirstOrDefault();
            List<UserCourse> courses = user.PastCourses;
            courses.Add(course);
            var update = Builders<UserDataCollection>.Update.Set("PastCourses", courses);
            UserDataCollection.FindOneAndUpdate(x => x.User_id == user_id, update);
        }

        public static string CourseToSubject(string course_name)
        {
            CourseList course = CoursesCollection.Find(x => x.courseName == course_name).FirstOrDefault();
            if (course != null)
            {
                return course.subTopic;
            }
            else
            {
                return "";
            }
        }

        public static List<CourseList> MatchCourseByTopic(string topic)
        {

            return CoursesCollection.Find(x => x.topic.ToLower() == topic.ToLower()).ToList();
        }

        public static List<CourseList> MatchCourseBySubTopic(string topic)
        {

            return CoursesCollection.Find(x => x.subTopic.ToLower() == topic.ToLower()).ToList();
        }

        public static List<dynamic> GetUniqueTopics()
        {
            var filter = new BsonDocument();
            List<dynamic> result = CoursesCollection.Distinct<dynamic>("topic", filter).ToList();

            return result;
        }

        public static List<dynamic> GetUniqueSubTopics(string topic)
        {

            FilterDefinition<CourseList> filter = new BsonDocument("topic", Grammar.Capitalise(topic).Trim());

            List<dynamic> result = CoursesCollection.Distinct<dynamic>("subTopic", filter).ToList();

            return result;
        }

        public static List<CourseList> TryMatchCourse(string sub_topic, string accreditation, string self_paced, bool financial_aid, string language, string level, int restrictive)
        {
            // capitalise first letter
            language = language.First().ToString().ToUpper() + language.ToLower().Substring(1);

            bool accreditation_new = accreditation == "Accredited" ? true : false;
            string education = "";
            if (level == "Late High School" || level == "Early High School")
            {
                education = "Intermediate";
            }
            else if (level == "University")
            {
                education = "Advanced";
            }
            else
            {
                education = "Beginner";
            }


            //Dictionary<string, int> education_to_ordinal = new Dictionary<string, int>();
            //education_to_ordinal.Add("Primary School", 0);
            //education_to_ordinal.Add("Early High School", 1);
            //education_to_ordinal.Add("Late High School", 1);
            //education_to_ordinal.Add("University", 2);

            //Dictionary<string, int> level_to_ordinal = new Dictionary<string, int>();
            //level_to_ordinal.Add("Beginner", 0);
            //level_to_ordinal.Add("Intermediate", 1);

            //level_to_ordinal.Add("Advanced", 2);


            bool both_language = language.ToLower() == "both" ? true : false;
            bool both_pace = self_paced.ToLower() == "both" ? true : false;
            bool self_paced_bool = self_paced.ToLower() == "self-paced" ? true : false;
            bool level_bool = false;

            // restrictive refers to how stringent the search parameters are for the courses
            // restrictive = 0 ==> full restrictions
            // restrictive = 1 ==> ignore the user's education level when searching
            // restrictive = 2 ==> ignore language
            // restrictive = 3 ==> ignore self_paced
            if (restrictive == 1)
            {
                level_bool = true;
            }
            if (restrictive == 2)
            {
                both_language = true;
            }
            if (restrictive == 3)
            {
                both_pace = true;
            }


            return CoursesCollection.Find(x => (x.languageDelivered.Contains(language) || both_language) && (both_pace || x.selfPaced == self_paced_bool) && (x.accreditationOption == accreditation_new) && (x.level == "" || x.level == education || level_bool) && (x.topic.ToLower() == sub_topic.ToLower() || x.subTopic.ToLower() == sub_topic.ToLower())).ToList();


        }

        public static bool CheckUserExists(string user_id)
        {
            /* Check for the user's Skype ID in the database */
            List<UserDataCollection> results = UserDataCollection.Find(x => x.User_id == user_id).ToList();
            if (results.Count >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static CourseList GetCourseByName(string name)
        {
            return CoursesCollection.Find(x => x.courseName.ToLower() == name.ToLower() || x.courseNameArabic == name).FirstOrDefault();
        }

        public static async Task SaveNewUser(string user_id, string name)
        {
            /* Saves a new user to the database who has accepted the privacy policy
             * Name and Skype ID are saved and the other fields are populated with default values
             * Preferences app is used to update these other values */

            await UserDataCollection.InsertOneAsync(new UserDataCollection
            {
                Name = name,
                Notification = 7,
                PreferedLang = "English",
                PreferedSub = new List<string>(),
                PastCourses = new List<UserCourse>(),
                User_id = user_id,
                interests = new List<string>(),
                consent = true,
                consent_time = (Int32)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                expiry_time = (Int32)DateTime.UtcNow.AddDays(365).Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                privacy_policy_version = 1,
                consent_text = "I have read and accept the privacy policy",
            });
        }


        public static async Task UpdatePastCourses(string user_id, List<UserCourse> courses)
        {
            var update = Builders<UserDataCollection>.Update.Set("PastCourses", courses);
            UserDataCollection.FindOneAndUpdate(x => x.User_id == user_id, update);
        }
        public static async Task SaveNewUserConsent(string user_id)
        {
            /* Saves a new user to the database who has declined the privacy policy
             * Skype ID and consent outcome are saved 
             * Preferences app is used to update these and add other values */

            await UserDataCollection.InsertOneAsync(new UserDataCollection
            {
                User_id = user_id,
                consent = false
            });
        }

        //public static async Task WithdrawUserConsent(ObjectId id) {
        //    /* Removes user data from the database
        //     * Skype ID and consent outcome are saved 
        //    */
        //    await UserDataCollection.InsertOneAsync(new UserDataCollection
        //    {
        //        User_id = id,
        //        consent = false
        //    });
        //}

        public static async Task SaveLanguagePreference(string language, string id)
        {
            /* Update a user's preferred learning language in the database */

            if (language == "الإنجليزية")
            {
                language = "English";
            }
            else if (language == "عربى")
            {
                language = "Arabic";
            }
            else if (language == "على حد سواء")
            {
                language = "Both";
            }
            var update = Builders<UserDataCollection>.Update.Set("language", language);
            UserDataCollection.FindOneAndUpdate(x => x.User_id == id, update);
        }

        public static async Task SaveSubjectPreferenceDelete(List<string> subjects, string id)
        {
            /* Remove a user's preferred subject from the database */

            var update = Builders<UserDataCollection>.Update.Set("PreferedSub", subjects);
            await UserDataCollection.FindOneAndUpdateAsync(x => x.User_id == id, update);
        }

        public static async Task SaveSubjectPreferenceAdd(string subject, string id)
        {
            /* Add a user's preferred subject to the database */

            List<string> subject_list = UserDataCollection.Find(x => x.User_id == id).First().PreferedSub;
            subject_list.Add(subject);
            var update = Builders<UserDataCollection>.Update.Set("PreferedSub", subject_list);
            UserDataCollection.FindOneAndUpdate(x => x.User_id == id, update);
        }

        public static async Task SaveNotification(long reminder, string id)
        {
            /* Update a user's notification frequency in the database */
            var update = Builders<UserDataCollection>.Update.Set("Notification", reminder);
            UserDataCollection.FindOneAndUpdate(x => x.User_id == id, update);
        }

        public static async Task SaveGenderPreference(string gender, string id)
        {
            /* Update a user's preferred gender in the database */

            if (gender == "الذكر")
            {
                gender = "Male";
            }
            else if (gender == "إناثا")
            {
                gender = "Female";
            }
            else if (gender == "افضل عدم القول")
            {
                gender = "Prefer not to say";
            }
            var update = Builders<UserDataCollection>.Update.Set("gender", gender);
            UserDataCollection.FindOneAndUpdate(x => x.User_id == id, update);
        }

        public static async Task SaveAccreditationPreference(string accreditation, string id)
        {
            /* Update a user's preferred accreditation setting in the database */
            if (accreditation == "معتمدة")
            {
                accreditation = "Accredited";
            }
            else if (accreditation == "غير معتمد")
            {
                accreditation = "Non-accredited";
            }
            else if (accreditation == "إما او")
            {
                accreditation = "Either";
            }
            var update = Builders<UserDataCollection>.Update.Set("accreditation", accreditation);
            UserDataCollection.FindOneAndUpdate(x => x.User_id == id, update);
        }

        public static async Task SaveDeliveryPreference(string delivery, string id)
        {
            /* Update a user's preferred accreditation setting in the database */
            if (delivery == "فترات منتظمة")
            {
                delivery = "Interval";
            }
            else if (delivery == "ذاتية السرعة")
            {
                delivery = "Self-paced";
            }
            else if (delivery == "إما او")
            {
                delivery = "Either";
            }
            var update = Builders<UserDataCollection>.Update.Set("delivery", delivery);
            UserDataCollection.FindOneAndUpdate(x => x.User_id == id, update);
        }

        public static async Task SaveEducationPreference(string education, string id)
        {
            /* Update a user's preferred accreditation setting in the database */

            if (education == "المدرسة الابتدائية")
            {
                education = "Primary School";
            }
            else if (education == "المدرسة الثانوية المبكرة")
            {
                education = "Early High School";
            }
            else if (education == "المدرسة الثانوية المتأخرة")
            {
                education = "Late High School";
            }
            else if (education == "جامعة")
            {
                education = "University";
            }
            var update = Builders<UserDataCollection>.Update.Set("education", education);
            UserDataCollection.FindOneAndUpdate(x => x.User_id == id, update);
        }


        public static async Task UpdateUserName(string name, string id)
        {
            /* Update a user's name in the database */

            var update = Builders<UserDataCollection>.Update.Set("Name", name);
            UserDataCollection.FindOneAndUpdate(x => x.User_id == id, update);

        }
    }

    [Serializable]
    [BsonIgnoreExtraElements]
    public class CourseList
    {
        //public Int32 __v { get; set; }
        public ObjectId _id { get; set; }
        public string courseName { get; set; }
        public string courseNameArabic { get; set; }
        public string topic { get; set; }
        public string subTopic { get; set; }
        public string[] languageDelivered { get; set; }
        public string level { get; set; }
        public bool accreditationOption { get; set; }
        public bool selfPaced { get; set; }
        public bool financialAid { get; set; }
        public long approxDuration { get; set; }
        public string approxDurationArabic { get; set; }
        public Uri url { get; set; }
        public string description { get; set; }
        public string descriptionArabic { get; set; }
        public bool prerequisites { get; set; }
        public string prerequisiteInfo { get; set; }

    }

    public class UserCourse
    {
        public string _t { get; set; }
        public string Name { get; set; }
        public string NameArabic { get; set; }
        public DateTime Date { get; set; }
        public Boolean Taken { get; set; }
        public Boolean InProgress { get; set; }
        public Boolean Complete { get; set; }
        public Boolean Queried { get; set; }
        public Int16 Rating { get; set; }
    }

    [Serializable]
    public class MessageCollection
    {
        public ObjectId _id { get; set; }
        public string ConversationId { get; set; }
        public List<string> Message { get; set; }
        public bool Status { get; set; }
    }


    [Serializable]
    public class ConversationObject
    {
        public ObjectId _id { get; set; }
        public string FromId { get; set; }
        public string FromName { get; set; }
        public string ToId { get; set; }
        public string ToName { get; set; }
        public string ServiceUrl { get; set; }
        public string ChannelId { get; set; }
        public string ConversationId { get; set; }
        public DateTime Date { get; set; }
        public string Language { get; set; }
        public bool Status { get; set; }
    }

    [Serializable]
    [BsonIgnoreExtraElements]
    public class UserDataCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string Name { get; set; }
        public string PreferedLang { get; set; }
        public string language { get; set; }
        public List<string> PreferedSub { get; set; }
        public int Notification { get; set; }
        public List<UserCourse> PastCourses { get; set; }
        public string User_id { get; set; }
        public List<string> interests { get; set; }
        public string gender { get; set; }
        public string accreditation { get; set; }
        public string delivery { get; set; }
        public string education { get; set; }
        public bool consent { get; set; }
        public Int32 consent_time { get; set; }
        public Int32 expiry_time { get; set; }
        public int privacy_policy_version { get; set; }
        public string consent_text { get; set; }

    }
}