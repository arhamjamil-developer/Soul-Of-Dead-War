using UnityEngine;
using UnityEngine.UI;
using SoulCrystalDefense.Systems;
using SoulCrystalDefense.Data;

namespace SoulCrystalDefense.Presentation
{
    // Presentation only: listens to the existing wave lifecycle; never controls combat.
    public class BattlePresentation : MonoBehaviour
    {
        public CanvasGroup announcement;
        public Text announcementTitle;
        public Text announcementDetail;
        public Text encounterStatus;
        public Image waveProgress;
        private WaveDirector director;
        private float visibleUntil;
        private int lastResolved = -1, lastTotal = -1;

        private void Start()
        {
            director = WaveDirector.Instance;
            if (director != null)
            {
                director.OnWaveStarted += Started;
                director.OnWaveCompleted += Completed;
            }
            if (announcement != null) { announcement.alpha = 0; announcement.blocksRaycasts = false; }
        }

        private void Started(WaveData wave)
        {
            Show("WAVE " + wave.waveNumber.ToString("00"), string.IsNullOrEmpty(wave.waveTitle) ? "DEFEND THE SOUL CRYSTAL" : wave.waveTitle.ToUpperInvariant());
        }
        private void Completed(WaveData wave) { Show("WAVE CLEARED", "THE CRYSTAL ENDURES"); }
        public void Show(string title, string detail)
        {
            if (announcementTitle != null) announcementTitle.text = title;
            if (announcementDetail != null) announcementDetail.text = detail;
            visibleUntil = Time.time + 2.2f;
        }
        private void Update()
        {
            if (announcement != null)
            {
                announcement.alpha = Mathf.MoveTowards(announcement.alpha, Time.time < visibleUntil ? 1 : 0, Time.deltaTime * 3);
            }
            if (director == null) return;
            int resolved = director.CompletedSpawnsCount, total = director.TotalSpawnsInWave;
            if (resolved == lastResolved && total == lastTotal) return;
            lastResolved = resolved; lastTotal = total;
            if (waveProgress != null) waveProgress.fillAmount = total > 0 ? (float)resolved / total : 0;
            if (encounterStatus != null) encounterStatus.text = total > 0 ? resolved + " / " + total + " VANQUISHED" : "PREPARE YOUR DEFENSE";
        }
        private void OnDestroy()
        {
            if (director == null) return;
            director.OnWaveStarted -= Started;
            director.OnWaveCompleted -= Completed;
        }
    }
}
