using UnityEngine;
namespace SoulCrystalDefense.Presentation
{
    /// <summary>Independent sprite scale, world-space anchor; immune to cell animation/scale.</summary>
    [ExecuteAlways]
    public sealed class AnchoredBattlefieldArt : MonoBehaviour
    {
        public Transform anchor;
        public Vector3 offset;
        public Vector2 worldSize = new Vector2(1.29f,.94f);
        SpriteRenderer spriteRenderer;
        void LateUpdate()
        {
            if(!anchor) return;
            if(!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
            transform.position = anchor.position + offset;
            if(spriteRenderer && spriteRenderer.sprite)
            {
                var b = spriteRenderer.sprite.bounds.size;
                var p = transform.parent ? transform.parent.lossyScale : Vector3.one;
                transform.localScale = new Vector3(worldSize.x/b.x/p.x, worldSize.y/b.y/p.y,1);
            }
        }
    }
}
