using UnityEngine;

public enum SoundType
{
    GameStart = 1,
    GameEnd = 2,
    Attacked = 3,
    Grind = 4,
    Kill = 5,
    Collect = 6,  // New: roast collection sound
    RoastShoot = 8,
    FireballShoot = 10,
    WallHit = 15,
    Step = 20
}

public class SoundEngine : MonoBehaviour
{
    public static SoundEngine Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public AudioClip clip;
        public SoundType type;
    }

    [Header("Sound Settings")]
    public Sound[] sounds;

    private AudioSource audioSource;
    private SoundType? currentlyPlaying = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        PlaySound(SoundType.GameStart);
    }

    public void PlaySound(SoundType type)
    {
        Sound sound = System.Array.Find(sounds, s => s.type == type);
        if (sound == null)
        {
            Debug.LogWarning($"Sound {type} not found!");
            return;
        }

        // If nothing is playing, play the sound
        if (!currentlyPlaying.HasValue)
        {
            PlaySoundDirectly(sound);
            return;
        }

        // If something is playing, check priority
        if ((int)type < (int)currentlyPlaying)
        {
            // New sound has higher priority (lower number), stop current and play new
            PlaySoundDirectly(sound);
        }
        // If new sound has lower priority, it's ignored
    }

    private void PlaySoundDirectly(Sound sound)
    {
        audioSource.clip = sound.clip;
        audioSource.Play();
        currentlyPlaying = sound.type;

        // Reset currentlyPlaying when sound finishes
        Invoke("OnSoundComplete", sound.clip.length);
    }

    private void OnSoundComplete()
    {
        currentlyPlaying = null;
    }
}
