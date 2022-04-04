using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
public class GridBuildingSystem : MonoBehaviour
{

    public static GridBuildingSystem current;
    public GridLayout gridLayout;
    public Tilemap baseTilemap;
    public Tilemap dragTilemap;
    public Button btn;
    private static Dictionary<Tiletype, TileBase> tileBases = new Dictionary<Tiletype, TileBase>();
    private Building temp;
    private Vector3 prevPos;
    private BoundsInt prevArea;
    #region Unity Methods

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        string tilePath = @"Tile\";

        tileBases.Add(Tiletype.Empty, null);
        tileBases.Add(Tiletype.White, Resources.Load<TileBase>(tilePath + "White"));
        tileBases.Add(Tiletype.Green, Resources.Load<TileBase>(tilePath + "Green"));
        tileBases.Add(Tiletype.Red, Resources.Load<TileBase>(tilePath + "Red"));
    }
    private void Update()
    {
        if(!temp){
            return; 
        }


        if(Input.GetMouseButtonUp(0)){
            if(EventSystem.current.IsPointerOverGameObject(0)){
                return;
            }

            if(!temp.Placed){
                Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPos = gridLayout.LocalToCell(touchPos);

                if(prevPos != cellPos){
                    temp.transform.localPosition = gridLayout.CellToLocalInterpolated(cellPos + new Vector3(0.5f, 0.5f, 0f));
                    prevPos = cellPos;
                    FollowBuilding();
                }

                if(temp.CanBePlaced()){
                    temp.Place();
                    btn.interactable = true;
                }
            }
        }
        else if(Input.GetKeyDown(KeyCode.Escape)){
            ClearArea();
            Destroy(temp.gameObject);
            btn.interactable = true;
        }
    }

    #endregion

    #region Tilemap Management
        private static void FillTiles(TileBase[] arr, Tiletype type){
           for(int i = 0; i < arr.Length; i++){
               arr[i] = tileBases[type];
           }
       }

       private static TileBase[] GetTilesBlock (BoundsInt area, Tilemap tilemap){
            TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
            int counter = 0;
            foreach (var v in area.allPositionsWithin){
                Vector3Int pos = new Vector3Int (v.x, v.y, 0);
                array[counter] = tilemap.GetTile(pos);
                counter++;
            }
            return array;
       }

       private static void SetTilesBlock(BoundsInt area, Tiletype type, Tilemap tilemap){
            int size = area.size.x * area.size.y * area.size.z;
            TileBase[] tileArray = new TileBase[size];
            FillTiles(tileArray, type);
            tilemap.SetTilesBlock(area, tileArray);
       }
    #endregion

    #region Building Placement
       public void InitializeWithBuilding(GameObject building){
           temp = Instantiate(building, Vector3.zero, Quaternion.identity).GetComponent<Building>();
            btn.interactable = false;
           FollowBuilding();
       } 

       private void ClearArea(){
           TileBase[] toClear = new TileBase[prevArea.size.x * prevArea.size.y * prevArea.size.z];
           FillTiles(toClear, Tiletype.Empty);
           dragTilemap.SetTilesBlock(prevArea, toClear);
       }

       private void FollowBuilding(){
           ClearArea();

           temp.area.position = gridLayout.WorldToCell(temp.gameObject.transform.position);
           BoundsInt buildingArea = temp.area;

           TileBase[] baseArray = GetTilesBlock(buildingArea, baseTilemap);
           int size = baseArray.Length;
           TileBase[] tileArray = new TileBase[size];

           for (int i = 0; i < baseArray.Length; i++){
               if(baseArray[i] == tileBases[Tiletype.White]){
                   tileArray[i] = tileBases[Tiletype.Green];
               }
               else{
                   FillTiles(tileArray, Tiletype.Red);
                   break;
               }
           }

           dragTilemap.SetTilesBlock(buildingArea, tileArray);
           prevArea = buildingArea;

       }
       
       public bool CanTakeArea(BoundsInt area){
           TileBase[] baseArray = GetTilesBlock(area, baseTilemap);
           foreach(var b in baseArray){
               if(b != tileBases[Tiletype.White]){
                   Debug.Log("Building Aready Present");
                   ClearArea();
                    Destroy(temp.gameObject);
                    btn.interactable = true;
                   return false;
               }
           }

           return true;
       }

       public void TakeArea(BoundsInt area){
           SetTilesBlock(area, Tiletype.Empty, dragTilemap);
           SetTilesBlock(area, Tiletype.Green, baseTilemap);
       }

       public void RemoveArea(BoundsInt area, GameObject building){
           temp = building.GetComponent<Building>();
           if(!temp.Placed){
                btn.interactable = false;
                SetTilesBlock(area, Tiletype.White, baseTilemap);
                FollowBuilding();
           }
       }

       public void SearchArea(GameObject building){
           temp = building.GetComponent<Building>();
           if(!temp.Placed){
                btn.interactable = false;
                FollowBuilding();
           }
       }

    #endregion
}

public enum Tiletype{
    Empty,
    White,
    Green,
    Red
}