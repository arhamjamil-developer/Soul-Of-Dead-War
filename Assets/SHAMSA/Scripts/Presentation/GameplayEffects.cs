using UnityEngine;

namespace SoulCrystalDefense.Presentation
{
    // Fixed-size pool: hit feedback never grows with a long battle.
    public sealed class GameplayEffects : MonoBehaviour
    {
        public static GameplayEffects Instance { get; private set; }
        public Material lineMaterial;
        public Font damageFont;
        public Sprite[] impactFrames,summonFrames,arcaneFrames,iceFrames;
        private sealed class Effect
        {
            public LineRenderer line;
            public TextMesh label;
            public Vector3 origin,end;
            public float start,duration,radius;
            public Color color;
            public bool lightning;
            public SpriteRenderer sprite;
            public Sprite[] frames;
        }
        private readonly Effect[] pool = new Effect[40];
        private int cursor;
        private void Awake()
        {
            Instance=this;
            for(int i=0;i<pool.Length;i++)
            {
                var go=new GameObject("PooledBattleFeedback_"+i);go.transform.SetParent(transform,false);
                var line=go.AddComponent<LineRenderer>();line.sharedMaterial=lineMaterial;line.useWorldSpace=true;
                line.sortingOrder=35;line.numCapVertices=2;line.enabled=false;
                var sprite=go.AddComponent<SpriteRenderer>();sprite.sortingOrder=34;sprite.enabled=false;
                var labelGO=new GameObject("Damage");labelGO.transform.SetParent(go.transform,false);
                var label=labelGO.AddComponent<TextMesh>();label.font=damageFont;label.fontSize=48;label.characterSize=.065f;
                label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.gameObject.SetActive(false);
                var renderer=label.GetComponent<MeshRenderer>();renderer.sharedMaterial=damageFont.material;renderer.sortingOrder=36;
                pool[i]=new Effect{line=line,label=label,sprite=sprite};
            }
        }
        private Effect Next(Vector3 origin,Color color,float duration)
        {
            var e=pool[cursor++%pool.Length];e.origin=origin;e.color=color;e.start=Time.time;e.duration=duration;
            e.lightning=false;e.radius=.5f;e.line.enabled=true;e.label.gameObject.SetActive(false);e.frames=null;e.sprite.enabled=false;return e;
        }
        public void Impact(Vector3 position,float damage)
        {
            var e=Next(position,new Color(1,.79f,.42f),.55f);e.radius=.38f;
            e.frames=impactFrames;
            e.label.text=Mathf.CeilToInt(damage).ToString();e.label.gameObject.SetActive(true);
        }
        public void Ring(Vector3 position,Color color,float radius,float duration)
        {var e=Next(position,color,duration);e.radius=radius;e.frames=summonFrames;}
        public void Lightning(Vector3 from,Vector3 to)
        {var e=Next(from,new Color(.82f,.65f,1),.45f);e.lightning=true;e.end=to;e.frames=arcaneFrames;}
        public void Frost(Vector3 position){var e=Next(position,new Color(.4f,.9f,1),.6f);e.frames=iceFrames;}
        private void Update()
        {
            foreach(var e in pool)
            {
                if(!e.line.enabled)continue;
                float f=(Time.time-e.start)/e.duration;
                if(f>=1){e.line.enabled=false;e.label.gameObject.SetActive(false);e.sprite.enabled=false;continue;}
                if(e.frames!=null&&e.frames.Length>0)
                {
                    e.sprite.enabled=true;e.sprite.sprite=e.frames[Mathf.Min(e.frames.Length-1,Mathf.FloorToInt(f*e.frames.Length))];
                    e.sprite.transform.position=e.lightning?e.end:e.origin;e.sprite.transform.localScale=Vector3.one;
                    e.sprite.color=new Color(1,1,1,1-f*.5f);
                }
                Color c=e.color;c.a=1-f;e.line.startColor=e.line.endColor=c;e.line.startWidth=e.line.endWidth=.035f*(1-f)+.005f;
                if(e.lightning)
                {
                    e.line.positionCount=10;
                    for(int i=0;i<10;i++){var p=Vector3.Lerp(e.origin,e.end,i/9f);if(i>0&&i<9)p.x+=Mathf.Sin(i*17+Mathf.Floor(Time.time*20))*.12f;e.line.SetPosition(i,p);}
                }
                else
                {
                    e.line.positionCount=33;
                    float radius=Mathf.Lerp(.06f,e.radius,f);
                    for(int i=0;i<33;i++){float a=i*Mathf.PI/16;e.line.SetPosition(i,e.origin+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.5f,0));}
                }
                if(e.label.gameObject.activeSelf){e.label.transform.position=e.origin+Vector3.up*(.45f+f*.55f);e.label.color=c;}
            }
        }
        private void OnDestroy(){if(Instance==this)Instance=null;}
    }
}
