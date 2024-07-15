using System.Collections.Generic;
using UnityEngine;

public class GridGenerator2D : MonoBehaviour
{
    public int rows = 10;
    public int columns = 12;
    public float cellSizeX = 243.0f;
    public float cellSizeY = 241.0f;
    public Material lineMaterial;
    public Transform startPoint; // Punto di partenza per la griglia

    public List<GameObject> rowsObject = new List<GameObject>();
    public List<GameObject> columsObject = new List<GameObject>();

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        Vector2 start = startPoint.position;

        for (int row = 0; row <= rows; row++)
        {
            if (row != 0 || row != rows)
                CreateLine(new Vector2(start.x, start.y + row * cellSizeY), new Vector2(start.x + columns * cellSizeX, start.y + row * cellSizeY),0);
        }

        for (int column = 0; column <= columns; column++)
        {
            if (column != 0 || column != columns)
                CreateLine(new Vector2(start.x + column * cellSizeX, start.y), new Vector2(start.x + column * cellSizeX, start.y + rows * cellSizeY),1);
        }

        Destroy(rowsObject[rowsObject.Count - 1]);
        Destroy(rowsObject[0]);

        Destroy(columsObject[columsObject.Count - 1]);
        Destroy(columsObject[0]);

    }

    void CreateLine(Vector2 start, Vector2 end, int direction)
    {
        GameObject line = new GameObject("Line");
        line.transform.position = start;
        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.gameObject.layer = LayerMask.NameToLayer("TransparentFX");
        lineRenderer.material = lineMaterial;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 10f;
        lineRenderer.endWidth = 10f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        if(direction == 0)
        {
            rowsObject.Add(line);
        }
        else
        {
            columsObject.Add(line);
        }
    }
}
