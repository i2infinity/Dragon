Dragon Sentiment Classifier
===========================

Dragon Sentiment Classifier is primarily used in the Product Review Search App - Solvy http://solvy.cloudapp.net/

<p align="center">
  <img src="http://solvy.cloudapp.net/images/blurr_Phone.png"/>
</p>

Description
----------------------

Dragon Sentiment API is a C# implementation of the Naive Bayes Sentiment Classifier to analyze the sentiment of a text corpus. Sentiment analysis calculates the attitude or opinion towards something, such as a product, location, organization or person.
This API provides easy to use mechanism to identify the positive or negative sentiment of an input document. Please note that this API works best on a large corpus of words (e.g. product reviews or blogs with 1000+ words) and targeted towards electronic/gadget reviews.


Training the Dragon
----------------------

Dragon API is a machine learning algorithm that first needs to be taught how to classify a random collection of words and this training is performed using a couple of included evidence files (Postive.Evidence.csv and Negative.Evidence.csv) that contain the frequency map for words that commonly occur in electronic gadget reviews.

Getting Started
----------------------

In order to classify plain text contents:

```c#
//positiveReviews and negativeReviews are the training set used by our Dragon Classigfier
var positiveReviews = new Evidence("Positive", "Repository\\Positive.Evidence.csv");
var negativeReviews = new Evidence("Negative", "Repository\\Negative.Evidence.csv");

//Instantiate the classifier using the training data set
var classifier = new Classifier(positiveReviews, negativeReviews);

//testData – String variable that contains the readable plain text contents of the document that needs to be classified (Strictly no HTML)
//The second parameter is the list of words that are excluded to improve classification performance
var scores = classifier.Classify(testData, DragonHelper.DragonHelper.ExcludeList);
Console.WriteLine("Positive Score - " + scores["Positive"]);
```
