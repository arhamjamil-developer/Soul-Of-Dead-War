using UnityEngine;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.Systems;
using SoulCrystalDefense.UI;

namespace SoulCrystalDefense.Presentation
{
    /// <summary>Fits the whole battle between the current HUD edges, without moving the grid.</summary>
    [DefaultExecutionOrder(100)]
    public sealed class BattlefieldCameraFraming : MonoBehaviour
    {
        public BoardGrid board;
        public CollapsibleBattleHUD hud;
        public CinemachineCameraPanController pan;
        public float edgePadding=18;
        public float worldPadding=.15f;
        public Bounds BattleBounds {get;private set;}
        public Rect AvailableViewport {get;private set;}
        Camera sceneCamera;
        Canvas canvas;
        bool initialized;
        readonly Vector3[] corners=new Vector3[4];

        void Start()
        {
            sceneCamera=Camera.main;canvas=hud.GetComponentInParent<Canvas>();
            var first=board.CalculateCellPosition(0,0);
            var last=board.CalculateCellPosition(BoardGrid.LANE_COUNT-1,BoardGrid.ROW_COUNT-1);
            var bounds=new Bounds((first+last)*.5f,Vector3.zero);
            bounds.Encapsulate(first-new Vector3(board.LaneSpacing*.5f,board.RowSpacing*.5f));
            bounds.Encapsulate(last+new Vector3(board.LaneSpacing*.5f,board.RowSpacing*.5f));
            bounds.Encapsulate(new Vector3(bounds.center.x,board.GetSpawnPosition(2).y+.55f));
            bounds.Encapsulate(new Vector3(bounds.center.x,board.CrystalY-.8f));
            bounds.Expand(worldPadding*2);BattleBounds=bounds;
        }

        void LateUpdate()
        {
            if(!sceneCamera||!canvas||!board||!hud||!pan)return;
            var safe=Screen.safeArea;
            var root=ScreenBounds(hud.layoutRoot);
            safe=Rect.MinMaxRect(Mathf.Max(safe.xMin,root.xMin),Mathf.Max(safe.yMin,root.yMin),Mathf.Min(safe.xMax,root.xMax),Mathf.Min(safe.yMax,root.yMax));
            float pad=edgePadding*canvas.scaleFactor;
            float bottom=Mathf.Max(safe.yMin,ScreenBounds(hud.bottomPanel).yMax)+pad;
            float top=Mathf.Min(safe.yMax,ScreenBounds(hud.topPanel).yMin)-pad;
            float left=safe.xMin+pad,right=safe.xMax-pad;
            if(top-bottom<100||right-left<100)return;
            var viewport=sceneCamera.pixelRect;
            var available=Rect.MinMaxRect((left-viewport.xMin)/viewport.width,(bottom-viewport.yMin)/viewport.height,(right-viewport.xMin)/viewport.width,(top-viewport.yMin)/viewport.height);
            AvailableViewport=available;
            float size=Mathf.Max(BattleBounds.size.y/(2*available.height),BattleBounds.size.x/(2*sceneCamera.aspect*available.width));
            var midpoint=available.center;
            var center=(Vector2)BattleBounds.center-new Vector2((midpoint.x-.5f)*2*size*sceneCamera.aspect,(midpoint.y-.5f)*2*size);
            pan.ConfigureBattleFrame(center,size,!initialized);initialized=true;
        }

        Rect ScreenBounds(RectTransform rect)
        {
            // Include decorative children which extend beyond the panel's own rect.
            var b=RectTransformUtility.CalculateRelativeRectTransformBounds(rect,rect);
            corners[0]=rect.TransformPoint(new Vector3(b.min.x,b.min.y));
            corners[1]=rect.TransformPoint(new Vector3(b.max.x,b.max.y));
            var a=RectTransformUtility.WorldToScreenPoint(canvas.worldCamera,corners[0]);
            var z=RectTransformUtility.WorldToScreenPoint(canvas.worldCamera,corners[1]);
            return Rect.MinMaxRect(Mathf.Min(a.x,z.x),Mathf.Min(a.y,z.y),Mathf.Max(a.x,z.x),Mathf.Max(a.y,z.y));
        }
    }
}
