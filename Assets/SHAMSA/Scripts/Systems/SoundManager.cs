using UnityEngine;

namespace SoulCrystalDefense.Systems
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip portalSpawnClip;
        [SerializeField] private AudioClip placementClip;
        [SerializeField] private AudioClip dwarfImpactClip;
        [SerializeField] private AudioClip chefFireImpactClip;
        [SerializeField] private AudioClip boneDeathClip;
        [SerializeField] private AudioClip soulReaperCastClip;
        [SerializeField] private AudioClip crystalDamageClip;
        [SerializeField] private AudioClip waveStartClip;
        [SerializeField] private AudioClip victoryClip;
        [SerializeField] private AudioClip defeatClip;
        [SerializeField] private AudioClip crystalForgeClip;

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

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f;
            }

            GenerateProceduralFallbacks();
            ApplySavedVolumes();
        }

        public void ApplySavedVolumes()
        {
            AudioListener.volume = PlayerPrefs.GetFloat("MainMenu.MasterVolume",1f);
            if(musicSource)musicSource.volume=PlayerPrefs.GetFloat("Audio.Music",.7f);
            if(sfxSource)sfxSource.volume=PlayerPrefs.GetFloat("Audio.Effects",1f);
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
        }

        private AudioClip CreateSynthTone(float duration, System.Func<float, float> waveform)
        {
            int sampleRate = 22050;
            int sampleCount = Mathf.Max(1, (int)(sampleRate * duration));
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                samples[i] = Mathf.Clamp(waveform(t), -1f, 1f);
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
                sfxSource.PlayOneShot(toPlay, volume);
            }
        }

        public void PlayPortalSpawn() => Play(portalSpawnClip, procPortalSpawn, 0.85f);
        public void PlayDefenderPlacement() => Play(placementClip, procPlacement, 0.9f);
        public void PlayDwarfImpact() => Play(dwarfImpactClip, procDwarfImpact, 0.95f);
        public void PlayChefFireImpact() => Play(chefFireImpactClip, procChefFire, 0.85f);
        public void PlayBoneDeath() => Play(boneDeathClip, procBoneDeath, 0.9f);
        public void PlaySoulReaperCast() => Play(soulReaperCastClip, procSoulReaperCast, 0.9f);
        public void PlayCrystalDamage() => Play(crystalDamageClip, procCrystalDamage, 1.0f);
        public void PlayWaveStart() => Play(waveStartClip, procWaveStart, 0.9f);
        public void PlayVictory() => Play(victoryClip, procVictory, 1.0f);
        public void PlayDefeat() => Play(defeatClip, procDefeat, 1.0f);
        public void PlayCrystalForge() => Play(crystalForgeClip, procCrystalForge, 0.9f);
    }
}
