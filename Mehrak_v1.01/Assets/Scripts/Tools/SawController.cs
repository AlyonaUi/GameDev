using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SawController : ToolController
{
    private void Update()
    {
        if (data != null)
        {
            transform.Rotate(Vector3.forward, data.rotationSpeed * Time.deltaTime);
        }
    }
}