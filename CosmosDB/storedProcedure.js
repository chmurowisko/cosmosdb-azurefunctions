function storedProcedure(paramName, paramValue) {
    var context = getContext();
    var response = context.getResponse();
    var collection = context.getCollection();
    var results = collection.queryDocuments(collection.getSelfLink(), 'SELECT * from metricsdata m where m.messageType = "measurements" and m.metrics.' + paramName + ' < ' + paramValue,
        {}, function (err, documents) {
            if (err) {
                throw new Error("Error: " + err.message);
            }
            response.setBody(documents);
        });
}