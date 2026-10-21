using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Cinemachine;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.Presentation;
using SoulCrystalDefense.Systems;
using SoulCrystalDefense.UI;

namespace SoulCrystalDefense.EditorTools
{
    public static class WorldExtensionAuthoring
    {
        [MenuItem("SoulCrystalDefense/Presentation/Install North South Art and Grid Framing")]
        public static void Install()
        {
            if(EditorApplication.isPlaying)throw new System.Exception("Stop Play Mode before authoring");
            var hud=Object.FindFirstObjectByType<CollapsibleBattleHUD>();
            var pan=Object.FindFirstObjectByType<CinemachineCameraPanController>();
            var board=Object.FindFirstObjectByType<BoardGrid>();
            if(!hud||!pan||!board)throw new System.Exception("Open the authored gameplay scene");
            var backdrop=Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).First(s=>s.name=="GraveyardBackdrop");
            var bounds=backdrop.bounds;
            var old=GameObject.Find("NorthSouthWorldArt");if(old)Object.DestroyImmediate(old);
            var root=new GameObject("NorthSouthWorldArt");
            float width=bounds.size.x*3;
            Add("NorthRoyalGarden","North-Royal-Garden",1);Add("SouthVillageApproach","South-Village-Approach",-1);
            var domain=bounds;
            foreach(var sprite in GameObject.Find("NeighborHomes").GetComponentsInChildren<SpriteRenderer>())domain.Encapsulate(sprite.bounds);
            foreach(var sprite in root.GetComponentsInChildren<SpriteRenderer>())domain.Encapsulate(sprite.bounds);
            var confiner=pan.CinemachineCam.GetComponent<CinemachineConfiner2D>();
            var shape=(PolygonCollider2D)confiner.BoundingShape2D;
            shape.SetPath(0,new[]{Local(domain.min),Local(new Vector3(domain.min.x,domain.max.y)),Local(domain.max),Local(new Vector3(domain.max.x,domain.min.y))});
            confiner.InvalidateBoundingShapeCache();
            var fit=pan.GetComponent<BattlefieldCameraFraming>();if(!fit)fit=pan.gameObject.AddComponent<BattlefieldCameraFraming>();
            fit.board=board;fit.pan=pan;fit.hud=hud;
            // Dock arrow handles away from the middle battle lane.
            hud.topHandle.anchorMin=hud.topHandle.anchorMax=new Vector2(.91f,1);
            hud.bottomHandle.anchorMin=hud.bottomHandle.anchorMax=new Vector2(.91f,0);
            hud.exploreHint.text="SWIPE TO EXPLORE / CRYSTAL TO RETURN";
            foreach(var o in new Object[]{shape,fit,hud,hud.topHandle,hud.bottomHandle,hud.exploreHint})EditorUtility.SetDirty(o);
            EditorSceneManager.MarkSceneDirty(board.gameObject.scene);EditorSceneManager.SaveScene(board.gameObject.scene);AssetDatabase.SaveAssets();
            Debug.Log("WORLD_EXTENSIONS|Saved two editable sprites, expanded confiner and adaptive complete-grid camera");

            Vector2 Local(Vector3 p)=>shape.transform.InverseTransformPoint(p);
            void Add(string name,string file,int sign)
            {
                string path="Assets/UI/Production art/World Extensions/"+file+".png";
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="iPhone",overridden=true,maxTextureSize=4096,format=TextureImporterFormat.ASTC_4x4,compressionQuality=100});importer.SaveAndReimport();
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
                const float overlap=.9f;
                float originalHeight=width*sprite.bounds.size.y/sprite.bounds.size.x;
                float height=originalHeight+overlap;
                var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(root.transform,false);
                go.transform.position=new Vector3(bounds.center.x,(sign>0?bounds.max.y:bounds.min.y)+sign*(originalHeight-overlap)*.5f,bounds.center.z);
                go.transform.localScale=new Vector3(width/sprite.bounds.size.x,height/sprite.bounds.size.y,1);
                string materialPath="Assets/UI/Production art/World Extensions/"+file+"-Join.mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if(!material){material=new Material(Shader.Find("SoulCrystalDefense/WorldArtJoin"));AssetDatabase.CreateAsset(material,materialPath);}
                material.SetFloat("_JoinAtTop",sign<0?1:0);material.SetFloat("_BlendWidth",overlap/height);EditorUtility.SetDirty(material);
                var renderer=go.GetComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sharedMaterial=material;renderer.sortingLayerID=backdrop.sortingLayerID;renderer.sortingOrder=backdrop.sortingOrder+1;
            }
        }
    }
}
