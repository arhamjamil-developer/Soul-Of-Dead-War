using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using SoulCrystalDefense.UI;

namespace SoulCrystalDefense.EditorTools
{
    public static class CrimsonMenuButtonAuthoring
    {
        const string Art="Assets/Art Production From Cards/MainMenu/Buttons/";
        static Sprite plate;
        static TMP_FontAsset font;
        static Material textMaterial;
        [MenuItem("SoulCrystalDefense/Presentation/Install Crimson Main Menu Buttons")]
        public static void Install()
        {
            if(EditorApplication.isPlaying)throw new System.Exception("Stop Play Mode before authoring");
            var menu=Object.FindFirstObjectByType<MainMenuController>();
            if(!menu||menu.gameObject.scene.name!="MainMenu")throw new System.Exception("Open MainMenu first");
            Import(Art+"Crimson-Button-Plate.png",new Vector4(82,82,82,82));
            foreach(var file in Directory.GetFiles(Art+"Icons","*.png"))Import(file.Replace('\\','/'),Vector4.zero);
            plate=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"Crimson-Button-Plate.png");
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Final/PaybAck SDF.asset");
            textMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"PAYBACK-White-Outline.mat");
            if(!textMaterial){textMaterial=new Material(font.material);AssetDatabase.CreateAsset(textMaterial,Art+"PAYBACK-White-Outline.mat");}
            textMaterial.SetColor("_FaceColor",Color.white);textMaterial.SetColor("_OutlineColor",new Color(.06f,.005f,.01f));textMaterial.SetFloat("_OutlineWidth",.15f);textMaterial.EnableKeyword("OUTLINE_ON");EditorUtility.SetDirty(textMaterial);
            var controller=Animations();
            var buttons=Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            string[] names={"Play","Shop","Settings","HowToPlay","Exit"};
            string[] captions={"PLAY","SHOP","SETTINGS","HOW TO PLAY","EXIT"};
            for(int i=0;i<names.Length;i++)
            {
                var button=buttons.First(b=>b.gameObject.scene==menu.gameObject.scene&&b.name=="Button_"+names[i]);
                var rect=button.GetComponent<RectTransform>();
                // Preserve the authored phone positions while docking controls from the top on iPad.
                if(rect.anchorMin==rect.anchorMax&&rect.anchorMin.y<.99f)
                {
                    var position=rect.anchoredPosition;position.y-=(1-rect.anchorMin.y)*1920;
                    rect.anchorMin=rect.anchorMax=new Vector2(rect.anchorMin.x,1);rect.anchoredPosition=position;
                }
                // Keep authored layout and persistent navigation events. Only rebuild our visual child.
                var old=rect.Find("CrimsonVisual");if(old)Object.DestroyImmediate(old.gameObject);
                var hit=button.GetComponent<Image>();hit.sprite=null;hit.type=Image.Type.Simple;hit.color=new Color(1,1,1,.001f);hit.raycastTarget=true;
                var rootOutline=button.GetComponent<Outline>();if(rootOutline)Object.DestroyImmediate(rootOutline);
                var visual=Rect("CrimsonVisual",rect);Stretch(visual);
                var surface=visual.gameObject.AddComponent<Image>();surface.sprite=plate;surface.type=Image.Type.Sliced;surface.pixelsPerUnitMultiplier=3;surface.raycastTarget=false;
                bool compact=rect.rect.width<500;
                float iconSize=compact?76:100;
                var icon=Rect("Icon",visual);icon.anchorMin=icon.anchorMax=new Vector2(0,.5f);icon.anchoredPosition=new Vector2(compact?62:96,0);icon.sizeDelta=new Vector2(iconSize,iconSize);
                var image=icon.gameObject.AddComponent<Image>();image.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"Icons/Icon-"+names[i]+".png");image.preserveAspect=true;image.raycastTarget=false;
                var label=Label("Label",visual,captions[i],compact?38:62);
                label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;label.rectTransform.offsetMin=new Vector2(compact?108:170,22);label.rectTransform.offsetMax=new Vector2(-28,-22);
                button.targetGraphic=surface;button.transition=Selectable.Transition.Animation;
                button.animationTriggers=new AnimationTriggers{normalTrigger="Normal",highlightedTrigger="Highlighted",pressedTrigger="Pressed",selectedTrigger="Selected",disabledTrigger="Disabled"};
                var animator=button.GetComponent<Animator>();if(!animator)animator=button.gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController=controller;animator.updateMode=AnimatorUpdateMode.UnscaledTime;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                EditorUtility.SetDirty(button);EditorUtility.SetDirty(animator);
            }
            var badge=menu.highestWaveText.transform.parent.GetComponent<RectTransform>();
            badge.anchorMin=badge.anchorMax=new Vector2(.5f,1);badge.pivot=new Vector2(.5f,.5f);badge.anchoredPosition=new Vector2(0,-68);badge.sizeDelta=new Vector2(1000,104);
            var bg=badge.GetComponent<Image>();bg.sprite=plate;bg.type=Image.Type.Sliced;bg.pixelsPerUnitMultiplier=4;bg.color=Color.white;bg.raycastTarget=false;
            var outline=badge.GetComponent<Outline>();if(outline)Object.DestroyImmediate(outline);
            foreach(var name in new[]{"CrimsonTrophy","HighestWaveTMP"}){var t=badge.Find(name);if(t)Object.DestroyImmediate(t.gameObject);}
            var trophy=Rect("CrimsonTrophy",badge);trophy.anchorMin=trophy.anchorMax=new Vector2(0,.5f);trophy.anchoredPosition=new Vector2(62,0);trophy.sizeDelta=new Vector2(70,70);
            var trophyImage=trophy.gameObject.AddComponent<Image>();trophyImage.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"Icons/Icon-Trophy.png");trophyImage.preserveAspect=true;trophyImage.raycastTarget=false;
            var highest=Label("HighestWaveTMP",badge,menu.highestWaveText.text,38);highest.rectTransform.anchorMin=Vector2.zero;highest.rectTransform.anchorMax=Vector2.one;highest.rectTransform.offsetMin=new Vector2(112,15);highest.rectTransform.offsetMax=new Vector2(-28,-15);highest.fontSizeMin=21;highest.textWrappingMode=TextWrappingModes.NoWrap;
            menu.highestWaveTMP=highest;menu.highestWaveText.enabled=false;menu.UpdateHighestWaveDisplay();EditorUtility.SetDirty(menu);EditorUtility.SetDirty(menu.highestWaveText);EditorUtility.SetDirty(highest);
            EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);EditorSceneManager.SaveScene(menu.gameObject.scene);AssetDatabase.SaveAssets();
            Debug.Log("CRIMSON_MENU|Saved five icon buttons, native Animator feedback and dynamic white TMP highest-wave badge");
        }
        static void Import(string path,Vector4 border)
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var i=(TextureImporter)AssetImporter.GetAtPath(path);
            i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.spriteBorder=border;i.spritePixelsPerUnit=100;i.alphaIsTransparency=true;i.mipmapEnabled=false;i.maxTextureSize=4096;i.textureCompression=TextureImporterCompression.Uncompressed;i.npotScale=TextureImporterNPOTScale.None;
            i.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="iPhone",overridden=true,maxTextureSize=4096,format=TextureImporterFormat.ASTC_4x4,compressionQuality=100});i.SaveAndReimport();
        }
        static RectTransform Rect(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return go.GetComponent<RectTransform>();}
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        static TextMeshProUGUI Label(string name,Transform parent,string caption,float size)
        {
            var t=Rect(name,parent).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSharedMaterial=textMaterial;t.text=caption;t.color=Color.white;t.fontSize=t.fontSizeMax=size;t.fontSizeMin=size*.65f;t.enableAutoSizing=true;t.textWrappingMode=TextWrappingModes.NoWrap;t.overflowMode=TextOverflowModes.Truncate;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.margin=new Vector4(2,0,2,0);return t;
        }
        static AnimatorController Animations()
        {
            string path=Art+"Animations/CrimsonButtonFeedback.controller";
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if(controller)return controller;
            controller=AnimatorController.CreateAnimatorControllerAtPath(path);var machine=controller.layers[0].stateMachine;
            string[] names={"Normal","Highlighted","Pressed","Selected","Disabled"};
            foreach(var name in names)
            {
                controller.AddParameter(name,AnimatorControllerParameterType.Trigger);
                var clip=new AnimationClip{name="Crimson-"+name,frameRate=60};
                float scale=name=="Pressed"?.97f:(name=="Highlighted"||name=="Selected"?1.02f:1);
                foreach(var axis in new[]{"x","y","z"})clip.SetCurve("CrimsonVisual",typeof(Transform),"m_LocalScale."+axis,AnimationCurve.Constant(0,.12f,axis=="z"?1:scale));
                var color=name=="Disabled"?new Color(.52f,.32f,.32f,.7f):name=="Pressed"?new Color(.78f,.72f,.72f,1):Color.white;
                string[] channels={"r","g","b","a"};float[] values={color.r,color.g,color.b,color.a};
                for(int c=0;c<4;c++)
                {
                    var curve=name=="Normal"&&c<3?new AnimationCurve(new Keyframe(0,.94f),new Keyframe(1.4f,1),new Keyframe(2.8f,.94f)):AnimationCurve.Constant(0,.12f,values[c]);
                    clip.SetCurve("CrimsonVisual",typeof(Image),"m_Color."+channels[c],curve);
                }
                var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=name=="Normal";AnimationUtility.SetAnimationClipSettings(clip,settings);
                AssetDatabase.CreateAsset(clip,Art+"Animations/"+name+".anim");var state=machine.AddState(name);state.motion=clip;
                if(name=="Normal")machine.defaultState=state;
                var transition=machine.AddAnyStateTransition(state);transition.AddCondition(AnimatorConditionMode.If,0,name);transition.hasExitTime=false;transition.duration=.085f;transition.canTransitionToSelf=false;
            }
            return controller;
        }
    }
}
