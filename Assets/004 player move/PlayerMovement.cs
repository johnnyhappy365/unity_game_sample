using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum MoveDirection
{
  up,
  down,
  left,
  right
}
public class PlayerMovement : MonoBehaviour
{
  public float moveSpeed = 2.5f;
  private bool faceRight = true;
  private MoveDirection moveDirection;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    HandleInput();
    HandleMovement();
    HandleFlip();
  }

  void HandleInput()
  {
    // 不能走相反的方向
    if (Input.GetKey("up") && moveDirection != MoveDirection.down)
    {
      moveDirection = MoveDirection.up;
    }
    else if (Input.GetKey("down") && moveDirection != MoveDirection.up)
    {
      moveDirection = MoveDirection.down;
    }
    else if (Input.GetKey("left") && moveDirection != MoveDirection.right)
    {
      moveDirection = MoveDirection.left;
      faceRight = false;
    }
    else if (Input.GetKey("right") && moveDirection != MoveDirection.left)
    {
      moveDirection = MoveDirection.right;
      faceRight = true;
    }
  }

  void HandleMovement()
  {
    switch (moveDirection)
    {
      case MoveDirection.up:
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime);
        break;
      case MoveDirection.down:
        transform.position += new Vector3(0, -moveSpeed * Time.deltaTime);
        break;
      case MoveDirection.left:
        transform.position += new Vector3(-moveSpeed * Time.deltaTime, 0);
        break;
      case MoveDirection.right:
        transform.position += new Vector3(moveSpeed * Time.deltaTime, 0);
        break;
    }
  }

  void HandleFlip()
  {
    if (faceRight)
    {
      GetComponent<SpriteRenderer>().flipX = false;
    }
    else
    {
      GetComponent<SpriteRenderer>().flipX = true;
    }
  }
}
