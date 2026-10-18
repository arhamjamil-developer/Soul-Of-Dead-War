using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SoulCrystalDefense.Combat;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.Units;
using SoulCrystalDefense.Presentation;

namespace SoulCrystalDefense.Systems
{
    public sealed class CrystalArts : MonoBehaviour
    {
        public Button[] buttons;
        public Text[] cooldownLabels;
        public Image[] cooldownFills;
        public SpriteRenderer barrierVisual;
        public BattlePresentation presentation;
        [SerializeField] private float pulseCooldown = 18;
        [SerializeField] private float stormCooldown = 28;
        [SerializeField] private float barrierCooldown = 35;
        [SerializeField] private float stormDamage = 36;
        private readonly float[] readyAt = new float[3];
        private readonly List<PlacementCell> corrupted = new List<PlacementCell>();
        private float corruptAt = 12, clearAt;
        private WaveDirector director;
        private EnemyUnit boss;
        public Text bossLabel;
        public Image bossHealth;
        public GameObject bossPanel;
        public int CorruptedCount => corrupted.Count;
        public float CooldownRemaining(int index) => Mathf.Max(0, readyAt[index]-Time.time);

        private void Start()
        {
            director = WaveDirector.Instance;
            if (director != null) director.OnEnemySpawned += Spawned;
            if (bossPanel) bossPanel.SetActive(false);
            if (barrierVisual) barrierVisual.enabled = false;
        }
        private void Spawned(EnemyUnit enemy)
        {
            if (enemy.Data == null || enemy.Data.enemyType != Data.EnemyType.ArmoredElephant) return;
            boss = enemy;
            if (bossPanel) bossPanel.SetActive(true);
            if(bossLabel)bossLabel.text="CORRUPTED COLOSSUS";
            presentation?.Show("THE COLOSSUS AWAKENS", "CORRUPTED COLOSSUS / HOSTILE");
        }
        private bool CanCast(int index)
        {
            return GameManager.Instance != null && GameManager.Instance.IsGameActive && !GameManager.Instance.IsPaused
                && Time.timeScale > 0 && SoulCrystal.Instance != null && !SoulCrystal.Instance.IsDestroyed
                && Time.time >= readyAt[index];
        }
        public void SoulPulse()
        {
            if (!CanCast(0)) return;
            readyAt[0] = Time.time + pulseCooldown;
            foreach (var enemy in FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None))
                if (!enemy.IsDead && !enemy.HasBreached) enemy.ApplyKnockback(1.1f);
            Purify();
            var crystal = SoulCrystal.Instance;
            GameplayEffects.Instance?.Ring(crystal.transform.position, new Color(.4f,1,1), 5f, 1.1f);
            presentation?.Show("SOUL PULSE", "KNOCKBACK  /  PURIFY  /  RECLAIM");
        }
        public void CrystalStorm()
        {
            if (!CanCast(1)) return;
            readyAt[1] = Time.time + stormCooldown;
            foreach (var enemy in FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None))
            {
                if (enemy.IsDead || enemy.HasBreached) continue;
                GameplayEffects.Instance?.Lightning(enemy.transform.position + Vector3.up*2.8f, enemy.transform.position);
                enemy.TakeDamage(stormDamage);
            }
            presentation?.Show("CRYSTAL STORM", "STRIKE EVERY LANE");
        }
        public void DivineBarrier()
        {
            if (!CanCast(2)) return;
            readyAt[2] = Time.time + barrierCooldown;
            SoulCrystal.Instance.ActivateBarrier(6f);
            presentation?.Show("DIVINE BARRIER", "THE CRYSTAL IS PROTECTED FOR 6 SECONDS");
        }
        public void CorruptTiles()
        {
            Purify();
            var grid = BoardGrid.Instance;
            if (!grid) return;
            int wave = GameManager.Instance != null ? GameManager.Instance.CurrentWaveIndex : 0;
            for (int i=0;i<2;i++)
            {
                var cell = grid.GetCell((wave+i*2+1)%5, (wave+i+2)%6);
                if (cell == null) continue;
                cell.SetCorrupted(true);corrupted.Add(cell);
                GameplayEffects.Instance?.Ring(cell.transform.position, new Color(.8f,.3f,1), .65f, 1);
            }
            clearAt = Time.time+10;
            presentation?.Show("CORRUPTION SPREADS", "ATTACK SPEED -35%  /  SOUL PULSE TO PURIFY");
        }
        public void Purify()
        {
            foreach (var cell in corrupted) if (cell != null) cell.SetCorrupted(false);
            corrupted.Clear();
        }
        private void Update()
        {
            bool active = GameManager.Instance != null && GameManager.Instance.IsGameActive;
            if (active && director != null && director.IsWaveActive && director.WaveTimer >= corruptAt)
            { CorruptTiles();corruptAt = director.WaveTimer+22; }
            if (corrupted.Count>0 && (!active || Time.time>=clearAt)) Purify();
            // A fresh wave restarts its own pressure timer.
            if (director != null && !director.IsWaveActive) corruptAt=12;
            for (int i=0;i<3;i++)
            {
                float remaining=CooldownRemaining(i);
                if (i<buttons.Length && buttons[i]) buttons[i].interactable=CanCast(i);
                if (i<cooldownLabels.Length && cooldownLabels[i]) cooldownLabels[i].text=remaining>0?Mathf.CeilToInt(remaining)+"s":"READY";
                float duration=i==0?pulseCooldown:i==1?stormCooldown:barrierCooldown;
                if (i<cooldownFills.Length && cooldownFills[i]) cooldownFills[i].fillAmount=remaining/duration;
            }
            if (barrierVisual) barrierVisual.enabled=active && SoulCrystal.Instance != null && SoulCrystal.Instance.IsBarrierActive;
            if (bossPanel)
            {
                bool live=boss!=null && !boss.IsDead && !boss.HasBreached && active;
                bossPanel.SetActive(live);
                if (live && bossHealth) bossHealth.fillAmount=boss.CurrentHealth/boss.MaxHealth;
            }
        }
        private void OnDestroy()
        {
            if (director != null) director.OnEnemySpawned-=Spawned;
            Purify();
        }
    }
}
