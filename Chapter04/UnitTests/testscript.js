var projectId = 'your project id goes here'; 
var sessionId = 'my-test-session-id';
var languageCode = 'en-US';

var dialogflow = require('dialogflow');
var sessionClient = new dialogflow.SessionsClient();
var sessionPath = sessionClient.sessionPath(projectId, sessionId);

callFortuneCookie('hello');
callFortuneCookie('give me quote');

function callFortuneCookie(query) {
  let request = {
    session: sessionPath,
    queryInput: { text: { text: query, languageCode: languageCode } }
  };
  sessionClient
  .detectIntent(request)
  .then(responses => {
    console.log('Detected intent');
    let result = responses[0].queryResult;
    console.log(`  Query: ${result.queryText}`);
    console.log(`  Response: ${result.fulfillmentText}`);
    if (result.intent) {
      console.log(`  Intent: ${result.intent.displayName}`);
    } else {
      console.log(`  No intent matched.`);
    }
  })
  .catch(err => {
    console.error('ERROR:', err);
  });
}
