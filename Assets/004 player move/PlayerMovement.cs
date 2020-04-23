using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum MoveDirection
{
  none,
  up,
  down,
  left,
  right
}
public class PlayerMovement : MonoBehaviour
{
  public float moveSpeed = 16f;
  public Transform teammate;
  private bool faceRight = true;
  private MoveDirection moveDirection;
  private Snake snake;

  void Awake()
  {
    snake = new Snake(moveSpeed);
  }
  // Start is called before the first frame update
  void Start()
  {
    snake.Add(this.transform);
  }

  // Update is called once per frame
  void Update()
  {
    // If the next update is reached
    HandleInput();
    HandleMovement();
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
    snake.Move(moveDirection);
  }



  private void OnTriggerEnter2D(Collider2D collision)
  {
    Debug.Log(collision.tag);
    if (collision.tag == "Teammate" && !snake.Contains(collision.transform))
    {
      snake.Add(collision.transform);
    }
  }

  private void OnTriggerExit2D(Collider2D collision)
  {
    Debug.Log("exit");
  }

  private void AddTeammate(Transform transform)
  {
    teammate = transform;
    Vector3 offset = new Vector3();
    // set position
    switch (moveDirection)
    {
      case MoveDirection.left:
        offset = new Vector3(1, 0, 0);
        break;
      case MoveDirection.right:
        offset = new Vector3(-1, 0, 0);
        break;
      case MoveDirection.up:
        offset = new Vector3(0, -1, 0);
        break;
      case MoveDirection.down:
        offset = new Vector3(0, 1, 0);
        break;
    }
    teammate.position = this.transform.position + offset;
  }

  class Snake
  {
    LinkedList<SnakeNode> nodes;
    float moveSpeed;

    public Snake(float moveSpeed)
    {
      this.nodes = new LinkedList<SnakeNode>();
      this.moveSpeed = moveSpeed;
    }

    public void Add(Transform transform)
    {
      // set position
      SnakeNode last = GetLastNode();
      if (last != null)
      {
        transform.position = last.GetTailToAddPosition();
      }
      SnakeNode node = new SnakeNode(this, transform, moveSpeed);
      nodes.AddLast(node);
    }

    public SnakeNode GetFrontNode(SnakeNode node)
    {
      LinkedListNode<SnakeNode> frontListNode = nodes.Find(node).Previous;
      if (frontListNode != null)
      {
        return frontListNode.Value;
      }
      else { return null; }
    }

    public SnakeNode GetNextNode(SnakeNode node)
    {
      LinkedListNode<SnakeNode> nextListNode = nodes.Find(node).Next;
      if (nextListNode != null)
      {
        return nextListNode.Value;
      }
      else { return null; }
    }

    public SnakeNode GetLastNode()
    {
      if (nodes.Last != null) return nodes.Last.Value;
      return null;
    }

    public void Move(MoveDirection moveDirection)
    {
      foreach (SnakeNode node in nodes)
      {
        node.Move(moveDirection);
      }
    }

    public bool Contains(Transform transform)
    {
      foreach (SnakeNode node in nodes)
      {
        if (node.transform == transform) { return true; }
      }
      return false;
    }
  }

  class SnakeNode
  {
    float moveSpeed;
    public Transform transform;
    Snake snake;
    MoveDirection currentMoveDirection;
    MoveDirection previousMoveDirection;

    Vector3 turnDirectionPosition;
    bool faceRight = true;

    public SnakeNode(Snake snake, Transform transform, float moveSpeed)
    {
      this.snake = snake;
      this.transform = transform;
      this.moveSpeed = moveSpeed;
    }

    public void Move(MoveDirection moveDirection)
    {
      SnakeNode front = snake.GetFrontNode(this);
      if (front == null)
      {
        // first node
        HandleFirstNodeMovement(moveDirection);
      }
      else
      {
        // other node
        // 在与相同方向移动时进行位置修正
        PositionOffsetFix();
        HandleOtherNodeMovement(front);
      }

      HandleFlip();

      if (previousMoveDirection != currentMoveDirection)
      {
        turnDirectionPosition = transform.position;
      }
      previousMoveDirection = currentMoveDirection;
    }

    void HandleFirstNodeMovement(MoveDirection moveDirection)
    {
      SnakeNode next = snake.GetNextNode(this);
      if (next == null)
      {
        currentMoveDirection = moveDirection;
        MoveByCurrentDirection();
      }
      else
      {
        if (next.currentMoveDirection == currentMoveDirection)
        {
          currentMoveDirection = moveDirection;
          MoveByCurrentDirection();
        }
        else
        {
          MoveByCurrentDirection();
        }
      }
    }

    void HandleOtherNodeMovement(SnakeNode front)
    {
      if (currentMoveDirection == MoveDirection.none)
      {
        // first time
        currentMoveDirection = front.currentMoveDirection;
        MoveByCurrentDirection();
      }
      else
      {
        if (currentMoveDirection == front.currentMoveDirection)
        {
          // 与front移动方向一样
          MoveByCurrentDirection();
        }
        else
        {
          // 如果与前端的位置不一样，要判断front的转向的坐标，小于该坐标按currentMoveDirection走，否则按front.currentMoveDirection走
          // turnDirectionPosition
          bool needTurn = false;
          switch (currentMoveDirection)
          {
            case MoveDirection.up:
              needTurn = front.turnDirectionPosition.y - transform.position.y <= 0;
              break;
            case MoveDirection.down:
              needTurn = front.turnDirectionPosition.y - transform.position.y >= 0;
              break;
            case MoveDirection.left:
              needTurn = front.turnDirectionPosition.x - transform.position.x >= 0;
              break;
            case MoveDirection.right:
              needTurn = front.turnDirectionPosition.x - transform.position.x <= 0;
              break;
          }

          if (needTurn)
          {
            currentMoveDirection = front.currentMoveDirection;
          }

          MoveByCurrentDirection();
        }
      }
    }

    void MoveByCurrentDirection()
    {
      MoveByDirection(currentMoveDirection);
    }

    void PositionOffsetFix()
    {
      SnakeNode front = snake.GetFrontNode(this);
      if (front.currentMoveDirection != currentMoveDirection) return;

      Vector3 fixPosition = new Vector3();
      Vector3 offset = new Vector3();
      float fixSpeed = 0.01f;
      float distanceY = front.transform.position.y - transform.position.y;
      float distanceX = front.transform.position.x - transform.position.x;

      switch (currentMoveDirection)
      {
        case MoveDirection.up:
          if (distanceY >= 1)
          {
            offset = new Vector3(0, fixSpeed);
          }
          else
          {
            offset = new Vector3(0, -fixSpeed);
          }
          break;
        case MoveDirection.down:
          if (distanceY >= -1)
          {
            offset = new Vector3(0, fixSpeed);
          }
          else
          {
            offset = new Vector3(0, -fixSpeed);
          }
          break;
        case MoveDirection.left:
          if (distanceX >= -1)
          {
            offset = new Vector3(fixSpeed, 0);
          }
          else
          {
            offset = new Vector3(-fixSpeed, 0);
          }
          break;
        case MoveDirection.right:
          if (distanceX >= 1)
          {
            offset = new Vector3(fixSpeed, 0);
          }
          else
          {
            offset = new Vector3(-fixSpeed, 0);
          }
          break;
      }
      transform.position += offset;
    }
    void MoveByDirection(MoveDirection moveDirection)
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
          faceRight = false;
          transform.position += new Vector3(-moveSpeed * Time.deltaTime, 0);
          break;
        case MoveDirection.right:
          faceRight = true;
          transform.position += new Vector3(moveSpeed * Time.deltaTime, 0);
          break;
      }
    }

    public Vector3 GetTailToAddPosition()
    {
      Vector3 offset = new Vector3();
      switch (currentMoveDirection)
      {
        case MoveDirection.left:
          offset = new Vector3(1, 0, 0);
          break;
        case MoveDirection.right:
          offset = new Vector3(-1, 0, 0);
          break;
        case MoveDirection.up:
          offset = new Vector3(0, -1, 0);
          break;
        case MoveDirection.down:
          offset = new Vector3(0, 1, 0);
          break;
      }
      return transform.position + offset;
    }

    void HandleFlip()
    {
      if (faceRight)
      {
        transform.gameObject.GetComponent<SpriteRenderer>().flipX = false;
      }
      else
      {
        transform.gameObject.GetComponent<SpriteRenderer>().flipX = true;
      }
    }
  }
}
