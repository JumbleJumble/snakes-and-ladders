using UnityEngine;

[ExecuteAlways]
public class GameBoard : MonoBehaviour
{
    [Header("Board Creation")]
    public Material blankMaterial;

    [Header("Spaces")]
    public float spaceSize = 1f;
    public int spacesX = 10;
    public int spacesY = 10;
    public Color boardColor = new Color(0.66f, 0.83f, 1);
    public float paperThickness = 0.01f;

    [Header("Grid")]
    public float gridThickness = 0.1f;
    public float gridHeight = 0.01f;
    public Color gridColor = Color.black;
    public GameObject numberPrefab;

    [Header("Border")]
    public Color borderColor = new Color(0.18f, 0, 0);
    public float borderWidth = 0.1f;
    public float boardThickness = 0.01f;

    private float boardWidth => spacesX * (spaceSize + gridThickness) + gridThickness;
    private float boardDepth => spacesY * (spaceSize + gridThickness) + gridThickness;
    private bool dirty;

    private void OnEnable() => CreateBoard();
    private void OnValidate() => dirty = true;

    private const float FontScale = 0.5f;
    private static int GetNumberFontSize(string numberText) => (int) ((240 - numberText.Length * 20) * FontScale);

    private void Update()
    {
        if (!dirty)
        {
            return;
        }

        dirty = false;
        CreateBoard();
    }

    private void CreateBoard()
    {
        var txfm = transform;
        int childCount = txfm.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(txfm.GetChild(i).gameObject);
        }

        var boardRegion = new GameObject("board region");
        boardRegion.transform.parent = txfm;
        boardRegion.transform.localPosition = Vector3.zero;
        boardRegion.transform.rotation = txfm.rotation;
        boardRegion.transform.localScale = new Vector3(1, 1, 1);

        CreateCardboard(boardRegion);
        CreatePaper(boardRegion);
        CreateGrid();
        CreateNumbers();
    }

    private void CreateCardboard(GameObject parent)
    {
        var cardboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cardboard.name = "cardboard";
        cardboard.transform.parent = parent.transform;
        cardboard.transform.localPosition = Vector3.zero;
        cardboard.transform.rotation = parent.transform.rotation;
        var sizeY = boardThickness;
        var sizeX = boardWidth + 2 * borderWidth;
        var sizeZ = boardDepth + 2 * borderWidth;
        cardboard.transform.localScale = new Vector3(sizeX, sizeY, sizeZ);
        var rdr = cardboard.GetComponent<Renderer>();
        rdr.sharedMaterial = new Material(blankMaterial) { color = borderColor };
    }

    private void CreatePaper(GameObject parent)
    {
        var backing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backing.name = "paper";
        backing.transform.parent = parent.transform;
        backing.transform.localPosition = Vector3.zero;
        backing.transform.rotation = parent.transform.rotation;
        backing.transform.localScale = new Vector3(
            boardWidth - gridThickness,
            paperThickness,
            boardDepth - gridThickness);

        backing.transform.Translate(0, boardThickness / 2 + paperThickness / 2, 0);
        var rdr = backing.GetComponent<Renderer>();
        rdr.sharedMaterial = new Material(blankMaterial) { color = boardColor };
    }

    private void CreateGrid()
    {
        var gridLines = new GameObject("gridlines");
        gridLines.transform.parent = transform;
        gridLines.transform.localScale = transform.localScale.Inverse();
        gridLines.transform.localPosition = Vector3.zero;
        gridLines.transform.rotation = transform.rotation;

        var lineHeight = gridHeight + paperThickness;
        var lineYpos = boardThickness / 2 + lineHeight / 2;
        var gridMaterial = new Material(blankMaterial) { color = gridColor };

        var xpos = -(spacesX / 2f) * (spaceSize + gridThickness);
        for (int x = 0; x <= spacesX; x++)
        {
            CreateGridLine(
                xpos,
                gridThickness,
                0,
                boardDepth,
                $"Vert @ {x}",
                gridLines,
                lineYpos,
                lineHeight,
                gridMaterial);

            xpos += spaceSize + gridThickness;
        }

        var zpos = -(spacesY / 2f) * (gridThickness + spaceSize);
        for (int z = 0; z <= spacesY; z++)
        {
            CreateGridLine(
                0,
                boardWidth,
                zpos,
                gridThickness,
                $"Horiz @ {z}",
                gridLines,
                lineYpos,
                lineHeight,
                gridMaterial);

            zpos += spaceSize + gridThickness;
        }
    }

    private static void CreateGridLine(
        float x,
        float xdim,
        float z,
        float zdim,
        string objectName,
        GameObject gridLines,
        float lineYpos,
        float lineHeight,
        Material gridMaterial)
    {
        var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = objectName;
        line.transform.parent = gridLines.transform;
        line.transform.localPosition = new Vector3(x, lineYpos, z);
        line.transform.rotation = gridLines.transform.rotation;
        line.transform.localScale = new Vector3(xdim, lineHeight, zdim);
        line.GetComponent<Renderer>().sharedMaterial = gridMaterial;
    }

    private void CreateNumbers()
    {
        var numbers = new GameObject("numbers");
        var txfm = transform;
        numbers.transform.parent = txfm;
        numbers.transform.localPosition = Vector3.zero;
        numbers.transform.rotation = txfm.rotation;
        int spaceNumber = 1;
        var zpos = -1 * (spaceSize + gridThickness) * (spacesY - 1) / 2f;
        for (int y = 0; y < spacesY; y++)
        {
            var sign = -1 + 2 * (y % 2);
            var xpos = sign * ((spacesX - 1) / 2f * (spaceSize + gridThickness));
            for (int x = 0; x < spacesX; x++)
            {
                var go = Instantiate(
                    numberPrefab,
                    numbers.transform);

                go.transform.localPosition = new Vector3(xpos, boardThickness / 2f + paperThickness + 0.005f, zpos);
                go.transform.localRotation = Quaternion.Euler(90, 0, 0);
                var textMesh = go.GetComponent<TextMesh>();
                var numberText = spaceNumber.ToString();
                go.name = numberText;
                textMesh.text = numberText;
                textMesh.fontSize = GetNumberFontSize(numberText);

                xpos += -sign * (spaceSize + gridThickness);
                spaceNumber++;
            }

            zpos += spaceSize + gridThickness;
        }
    }
}
