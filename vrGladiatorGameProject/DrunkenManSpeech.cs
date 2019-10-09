using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DrunkenManSpeech : MonoBehaviour
{
    public static DrunkenManSpeech Instance;

    private AudioSource audioSource;
    public float StartDelay;
    public bool IsLooked;

    public AudioClip ReactionClip;

    #region singleton
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }
    #endregion

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1;

        StartCoroutine(GetAttention());
    }

    public IEnumerator GetAttention()
    {
        yield return new WaitForSecondsRealtime(StartDelay);

        var lineIndex = 0;
        do
        {
            audioSource.clip = AudioClips.Instance.DrunkenManHeyLines[lineIndex];
            audioSource.Play();
            yield return new WaitForSecondsRealtime(audioSource.clip.length);
            lineIndex = lineIndex < AudioClips.Instance.DrunkenManHeyLines.Count - 1 ? lineIndex + 1 : 0;
        } while (!IsLooked);

        StartCoroutine(Tutorial());
    }

    public IEnumerator Tutorial()
    {
        foreach (var line in AudioClips.Instance.DrunkenManTutorialLines)
        {
            audioSource.clip = line;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(line.length);
        }
    }

    public void Tip(FightObject fight)
    {
        StartCoroutine(PlayClip(AudioClips.Instance.DrunkenManTipLines.Find(x => x.Name == fight.name).Line));
    }

    public void ReactionToShopKeeper()
    {
        StartCoroutine(PlayClip(ReactionClip));
    }

    public IEnumerator PlayClip(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(audioSource.clip.length);
    }

    public void InterruptSpeech()
    {
        StopAllCoroutines();
    }
}
