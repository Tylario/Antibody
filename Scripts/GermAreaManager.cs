using UnityEngine;

public class GermAreaManager : MonoBehaviour
{
    public static Transform LowerPoint;
    public static Transform UpperPoint;
    public static Transform CenterPoint;
    public static Transform OuterPoint;

    public Transform lowerPoint;    // Assign in inspector
    public Transform upperPoint;    // Assign in inspector
    public Transform centerPoint;   // Assign in inspector
    public Transform outerPoint;    // Assign in inspector

    private void Awake()
    {
        // Assign static references at the start of the game
        LowerPoint = lowerPoint;
        UpperPoint = upperPoint;
        CenterPoint = centerPoint;
        OuterPoint = outerPoint;
    }
}
