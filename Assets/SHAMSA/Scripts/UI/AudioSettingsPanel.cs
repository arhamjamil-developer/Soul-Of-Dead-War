using UnityEngine;
using UnityEngine.UI;
using SoulCrystalDefense.Systems;
namespace SoulCrystalDefense.UI
{
    public sealed class AudioSettingsPanel : MonoBehaviour
    {
        public Slider master,music,effects;
        public Text masterValue,musicValue,effectsValue;
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
        private void Refresh(){masterValue.text=Mathf.RoundToInt(master.value*100)+"%";musicValue.text=Mathf.RoundToInt(music.value*100)+"%";effectsValue.text=Mathf.RoundToInt(effects.value*100)+"%";}
    }
}
