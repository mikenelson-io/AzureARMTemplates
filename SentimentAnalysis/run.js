var http = require('https');

module.exports = function(context, data) {
    
    var TweetText = data.TweetText;
    var RetweetCount = data.RetweetCount;
    var TweetBy = data.TweetBy;
    var TweetDate = data.TweetDate;
    var TweetId = data.TweetId;
    
    var text_data = '{ "documents": [{ "language": "en", "text": "' + TweetText + '", "id": "string", }] }';

    var post_options = {
        host: 'westus.api.cognitive.microsoft.com',
        port: '443',
        path: '/text/analytics/v2.0/sentiment',
        method: 'POST',
        headers: {
            "Content-Type": "application/json",
            "Ocp-Apim-Subscription-Key": ""
        }
    }

    var text_request = http.request(post_options);
    text_request.end(text_data);
    text_request.on('response', function (response) {
        response.on('data', function (chunk) {
            reply = JSON.parse(chunk);
        });
    });
    
    context.res = {
        TweetText: TweetText,
        RetweetCount: RetweetCount,
        TweetBy: TweetBy,
        TweetScore: reply.documents[0].score,
        TweetDate: TweetDate,
        TweetId: TweetId
    };

    context.done();
};


