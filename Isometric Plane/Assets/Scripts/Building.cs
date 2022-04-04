using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public bool Placed {get; private set; }
    public BoundsInt area;
    [SerializeField] private bool is_placed;


    private Vector2 mousePosition;
    #region Drag and Drop
    private void OnMouseDown(){
        if(Input.GetMouseButtonDown(0)){
            Placed = is_placed;
            if(Placed){
                Vector3Int positionInt = GridBuildingSystem.current.gridLayout.LocalToCell(transform.position);
                BoundsInt areaTemp = area;
                areaTemp.position = positionInt;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up);
                if(hit){
                    Placed = false;
                    is_placed = false;
                    GridBuildingSystem.current.RemoveArea(areaTemp, hit.collider.gameObject);
                }
            }    
        }
    }
    private void OnMouseDrag()
    {
        transform.position = mouseWorldPos() + mousePosition;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up);
        if(hit){
            Placed = false;
            is_placed = false;
            GridBuildingSystem.current.SearchArea(hit.collider.gameObject);
        }
    }

    private Vector2 mouseWorldPos(){
        Vector2 mpos = Input.mousePosition;
        return Camera.main.ScreenToWorldPoint(mpos);
    }
    #endregion

    #region Build Region

    public bool CanBePlaced(){
        Vector3Int positionInt = GridBuildingSystem.current.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;

        if(GridBuildingSystem.current.CanTakeArea(areaTemp)){
            return true;
        }
    
        return false;
    }

    public void Place(){
        Vector3Int positionInt = GridBuildingSystem.current.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;
        Placed= true;
        is_placed = true;
        GridBuildingSystem.current.TakeArea(areaTemp);
    }
        
    #endregion

}
