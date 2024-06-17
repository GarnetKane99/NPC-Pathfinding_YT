using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement speed
    public float moveSpeed = 5f;

    // Jump force
    public float jumpForce = 5f;

    // Ground check variables
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // Private variables
    private Rigidbody2D rb;
    public bool isGrounded;

    public Projectile projectile;

    public float lastInput = 1;

    void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Handle movement
        float moveInput = Input.GetAxis("Horizontal");

        if(moveInput > 0 || moveInput < 0)
            lastInput = moveInput;

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        // Check if the character is on the ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Handle jumping
        if (isGrounded && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Projectile projectileInstance = Instantiate(projectile, transform.position, Quaternion.Euler(new Vector3(0,0,90)));
            projectileInstance.rb.AddForce(new Vector2(lastInput < 0 ? -1 : 1,0) * 10, ForceMode2D.Impulse);
        }
    }
}
