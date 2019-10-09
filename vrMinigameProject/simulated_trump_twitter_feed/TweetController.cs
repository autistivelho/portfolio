using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TweetController : MonoBehaviour
{
    public static TweetController Instance;
    private bool firstTweet = true;
    public AudioClip FirstTweetClip;
    public GameObject Tweet;

    public List<Tweet> TweetScripts;

    public List<string> Tweets;
    public int Index;

    public bool SendingNukeQuestionTweet;

    //private float testCounter;
    //private float testIterative;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        GetTweets();
        TweetScripts = new List<Tweet>();
    }

    /*private void Update()
    {
        if (testIterative > 4) return;
        testCounter += Time.deltaTime;
        if (!(testCounter > 5f)) return;

        PublishTweet();
        testCounter = 0;
        testIterative++;
    }*/

    private void GetTweets()
    {
        var tweetFile = Resources.Load<TextAsset>("trump_tweets(utf8)");
        Tweets = tweetFile.text.Split(new [] { "\r\n" }, StringSplitOptions.None).ToList();
        TweetWriter.Instance.GetCurrentTweet();
    }

    public void PublishTweet(bool nukeTweet = false)
    {
        if (firstTweet)
        {
            firstTweet = false;
            TrumpSingle.Instance.AudioSrc.PlayOneShot(FirstTweetClip);
        }

        var newTweet = Instantiate(Tweet, transform);
        var newTweetScript = newTweet.GetComponent<Tweet>();
        newTweetScript.GetContent();
        StartCoroutine(newTweetScript.CountTime());
        if (!nukeTweet)
        {
            StartCoroutine(newTweetScript.AddReactions());
        }
        else
        {
            newTweetScript.AddOneLike();
        }
        newTweet.transform.SetAsFirstSibling();
        var tweetRect = newTweet.GetComponent<RectTransform>();
        tweetRect.anchoredPosition = Vector2.zero;
        if (TweetScripts.Count > 0)
        {
            foreach (var tweet in TweetScripts)
            {
                StartCoroutine(tweet.Move(tweetRect.rect));
            }
        }
        TweetScripts.Insert(0, newTweet.GetComponent<Tweet>());
        TweetWriter.Instance.GetCurrentTweet();
    }

    public void TriggerNukeQuestionTweet()
    {
        TweetWriter.Instance.EmptyInputField();
        TweetWriter.Instance.SetCurrentTweet("One like and I'll NUKE Mikkeli. THAT HORRIFIC BROWN HOLE HAS STOOD THERE FOR LONG ENOUGH!");
        SendingNukeQuestionTweet = true;
    }

    public void TriggerNukeAnswerTweet()
    {
        TweetWriter.Instance.EmptyInputField();
        TweetWriter.Instance.SetCurrentTweet("Say no more.");
    }
}
