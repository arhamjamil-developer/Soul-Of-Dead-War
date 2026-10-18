using System.Collections.Generic;
using UnityEngine;
using SoulCrystalDefense.Units;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.Presentation;
namespace SoulCrystalDefense.Systems
{
    public sealed class SoulTrainSweep : MonoBehaviour
    {
        private int lane;
        private readonly HashSet<EnemyUnit> struck=new HashSet<EnemyUnit>();
        public void Initialize(int value){lane=value;}
        private void Update()
        {
            if(GameManager.Instance==null||!GameManager.Instance.IsGameActive){Destroy(gameObject);return;}
            transform.position+=Vector3.up*(8*Time.deltaTime);
            foreach(var enemy in FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None))
            {
                if(enemy.IsDead||enemy.HasBreached||enemy.LaneIndex!=lane||struck.Contains(enemy))continue;
                if(Mathf.Abs(enemy.transform.position.y-transform.position.y)>.7f)continue;
                struck.Add(enemy);enemy.TakeDamage(90);enemy.ApplyKnockback(.6f);GameplayEffects.Instance?.Ring(enemy.transform.position,new Color(.4f,.9f,1),.6f,.4f);
            }
            if(transform.position.y>BoardGrid.Instance.GetSpawnPosition(lane).y+1)Destroy(gameObject);
        }
    }
}
