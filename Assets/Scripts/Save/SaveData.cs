using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int day = 1;
    public int coins;
    public int slotsBought;

    // Water *used*, not water left, so saves from before 1.2 (missing field = 0) load full.
    public int waterUsed;

    public List<ItemSave> items = new();
    public List<CellSave> unlockedCells = new();
    public List<TileSave> tiles = new();
    public List<StateSave> tileStates = new();
    public List<CropSave> crops = new();
    public List<EdgeSave> fenceOpenings = new();
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
