using UnityEngine;
namespace SoulCrystalDefense.Presentation
{
    /// <summary>Small authored sprite motion. Uses game time so pause freezes atmosphere.</summary>
    public sealed class GardenAmbientSprite : MonoBehaviour
    {
        public float phase, speed = 1.2f, scaleAmount=.035f, floatAmount=.025f;
        public float alphaMin=.7f, alphaMax=1;
        Vector3 origin, baseScale;
        SpriteRenderer sr;
        Color tint;
        void Awake() { origin=transform.localPosition; baseScale=transform.localScale; sr=GetComponent<SpriteRenderer>(); if(sr)tint=sr.color; }
        void LateUpdate()
        {
            float wave=Mathf.Sin(Time.time*speed+phase);
            transform.localScale=baseScale*(1+wave*scaleAmount);
            transform.localPosition=origin+Vector3.up*(wave*floatAmount);
            if(sr)sr.color=new Color(tint.r,tint.g,tint.b,tint.a*Mathf.Lerp(alphaMin,alphaMax,(wave+1)*.5f));
        }
    }
}
