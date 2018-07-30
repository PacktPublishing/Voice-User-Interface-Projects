var redis = require("redis");
var redisAuth = 'your redis auth key goes here';
var redisHost = 'henrytest.redis.cache.windows.net';
var redisPort = 6380;

var winston = require("winston");
require("winston-azure-blob-transport");
var loggingLevel = "info";
var logger = new winston.Logger({
    level: loggingLevel,
    transports: [
        new winston.transports.Console({
            stderrLevels: [loggingLevel],
            colorize: true
        }),
        new winston.transports.AzureBlob({
            account: {
                name: "henrytestlog",
                key: "your azure blob key goes here"
            },
            containerName: "applog",
            blobName: "henrytestlog",
        })
    ]
});

var express = require('express');
var bodyParser = require('body-parser');
var http = require('http');
var unirest = require('unirest');
var app = express();

app.use("/ping", function (req, res, next) {
    logger.info("ping service.");
    res.send('Welcome to Cooking Service');
});

var verifier = require('alexa-verifier-middleware');
var alexaRouter = express.Router();
app.use("/alexa", alexaRouter);
if(!process.env.ISDEBUG) {
    logger.info("Setup Alexa verifier.");
    alexaRouter.use(verifier);
}
alexaRouter.use(bodyParser.json());

var dialogflowRouter = express.Router();
app.use("/dialogflow", dialogflowRouter);
dialogflowRouter.use(bodyParser.json());

var server = http.createServer(app);
var port = process.env.PORT || 8000;
server.listen(port, function () {
    logger.info("Server is up and running...");
});

alexaRouter.post('/cookingApi', function (req, res) {
    try{
        logger.info(JSON.stringify(req.body, null, '\t')); 
        if (req.body.request.type === 'LaunchRequest') {
            logger.info("LaunchRequest");
            res.json({
                "version": "1.0",
                "response": {
                  "shouldEndSession": true,
                  "outputSpeech": {
                    "type": "PlainText",
                    "text": "Welcome to Henry's Cooking App"
                  }
                }
              });    
        } else if (req.body.request.type === 'IntentRequest' &&
                 req.body.request.intent.name === 'GetCookingIntent') {
            if(!StartCookingInstructionDialog(req, res))
                BuildGetCookingInstruction(req, res);
        } else if (req.body.request.type === 'SessionEndedRequest') { 
            logger.error('Session ended', req.body.request.reason);            
            if(req.body.request.reason=='ERROR')
                logger.error(JSON.stringify(req.body.request, null, '\t'));
        } else if (req.body.request.type === 'IntentRequest' &&
                req.body.request.intent.name === 'AMAZON.HelpIntent') {
            BuildHelpIntentResponse(req, res);                    
        } else if (req.body.request.type === 'IntentRequest' &&
            req.body.request.intent.name === 'GetMoreRecipesIntent') {
            GetOffset(req, res, false);                    
        }

        
    } catch(e){
        logger.error(e);
    }    
});

dialogflowRouter.post('/cookingApi', function (req, res) {
    try{
        logger.info(JSON.stringify(req.body, null, '\t')); 
        var secret = req.get("mysecret");
        if(secret === "12345" && req.body.queryResult){
            if(req.body.queryResult.action == 'input.cooking'){
                logger.info("input.cooking"); 
                BuildGetCookingInstruction(req, res);
            }else if(req.body.queryResult.action == 'input.more'){
                GetOffset(req, res, false);
            }else if(req.body.queryResult.action == 'input.help'){
                BuildHelpIntentResponse(req, res);
            }else{
                var responseJson = { fulfillmentText: "Welcome to Henrys Kitchen" }; 
                res.json(responseJson);            
            }
        }else {
            return res.status(403).end('Access denied!');
        }
    } catch(e){
        logger.error(e);
    }    
});

function GetOffset(req, res, saveFromRequest){
    logger.info('GetOffset');
    var client = redis.createClient(redisPort, redisHost, 
        {auth_pass: redisAuth, tls: {servername: redisHost}});
    if(req.body.queryResult){
        var parameters = req.body.queryResult.parameters;
        var key = req.body.originalDetectIntentRequest.payload.user.userId;
    }else{
        var request = req.body.request;
        var session = req.body.session;
        var key = session.user.userId;
    }        

    var timeInSeconds = 60*10;
    if(saveFromRequest) {
        var queryObject = {
            offset: 0,
            foodName: req.body.queryResult ? parameters.Foods : request.intent.slots.Foods.value,
            dietTypes: req.body.queryResult ?  parameters.DietTypes : request.intent.slots.DietTypes.value
        };
        client.set(key, JSON.stringify(queryObject), 'EX', timeInSeconds);
    }
    else {
        client.get(key, function(error, result){ 
            if (error) {
                logger.error(error);
            } else if (result) { 
                var queryObject = JSON.parse(result);
                queryObject.offset = queryObject.offset + 3;
                client.set(key, JSON.stringify(queryObject), 'EX', timeInSeconds);                
                BuildGetCookingInstruction(req, res, queryObject);
            } else {
                logger.info('GetOffset: queryObject is not found or expired.');
                if(req.body.queryResult){
                    var responseText = "You can say ok google ask henrys kitchen I want to cook burger to get the list of recipes."
                    var responseToDialogflow = { fulfillmentText: responseText }; 
                    res.json(responseToDialogflow);     
                }else{
                    res.json({
                        "version": "1.0",
                        "response": {
                            "shouldEndSession": true,
                            "outputSpeech": {
                                "type": "PlainText",
                                "text": "You can say alexa ask henry's kitchen I want to cook burger to get the list of recipes."
                            }
                        }
                    });    
                }
            }
        });
    }
};

function BuildHelpIntentResponse(req, res){
    logger.info('BuildHelpIntentResponse');
    if(req.body.queryResult){
        var responseText = "You can say ok google ask henrys kitchen I want to cook burger."
        var responseToDialogflow = { fulfillmentText: responseText }; 
        res.json(responseToDialogflow);     
    }else{
        res.json({
            "version": "1.0",
            "response": {
                "shouldEndSession": true,
                "outputSpeech": {
                    "type": "PlainText",
                    "text": "You can say alexa ask henry's kitchen I want to cook burger."
                }
            }
        });
    }
};

function StartCookingInstructionDialog(req, res) {
    var request = req.body.request;
    logger.info(`StartCookingInstructionDialog ${request.intent.name} ${request.dialogState}`);
    
    if(request.dialogState == 'STARTED'){
        res.json({
            "version": "1.0",
            "response": {
                "shouldEndSession": false,
                "directives": [
                    {
                      "type": "Dialog.Delegate",
                      "updatedIntent":{
                        "name": "GetCookingIntent",
                        "slots":{
                            "DietTypes": {
                                "name": "DietTypes",
                                "value": request.intent.slots.DietTypes.value 
                                         ? request.intent.slots.DietTypes.value 
                                         : ""
                            },
                            "Foods": {
                                "name": "Foods",
                                "value": request.intent.slots.Foods.value 
                                         ? request.intent.slots.Foods.value 
                                         : ""                                
                            }
                        }
                      }
                    }
                ]
            },            
        });
    } else if (request.dialogState != 'COMPLETED'){
        res.json({
            "version": "1.0",
            "response": {
                "shouldEndSession": false,
                "directives": [
                    {
                      "type": "Dialog.Delegate"
                    }
                ]
            }
        });
    } else {
        return false;
    }
    return true;
};

function BuildGetCookingInstruction(req, res, queryObject) {
    logger.info("BuildGetCookingInstruction"); 
    var url = 'https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/search?';
    url += 'number=3&instructionsRequired=true';
    var offset = queryObject ? queryObject.offset : 0;  
    url += `&offset=${offset}`;
    if(req.body.queryResult){
        var parameters = req.body.queryResult.parameters;
        if(queryObject || parameters.Foods) {
            var foodName = queryObject ? queryObject.foodName : parameters.Foods;
            url += `&query=${foodName}`;
        }
        if(queryObject || parameters.DietTypes) {
            var dietTypes = queryObject ? queryObject.dietTypes : parameters.DietTypes;
            url += `&diet=${dietTypes}`;
        }
    }else{
        var request = req.body.request;    
        if(queryObject || request.intent.slots.Foods.value) {
            var foodName = queryObject ? queryObject.foodName : request.intent.slots.Foods.value;
            url += `&query=${foodName}`;
        }
        if(queryObject || request.intent.slots.DietTypes.value) {
            var dietTypes = queryObject ? queryObject.dietTypes : request.intent.slots.DietTypes.value;
            url += `&diet=${dietTypes}`;
        }
    }
    logger.info("Executing spoonecular: "+url); 
    unirest.get(url)
        .header("X-Mashape-Key", "your spoonacular key goes here")
        .header("X-Mashape-Host", "spoonacular-recipe-food-nutrition-v1.p.mashape.com")
        .end(function (result) {
            var responseText = "";
            if(result.error){
                logger.error('Error processing spoonacular.');
                logger.error(result.body);
                logger.error(result.error);                
                responseText = `I am sorry there was an issue processing your request.`;
            } else {
                logger.info("Successfully received results from spoonacular.");
                logger.info(result.body.results);
                var dishTitle = '';
                for(i=0; i < result.body.results.length; i++) {
                    dishTitle += result.body.results[i].title + ', ';
                }                
                responseText = `I found following dishes that you can cook. ${dishTitle}`;
            }

            logger.info('BuildGetCookingInstruction Saving queryObject');
            if(!queryObject){
                GetOffset(req, res, true);
            }

            if(req.body.queryResult){
                var responseToDialogflow = { fulfillmentText: responseText }; 
                logger.info(JSON.stringify(responseToDialogflow, null, '\t'));
                res.json(responseToDialogflow); 
            }else{
                var responseToAlexa = {
                    "version": "1.0",
                    "response": {
                        "shouldEndSession": true,
                        "outputSpeech": {
                            "type": "PlainText",
                            "text": responseText
                        }
                    }
                };
                logger.info(JSON.stringify(responseToAlexa, null, '\t'));
                res.json(responseToAlexa); 
            }  
        }); 
};