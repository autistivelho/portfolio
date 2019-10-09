using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Tweet : MonoBehaviour
{
    private RectTransform rt;

    public Text Content;
    public Text TimeStamp;
    public Text LikesText;
    public Text CommentsText;
    public Text RetweetsText;
    public Image LikeIcon;

    public float MovementSpeed;

    private void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    public void GetContent()
    {
        Content.text = TweetWriter.Instance.CurrentTweet;
        if (TweetController.Instance.SendingNukeQuestionTweet)
        {
            TweetController.Instance.SendingNukeQuestionTweet = false;
            return;
        }
        if (TweetController.Instance.Index < TweetController.Instance.Tweets.Count-1)
        {
            TweetController.Instance.Index++;
        }
        else
        {
            TweetController.Instance.Index = 0;
        }
    }

    public IEnumerator Move(Rect newTweetRect)
    {
        yield return new WaitForSecondsRealtime(0);
        var movementAmount = TweetController.Instance.TweetScripts[0].rt.rect.height;
        var distanceTravelled = 0f;
        while (distanceTravelled < movementAmount)
        {
            var newPosition = Time.deltaTime * MovementSpeed;
            distanceTravelled += newPosition;
            rt.anchoredPosition += Vector2.down * newPosition;
            yield return new WaitForSecondsRealtime(Time.deltaTime);
        }

        if (!(rt.anchoredPosition.y < -500)) yield break;
        StopAllCoroutines();
        Destroy(gameObject);
    }

    public IEnumerator CountTime()
    {
        var publishTime = Time.time;
        var minutesPassed = 0;
        TimeStamp.text = "A moment ago";
        while (minutesPassed < 60f)
        {
            yield return new WaitForSecondsRealtime(60f);
            TimeStamp.text = (minutesPassed+1) + " m";
            minutesPassed++;
        }

        var hoursPassed = 1;
        TimeStamp.text = hoursPassed + " h";
        while (hoursPassed < 24f)
        {
            yield return new WaitForSecondsRealtime(3600f);
            TimeStamp.text = (hoursPassed+1) + " h";
            hoursPassed++;
        }

        TimeStamp.text = "1 d";
    }

    public IEnumerator AddReactions()
    {
        var totalLikes = UnityEngine.Random.Range(5000, 150000);
        var totalComments = (int)UnityEngine.Random.Range(totalLikes/2.5f, totalLikes/1.5f);
        var totalRetweets = (int)UnityEngine.Random.Range(totalComments/2.5f, totalComments/1.5f);
        var likes = 0;
        var comments = 0;
        var retweets = 0;
        var remainingLikes = totalLikes;
        var remainingComments = totalComments;
        var remainingRetweets = totalRetweets;
        
        yield return new WaitForSecondsRealtime(1);
        while (likes < totalLikes && comments < totalComments && retweets < totalRetweets)
        {
            var likesChanged = remainingLikes / UnityEngine.Random.Range(50, 100);
            likes += likesChanged;
            remainingLikes -= likesChanged;
            var commentsChanged = remainingComments / UnityEngine.Random.Range(50, 100);
            comments += commentsChanged;
            remainingComments -= commentsChanged;
            var retweetsChanged = remainingRetweets / UnityEngine.Random.Range(50, 100);
            retweets += retweetsChanged;
            remainingRetweets -= retweetsChanged;

            if (likes < 1000)
            {
                LikesText.text = likes.ToString();
            }
            else if (likes >= 1000 && likes < 10000)
            {
                LikesText.text = ((float)likes/1000).ToString("#.# t");
            }
            else
            {
                LikesText.text = (likes/1000).ToString("# t");
            }

            if (comments < 1000)
            {
                CommentsText.text = comments.ToString();
            }
            else if (comments >= 1000 && comments < 10000)
            {
                CommentsText.text = ((float)comments / 1000).ToString("#.# t");
            }
            else
            {
                CommentsText.text = (comments / 1000).ToString("# t");
            }

            if (retweets < 1000)
            {
                RetweetsText.text = retweets.ToString();
            }
            else if (retweets >= 1000 && retweets < 10000)
            {
                RetweetsText.text = ((float)retweets / 1000).ToString("#.# t");
            }
            else
            {
                RetweetsText.text = (retweets / 1000).ToString("# t");
            }

            yield return new WaitForSecondsRealtime(1);
        }
    }

    public void AddOneLike()
    {
        LikesText.text = "1";
        LikeIcon.color = new Color(223f, 41f, 94, 255f);
    }
}
