var  express = require('express');
var  bodyParser = require('body-parser');
var  http = require('http');

var  app = express();
var server = http.createServer(app);
app.use(bodyParser.json());

var port = process.env.PORT || 8000;
server.listen(port, function () {
    console.log("Server is up and running...");
});

var request, response, parameters;

var documentClient = require('documentdb').DocumentClient
var endpoint = "https://henrydb.documents.azure.com:443/";
var primaryKey = "your document db key goes here";
var dbClient = new documentClient(endpoint, { "masterKey": primaryKey });
var dbLink = 'dbs/myDb';
var collLink = dbLink + '/colls/myCollection';

app.post('/fortuneCookie', function (req, res) {
    let secret = req.get("mysecret");
    if(secret === "12345"){
      request = req;
      response = res;  
      console.log('Fortune Cookie Request headers: ' + JSON.stringify(request.headers));
      console.log('Fortune Cookie Request body: ' + JSON.stringify(request.body));
      if (request.body.queryResult) {
        processV2Request();
      } else {
        console.log('Invalid Request');
        return response.status(400).end('Invalid Webhook Request');
      }
    } 
    else {
      return response.status(403).end('Access denied!');
    }
});

const intentHandlers = {
    'input.welcome': () => {
      sendWelcome(); 
    },
    'input.unknown': () => {
      sendResponse('I\'m having trouble, can you try that again?'); 
    },
    'input.fortune': () => {
      sendQuote(); 
    },
    'input.feeling': () => {
      sendQuoteWithFeeling(); 
    },
    'input.authors': () => {
      sendAuthors(); 
    },
    'input.author.quote': () => {
      sendAuthorQuote(); 
    },
    'default': () => {
      sendResponse('This is Henry\'s Fortune Cookie!' );
    }
  };

  function processV2Request () {
    let action = (request.body.queryResult.action) ? request.body.queryResult.action : 'default';
    parameters = request.body.queryResult.parameters || {};
  
    if (intentHandlers[action]) { 
      intentHandlers[action]();
    }
    else {
      intentHandlers['default']();   
    }
  }
  function sendQuoteWithFeeling () {
    let responseJson, randomNumber;
    if(parameters.Feeling === "happy"){
      randomNumber = Math.floor(Math.random() * 5);
    }
    else {
      randomNumber = Math.floor(Math.random() * 4) + 5;
    }
    let query = "SELECT * FROM c";
    sendQuoteResponse(query, randomNumber, "sendQuoteWithFeeling");
  }
  function sendQuote () {
    let randomNumber = Math.floor(Math.random() * 9);
    let query = "SELECT * FROM c";
    sendQuoteResponse(query, randomNumber, "sendQuote");
  }
  function sendAuthorQuote () {
    let query = "SELECT * FROM c where contains(lower(c.author), '" + parameters.Author.toLowerCase() + "')";
    sendQuoteResponse(query, 0, "sendAuthorQuote");
  }
  
  function sendQuoteResponse(query, index, functionName) {
    dbClient.queryDocuments(collLink, query).toArray(function (err, quotes) {
      if(err)
        return response.status(400).end('Error while calling cosmoDB in ' + functionName);
      else {
        let responseJson = { fulfillmentText: quotes[index].quote }; 
        console.log(functionName + ":" + JSON.stringify(responseJson));
        response.json(responseJson);
      }
    }); 
  }
  function sendResponse (responseToUser) {
    let responseJson = { fulfillmentText: responseToUser }; 
    console.log('Response to Fortune Cookie: ' + JSON.stringify(responseJson));
    response.json(responseJson);
  }
  function sendWelcome () {
    let responseJson;
    let executeCustomWelcome = Math.random() >= 0.5;
    if(executeCustomWelcome) { 
      responseJson = {
        followupEventInput: {
          name: "custom_welcome_event"
        }
      };
    }
    else {
      responseJson = { 
        fulfillmentText: 'Hello, Welcome to Henry\'s Fortune Cookie!',
        fulfillmentMessages: [
          {
            platform: "ACTIONS_ON_GOOGLE",
            simpleResponses: {
              simpleResponses: [
                {
                  ssml: `<speak>
                          <audio src="https://actions.google.com/sounds/v1/weather/thunder_crack.ogg" />
                          <break time="200ms"/>
                          <prosody rate="medium" pitch="+2st">
                            Hello Welcome to Henry's Fortune Cookie!
                          </prosody>
                        </speak>`,
                  displayText: "Hello, Welcome to Henry\'s Fortune Cookie!"
                }
              ]
            }
          }
        ] 
      }; 
    }
    console.log('sendWelcome: ' + JSON.stringify(responseJson));
    response.json(responseJson);
  }
  function sendAuthors () {
    let defaultText = "Choose an author. T S Eliot, J B White, Dave Stutman, Winston Churchill, ";
    defaultText = defaultText + "Woody Allen, Confucius, Mark Twain, Albert Einstein, Steven Wright";
    let responseJson = { 
      fulfillmentText: defaultText,
      fulfillmentMessages: [
        {
          platform: "ACTIONS_ON_GOOGLE",
          listSelect: {
            title: "Select an Author",
            items: [
              { info: { key: "Eliot" }, title: "T. S. Eliot"},
              { info: { key: "White" }, title: "J. B. White"},
              { info: { key: "Stutman" }, title: "Dave Stutman"},
              { info: { key: "Churchill" }, title: "Winston Churchill"},
              { info: { key: "Allen" }, title: "Woody Allen"},
              { info: { key: "Confucius" }, title: "Confucius"},
              { info: { key: "Twain" }, title: "Mark Twain"},
              { info: { key: "Einstein" }, title: "Albert Einstein"},
              { info: { key: "Wright" }, title: "Steven Wright"}
            ]
          }
        }
      ] 
    }; 
    console.log('sendAuthors: ' + JSON.stringify(responseJson));
    response.json(responseJson);  
  }
  