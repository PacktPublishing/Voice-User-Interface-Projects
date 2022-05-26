## [Get this title for $10 on Packt's Spring Sale](https://www.packt.com/B08554?utm_source=github&utm_medium=packt-github-repo&utm_campaign=spring_10_dollar_2022)
-----
For a limited period, all eBooks and Videos are only $10. All the practical content you need \- by developers, for developers

# Voice User Interface Projects

<a href="https://www.packtpub.com/web-development/voice-user-interface-projects?utm_source=github&utm_medium=repository&utm_campaign=9781788473354"><img src="https://d255esdrn735hr.cloudfront.net/sites/default/files/imagecache/ppv4_main_book_cover/B08554_MockupCoverNew.png" alt="Book Name" height="256px" align="right"></a>

This is the code repository for [Voice User Interface Projects](https://www.packtpub.com/web-development/voice-user-interface-projects?utm_source=github&utm_medium=repository&utm_campaign=9781788473354), published by Packt.

**Build voice-enabled applications using Dialogflow for Google Home and Alexa Skills Kit for Amazon Echo**

## What is this book about?
From touchscreen and mouse-click, we are moving to voice- and conversation-based user interfaces. By adopting Voice User Interfaces (VUIs), you can create a more compelling and engaging experience for your users. Voice User Interface Projects teaches you how to develop voice-enabled applications for desktop, mobile, and Internet of Things (IoT) devices.
This book explains in detail VUI and its importance, basic design principles of VUI, fundamentals of conversation, and the different voice-enabled applications available in the market. You will learn how to build your first voice-enabled application by utilizing DialogFlow and Alexa’s natural language processing (NLP) platform. Once you are comfortable with building voice-enabled applications, you will understand how to dynamically process and respond to the questions by using NodeJS server deployed to the cloud. You will then move on to securing NodeJS RESTful API for DialogFlow and Alexa webhooks, creating unit tests and building voice-enabled podcasts for cars. Last but not the least you will discover advanced topics such as handling sessions, creating custom intents, and extending built-in intents in order to build conversational VUIs that will help engage the users.

This book covers the following exciting features:
* Understand NLP platforms with machine learning
* Exploit best practices and user experiences in creating VUI
* Build voice-enabled chatbots
* Host, secure, and test in a cloud platform
* Create voice-enabled applications for personal digital assistant devices

If you feel this book is for you, get your [copy](https://www.amazon.com/dp/1788473353) today!

<a href="https://www.packtpub.com/?utm_source=github&utm_medium=banner&utm_campaign=GitHubBanner"><img src="https://raw.githubusercontent.com/PacktPublishing/GitHub/master/GitHub.png" 
alt="https://www.packtpub.com/" border="5" /></a>


## Instructions and Navigations
All of the code is organized into folders. For example, Chapter02.

The code will look like the following:
```
var request, response;
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
```

**Following is what you need for this book:**
Copy and paste the Audience section from the EPIC.

With the following software and hardware list you can run all code files present in the book (Chapter 1-10).

### Software and Hardware List

| Chapter  | Software required                   | OS required                        |
| -------- | ------------------------------------| -----------------------------------|
| 4-10     | Visual Studio, Node.js, Postman,    | Windows, Mac OS X, and Linux (Any) |
|          | Visual Studio Community             |

### Related products <Paste books from the Other books you may enjoy section>
* Hands-On Chatbots and Conversational UI Development [[Packt]](https://www.packtpub.com/application-development/hands-chatbots-and-conversational-ui-development?utm_source=github&utm_medium=repository&utm_campaign=9781788294669) [[Amazon]](https://www.amazon.com/dp/1788294661)

* Alexa Skills Projects [[Packt]](https://www.packtpub.com/hardware-and-creative/alexa-skills-projects?utm_source=github&utm_medium=repository&utm_campaign=9781788997256) [[Amazon]](https://www.amazon.com/dp/1788997255)

## Get to Know the Author
**Henry Lee** has over 18 years of experience in software engineering. His passion for software engineering has led him to work at various start-ups. Currently, he works as the principal architect responsible for the R&D and the digital strategies. In his spare time, He loves to travel and snowboard, and enjoys discussing the latest technology trends over a cigar! Also, he authored three books at Apress on mobile development.

### Suggestions and Feedback
[Click here](https://docs.google.com/forms/d/e/1FAIpQLSdy7dATC6QmEL81FIUuymZ0Wy9vH1jHkvpY57OiMeKGqib_Ow/viewform) if you have any feedback or suggestions.
