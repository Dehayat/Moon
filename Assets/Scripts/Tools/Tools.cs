using UnityEngine;

public class Tools : MonoBehaviour
{
    [Header("Screen Shot")]
    public bool takeScreenShot;
    public string fileName = "ScreenShot";

    [Header("Nodes replacement")]
    public WorldMap map;
    public GameObject nodePrefab;

    private void Update()
    {
        if (takeScreenShot)
        {
            takeScreenShot = false;
            ScreenCapture.CaptureScreenshot(fileName+".png");
        }
    }
}
