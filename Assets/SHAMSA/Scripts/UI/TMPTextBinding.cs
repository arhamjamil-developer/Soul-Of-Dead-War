using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace SoulCrystalDefense.UI {
// Preserves existing gameplay text drivers while the saved TMP component renders the label.
public sealed class TMPTextBinding : MonoBehaviour {
 public Text source; public TextMeshProUGUI target;
 string previous; Color previousColor;
 void OnEnable(){if(source){source.enabled=false;previous=null;}Sync();}
 void LateUpdate(){Sync();}
 void Sync(){if(!source||!target)return;source.enabled=false;if(previous!=source.text){previous=source.text;target.text=previous;}if(previousColor!=source.color){previousColor=source.color;target.color=previousColor;}}
}
}

