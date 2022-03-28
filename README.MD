Let’s face it: working with conversational data and transcript data is a lot of hard work. You’ve worked (or had your RAs work) for dozens of hours getting all of your language data transcribed into nicely-formatted files, complete with tags of who is speaking at each turn. Now, you want to run some analyses on each person’s language samples. A lot of the time, this means that you have to go back through all of your files and separate out the language for each speaker. Exhausting!

Enter ConverSplitter Plus! This software will help split apart your transcribed files by each speaker, making it fast and easy to look at the language of each person in your texts individually. Why, it can even help you detect the speaker tags in your files and remove extraneous information like timestamps!


# Features and Use

The most important thing to know for getting the most out of this software: consistency is key. If your speaker tags are not consistent from turn to turn, or if each line in your text starts differently, you’ll probably not benefit a whole lot from this software. In the examples below, you’ll get a sense of what the easiest / “best” format looks like for ConverSplitter Plus! to work with.

If you have a bunch of timestamps or other extraneous information in your text that is making it difficult to get the most from this software, worry not! ConverSplitter Plus! has some features that can help you out there, too!

Worst case scenario, feel free to send me an e-mail, and I can steer you in the right direction 🙂

At its core, this software takes .txt files as input and, using your own list of speaker tags, will separate out the texts by person. Simple as that!

To use the software, you’ll need to have your texts formatting using some kind of standardization — ConverSplitter Plus! is fast, but not that smart. Typical transcript examples might look something like this:

```
    Person 1: Hello, how are you today?
    Person 2: I’m just great, thanks for asking!
    Person 1: Say, did you try that new Thai restaurant around the corner?
    Person 3: Nope! But I heard that it’s incredible.
```

In this case, you know all of your speaker tags, and can readily supply them in the software’s “Speaker Tags” box:

```
Person 1:
Person 2:
Person 3:
```

## Detect Speakers

In the event that you don’t know all of the Speaker Tags across all of your files (or if you don’t want to type them all in manually), you can use this feature of the software. Essentially, it asks for 2 things:

- Speaker Tag Delimiter
- Maximum Speaker Tag Length

The Speaker Tag Delimiter should be whatever character (or string of characters) is used across all of your files to denote the end of the Speaker Tag. It is super important that this be consistent across all speakers in all of your text files — ConverSplitter Plus! isn’t super smart, so it only knows to look for whatever you tell it.

The Maximum Speaker Tag Length number is something that you set so that ConverSplitter Plus! can be a bit more discriminating in terms of what it believes to be a Speaker Tag. Take a look at the example text below:

```
    Person 1: Hello, how are you doing today?
    Person 2: Well, that’s a great question.
    You see, it all started like this: I was a young lad growing up in Ireland, and my father told me something…
    Person 1: “Fine, yourself” would have been an acceptable answer.
```

In this example, we might use a colon (“:”) as the Speaker Tag delimiter. However, once ConverSplitter Plus! gets to the line that starts with “You see, it all started like this:”, it’s going to see the colon and think “Hey, this looks like another Speaker! I should add ‘You see, it all started like this:’ to the Speaker Tags list!”

Oh, ConverSplitter Plus! — you tried your best. However, if we tell it that our Maximum Speaker Tag Length is 20 characters, it will know that any Speaker Tags that it finds that are longer than this should be ignored. Simple as that!

Once ConverSplitter Plus! has looked through your text files to detect Speaker Tags, you can manually edit this list to add/remove any tags that it might have gotten wrong.

## Speakers can have Multiple Lines of Text

Sometimes, transcripts are formatted so that long speaking segments are spread out across multiple lines. Let’s look back at the previous example:

```
    Person 1: Hello, how are you doing today?
    Person 2: Well, that’s a great question.
    You see, it all started like this: I was a young lad growing up in Ireland, and my father told me something…
    Person 1: “Fine, yourself” would have been an acceptable answer.
```

In this case, Person 2 only speaks once, but their text spans 2 separate lines (line #2 and line #3). By default, ConverSplitter Plus! will look at each line and, when it comes to the 3rd line, it won’t see any Speaker Tags. This leads the software to conclude that this must be some extraneous text, and it should be ignored.

However, if you check the Speakers can have Multiple Lines of Text checkbox before scanning through your texts, the software knows that free-floating lines of text belong to whatever speaker it previously identified. With this feature enabled, the software knows that line #3 should be included with line #2 when splitting apart each speaker’s text.

## RegEx Removal

Regular Expressions (RegExes) are a way to identify patterns in text in a very generalized way. For example, \d will find any number in a text, and \d:\d\d would be able to capture any pattern with a number, a colon, then 2 more numbers (e.g., 2:45, 1:23, etc.). The RegEx Removal feature will allow you to specify (using regular expressions) different patterns that you want to remove from your texts to help with splitting them apart. Consider the following example text:

```
    Ryan (2:21pm): Hey there!
    Natalie (2:21pm): Hey!
    Ryan (2:22pm): Haven’t talked in a while. What’s going on with you?
    Natalie (2:23pm): I know! Not much, just having a taco!
```

If we want to split apart the speakers in this text, notice that it would be very tricky, because each “speaker tag” is technically different due to the timestamps. Using Regular Expressions, however, we can identify and remove these rather easily. By filling the RegEx Removal box with the following pattern:

```\s\(\d+:\d+\S\S\)```

…we can omit the timestamps altogether. If you’re not familiar with RegExes, this might look confusing, so let’s break it down:
| RegEx | Meaning |
| --- | --- |
| \s | any space |
| \( | the opening parentheses
| \d+ | any sequential series of numbers
| : |	the colon
| \d+ | any sequential series of numbers
| \S | any non-space character
| \S | any non-space character
| \) | the closing parentheses

This RegEx captures (2:21pm), (2:22pm), and (2:23pm) and complete removes them from the text before splitting them apart. The example above essentially becomes this:

    Ryan: Hey there!
    Natalie: Hey!
    Ryan: Haven’t talked in a while. What’s going on with you?
    Natalie: I know! Not much, just having a taco!

As you can see, ConverSplitter Plus! will have a much easier time separating out these speakers now!

RegExes can be somewhat tricky to learn, but they are extremely powerful. Use them to remove any patterns from your texts that would get in the way of cleanly separating your speakers.

A great resource for learning about RegExes is https://www.regular-expressions.info. Most of the time when using this software, you’ll be need to construct RegExes that are fairly simply by most standards. For most timestamp formats, for example, you can find plenty of places online where people have already created the best RegEx patterns for capturing/cleaning them up, which means that you don’t have to worry about making them from scratch.

See, for example, the following page for a ton of examples of RegEx patterns for capturing different formats of dates/times:

http://www.regexlib.com/DisplayPatterns.aspx?cattabindex=4&categoryId=5
