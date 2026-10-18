using System.Collections.Generic;
using UnityEngine;
using SoulCrystalDefense.Units;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.Presentation;
namespace SoulCrystalDefense.Systems
{
    public sealed class TemporaryGuardian : MonoBehaviour
    {
        public float seconds=20;
        private void Update(){if(GameManager.Instance!=null&&!GameManager.Instance.IsGameActive)return;seconds-=Time.deltaTime;if(seconds<=0)Destroy(gameObject);}
    }
}
