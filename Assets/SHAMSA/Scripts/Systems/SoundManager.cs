using UnityEngine;

namespace SoulCrystalDefense.Systems
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;
        [Header("Soft production mix")]
        [Range(0f,1f)] [SerializeField] private float effectsMixGain=.65f;
        [Range(0f,1f)] [SerializeField] private float musicMixGain=.85f;
        [Range(.02f,.2f)] [SerializeField] private float minimumRepeatInterval=.06f;
        private readonly System.Collections.Generic.Dictionary<AudioClip,float> lastPlayed=new System.Collections.Generic.Dictionary<AudioClip,float>();
        private float recentDensity,lastMixTime;

        [Header("Audio Clips - Core Gameplay")]
        [SerializeField] private AudioClip portalSpawnClip;
        [SerializeField] private AudioClip placementClip;
        [SerializeField] private AudioClip dwarfImpactClip;
        [SerializeField] private AudioClip chefFireImpactClip;
        [SerializeField] private AudioClip archerAttackClip;
        [SerializeField] private AudioClip magicAttackClip;
        [SerializeField] private AudioClip boneDeathClip;
        [SerializeField] private AudioClip soulReaperCastClip;
        [SerializeField] private AudioClip crystalDamageClip;
        [SerializeField] private AudioClip waveStartClip;
        [SerializeField] private AudioClip victoryClip;
        [SerializeField] private AudioClip defeatClip;
        [SerializeField] private AudioClip crystalForgeClip;
        [SerializeField] private AudioClip soulEnergyGainClip;

        [Header("Audio Clips - UI")]
        [SerializeField] private AudioClip uiClickClip;
        [SerializeField] private AudioClip uiPanelOpenClip;
        [SerializeField] private AudioClip uiPanelCloseClip;

        [Header("Audio Clips - Music")]
        [SerializeField] private AudioClip bgmClip;

        private AudioClip procPortalSpawn;
        private AudioClip procPlacement;
        private AudioClip procDwarfImpact;
        private AudioClip procChefFire;
        private AudioClip procBoneDeath;
        private AudioClip procSoulReaperCast;
        private AudioClip procCrystalDamage;
        private AudioClip procWaveStart;
        private AudioClip procVictory;
        private AudioClip procDefeat;
        private AudioClip procCrystalForge;
        private AudioClip procSoulEnergyGain;
        private AudioClip procUIClick;
        private AudioClip procUIPanelOpen;
        private AudioClip procUIPanelClose;
        private AudioClip procArcherAttack;
        private AudioClip procMagicAttack;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                // If this GameObject has other components (like GameManagers), only destroy this component!
                if (GetComponents<Component>().Length <= 2)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(this);
                }
                return;
            }

            SetupAudioSources();
            GenerateProceduralFallbacks();
            ApplySavedVolumes();
            PlayMusic();
        }

        private void SetupAudioSources()
        {
            if (musicSource == null || musicSource == sfxSource)
            {
                var musicChild = transform.Find("MusicSource");
                if (musicChild == null)
                {
                    var go = new GameObject("MusicSource");
                    go.transform.SetParent(transform, false);
                    musicChild = go.transform;
                }
                musicSource = musicChild.GetComponent<AudioSource>();
                if(!musicSource)musicSource=musicChild.gameObject.AddComponent<AudioSource>();
            }
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.loop = true;

            if (sfxSource == null || sfxSource == musicSource)
            {
                var sfxChild = transform.Find("SFXSource");
                if (sfxChild == null)
                {
                    var go = new GameObject("SFXSource");
                    go.transform.SetParent(transform, false);
                    sfxChild = go.transform;
                }
                sfxSource = sfxChild.GetComponent<AudioSource>();
                if(!sfxSource)sfxSource=sfxChild.gameObject.AddComponent<AudioSource>();
            }
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.loop = false;
        }

        public void PlayMusic()
        {
            if (musicSource != null && bgmClip != null && !musicSource.isPlaying)
            {
                musicSource.clip = bgmClip;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        public void ApplySavedVolumes()
        {
            AudioListener.volume = PlayerPrefs.GetFloat("MainMenu.MasterVolume", 1f);
            if (musicSource != null)
            {
                musicSource.volume = Mathf.Clamp01(PlayerPrefs.GetFloat("Audio.Music", 0.7f))*musicMixGain;
            }
            if (sfxSource != null)
            {
                sfxSource.volume = Mathf.Clamp01(PlayerPrefs.GetFloat("Audio.Effects", 1f))*effectsMixGain;
            }
        }

        public void SetMasterVolume(float value)
        {
            AudioListener.volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("MainMenu.MasterVolume", AudioListener.volume);
            PlayerPrefs.Save();
            ApplySavedVolumes();
        }

        public void SetMusicVolume(float value)
        {
            float v = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("Audio.Music", v);
            PlayerPrefs.Save();
            if (musicSource != null) musicSource.volume = v*musicMixGain;
        }

        public void SetEffectsVolume(float value)
        {
            float v = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("Audio.Effects", v);
            PlayerPrefs.Save();
            if (sfxSource != null) sfxSource.volume = v*effectsMixGain;
        }

        private void GenerateProceduralFallbacks()
        {
            procPortalSpawn = CreateSynthTone(0.35f, (t) => Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(220f, 90f, t) * t) * (1f - t) * 0.4f);
            procPlacement = CreateSynthTone(0.2f, (t) => (Mathf.Sin(2f * Mathf.PI * 523.25f * t) + Mathf.Sin(2f * Mathf.PI * 659.25f * t) * 0.5f) * (1f - t) * 0.35f);
            procDwarfImpact = CreateSynthTone(0.15f, (t) => Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(140f, 60f, t) * t) * (1f - t * t) * 0.5f);
            procChefFire = CreateSynthTone(0.25f, (t) => (Random.Range(-0.3f, 0.3f) + Mathf.Sin(2f * Mathf.PI * 340f * t) * 0.3f) * (1f - t) * 0.4f);
            procBoneDeath = CreateSynthTone(0.22f, (t) => (Random.Range(-0.2f, 0.2f) * (1f - t) + Mathf.Sin(2f * Mathf.PI * 180f * t) * (1f - t * 2f > 0 ? 1f - t * 2f : 0f)) * 0.35f);
            procSoulReaperCast = CreateSynthTone(0.4f, (t) => Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(660f, 1100f, t) * t) * Mathf.Sin(Mathf.PI * t) * 0.35f);
            procCrystalDamage = CreateSynthTone(0.3f, (t) => (Mathf.Sin(2f * Mathf.PI * 880f * t) + Mathf.Sin(2f * Mathf.PI * 1320f * t) * 0.6f) * (1f - t) * 0.45f);
            procWaveStart = CreateSynthTone(0.5f, (t) => Mathf.Sin(2f * Mathf.PI * (t < 0.25f ? 440f : 554.37f) * t) * (1f - t * 0.8f) * 0.4f);
            procVictory = CreateSynthTone(0.7f, (t) => {
                float freq = t < 0.2f ? 523.25f : (t < 0.4f ? 659.25f : 783.99f);
                return Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - t * 0.9f) * 0.4f;
            });
            procDefeat = CreateSynthTone(0.8f, (t) => {
                float freq = t < 0.35f ? 392f : 311.13f;
                return Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - t * 0.85f) * 0.45f;
            });
            procCrystalForge = CreateSynthTone(0.6f, (t) => Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(440f, 880f, t) * t) * Mathf.Sin(Mathf.PI * t) * 0.4f);
            procSoulEnergyGain = CreateSynthTone(0.2f, (t) => Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(987.77f, 1318.5f, t) * t) * (1f - t) * 0.35f);
            procUIClick = CreateSynthTone(0.06f, (t) => Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(800f, 280f, t) * t) * (1f - t) * 0.35f);
            procUIPanelOpen = CreateSynthTone(0.18f, (t) => Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(280f, 750f, t) * t) * (1f - t) * 0.35f);
            procUIPanelClose = CreateSynthTone(0.14f, (t) => Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(650f, 220f, t) * t) * (1f - t) * 0.35f);
            procArcherAttack = CreateSynthTone(0.25f, (t) => Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(480f, 380f, t) * t) * (1f - t) * 0.4f);
            procMagicAttack = CreateSynthTone(0.35f, (t) => Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(1046f, 2093f, t) * t) * (1f - t) * 0.35f);
        }

        private AudioClip CreateSynthTone(float duration, System.Func<float, float> waveform)
        {
            int sampleRate = 22050;
            int sampleCount = Mathf.Max(1, (int)(sampleRate * duration));
            float[] samples = new float[sampleCount];
            float smooth=0;
            float coefficient=1f-Mathf.Exp(-2f*Mathf.PI*2800f/sampleRate);
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float seconds=(float)i/sampleRate;
                float attack=Mathf.SmoothStep(0,1,Mathf.Clamp01(seconds/.008f));
                float release=Mathf.SmoothStep(0,1,Mathf.Clamp01((duration-seconds)/.025f));
                smooth+=coefficient*(Mathf.Clamp(waveform(t),-1f,1f)-smooth);
                samples[i]=smooth*attack*release*.8f;
            }
            AudioClip clip = AudioClip.Create("ProceduralTone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void Play(AudioClip clip, AudioClip fallback, float volume = 1f)
        {
            AudioClip toPlay = clip != null ? clip : fallback;
            if (toPlay != null && sfxSource != null)
            {
                float now=Time.unscaledTime;
                if(lastPlayed.TryGetValue(toPlay,out var last)&&now-last<minimumRepeatInterval)return;
                lastPlayed[toPlay]=now;
                recentDensity*=Mathf.Exp(-(now-lastMixTime)*5f);lastMixTime=now;
                float densityGain=1f/Mathf.Sqrt(1f+recentDensity*.22f);
                recentDensity=Mathf.Min(12f,recentDensity+1f);
                sfxSource.PlayOneShot(toPlay,Mathf.Clamp01(volume)*densityGain);
            }
        }

        public void PlayPortalSpawn() => Play(portalSpawnClip, procPortalSpawn, 0.85f);
        public void PlayDefenderPlacement() => Play(placementClip, procPlacement, 0.9f);
        public void PlayDwarfImpact() => Play(dwarfImpactClip, procDwarfImpact, 0.95f);
        public void PlayChefFireImpact() => Play(chefFireImpactClip, procChefFire, 0.85f);
        public void PlayArcherAttack() => Play(archerAttackClip, procArcherAttack, 0.9f);
        public void PlayMagicAttack() => Play(magicAttackClip, procMagicAttack, 0.9f);
        public void PlayBoneDeath() => Play(boneDeathClip, procBoneDeath, 0.9f);
        public void PlaySoulReaperCast() => Play(soulReaperCastClip, procSoulReaperCast, 0.9f);
        public void PlayCrystalDamage() => Play(crystalDamageClip, procCrystalDamage, 1.0f);
        public void PlayWaveStart() => Play(waveStartClip, procWaveStart, 0.9f);
        public void PlayVictory() => Play(victoryClip, procVictory, 1.0f);
        public void PlayDefeat() => Play(defeatClip, procDefeat, 1.0f);
        public void PlayCrystalForge() => Play(crystalForgeClip, procCrystalForge, 0.9f);
        public void PlaySoulEnergyGain() => Play(soulEnergyGainClip, procSoulEnergyGain, 0.85f);
        public void PlayUIClick() => Play(uiClickClip, procUIClick, 0.8f);
        public void PlayUIPanelOpen() => Play(uiPanelOpenClip, procUIPanelOpen, 0.85f);
        public void PlayUIPanelClose() => Play(uiPanelCloseClip, procUIPanelClose, 0.8f);

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
