﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    enum OptionalToggle { Ignore, No, Yes }

    public HexGrid hexGrid;
    public Client client;

    string data = "";
    string previousData = "";
    int activeTerrainBiomeIndex;
    int activeElevation;
    int activeFeature;
    bool applyElevation;
    bool isDrag;

    HexDirection dragDirection;
    HexCell previousCell;

    OptionalToggle roadMode;
    OptionalToggle underWaterMode;

    void Awake()
    {
        SetEditMode(false);
    }

    void Start()
    {
        hexGrid = FindObjectOfType<HexGrid>();
        client = FindObjectOfType<Client>();
    }

    void Update()
    {
        if(!EventSystem.current.IsPointerOverGameObject())
        {
            if(Input.GetMouseButton(0))
                HandleInput();
            else if(Input.GetKeyDown(KeyCode.U))
            {
                if(Input.GetKey(KeyCode.LeftShift))
                    DestroyUnit();
                else
                    CreateUnit();
            }
        }
        else
            previousCell = null;
    }

    HexCell GetCellUnderCursor()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        return hexGrid.GetCell(inputRay);
    }

    void HandleInput()
    {
        HexCell currentCell = GetCellUnderCursor();
        if(currentCell)
        {
            if(previousCell && previousCell != currentCell)
                ValidateDrag(currentCell);
            else
                isDrag = false;
            EditCell(currentCell);
            previousCell = currentCell;
            isDrag = true;
        }
        else
            previousCell = null;
    }

    void ValidateDrag(HexCell currentCell)
    {
        for(dragDirection = HexDirection.NE; dragDirection <= HexDirection.NW; ++dragDirection)
        {
            if(previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }

    void EditCell(HexCell cell)
    {
        if(cell)
        {
            data = "CEDI|" + cell.coordinates.X + "." + cell.coordinates.Z + "#";

            if(activeTerrainBiomeIndex >= 0)
                data += activeTerrainBiomeIndex + "#";
            else
                data += "-1#";

            if(applyElevation)
                data += activeElevation + ".";
            else
                data += "-1.";

            if(underWaterMode == OptionalToggle.No && cell.IsUnderWater)
                data += "0#";
            else if(underWaterMode == OptionalToggle.Yes && !cell.IsUnderWater)
                data += "1#";
            else
                data += "-1#";

            if(activeFeature > 0)
                data += activeFeature + "#";
            else
                data += "-1#";

            if(roadMode == OptionalToggle.No)
                data += "0.-1#";
            else if(roadMode == OptionalToggle.Yes)
                data += "1.";
            else
                data += "-1.-1#";

            if(isDrag)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if(otherCell && roadMode == OptionalToggle.Yes)
                {
                    data += (int)dragDirection + "." + otherCell.coordinates.X + "." + otherCell.coordinates.Z + "#";
                }
            }
            else
                data += "-1#";

            if(data != previousData)
            {
                previousData = data;
                if(FindObjectOfType<Client>())
                    client.Send(data);
            }
        }
    }

    public void NetworkEditedCell(string data)
    {
        string[] receivedData = data.Split('#');
        string[] position = receivedData[0].Split('.');

        int X = int.Parse(position[0]);
        int Z = int.Parse(position[1]);
        HexCell cell = hexGrid.GetCell(new HexCoordinates(X, Z));

        if(cell)
        {
            string[] road = receivedData[4].Split('.');
            string[] state = receivedData[2].Split('.');
            int newBiomeIndex = int.Parse(receivedData[1]);
            int newElevation = int.Parse(state[0]);
            int newWater = int.Parse(state[1]);
            int newFeature = int.Parse(receivedData[3]);

            if(newBiomeIndex != -1)
                cell.TerrainBiomeIndex = newBiomeIndex;
            if(newElevation != -1)
                cell.Elevation = newElevation;
            if(newWater != -1)
                cell.IsUnderWater = Convert.ToBoolean(newWater);
            if(newFeature != -1)
                cell.FeatureIndex = newFeature;
            if(road[0] == "0")
                cell.RemoveRoads();
            else if(road[1] != "-1")
            {
                int neighborX = int.Parse(road[2]);
                int neighborZ = int.Parse(road[3]);
                HexCell otherCell = hexGrid.GetCell(new HexCoordinates(neighborX, neighborZ));

                otherCell.AddRoad(((HexDirection)int.Parse(road[1])));
            }
        }
    }

    void CreateUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if(cell && !cell.Unit)
        {
            HexUnit unit = Instantiate(HexUnit.unitPrefab);
            float orientation = UnityEngine.Random.Range(0f, 360f);
            hexGrid.AddUnit(unit, cell, orientation);
        }
    }

    void DestroyUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if(cell && cell.Unit)
            hexGrid.RemoveUnit(cell.Unit);
    }

    public void SetEditMode(bool toggle)
    {
        enabled = toggle;
    }

    public void SetTerrainBiomeIndex(int index)
    {
        activeTerrainBiomeIndex = index;
    }

    public void SetElevation(float elevation)
    {
        activeElevation = (int) elevation;
    }

    public void SetFeatureIndex(int featureIndex)
    {
        activeFeature = featureIndex;
    }

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    public void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle) mode;
    }

    public void SetUnderWaterMode(int mode)
    {
        underWaterMode = (OptionalToggle) mode;
    }
}
