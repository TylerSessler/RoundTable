using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trajectoryRender : MonoBehaviour
{
    public static trajectoryRender instance;
    [SerializeField] float maxLineLength; // Max duration the line draws for
    [SerializeField] public LineRenderer trajectoryLine;
    [SerializeField] int smooth; // How many line segments are used to show the trajectory.

    private void Start()
    {
         instance = this;
    }

    public void drawLine(Vector3 start, Vector3 velocity)
    {
        float seperation = maxLineLength / smooth;

        Vector3[] renderPoints = calculateLine(start, velocity, seperation);
        trajectoryLine.positionCount = smooth;
        for (int i = 0; i < renderPoints.Length; i++)
        {
            //Debug.Log("point: " + i + " is at " + renderPoints[i]);
        }
        trajectoryLine.SetPositions(renderPoints);
    }

    Vector3[] calculateLine(Vector3 start, Vector3 velocity, float spacing)
    {
        Vector3[] renderPoints = new Vector3[smooth];
        renderPoints[0] = start;
        for (int i = 1; i < smooth; i++)
        {
            float offset = spacing * i;

            Vector3 preGrav = start * offset;
            Vector3 gravOffset = Vector3.up * 1f * 1 * offset * offset;
            Vector3 postGrav = start + preGrav - gravOffset;
            renderPoints[i] = postGrav;
        }

        return renderPoints;
    }
}
