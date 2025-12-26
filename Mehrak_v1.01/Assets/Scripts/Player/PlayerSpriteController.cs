using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpriteController : MonoBehaviour
{
    public Sprite spriteRight;
    public Sprite spriteLeft;
    public Sprite spriteUp;
    public Sprite spriteDown;

    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h > 0.01f)
            spriteRenderer.sprite = spriteRight;
        else if (h < -0.01f)
            spriteRenderer.sprite = spriteLeft;
        else if (v > 0.01f)
            spriteRenderer.sprite = spriteUp;
        else if (v < -0.01f)
            spriteRenderer.sprite = spriteDown;
    }
}