using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public struct CultivablePoint
{
    public float percentual;
    public string objectName;
    public bool isCompleted;
    public float maxPercentual;
    public string seasonOfGrowth;
    public int maxToReturn;

    public CultivablePoint (string objName, string season)
    {
        this.percentual = 0f;
        this.objectName = objName;
        this.isCompleted = false;
        this.maxPercentual = 100.0f;
        this.seasonOfGrowth = season;
        this.maxToReturn = 0;
    }
}

public class CuiltivableField : BuildingAccessory
{
    public readonly SyncList<CultivablePoint> cultivablePoints = new SyncList<CultivablePoint> ();
    public List<PlantableSlot> slots = new List<PlantableSlot>();
    private CultivablePoint sample;
    private Color c;
    private Vector3 maxScale; 
    ScriptableItem itm;

    new void Start()
    {
        base.Start();
        maxScale = new Vector3(2.0f, 2.0f, 0.0f);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        cultivablePoints.Callback += OnPointChanged;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (cultivablePoints.Count == 0)
        {
            for (int i = 0; i < 6; i++)
            {
                cultivablePoints.Add(new CultivablePoint(string.Empty, string.Empty));
            }
        }
        InvokeRepeating(nameof(ManageCultivablePoint), 1.0f, 1.0f);
    }

    public void ManageCultivablePoint()
    {
        for (int i = 0; i < cultivablePoints.Count; i++)
        {
            sample = cultivablePoints[i];
            if (!string.IsNullOrEmpty(sample.objectName))
            {
                if(sample.isCompleted)
                {
                    sample.percentual += 3.0f;                    
                    if (sample.percentual > 130.0f)
                    {
                        sample = new CultivablePoint(string.Empty, string.Empty);
                    }
                }
                else if(sample.percentual < sample.maxPercentual && !sample.isCompleted)
                {
                    if(TemperatureManager.singleton.season == sample.seasonOfGrowth)
                    {
                        sample.percentual += 3.0f;
                        if (sample.percentual >= 100.0f) sample.isCompleted = true;
                    }
                    else
                    {
                        sample.isCompleted = true;
                        sample.percentual -= 2.0f;
                        if (sample.percentual < 20.0f)
                        {
                            sample = new CultivablePoint(string.Empty, string.Empty);
                        }
                    }
                }
            }
            cultivablePoints[i] = sample;
        }
    }

    public void OnPointChanged(SyncList<CultivablePoint>.Operation op, int itemIndex, CultivablePoint oldItem, CultivablePoint newItem)
    {
        c = slots[itemIndex].slotRenderer.color;
        c.a = newItem.objectName == string.Empty ? 1.0f : 0.0f;
        slots[itemIndex].slotRenderer.color = c;

        c = slots[itemIndex].plantedRenderer.color;
        c.a = newItem.objectName != string.Empty ? 1.0f : 0.0f;
        slots[itemIndex].plantedRenderer.color = c;

        if(newItem.objectName != string.Empty)
        {
            if (ScriptableItem.All.TryGetValue(newItem.objectName.GetStableHashCode(), out itm))
            {
                if (itm is FoodItem)
                {
                    slots[itemIndex].plantedRenderer.sprite = itm.image;
                }
            }
        }

        slots[itemIndex].plantedRenderer.material = newItem.percentual >= newItem.maxPercentual ? ModularBuildingManager.singleton.plantedCompleted : ModularBuildingManager.singleton.plantedNotCompleted;

        slots[itemIndex].plantedTransform.localScale = newItem.objectName == string.Empty ? Vector3.zero : CalculateGrown(newItem);
    }


    public Vector3 CalculateGrown(CultivablePoint point)
    {
        if (point.percentual >= 100f)
        {
            return maxScale;
        }
        else
        {
            float scaleFactor = Mathf.Clamp(point.percentual / 100f, 0f, 1f);

            return new Vector3(maxScale.x * scaleFactor, maxScale.y * scaleFactor, 0f);
        }
    }
}
