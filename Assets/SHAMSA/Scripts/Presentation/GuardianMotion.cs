using UnityEngine;
using SoulCrystalDefense.Units;
namespace SoulCrystalDefense.Presentation
{
    public sealed class GuardianMotion : MonoBehaviour
    {
        public Transform visual;
        private Vector3 origin;
        private DefenderUnit unit;
        private void Awake(){origin=visual.localPosition;unit=GetComponent<DefenderUnit>();}
        private void Update()
        {
            if(!unit || unit.IsDead)return;
            float cycle=Time.time*2.8f+transform.position.x;
            visual.localPosition=origin+Vector3.up*(Mathf.Sin(cycle)*.025f);
            visual.localRotation=Quaternion.Euler(0,0,Mathf.Sin(cycle*.6f)*1.5f);
        }
    }
}
