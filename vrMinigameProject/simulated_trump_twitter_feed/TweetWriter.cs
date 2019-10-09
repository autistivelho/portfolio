using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TweetWriter : MonoBehaviour
{
    public static TweetWriter Instance;

    public TweetController TweetController;
    public InputField TweetInputField;
    public string CurrentTweet;
    public List<string> CurrentTweetWords;
    public int WordIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            WriteWord();
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            TweetController.TriggerNukeQuestionTweet();
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            TweetController.TriggerNukeAnswerTweet();
        }
    }

    public void GetCurrentTweet()
    {
        CurrentTweet = TweetController.Tweets[TweetController.Index];
        CurrentTweetWords = CurrentTweet.Split().ToList();
    }

    public void SetCurrentTweet(string tweet)
    {
        CurrentTweet = tweet;
        CurrentTweetWords = tweet.Split().ToList();
        WordIndex = 0;
    }

    public void EmptyInputField()
    {
        TweetInputField.text = "";
    }

    public void WriteWord()
    {
        if (WordIndex > CurrentTweetWords.Count - 1)
        {
            if (TweetController.SendingNukeQuestionTweet)
            {
                TweetController.PublishTweet(true);
            }
            else
            {
                TweetController.PublishTweet();
            }
            TweetInputField.text = "";
            WordIndex = 0;
            return;
        }

        TweetInputField.Select();
        TweetInputField.text += CurrentTweetWords[WordIndex] + " ";
        TweetInputField.MoveTextEnd(true);
        WordIndex ++;
    }
}
