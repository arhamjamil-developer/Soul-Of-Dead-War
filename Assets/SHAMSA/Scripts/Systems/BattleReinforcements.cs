using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.UI;
using SoulCrystalDefense.Units;
using SoulCrystalDefense.Presentation;
namespace SoulCrystalDefense.Systems
{
    public sealed class BattleReinforcements : MonoBehaviour
    {
        public DefenderData elephantData;
        public GameObject trainPrefab;
        public Button elephantButton,trainButton;
        public Text elephantState,trainState;
        private bool elephantUsed,trainUsed;
        private int armed;
        private readonly List<RaycastResult> hits=new List<RaycastResult>();
        private bool Active=>GameManager.Instance!=null&&GameManager.Instance.IsGameActive&&!GameManager.Instance.IsPaused&&Time.timeScale>0;
        public bool ElephantUsed=>elephantUsed;
        public bool TrainUsed=>trainUsed;
        public void SelectElephant(){Arm(1);}
        public void SelectTrain(){Arm(2);}
        private void Arm(int kind)
        {
            if(!Active||(kind==1?elephantUsed:trainUsed))return;
            PlacementController.Instance?.CancelPlacement();armed=armed==kind?0:kind;
            FindFirstObjectByType<BattlePresentation>()?.Show(kind==1?"ROYAL BULWARK":"SOUL EXPRESS",kind==1?"CHOOSE AN EMPTY TILE / 200 SOUL / 20 SECONDS":"CHOOSE A LANE / 150 SOUL / ONE SWEEP");
        }
        public bool SummonElephant(int lane,int row)
        {
            if(!Active||elephantUsed||!elephantData)return false;
            if(!PlacementController.Instance.DeployDefender(elephantData,lane,row))return false;
            var cell=BoardGrid.Instance.GetCell(lane,row);
            cell.Occupant.gameObject.AddComponent<TemporaryGuardian>().seconds=20;
            elephantUsed=true;armed=0;return true;
        }
        public bool RunTrain(int lane)
        {
            if(!Active||trainUsed||lane<0||lane>=5||!trainPrefab)return false;
            if(!SoulEnergyWallet.Instance.TrySpendEnergy(150))return false;
            var pos=new Vector3(BoardGrid.Instance.GetLaneX(lane),BoardGrid.Instance.CrystalY,0);
            var train=Instantiate(trainPrefab,pos,Quaternion.identity).GetComponent<SoulTrainSweep>();train.Initialize(lane);
            trainUsed=true;armed=0;SoundManager.Instance?.PlayPortalSpawn();return true;
        }
        private void Update()
        {
            if(elephantButton)elephantButton.interactable=Active&&!elephantUsed;
            if(trainButton)trainButton.interactable=Active&&!trainUsed;
            if(elephantState)elephantState.text=elephantUsed?"USED":armed==1?"CHOOSE TILE":"200 SOUL / ONCE";
            if(trainState)trainState.text=trainUsed?"USED":armed==2?"CHOOSE LANE":"150 SOUL / ONCE";
            if(!Active){armed=0;return;}
            if(armed==0||Pointer.current==null||!Pointer.current.press.wasPressedThisFrame)return;
            var pos=Pointer.current.position.ReadValue();hits.Clear();
            if(EventSystem.current){EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=pos},hits);if(hits.Count>0)return;}
            var world=Camera.main.ScreenToWorldPoint(new Vector3(pos.x,pos.y,10));var cell=BoardGrid.Instance.GetCellAtWorldPosition(world);
            if(cell==null){armed=0;return;}
            bool success=armed==1?SummonElephant(cell.Lane,cell.Row):RunTrain(cell.Lane);
            if(!success)FindFirstObjectByType<BattlePresentation>()?.Show(cell.IsOccupied&&armed==1?"TILE OCCUPIED":"NOT ENOUGH SOUL ENERGY","CHOOSE AN EMPTY TILE OR WAIT FOR SOUL REGENERATION");
        }
    }
}
