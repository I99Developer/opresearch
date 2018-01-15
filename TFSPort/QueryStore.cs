using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
namespace TFSPort
{
    class QueryStore
    {
        public static WorkItemCollection GetItemsByBoardColumn(string boardColumn, string sourceProject)
        {
            string info = String.Empty;

            var tpc = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri("yourserver/tfs/defaultcollection"));
            WorkItemStore workItemStore = new WorkItemStore(tpc);

            string queryText = String.Format("" +
                "   SELECT * FROM WorkItems WHERE [System.TeamProject] = @project " +
                "   and [System.State] <> 'Closed'" +
                "   and [System.State] <> 'Completed'" +
                "   and [System.State] <> 'Removed'" +
                "   and [System.BoardColumn] = '{0}'" +
                "   and [System.WorkItemType] <> 'Task'" +
                "   and [System.WorkItemType] <> 'Test Case'" +
                "   and [System.WorkItemType] <> 'Test Suite'" +
                "   and [System.WorkItemType] <> 'Test Plan'", boardColumn);

            Query query = new Query(workItemStore, 
                queryText, 
                new Dictionary<string, string>()
                {
                    { "project", sourceProject }
                });

            WorkItemCollection wic = query.RunQuery();

            return wic;
        }        
    }
}
