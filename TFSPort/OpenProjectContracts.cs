using Newtonsoft.Json;
using System;
namespace TFSPort.OP.Contracts
{
    public class WorkPackageStatusOnly
    {
        [JsonProperty("_links")]
        public WorkPackageLinksStatusOnly links { get; set; }
    }

    public class WorkPackageLinksStatusOnly
    {        
        [JsonProperty("status")]
        public GenericHrefLink status { get; set; }        
    }

    public class WorkPackage
    {
        [JsonProperty("customField7")]
        public string tfsid { get; set; }

        [JsonProperty("subject")]
        public String subject { get; set; }

        [JsonProperty("description")]
        public WorkPackageDescription description { get; set; }

        [JsonProperty("_links")]
        public WorkPackageLinks links { get; set; }
    }    

    public class WorkPackageDescription
    {
        [JsonProperty("format")]
        public string format { get; set; }
        [JsonProperty("raw")]
        public string raw { get; set; }
    }

    public class WorkPackageLinks
    {
        [JsonProperty("type")]
        public TypeLink type { get; set; }
        [JsonProperty("status")]
        public GenericHrefLink status { get; set; }
        [JsonProperty("priority")]
        public PriorityLink priority { get; set; }
        [JsonProperty("assignee")]
        public GenericHrefLink assignee { get; set; }
        [JsonProperty("responsible")]
        public GenericHrefLink responsible { get; set; }
        [JsonProperty("project")]
        public GenericHrefLink project { get; set; }
    }

    public class TypeLink
    {
        [JsonProperty("href")]
        public string href { get; set; }
        [JsonProperty("title")]
        public string title { get; set; }
    }

    public class PriorityLink
    {
        [JsonProperty("href")]
        public string href { get; set; }
        [JsonProperty("title")]
        public string title { get; set; }
    }

    public class GenericHrefLink
    {
        [JsonProperty("href")]
        public string href { get; set; }
    }

    public class OpenProjectUser
    {
        [JsonProperty("_type")]
        public string _type { get; set; }
        [JsonProperty("id")]
        public int id { get; set; }
        [JsonProperty("login")]
        public string login { get; set; }
        [JsonProperty("admin")]
        public bool admin { get; set; }
        [JsonProperty("subtype")]
        public string subtype { get; set; }
        [JsonProperty("firstName")]
        public string firstName { get; set; }
        [JsonProperty("lastName")]
        public string lastName { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("email")]
        public object email { get; set; }
        [JsonProperty("avatar")]
        public string avatar { get; set; }        
        [JsonProperty("status")]
        public string status { get; set; }
        [JsonProperty("identityUrl")]
        public object identityUrl { get; set; }        
    }

    public class WorkItemType
    {
        [JsonProperty("_type")]
        public string _type { get; set; }
        [JsonProperty("id")]
        public int id { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("color")]
        public string color { get; set; }
        [JsonProperty("position")]
        public int position { get; set; }
        [JsonProperty("isDefault")]
        public bool isDefault { get; set; }
        [JsonProperty("isMilestone")]
        public bool isMilestone { get; set; }
    }

    public class StatusType
    {
        [JsonProperty("_type")]
        public string _type { get; set; }
        [JsonProperty("id")]
        public int id { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("isClosed")]
        public bool isClosed { get; set; }
        [JsonProperty("isDefault")]
        public bool isDefault { get; set; }
        [JsonProperty("defaultDoneRatio")]
        public object defaultDoneRatio { get; set; }
        [JsonProperty("position")]
        public int position { get; set; }        
    }

    public class PriorityType
    {
        [JsonProperty("_type")]
        public string _type { get; set; }
        [JsonProperty("id")]
        public int id { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("position")]
        public int position { get; set; }
        [JsonProperty("isDefault")]
        public bool isDefault { get; set; }
        [JsonProperty("isActive")]
        public bool isActive { get; set; }        
    }

    public class Project
    {
        [JsonProperty("_type")]
        public string _type { get; set; }
        [JsonProperty("id")]
        public int id { get; set; }
        [JsonProperty("identifier")]
        public string identifier { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("description")]
        public string description { get; set; }
        [JsonProperty("type")]
        public string type { get; set; }        
    }
}