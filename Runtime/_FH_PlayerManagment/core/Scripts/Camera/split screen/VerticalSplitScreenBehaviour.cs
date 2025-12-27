public class VerticalSplitScreenBehaviour : BaseSplitScreenBehaviour
{
    public override void UpdateSplitScreen(CameraData cameraA)
    {
        cameraA.SetRect(0, 0, 1, 1);
    }

    public override void UpdateSplitScreen(CameraData cameraA, CameraData cameraB)
    {
        cameraA.SetRect(0, 0, 0.5f, 1);
        cameraB.SetRect(0.5f, 0, 0.5f, 1);
    }

    public override void UpdateSplitScreen(CameraData cameraA, CameraData cameraB, CameraData cameraC)
    {
        cameraA.SetRect(0, 0, 0.5f, 1);
        cameraB.SetRect(0.5f, 0.5f, 0.5f, 0.5f);
        cameraC.SetRect(0.5f, 0, 0.5f, 0.5f);
    }

    public override void UpdateSplitScreen(CameraData cameraA, CameraData cameraB, CameraData cameraC, CameraData cameraD)
    {
        cameraA.SetRect(0, 0.5f, 0.5f, 0.5f);
        cameraB.SetRect(0.5f, 0.5f, 0.5f, 0.5f);
        cameraC.SetRect(0, 0, 0.5f, 0.5f);
        cameraD.SetRect(0.5f, 0, 0.5f, 0.5f);
    }
}
