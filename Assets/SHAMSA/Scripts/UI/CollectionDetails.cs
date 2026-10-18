using System;
using UnityEngine;
using UnityEngine.UI;
namespace SoulCrystalDefense.UI
{
    public sealed class CollectionDetails : MonoBehaviour
    {
        [Serializable]public class Entry{public string title,role,description;public Sprite portrait;}
        public Entry[] entries;
        public Image portrait;
        public Text title,role,description;
        public GameObject[] highlights;
        private void Start(){Select(0);}
        public void Select(int index)
        {
            if(index<0||index>=entries.Length)return;
            var e=entries[index];portrait.sprite=e.portrait;title.text=e.title;role.text=e.role;description.text=e.description;
            for(int i=0;i<highlights.Length;i++)highlights[i].SetActive(i==index);
        }
    }
}
