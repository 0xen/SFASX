using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIActionSelector : Graphic
{
    // How thick the bar is
    public int thikness = 5;
    // How large the action wheel is
    public float size = 10.0f;
    // How many segments will make up the action menu
    [Range(0, 360)]
    public int segments = 360;
    // What color will selected elements be
    public Color selectionColor;

    // The current canvas the selector is in
    public Canvas selectorCanvas;

    // All actions that are avalaibe to the selector wheel
    public List<TileActions> actions;

    // The current active selection
    private int mSelection;


    public TextMeshProUGUI ActionSelectorLable;

    protected override void Start()
    {
        actions = new List<TileActions>();
        mSelection = -1;
    }

    public void SetEnabled(bool enabled)
    {
        selectorCanvas.enabled = enabled;
    }

    public void Update()
    {
        // Since Unity trys to run UI elements in the editor mode before it has called start, we need to add this as a error check
        if (actions == null) return;
        // Calculate the mouse position reletive to the center of the screen
        Vector3 mousePos = Input.mousePosition;
        mousePos.x -= Screen.width / 2;
        mousePos.y -= Screen.height / 2;
        // If the mouse position is away from the center of the screen, find out there selection
        if(mousePos.magnitude > size / 2)
        {
            // Calculate based on the mouse position, where the mouse is on the scroll wheel
            float degrees = Mathf.Rad2Deg * Mathf.Atan2(-mousePos.x, -mousePos.y) + 180.0f;
            float segmentsPerOption = 360.0f / actions.Count;

            // Calculate the selection
            mSelection = ((int)(degrees / segmentsPerOption));

            if (mSelection < actions.Count)
            {
                ActionSelectorLable.text = actions[mSelection].name;
            }

        }
        else // If we are in the center of the screen, default to no option selected
        {
            mSelection = -1;
            ActionSelectorLable.text = "";
        }
        // Define that the UI element needs updated
        this.SetAllDirty();
    }

    // Defines a click on the UI and runs the appropriate action function
    public void Select()
    {
        if (mSelection >= actions.Count || mSelection < 0) return;
        actions[mSelection].Run();
    }

    // Generate Vertex data for the current segment
    protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs, Color color)
    {
        UIVertex[] vbo = new UIVertex[4];
        for (int i = 0; i < vertices.Length; i++)
        {
            var vert = UIVertex.simpleVert;
            vert.color = color;
            vert.position = vertices[i];
            vert.uv0 = uvs[i];
            vbo[i] = vert;
        }
        return vbo;
    }

    // Render's the UI Element
    protected override void OnPopulateMesh(Mesh toFill)
    {
        // Remove all pre existing UI data in the mesh
        toFill.Clear();
        // If we have no current actions, return
        if (actions == null || actions.Count == 0) return;
        // Calculate the max inner and outer rad of the circle
        float outer = -rectTransform.pivot.x * rectTransform.rect.width + size;
        float inner = -rectTransform.pivot.x * rectTransform.rect.width + size + thikness;

        var vbo = new VertexHelper(toFill);
        UIVertex vert = UIVertex.simpleVert;
        Vector2 prevX = Vector2.zero;
        Vector2 prevY = Vector2.zero;
        // create default UV data for the circle
        Vector2 uv0 = new Vector2(0, 0);
        Vector2 uv1 = new Vector2(0, 1);
        Vector2 uv2 = new Vector2(1, 1);
        Vector2 uv3 = new Vector2(1, 0);
        Vector2 pos0;
        Vector2 pos1;
        Vector2 pos2;
        Vector2 pos3;
        // Calculate how big each segment offset is in degrees
        float degrees = 360f / segments;
        // Define how many segments are in each options area
        int segmentsPerOption = segments / actions.Count;
        // Calculate the starting offset of the options segments
        int optionOffset = mSelection * segmentsPerOption;
        for (int i = 0; i < segments + 1; i++)
        {
            // Generate the rad, sin and cos for the circle
            float rad = Mathf.Deg2Rad * (i * degrees);
            float c = Mathf.Cos(rad);
            float s = Mathf.Sin(rad);
            float x = outer * s;
            float y = inner * s;
            // Reset the UV data
            uv0 = new Vector2(0, 1);
            uv1 = new Vector2(1, 1);
            uv2 = new Vector2(1, 0);
            uv3 = new Vector2(0, 0);
            pos0 = prevX;
            pos1 = new Vector2(outer * s, outer * c);

            // Draw inner ring further out
            pos2 = new Vector2(inner * s, inner * c);
            pos3 = prevY;
            // Define what the previous x and y where so we can use it in the next loop
            prevX = pos1;
            prevY = pos2;
            // See if the current segment is selected or not
            bool selected = mSelection >= 0 && i >= optionOffset && i <= optionOffset + segmentsPerOption;
            // Are we on the border of a segment
            bool border = (i % segmentsPerOption) == 0;
            Color col = color;
            // Modify the segment color based on if its selected or on a border
            if (selected) col = selectionColor;
            if (border) col = Color.black;
            // Generate the quad data
            vbo.AddUIVertexQuad(SetVbo(new[] { pos0, pos1, pos2, pos3 }, new[] { uv0, uv1, uv2, uv3 }, col));
        }
        if (vbo.currentVertCount > 3)
        {
            vbo.FillMesh(toFill);
        }
    }
}