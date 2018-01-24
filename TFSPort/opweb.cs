using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using TFSPort.OP.Contracts;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace TFSPort
{
    public struct OPAPIS
    {
        public const string CREATEWP = "work_packages/?notify=false";
        public const string UPDATEWP = "work_packages/{0}?notify=false";
    }

    public struct HTTPMETHODS
    {
        public const string POST = "POST";
        public const string PATCH = "PATCH";
        public const string GET = "GET";
    }

    class opweb
    {
        private List<OpenProjectUser> m_Users = new List<OpenProjectUser>();
        private List<WorkItemType> m_Types = new List<WorkItemType>();
        private List<StatusType> m_StatusTypes = new List<StatusType>();
        private List<PriorityType> m_PriorityTypes = new List<PriorityType>();
        private List<Project> m_Projects = new List<Project>();
        private string m_SourceProject;
        private int m_TargetProjectId;

        internal void Init(string targetProject, string sourceProject, string baseurl)
        {
            m_Users = GetItems<OpenProjectUser>("/api/v3/users/");
            m_Types = GetItems<WorkItemType>("/api/v3/types/");
            m_StatusTypes = GetItems<StatusType>("/api/v3/statuses/");
            m_PriorityTypes = GetItems<PriorityType>("/api/v3/priorities/");
            m_Projects = GetItems<Project>("/api/v3/projects/");
            m_SourceProject = sourceProject;

            var m = m_Projects.Where(c => c.identifier == targetProject).First();

            if (null == m)
                throw new Exception("The specified project does not exist in your open project server or you do not have permission to view it");

            m_TargetProjectId = m.id;

        }

        private void DoBasicAuthTest()
        {            
            WebRequest wr = WebRequest.Create("http://yourserver.com/openproject/api/v3/");
            wr.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("apikey" + ":" + "yourkey"));            
            wr.GetResponse();                                   
        }

        internal List<T> GetItems<T>(string api)
        {
            List<T> retVal = new List<T>();

            WebRequest wr = WebRequest.Create("http://yourserver.com/openproject/" + api);
        
            wr.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("apikey" + ":" + "409644f68d2e7bfe46645d298e811850686ea083"));
            wr.Method = "GET";
            wr.ContentType = "application/json";

            //Get the stream of JSON
            Stream responseStream = wr.GetResponse().GetResponseStream();

            //Deserialize the JSON stream
            using (StreamReader reader = new StreamReader(responseStream))
            {
                string r = reader.ReadToEnd();

                var userResults = AllChildren(JObject.Parse(r)).First(c => c.Type == JTokenType.Array && c.Path.Contains("elements")).Children<JObject>();                

                foreach(var result in userResults)
                {                    
                    DataContractJsonSerializer sr = new DataContractJsonSerializer(typeof(T));
                    
                    using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(result.ToString())))
                    {
                        var jsonResponse = (T)sr.ReadObject(ms);
                        retVal.Add(jsonResponse);
                    }                        
                }                
            }

            return retVal;
        }        

        internal IEnumerable<JToken> AllChildren(JToken json)
        {
            foreach (var c in json.Children())
            {
                yield return c;
                foreach (var cc in AllChildren(c))
                {
                    yield return cc;
                }
            }
        }

        private string CallGetOrDelete(string api, string httpMethod)
        {
            WebRequest wr = WebRequest.Create("http://yourserver.com/openproject/api/v3/" + api);
            wr.Proxy = null;
            wr.Timeout = 120000;
            wr.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("apikey" + ":" + "yourkey"));
            wr.Method = httpMethod;
            wr.ContentType = "application/json";

            try
            {                
                var response = (HttpWebResponse)wr.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var results = reader.ReadToEnd();

                    response.Close();

                    return results;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred attempting to create the work package", ex);
            }
            finally
            {
                wr.Abort();
            }            
        }

        private void CallPostOrPatchApi(string workPackage, string api, string httpMethod)
        {
            WebRequest wr = WebRequest.Create("http://yourserver.com/openproject/api/v3/" + api);
            wr.Proxy = null;
            wr.Timeout = 120000;
            wr.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("apikey" + ":" + "yourkey"));
            wr.Method = httpMethod;
            wr.ContentType = "application/json";

            try
            {
                using (var streamWriter = new StreamWriter(wr.GetRequestStream()))
                {
                    streamWriter.Write(
                        workPackage
                        );

                    streamWriter.Flush();

                    streamWriter.Close();
                }

                var response = (HttpWebResponse)wr.GetResponse();

                response.Close();
            }
            catch(Exception ex)
            {
                throw new Exception("An error occurred attempting to create the work package", ex);
            }
            finally
            {
                wr.Abort();
            }
        }

        internal void StartConversion(string boardColumn)
        {
            Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemCollection tfsWork = QueryStore.GetItemsByBoardColumn(boardColumn, m_SourceProject);

            if (tfsWork.Count == 0)
                throw new Exception("The TFS query failed to return any results for the given project or board column, review your settings and try again");                       

            foreach (Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem wi in tfsWork)
            {
                int tfsId;               
                string priorityTitle;
                string typeTitle;

                int assignedToId = ParseName(wi.Fields[Microsoft.TeamFoundation.WorkItemTracking.Client.CoreField.AssignedTo].Value.ToString());

                int responsibleId = ParseName(wi.Fields[Microsoft.TeamFoundation.WorkItemTracking.Client.CoreField.CreatedBy].Value.ToString());

                string subject = wi.Fields[Microsoft.TeamFoundation.WorkItemTracking.Client.CoreField.Title].Value.ToString();                

                Int32.TryParse(wi.Fields[Microsoft.TeamFoundation.WorkItemTracking.Client.CoreField.Id].Value.ToString(), out tfsId);

                string description = ParseDescription(wi.Fields[Microsoft.TeamFoundation.WorkItemTracking.Client.CoreField.Description].Value.ToString());
                
                int typeId = ParseType(wi.Fields[Microsoft.TeamFoundation.WorkItemTracking.Client.CoreField.WorkItemType].Value.ToString(), out typeTitle);

                int statusId = ParseStatus(wi.Fields[Microsoft.TeamFoundation.WorkItemTracking.Client.CoreField.State].Value.ToString(), boardColumn);

                int priorityId = ParsePriority(boardColumn, out priorityTitle);
                
                var workPackage = GetNewWorkPackage(tfsId, subject, description, typeId, typeTitle, statusId, assignedToId, responsibleId, priorityId, priorityTitle);

                try
                {
                    CallPostOrPatchApi(workPackage, OPAPIS.CREATEWP, HTTPMETHODS.POST);
                }
                catch(Exception ex)
                {
                    Console.Out.WriteLine(ex);
                }                
            }            
        }

        private string ParseDescription(string htmlDescription)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlDescription);
            string result = htmlDoc.DocumentNode.InnerText;
            result = result.Replace("&nbsp", "");
            return result;
        }

        private int ParsePriority(string boardColumn, out string priorityTitle)
        {
            string convertedPriority = "";

            switch (boardColumn.ToLower())
            {
                case "tracking":
                    convertedPriority = "high";
                    break;
                case "graveyard":
                    convertedPriority = "high";
                    break;
                case "queued":
                    convertedPriority = "immediate";
                    break;
                case "approved":
                    convertedPriority = "immediate";
                    break;
                case "committed":
                    convertedPriority = "immediate";
                    break;
                case "validation":
                    convertedPriority = "high";
                    break;                
                default:
                    convertedPriority = "normal";
                    break;
            }

            var priority = m_PriorityTypes.Where(c => c.name.ToLower() == convertedPriority.ToLower()).First();
            priorityTitle = priority.name;
            return priority.id;
        }

        public int ParseStatus(string tfsStatus, string boardColumn)
        {
            string convertedStatus;

            switch(tfsStatus.ToLower())
            {
                case "active":
                    convertedStatus = "in progress";
                break;
                case "resolved":
                    convertedStatus = "validation";
                    break;
                default:
                    convertedStatus = tfsStatus.ToLower();
                    break;
            }

            switch(boardColumn.ToLower())
            {
                case "tracking":
                    convertedStatus = "tracking";
                    break;
                case "graveyard":
                    convertedStatus = "on hold";
                    break;
                case "queued":
                    convertedStatus = "new";
                    break;
                case "approved":
                    convertedStatus = "approved";
                    break;
                case "committed":
                    convertedStatus = "in progress";                                    
                    break;
                case "validation":
                    convertedStatus = "validation";
                    break;                    
            }

            var status = m_StatusTypes.Where(c => c.name.ToLower() == convertedStatus.ToLower()).First();

            return status.id;
        }

        public int ParseType(string tfsType, out string title)
        {
            var type = m_Types.Where(c => c.name.ToLower() == tfsType.ToLower()).First();

            title = type.name;
            return type.id;
        }

        public int ParseName(string name)
        {
            int retVal = 0;

            var parsedName = Regex.Match(name, @"([A-Z])\w+[.]+([A-Z])\w+").Value;

            if (parsedName == string.Empty)
                parsedName = Regex.Match(name, @"([A-Z])\w+[ ]+([A-Z])\w+").Value;

            var opObject = m_Users.Where(c => c.login.ToLower() == parsedName.ToLower() || c.name.ToLower() == parsedName.ToLower());

            if (opObject != null && opObject.Count() > 0)
                retVal = opObject.First().id;            

            return retVal;            
        }

        public String GetNewWorkPackage(
            int tfsId, 
            string subject, 
            string description, 
            int typeId, 
            string typeTitle, 
            int statusId, 
            int assigneeId, 
            int responsibleId, 
            int priorityId, 
            string priorityTitle)
        {
            WorkPackage retVal = new WorkPackage()
            {
                tfsid = string.Format("{0}", tfsId),
                subject = string.Format("{0}", subject),
                description = new WorkPackageDescription()
                {
                    format = "textile",
                    raw = string.Format("{0}", string.IsNullOrEmpty(description) ? "No Description Given" : description)
                },
                links = new WorkPackageLinks()
                {
                    type = new TypeLink()
                    {
                        href = string.Format("/api/v3/types/{0}", typeId),
                        title = string.Format("{0}", typeTitle)
                    },
                    status = new GenericHrefLink()
                    {
                        href = string.Format("/api/v3/statuses/{0}", statusId)
                    },
                    priority = new PriorityLink()
                    {
                        href = string.Format("/api/v3/priorities/{0}", priorityId),
                        title = string.Format("{0}", priorityTitle)
                    },
                    assignee = new GenericHrefLink()
                    {
                        href = string.Format("/api/v3/users/{0}", assigneeId)
                    },
                    responsible = new GenericHrefLink()
                    {
                        href = string.Format("/api/v3/users/{0}", responsibleId)
                    },
                    project = new GenericHrefLink()
                    {
                        href = string.Format("/api/v3/projects/{0}", m_TargetProjectId)
                    }
                }
            };

            var conver = JsonConvert.SerializeObject(retVal);

            Console.Out.WriteLine(conver);

            if (assigneeId == 0) // this is not a required field in tfs, if it is empty but the _link is sent for assignees to op with an id of 0, the post will fail
                conver = conver.Replace("\"assignee\":{\"href\":\"/api/v3/users/0\"},", "");

            if(responsibleId == 0)
                conver = conver.Replace("\"responsible\":{\"href\":\"/api/v3/users/0\"},", "");

            return conver;
        }

        public String PatchWorkPackageStatusOnly(int id)
        {
            int statusId = ParseStatus("rejected", "rejected");

            WorkPackageStatusOnly retVal = new WorkPackageStatusOnly()
            {                
                links = new WorkPackageLinksStatusOnly()
                {                    
                    status = new GenericHrefLink()
                    {
                        href = string.Format("/api/v3/statuses/{0}", statusId)
                    }
                }
            };

            var conver = JsonConvert.SerializeObject(retVal);            

            return conver;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        public void RejectWorkItems(int startId, int endId)
        {
            for (int index = startId; index <= endId; index++)
            {
                var updateUrl = string.Format(OPAPIS.UPDATEWP, index);
                var stringToUpdate = CallGetOrDelete(updateUrl, HTTPMETHODS.GET);
                stringToUpdate = stringToUpdate.Replace("statuses/1", "statuses/15");
                CallPostOrPatchApi(stringToUpdate, updateUrl, HTTPMETHODS.PATCH);
            }
        }
    }
}
