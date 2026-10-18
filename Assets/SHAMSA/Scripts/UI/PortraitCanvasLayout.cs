using UnityEngine;
namespace SoulCrystalDefense.UI
{
    [ExecuteAlways]
    public sealed class PortraitCanvasLayout : MonoBehaviour
    {
        public RectTransform safeArea;
        private void LateUpdate()
        {
            if(!safeArea)return;
            var r=(RectTransform)transform;
            r.sizeDelta=new Vector2(1080,1920);
            r.localScale=Vector3.one*Mathf.Min(safeArea.rect.width/1080,safeArea.rect.height/1920);
        }
    }
}
