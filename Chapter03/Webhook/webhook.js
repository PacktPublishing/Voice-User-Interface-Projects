'use strict';

const firebase = require('firebase-functions');
var request, response, parameters;
exports.dialogflowFirebaseFulfillment = firebase.https.onRequest((req, res) => {
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
const quotes = [
  {
    "author": "T. S. Eliot", "tags": "inspire",
    "quote": "Do not stop to ask what is it;  Let us go and make our visit."
  },
  {
    "author": "J. B. White", "tags": "happy",
    "quote": "at least I thought I was dancing, til somebody stepped on my hand."
  },
  {
    "author": "Dave Stutman", "tags": "inspire",
    "quote": "Complacency is the enemy of progress."    
  },
  {
    "author": "Winston Churchill", "tags": "inspire",
    "quote": "Success is the ability to go from one failure to another with no loss of enthusiasm."
  },
  {
    "author": "Woody Allen", "tags": "happy",
    "quote": "There's more to life than sitting around in the sun in your underwear playing the clarinet."
  },
  {
    "author": "Confucius", "tags": "inspire",
    "quote": "It does not matter how slowly you go so long as you do not stop."
  },
  {
    "author": "Mark Twain", "tags": "inspire",
    "quote": "It usually takes me more than three weeks to prepare a good impromptu speech."    
  },
  {
    "author": "Albert Einstein", "tags": "inspire",
    "quote": "Imagination is more important than knowledge."
  },
  {
    "author": "Steven Wright", "tags": "inspire",
    "quote": "You can't have everything. Where would you put it?"    
  }
];
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
    randomNumber = Math.floor(Math.random() * 4) + 6;
  }
  responseJson = { fulfillmentText: quotes[randomNumber].quote }; 
  console.log('sendQuote: ' + JSON.stringify(responseJson));
  response.json(responseJson);
}
function sendResponse (responseToUser) {
  let responseJson = { fulfillmentText: responseToUser }; 
  console.log('Response to Fortune Cookie: ' + JSON.stringify(responseJson));
  response.json(responseJson);
}
function sendQuote () {
  let randomNumber = Math.floor(Math.random() * 9);
  let responseJson = { fulfillmentText: quotes[randomNumber].quote }; 
  console.log('sendQuote: ' + JSON.stringify(responseJson));
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
function sendAuthorQuote () {
  let authorQuote = quotes.find(x => x.author.toLowerCase().indexOf(parameters.Author.toLowerCase())>=0).quote;
  let responseJson = { fulfillmentText: authorQuote }; 
  console.log('sendAuthorQuote: ' + JSON.stringify(responseJson));
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
