using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleSpriteAnimation : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 10f;
    [SerializeField] private bool loop = true;

    private SpriteRenderer sr;
    private float timer;
    private int currentFrame;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (frames != null && frames.Length > 0 && sr != null && sr.sprite == null)
        {
            sr.sprite = frames[0];
        }
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0 || frameRate <= 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            timer -= 1f / frameRate;
            currentFrame++;
            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = frames.Length - 1;
                    enabled = false;
                }
            }

            if (sr != null && currentFrame < frames.Length)
            {
                sr.sprite = frames[currentFrame];
            }
        }
    }

    public void SetFrames(Sprite[] newFrames, float newFrameRate = 10f, bool loopAnim = true)
    {
        frames = newFrames;
        frameRate = newFrameRate;
        loop = loopAnim;
        currentFrame = 0;
        timer = 0;
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null && frames != null && frames.Length > 0)
        {
            sr.sprite = frames[0];
        }
    }
}
