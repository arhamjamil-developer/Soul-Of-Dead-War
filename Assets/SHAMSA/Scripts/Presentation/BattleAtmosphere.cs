using UnityEngine;
using UnityEngine.UI;
using SoulCrystalDefense.Systems;
using SoulCrystalDefense.Data;
namespace SoulCrystalDefense.Presentation
{
    public sealed class BattleAtmosphere : MonoBehaviour
    {
        public SpriteRenderer backdrop;
        public Sprite[] realms;
        public Text stageLabel,worldLabel;
        private Vector2 size;
        private WaveDirector director;
        private void Start(){size=backdrop.bounds.size;director=WaveDirector.Instance;if(director)director.OnWaveStarted+=OnWave;}
        private void OnWave(WaveData wave)
        {
            int realm=Mathf.Clamp((wave.waveNumber-1)/5,0,realms.Length-1);
            var sprite=realms[realm];if(!sprite)return;
            backdrop.sprite=sprite;backdrop.transform.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);
            string[] names={"THE FORGOTTEN GRAVEYARD","THE FROZEN TOMBS","THE EMBER WASTELAND","THE ROYAL SOUL CITADEL"};
            if(stageLabel)stageLabel.text=names[realm];if(worldLabel)worldLabel.text="REALM "+(realm+1)+" / SOUL CRYSTAL DEFENSE";
        }
        private void OnDestroy(){if(director)director.OnWaveStarted-=OnWave;}
    }
}
