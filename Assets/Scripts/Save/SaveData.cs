using System;
using System.Collections.Generic;

// Everything one FarmGrid needs to rebuild itself.
[Serializable]
public class GridSave
{
    public List<CellSave> unlockedCells = new();
    public List<TileSave> tiles = new();
    public List<StateSave> tileStates = new();
    public List<CropSave> crops = new();
    public List<EdgeSave> fenceOpenings = new();
}

[Serializable]
public class NamedGridSave : GridSave
{
    public string id;
}

// The main farm's grid fields are inherited from GridSave and stay at the top level of
// the file, so saves from before 1.3 still load. Extra grids (greenhouse beds) go in a list.
[Serializable]
public class SaveData : GridSave
{
    public int day = 1;
    public int coins;
    public int slotsBought;

    // Water *used*, not water left, so saves from before 1.2 (missing field = 0) load full.
    public int waterUsed;

    public List<ItemSave> items = new();

    public bool greenhouseOwned;

    public int chickens;
    public int eggsWaiting;
    public List<NamedGridSave> extraGrids = new();
}

[Serializable]
public class EdgeSave
{
    public int x, z;
    public int axis;
}

[Serializable]
public class ItemSave
{
    public string item;
    public int count;
}

[Serializable]
public class CellSave
{
    public int x, z;
}

[Serializable]
public class TileSave
{
    public int x, z;
    public string tile;
}

[Serializable]
public class StateSave
{
    public int x, z;
    public int state;
    public int wetDaysLeft;
}

[Serializable]
public class CropSave
{
    public int x, z;
    public string crop;
    public float grownDays;
    public int nights;
    public int wetNights;
    public int bonusYield;   // bonus locked in at planting (1.1 rules); 0 for crops planted in 1.2+
}
