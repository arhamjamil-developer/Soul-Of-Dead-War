using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SoulCrystalDefense.EditorTools
{
    public static class MainMenuReview
    {
        public static void SetGameSize(int width,int height)
        {
            var assembly=typeof(Editor).Assembly;
            var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes=singleton.GetProperty("instance").GetValue(null);
            var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{0});
            var sizeType=assembly.GetType("UnityEditor.GameViewSize");
            var kind=assembly.GetType("UnityEditor.GameViewSizeType");
            var size=Activator.CreateInstance(sizeType,new[]{Enum.ToObject(kind,1),(object)width,height,"Menu QA "+width+"x"+height});
            group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
            int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);
            var viewType=assembly.GetType("UnityEditor.GameView");
            var window=EditorWindow.GetWindow(viewType);
            viewType.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(window,count-1);
            window.Repaint();
        }

        public static string Capture(string name)
        {
            string path=Path.GetFullPath("ArtReview/MainMenu/"+name+".png");
            ScreenCapture.CaptureScreenshot(path);
            return path;
        }

        // Simulates safe-area insets in the actual Game View, leaving the runtime
        // component enabled during normal play. Only used for Editor QA.
        public static void PreviewSafeArea(float top,float bottom,float sides=0)
        {
            var safe=GameObject.Find("MainMenuCanvas/SafeArea").GetComponent<RectTransform>();
            safe.GetComponent<SoulCrystalDefense.UI.SafeAreaFitter>().enabled=false;
            safe.anchorMin=new Vector2(sides/Screen.width,bottom/Screen.height);
            safe.anchorMax=new Vector2(1-sides/Screen.width,1-top/Screen.height);
            safe.offsetMin=safe.offsetMax=Vector2.zero;
            Canvas.ForceUpdateCanvases();
        }

        public static object CheckButtons()
        {
            Canvas.ForceUpdateCanvases();
            var eventSystem=EventSystem.current;
            var canvas=GameObject.Find("MainMenuCanvas").GetComponent<Canvas>();
            var raycaster=canvas.GetComponent<GraphicRaycaster>();
            var results=new List<object>();
            var safe=canvas.transform.Find("SafeArea").GetComponent<RectTransform>();
            var corners=new Vector3[4];safe.GetWorldCorners(corners);
            var safeRect=new Rect(corners[0],corners[2]-corners[0]);
            foreach(var button in canvas.GetComponentsInChildren<Button>())
            {
                var rect=button.GetComponent<RectTransform>();rect.GetWorldCorners(corners);
                var center=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
                var hits=new List<RaycastResult>();
                raycaster.Raycast(new PointerEventData(eventSystem){position=center},hits);
                bool hit=hits.Count>0 && hits[0].gameObject==button.gameObject;
                bool inside=safeRect.Contains(corners[0])&&safeRect.Contains(corners[2]);
                results.Add(new{name=button.name,hit,insideSafeArea=inside,width=corners[2].x-corners[0].x,height=corners[2].y-corners[0].y,listeners=button.onClick.GetPersistentEventCount()});
            }
            return new {width=Screen.width,height=Screen.height,buttons=results};
        }

        public static string Click(string name)
        {
            var go=GameObject.Find(name);
            if(!go) throw new Exception("Missing visible control: "+name);
            ExecuteEvents.Execute(go,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
            return name+" pointer click sent";
        }
    }
}
