using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMoveController : MonoBehaviour
{
    [Header("Movement")]
    public float moveAccel;
    public float maxSpeed;

    [Header("Jump")]
    public float jumpAccel;
    private float lastJumpTime;

    [Header("Ground Raycast")]
    public float groundRaycastDistance;
    public LayerMask groundLayerMask;

    [Header("Scoring")]
    public ScoreController score;
    public float scoringRatio;
    private float lastPositionX;

    [Header("GameOver")]
    public GameObject gameOverScreen;
    public float fallPositionY;

    [Header("Camera")]
    public CameraMoveController gameCamera;


    private Rigidbody2D rb;
    private Animator anim;
    private CharacterSoundController sound;

    private bool isOnGround;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sound = GetComponent<CharacterSoundController>();
    }

    //saya sengaja memindahkan penerimaan input dari update ke fixedupdate karena berdasarkan pengalaman (dan yang saya lakukan pada game saya yg lain), semua input dan physics lebih baik diletakan di fixedupdate
    //saya juga modif code jump berdasarkan code jump yang saya pakai di game saya yang lain (memastikan supaya jump benar2 satu kali saja setiap kali bisa jump/menyentuh tanah), tolong jangan kurangi nilai saya karena ini :)

    private void FixedUpdate()
    {
        // calculate velocity vector
        Vector2 velocityVector = rb.velocity;

        // raycast ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRaycastDistance, groundLayerMask);
        if (hit && rb.velocity.y == 0)
        {
            isOnGround = true;
        }
        else
        {
            isOnGround = false;
        }

        // read input
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {
            if ((Time.time - lastJumpTime > 0.1f) && isOnGround)
            {
                velocityVector.y += jumpAccel;
                sound.PlayJump();
                lastJumpTime = Time.time;
            }
        }

        velocityVector.x = Mathf.Clamp(velocityVector.x + moveAccel * Time.deltaTime, 0.0f, maxSpeed);

        rb.velocity = velocityVector;
    }

    private void Update()
    {
        // change animation
        anim.SetBool("isOnGround", isOnGround);

        // calculate score
        int distancePassed = Mathf.FloorToInt(transform.position.x - lastPositionX);
        int scoreIncrement = Mathf.FloorToInt(distancePassed / scoringRatio);

        if (scoreIncrement > 0)
        {
            score.IncreaseCurrentScore(scoreIncrement);
            lastPositionX += distancePassed;
        }

        // game over
        if (transform.position.y < fallPositionY)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        // set high score
        score.FinishScoring();

        // stop camera movement
        gameCamera.enabled = false;

        // show gameover
        gameOverScreen.SetActive(true);

        // disable this too
        this.enabled = false;
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + (Vector3.down * groundRaycastDistance), Color.red);
    }
}
