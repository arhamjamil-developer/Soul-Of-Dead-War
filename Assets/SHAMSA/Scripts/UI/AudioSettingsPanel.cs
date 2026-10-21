using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SoulCrystalDefense.Systems;
namespace SoulCrystalDefense.UI
{
    public sealed class AudioSettingsPanel : MonoBehaviour
    {
        public Slider master,music,effects;
        public Text masterValue,musicValue,effectsValue;
        public TextMeshProUGUI masterValueTMP,musicValueTMP,effectsValueTMP;
        private void OnEnable()
        {
            if(!master||!music||!effects)return;
            master.SetValueWithoutNotify(PlayerPrefs.GetFloat("MainMenu.MasterVolume",1));
            music.SetValueWithoutNotify(PlayerPrefs.GetFloat("Audio.Music",.7f));
            effects.SetValueWithoutNotify(PlayerPrefs.GetFloat("Audio.Effects",1));
            Refresh();
        }
        public void Master(float value){PlayerPrefs.SetFloat("MainMenu.MasterVolume",value);Apply();}
        public void Music(float value){PlayerPrefs.SetFloat("Audio.Music",value);Apply();}
        public void Effects(float value){PlayerPrefs.SetFloat("Audio.Effects",value);Apply();}
        private void Apply(){PlayerPrefs.Save();AudioListener.volume=master.value;SoundManager.Instance?.ApplySavedVolumes();Refresh();}
        private void Refresh()
        {
            string a=Mathf.RoundToInt(master.value*100)+"%",b=Mathf.RoundToInt(music.value*100)+"%",c=Mathf.RoundToInt(effects.value*100)+"%";
            if(masterValue)masterValue.text=a;if(musicValue)musicValue.text=b;if(effectsValue)effectsValue.text=c;
            if(masterValueTMP)masterValueTMP.text=a;if(musicValueTMP)musicValueTMP.text=b;if(effectsValueTMP)effectsValueTMP.text=c;
        }
    }
}
