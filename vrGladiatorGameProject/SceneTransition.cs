using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    public GameObject OverlayCanvas;
    public Image FadeOverlay;

    public float FadeOutTime = 1f;
    public float FadeInTime = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (OverlayCanvas == null) Debug.Log("OverlayCanvas doesn't exist or is not linked to this script.");

        if (FadeOverlay == null) Debug.Log("FadeOverlay doesn't exist or is not linked to this script.");
        
        var color = FadeOverlay.color;
        color = new Color(color.r, color.g, color.b, 1);
        FadeOverlay.color = color;
        StartCoroutine(Fade(TransitionDirection.In));
    }

    public IEnumerator Fade(TransitionDirection transitionDirection)
    {
        if (OverlayCanvas == null) yield break;

        OverlayCanvas.SetActive(true);

        yield return new WaitForFixedUpdate();

        if (FadeOverlay == null) yield break;

        var alpha = (float) transitionDirection;

        if (transitionDirection == TransitionDirection.Out)
        {
            while (alpha < 1f)
            {
                alpha += Time.deltaTime / FadeOutTime;
                var color = FadeOverlay.color;
                color = new Color(color.r, color.g, color.b, alpha);
                FadeOverlay.color = color;
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            while (alpha > 0)
            {
                alpha -= Time.deltaTime / FadeInTime;
                var color = FadeOverlay.color;
                color = new Color(color.r, color.g, color.b, alpha);
                FadeOverlay.color = color;
                yield return new WaitForFixedUpdate();
            }

            OverlayCanvas.SetActive(false);
        }
    }
}

public enum TransitionDirection
{
    Out,
    In
}
