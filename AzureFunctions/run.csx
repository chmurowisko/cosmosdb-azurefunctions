using System.Net;
using System.Text;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{

    log.Info("C# HTTP trigger function processed a request.");
    // parse query parameter
    string paramName = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "paramName", true) == 0)
        .Value;
    string paramValue = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "paramValue", true) == 0)
        .Value;
    string databaseName = System.Environment.GetEnvironmentVariable("databaseName", EnvironmentVariableTarget.Process); 
    string collectionName = System.Environment.GetEnvironmentVariable("collectionName", EnvironmentVariableTarget.Process);
    string storeProcedureName = System.Environment.GetEnvironmentVariable("storedProcedureName", EnvironmentVariableTarget.Process);
    string EndpointUri = System.Environment.GetEnvironmentVariable("endpointUri", EnvironmentVariableTarget.Process);
    string PrimaryKey = System.Environment.GetEnvironmentVariable("primaryKey", EnvironmentVariableTarget.Process); 

    DocumentClient client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

    FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

    Database database = client.CreateDatabaseQuery().Where(db => db.Id == databaseName).ToArray().FirstOrDefault();

    DocumentCollection collection = client.CreateDocumentCollectionQuery(database.SelfLink).Where(c => c.Id == collectionName).ToArray().FirstOrDefault();
    var storedProcedureLink = collection.StoredProceduresLink;
    StoredProcedure storedProcedure = client.CreateStoredProcedureQuery(collection.StoredProceduresLink).Where(p => p.Id == storeProcedureName)
                                                                                                              .AsEnumerable().FirstOrDefault();
    string message = "";
    dynamic results = null;
    if (storedProcedure != null)
    {
        var response = client.ExecuteStoredProcedureAsync<dynamic>(storedProcedure.SelfLink, new RequestOptions { PartitionKey = new PartitionKey("measurements") }, paramName, paramValue);
        results = response.Result;
        message = JsonConvert.SerializeObject(results.Response, Formatting.Indented);
    }

    return (paramName == null || paramValue == null)
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a paramName and paramValue on the query string or in the request body")
        : new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(message, Encoding.UTF8, "application/json")
        };
}
