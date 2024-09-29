using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Globalization;

public static class Utilities
{
    public static List<int> LayerMaskToList(LayerMask layerMask)
    {
        List<int> layers = new List<int>();
        for (int i = 0; i < 32; i++)
        {
            if (((1 << i) & layerMask.value) != 0)
            {
                layers.Add(i);
            }
        }
        return layers;
    }

    public static void DebugStates(string codePoint)
    {
        string code = codePoint + " : ";
        for(int i = 0; i < Player.localPlayer.playerMove.states.Count; i++)
        {
            code = code + Player.localPlayer.playerMove.states[i] + " , ";
        }
        Debug.Log(code);
    }

    public static void ApplyEulerRotation(Transform child, Vector3 eulerAngles)
    {
        Quaternion rotation = Quaternion.Euler(eulerAngles);
        child.localRotation = rotation;
    }

    public static Transform FindChildRecursive(Transform parent, string name)
    {
        Transform result = parent.Find(name);
        if (result != null) return result;
        foreach (Transform child in parent)
        {
            result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }

    public static bool IsInside(BoxCollider2D enterableCollider, BoxCollider2D enteringCollider)
    {
        Bounds enterableBounds = enterableCollider.bounds;
        Bounds enteringBounds = enteringCollider.bounds;

        Vector2 center = enteringBounds.center;
        Vector2 extents = enteringBounds.extents;
        Vector2[] enteringVerticles = new Vector2[4];

        enteringVerticles[0] = new Vector2(center.x + extents.x, center.y + extents.y);
        enteringVerticles[1] = new Vector2(center.x - extents.x, center.y + extents.y);
        enteringVerticles[2] = new Vector2(center.x + extents.x, center.y - extents.y);
        enteringVerticles[3] = new Vector2(center.x - extents.x, center.y - extents.y);

        foreach (Vector2 verticle in enteringVerticles)
        {
            if (!enterableBounds.Contains(verticle))
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsInsideGeneric(Collider2D enterableCollider, BoxCollider2D enteringCollider)
    {
        Bounds enterableBounds = enterableCollider.bounds;
        Bounds enteringBounds = enteringCollider.bounds;

        Vector2 center = enteringBounds.center;
        Vector2 extents = enteringBounds.extents;
        Vector2[] enteringVerticles = new Vector2[4];

        enteringVerticles[0] = new Vector2(center.x + extents.x, center.y + extents.y);
        enteringVerticles[1] = new Vector2(center.x - extents.x, center.y + extents.y);
        enteringVerticles[2] = new Vector2(center.x + extents.x, center.y - extents.y);
        enteringVerticles[3] = new Vector2(center.x - extents.x, center.y - extents.y);

        foreach (Vector2 verticle in enteringVerticles)
        {
            if (!enterableBounds.Contains(verticle))
            {
                return false;
            }
        }
        return true;
    }

    public static string ConvertToTimer(int totalSecond)
    {
        int day = 86400;
        int hour = 3600;
        int minutes = 60;

        int tDay = 0;
        int tHours = 0;
        int tMinutes = 0;


        tDay = totalSecond / day;
        totalSecond = (totalSecond - (tDay * day));

        tHours = totalSecond / hour;
        totalSecond = (totalSecond - (tHours * hour));

        tMinutes = totalSecond / minutes;
        totalSecond = (totalSecond - (tMinutes * minutes));

        string Sday = tDay < 10 ? "0" + tDay : tDay.ToString();
        string Shours = tHours < 10 ? "0" + tHours : tHours.ToString();
        string SMinute = tMinutes < 10 ? "0" + tMinutes : tMinutes.ToString();
        string SSeconds = totalSecond < 10 ? "0" + totalSecond : totalSecond.ToString();

        return Sday + " : " + Shours + " : " + SMinute + " : " + SSeconds;

    }

    public static string ConvertToTimerMinuteAndSeconds(int totalSecond)
    {
        int hour = 3600;
        int minutes = 60;

        int tHours = 0;
        int tMinutes = 0;



        tHours = totalSecond / hour;
        totalSecond = (totalSecond - (tHours * hour));

        tMinutes = totalSecond / minutes;
        totalSecond = (totalSecond - (tMinutes * minutes));

        string SMinute = tMinutes < 10 ? "0" + tMinutes : tMinutes.ToString();
        string SSeconds = totalSecond < 10 ? "0" + totalSecond : totalSecond.ToString();

        return SMinute + " : " + SSeconds;

    }

    public static string ConvertToTimerLong(long totalSecond)
    {
        long day = 86400;
        long hour = 3600;
        long minutes = 60;

        long tDay = 0;
        long tHours = 0;
        long tMinutes = 0;


        tDay = totalSecond / day;
        totalSecond = (totalSecond - (tDay * day));

        tHours = totalSecond / hour;
        totalSecond = (totalSecond - (tHours * hour));

        tMinutes = totalSecond / minutes;
        totalSecond = (totalSecond - (tMinutes * minutes));

        string Sday = tDay < 10 ? "0" + tDay : tDay.ToString();
        string Shours = tHours < 10 ? "0" + tHours : tHours.ToString();
        string SMinute = tMinutes < 10 ? "0" + tMinutes : tMinutes.ToString();
        string SSeconds = totalSecond < 10 ? "0" + totalSecond : totalSecond.ToString();

        return Sday + " : " + Shours + " : " + SMinute + " : " + SSeconds;

    }

    [Serializable]
    public struct LinearInt
    {
        public int baseValue;
        public int bonusPerLevel;
        public List<CustomItem> upgradeItems;

        public int Get(int level) { return bonusPerLevel * (level - 1) + baseValue; }
    }

    [Serializable]
    public struct LinearLong
    {
        public long baseValue;
        public long bonusPerLevel;
        public List<CustomItem> upgradeItems;

        public long Get(int level) { return bonusPerLevel * (level - 1) + baseValue; }
    }

    [Serializable]
    public struct LinearFloat
    {
        public float baseValue;
        public float bonusPerLevel;
        public List<CustomItem> upgradeItems;

        public float Get(int level) { return bonusPerLevel * (level - 1) + baseValue; }
    }

    [Serializable]
    public struct ExponentialInt
    {
        public int multiplier;
        public float baseValue;
        public List<CustomItem> upgradeItems;

        public int Get(int level) => Convert.ToInt32(multiplier * Mathf.Pow(baseValue, (level - 1)));
    }

    [Serializable]
    public struct ExponentialLong
    {
        public long multiplier;
        public float baseValue;
        public List<CustomItem> upgradeItems;

        public long Get(int level) => Convert.ToInt64(multiplier * Mathf.Pow(baseValue, (level - 1)));
    }

    [Serializable]
    public struct ExponentialFloat
    {
        public float multiplier;
        public float baseValue;
        public List<CustomItem> upgradeItems;

        public float Get(int level) => multiplier * Mathf.Pow(baseValue, (level - 1));
    }

    public static void DebugAllParent (Transform initial)
    {
        Transform t = null;
        string debug = string.Empty;
        for (int i = 0; i < 100; i++)
        {
            if (t == null)
            {
                t = initial;
                debug = t.gameObject.name;
            }
            else
            {
                if (t.parent)
                {
                    t = t.parent.transform;
                    debug += " -> " + t.gameObject.name;
                }
            }
        }
        Debug.Log(debug);
    }

    public static void CopyInformation(GameObject oldGameObject, GameObject newGameObject)
    {
        BuildingAccessory buildingAccessoryOld = oldGameObject.GetComponent<BuildingAccessory>();
        BuildingAccessory buildingAccessoryNew = newGameObject.GetComponent<BuildingAccessory>();

        buildingAccessoryNew.group = buildingAccessoryOld.group;
        buildingAccessoryNew.owner = buildingAccessoryOld.owner;
        buildingAccessoryNew.health = buildingAccessoryOld.health;
        buildingAccessoryNew.maxHealth = buildingAccessoryOld.maxHealth;

        if (oldGameObject.GetComponent<CraftAccessory>())
        {
            for (int i = 0; i < ((CraftAccessory)buildingAccessoryOld).craftingItem.Count; i++)
            {
                ((CraftAccessory)buildingAccessoryNew).craftingItem.Add(((CraftAccessory)buildingAccessoryOld).craftingItem[i]);
            }
        }
        if (oldGameObject.GetComponent<Library>())
        {
            for (int i = 0; i < ((Library)buildingAccessoryOld).slots.Count; i++)
            {
                ((Library)buildingAccessoryNew).slots.Add(((Library)buildingAccessoryOld).slots[i]);
            }
        }
        if (oldGameObject.GetComponent<Billboard>())
        {
            ((Billboard)buildingAccessoryNew).message = ((Billboard)buildingAccessoryOld).message;
        }
        if (oldGameObject.GetComponent<Cabinet>())
        {
            for (int i = 0; i < ((Cabinet)buildingAccessoryOld).inventory.Count; i++)
            {
                ((Cabinet)buildingAccessoryNew).inventory.Add(((Cabinet)buildingAccessoryOld).inventory[i]);
            }
        }
        if (oldGameObject.GetComponent<Flag>())
        {
            ((Flag)buildingAccessoryNew).flag = ((Flag)buildingAccessoryOld).flag;
        }
        if (oldGameObject.GetComponent<Fridge>())
        {
            for (int i = 0; i < ((Fridge)buildingAccessoryOld).slots.Count; i++)
            {
                ((Fridge)buildingAccessoryNew).slots.Add(((Fridge)buildingAccessoryOld).slots[i]);
            }
        }
        if (oldGameObject.GetComponent<Furnace>())
        {
            for (int i = 0; i < ((Furnace)buildingAccessoryOld).elements.Count; i++)
            {
                ((Furnace)buildingAccessoryNew).elements.Add(((Furnace)buildingAccessoryOld).elements[i]);
            }

            for (int i = 0; i < ((Furnace)buildingAccessoryOld).results.Count; i++)
            {
                ((Furnace)buildingAccessoryNew).results.Add(((Furnace)buildingAccessoryOld).results[i]);
            }

            for (int i = 0; i < ((Furnace)buildingAccessoryOld).wood.Count; i++)
            {
                ((Furnace)buildingAccessoryNew).wood.Add(((Furnace)buildingAccessoryOld).wood[i]);
            }
            ((Furnace)buildingAccessoryNew).on = ((Furnace)buildingAccessoryOld).on;
        }
        if (oldGameObject.GetComponent<Warehouse>())
        {
            for (int i = 0; i < ((Warehouse)buildingAccessoryOld).slots.Count; i++)
            {
                ((Warehouse)buildingAccessoryNew).slots.Add(((Warehouse)buildingAccessoryOld).slots[i]);
            }
            ((Warehouse)buildingAccessoryNew).newName = ((Warehouse)buildingAccessoryOld).newName;
        }
        if (oldGameObject.GetComponent<WaterContainer>())
        {
            ((WaterContainer)buildingAccessoryNew).water = ((WaterContainer)buildingAccessoryOld).water;
        }
        if (oldGameObject.GetComponent<WeaponStorage>())
        {
            for (int i = 0; i < ((WeaponStorage)buildingAccessoryOld).weapon.Count; i++)
            {
                ((WeaponStorage)buildingAccessoryNew).weapon.Add(((WeaponStorage)buildingAccessoryOld).weapon[i]);
            }
        }
        if (oldGameObject.GetComponent<Aquarium>())
        {
            ((Aquarium)buildingAccessoryNew).dirt = ((Aquarium)buildingAccessoryOld).dirt;
        }
        if (oldGameObject.GetComponent<Tree>())
        {
            ((Tree)buildingAccessoryNew).rewardAmount = ((Tree)buildingAccessoryOld).rewardAmount;
        }
        if (oldGameObject.GetComponent<CuiltivableField>())
        {
            for (int i = 0; i < ((CuiltivableField)buildingAccessoryOld).cultivablePoints.Count; i++)
            {
                ((CuiltivableField)buildingAccessoryNew).cultivablePoints.Add(((CuiltivableField)buildingAccessoryOld).cultivablePoints[i]);
            }
        }

    }
    public static List<GameObject> GetNewConnections(List<Collider2D> newColliders, List<Collider2D> oldColliders)
    {
        List<GameObject> newConnections = new List<GameObject>();

        foreach (var collider in newColliders)
        {
            NetworkIdentity networkIdentity = collider.GetComponent<NetworkIdentity>();
            if (networkIdentity != null)
            {
                if(!oldColliders.Contains(collider))
                    newConnections.Add(networkIdentity.gameObject);
            }
        }

        return newConnections;
    }


    public static List<GameObject> GetRemovedConnections(List<Collider2D> newColliders, List<Collider2D> oldColliders)
    {
        List<GameObject> newConnections = new List<GameObject>();

        foreach (var collider in oldColliders)
        {
            NetworkIdentity networkIdentity = collider.GetComponent<NetworkIdentity>();
            if (networkIdentity != null)
            {
                if (!newColliders.Contains(collider))
                    newConnections.Add(networkIdentity.gameObject);
            }
        }

        return newConnections;
    }

    public static int CalculateWeekOfMonth(DateTime data)
    {
        // Trova il primo giorno del mese
        DateTime primoGiornoMese = new DateTime(data.Year, data.Month, 1);

        // Trova il primo lunedì del mese
        DateTime primoLunedì = primoGiornoMese.AddDays((DayOfWeek.Monday - primoGiornoMese.DayOfWeek + 7) % 7);

        // Calcola il numero della settimana del mese
        int numeroSettimanaMese = 1; // Iniziamo dalla prima settimana

        while (primoLunedì < data)
        {
            primoLunedì = primoLunedì.AddDays(7); // Passa al lunedì successivo
            numeroSettimanaMese++;
        }

        return numeroSettimanaMese;
    }


    public static Vector2 RandomPointInRectangle(Vector2 bottomLeftCorner, Vector2 size, float border)
    {
        float randomX = UnityEngine.Random.Range(bottomLeftCorner.x, bottomLeftCorner.x + (size.x + (- border)));
        float randomY = UnityEngine.Random.Range(bottomLeftCorner.y, bottomLeftCorner.y + (size.y + (- border)));
        return new Vector2(randomX, randomY);
    }

    public static Color GenerateRandomColor()
    {
        float r = UnityEngine.Random.Range(0f, 1f);
        float g = UnityEngine.Random.Range(0f, 1f);
        float b = UnityEngine.Random.Range(0f, 1f);

        return new Color(r, g, b);
    }

    public static string GenerateRandomColorHex()
    {
        float r = UnityEngine.Random.Range(0f, 1f);
        float g = UnityEngine.Random.Range(0f, 1f);
        float b = UnityEngine.Random.Range(0f, 1f);

        int rInt = Mathf.FloorToInt(r * 255);
        int gInt = Mathf.FloorToInt(g * 255);
        int bInt = Mathf.FloorToInt(b * 255);

        return $"#{rInt:X2}{gInt:X2}{bInt:X2}";
    }


    public static bool IsColliderCompletelyInside(Collider2D innerCollider, Collider2D outerCollider)
    {
        List<Vector2> points = GetColliderPoints(innerCollider);

        foreach (Vector2 point in points)
        {
            Vector2 worldPoint = innerCollider.transform.TransformPoint(point);
            if (!outerCollider.OverlapPoint(worldPoint))
            {
                return false; // Il punto non è contenuto, quindi il collider non è completamente all'interno
            }
        }

        return true; // Tutti i punti sono contenuti
    }


    public static List<Vector2> GetColliderPoints(Collider2D collider)
    {
        List<Vector2> points = new List<Vector2>();

        if (collider is PolygonCollider2D polyCollider)
        {
            points.AddRange(polyCollider.points);
        }
        else if (collider is BoxCollider2D boxCollider)
        {
            Vector2 size = boxCollider.size * 0.5f;
            points.Add(new Vector2(-size.x, -size.y));
            points.Add(new Vector2(size.x, -size.y));
            points.Add(new Vector2(size.x, size.y));
            points.Add(new Vector2(-size.x, size.y));
        }
        else if (collider is CircleCollider2D circleCollider)
        {
            int segments = 16; // Maggiore è il numero, migliore è l'approssimazione
            float angleStep = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle = Mathf.Deg2Rad * angleStep * i;
                Vector2 point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * circleCollider.radius;
                points.Add(point + circleCollider.offset);
            }
        }
        else if (collider is CapsuleCollider2D capsuleCollider)
        {
            // Approssimazione del CapsuleCollider2D
            int segments = 16; // Numero di segmenti per approssimare le semi-circonferenze
            float height = capsuleCollider.size.y;
            float width = capsuleCollider.size.x;
            float radius;
            float lineHeight;
            List<Vector2> capsulePoints = new List<Vector2>();

            if (capsuleCollider.direction == CapsuleDirection2D.Vertical)
            {
                radius = width * 0.5f;
                lineHeight = height - width;

                // Semi-circonferenza superiore
                for (int i = 0; i <= segments; i++)
                {
                    float angle = Mathf.PI * i / segments;
                    Vector2 point = new Vector2(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius + lineHeight * 0.5f
                    );
                    capsulePoints.Add(point);
                }

                // Semi-circonferenza inferiore
                for (int i = 0; i <= segments; i++)
                {
                    float angle = Mathf.PI * (1 + i / (float)segments);
                    Vector2 point = new Vector2(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius - lineHeight * 0.5f
                    );
                    capsulePoints.Add(point);
                }
            }
            else // CapsuleDirection2D.Horizontal
            {
                radius = height * 0.5f;
                lineHeight = width - height;

                // Semi-circonferenza destra
                for (int i = 0; i <= segments; i++)
                {
                    float angle = Mathf.PI / 2 + Mathf.PI * i / segments;
                    Vector2 point = new Vector2(
                        Mathf.Cos(angle) * radius + lineHeight * 0.5f,
                        Mathf.Sin(angle) * radius
                    );
                    capsulePoints.Add(point);
                }

                // Semi-circonferenza sinistra
                for (int i = 0; i <= segments; i++)
                {
                    float angle = 3 * Mathf.PI / 2 + Mathf.PI * i / segments;
                    Vector2 point = new Vector2(
                        Mathf.Cos(angle) * radius - lineHeight * 0.5f,
                        Mathf.Sin(angle) * radius
                    );
                    capsulePoints.Add(point);
                }
            }

            // Aggiungi l'offset del collider
            for (int i = 0; i < capsulePoints.Count; i++)
            {
                capsulePoints[i] += capsuleCollider.offset;
            }

            points.AddRange(capsulePoints);
        }
        // Aggiungi altri tipi di collider se necessario

        return points;
    }

}
