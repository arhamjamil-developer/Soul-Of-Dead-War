using UnityEngine;
namespace SoulCrystalDefense.UI
{
    [ExecuteAlways]
    public sealed class GameplayComposition : MonoBehaviour
    {
        public Camera gameplayCamera;
        public RectTransform safeArea;
        public RectTransform battlefield;
        public float orthographicSize=6.05f;
        private RectTransform root;
        private readonly Vector3[] corners=new Vector3[4];
        private void LateUpdate(){Apply();}
        public void Apply()
        {
            if(!safeArea || !battlefield || !gameplayCamera)return;
            gameplayCamera.orthographicSize=orthographicSize;
            if(!root)root=GetComponent<RectTransform>();
            float scale=Mathf.Min(safeArea.rect.width/1080f,safeArea.rect.height/1920f);
            root.sizeDelta=new Vector2(1080,1920);root.localScale=Vector3.one*scale;
            battlefield.GetWorldCorners(corners);
            var lo=RectTransformUtility.WorldToScreenPoint(null,corners[0]);
            var hi=RectTransformUtility.WorldToScreenPoint(null,corners[2]);
            if(Screen.width>0 && Screen.height>0)gameplayCamera.rect=new Rect(lo.x/Screen.width,lo.y/Screen.height,(hi.x-lo.x)/Screen.width,(hi.y-lo.y)/Screen.height);
        }
    }
}
