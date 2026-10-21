using UnityEngine;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.Units;
namespace SoulCrystalDefense.Presentation
{
    // References an authored summon ring. Never creates art or changes gameplay coordinates.
    public sealed class GridLandingVisual : MonoBehaviour
    {
        public PlacementCell cell;
        public SpriteRenderer ring;
        DefenderUnit previous;
        float remaining;
        Vector3 size;
        void Awake(){size=ring.transform.localScale;ring.enabled=false;}
        void Update()
        {
            if(cell.Occupant!=previous){previous=cell.Occupant;if(previous)remaining=.65f;}
            remaining=Mathf.Max(0,remaining-Time.deltaTime);
            ring.enabled=remaining>0;
            if(ring.enabled){float t=remaining/.65f;ring.color=new Color(1,1,1,t);ring.transform.localScale=size*Mathf.Lerp(1.35f,.85f,t);}
        }
    }
}
