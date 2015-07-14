using UnityEngine;

public class WebCamToWall : MonoBehaviour
{
    void Start()
    {
        var webcam = new WebCamTexture();
        GetComponent<Kvant.Wall>().displacementMap = webcam;
        webcam.Play();
    }
}
