using Pathfinding;
using Pathfinding.RVO;
using System.Collections;
using UnityEngine;

public class DinoLocomotion : MonoBehaviour
{
    public System.Action OnEat;

    public float moveSpeed = 0.3f;
    public float rotationSpeed = 10f;
    public float stoppingDistance = 0.1f;
    public Vector2 randomArea = new Vector2(2f, 1f);
    public float idleTime = 3f;
    public Animator[] animators;
    public RVOController controller;
    public bool canEat;
    public Seeker seeker;

    private Vector3 targetPoint;
    private bool isRun;
    private float timer;
    private bool canMove;
    private int ran;
    private bool isEating;
    private Path path;
    private int currentWaypoint;

    private FoodStorage foodStorage => HomeController.Current.foodsController.foodStorage;

    public void Init()
    {
        animators = GetComponentsInChildren<Animator>();
    }

    public void SetCanEat(bool canEat)
    {
        this.canEat = canEat;
        if (canEat)
        {
            StartCoroutine(SetRadius(0.01f, 0));
            var randomPoint = foodStorage.GetRandomPointAround();
            seeker.StartPath(transform.position, randomPoint, OnPathComplete);
        }
        else
        {
            StartCoroutine(SetRadius(0.05f, 3));
            isEating = false;
            TakeMove();
        }
    }

    public IEnumerator SetRadius(float radius, float time)
    {
        yield return new WaitForSeconds(time);
        controller.radius = radius;
    }

    private void Update()
    {
        if (!canMove || isEating) return;

        if (canEat)
        {
            if (transform.position == targetPoint)
            {
                Quaternion lookRotation = Quaternion.LookRotation(foodStorage.transform.position - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed * 20);
                if (transform.rotation == lookRotation)
                {
                    isEating = true;
                    OnEat?.Invoke();
                }
            }
            else
            {
                SetSpeedAnim(controller.velocity.magnitude * 5);
                Move();
            }
        }
        else
        {
            if (isRun)
            {
                SetSpeedAnim(controller.velocity.magnitude * 5);
                Move();

                if (Vector3.Distance(transform.position, targetPoint) < stoppingDistance)
                {
                    RandomAction();
                }
            }
            else
            {
                SetSpeedAnim(1);
                timer += Time.deltaTime;
                if (timer > idleTime)
                {
                    timer = 0;
                    RandomAction();
                }
            }
        }
    }

    private void Move()
    {
        if (path == null || currentWaypoint >= path.vectorPath.Count) return;

        controller.SetTarget(path.vectorPath[currentWaypoint], moveSpeed, moveSpeed * 1.2f, path.vectorPath[currentWaypoint]);
        var delta = controller.CalculateMovementDelta(transform.position, Time.deltaTime);
        transform.position = transform.position + delta;

        if (controller.velocity.magnitude > 0.2f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(controller.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        if (transform.position == path.vectorPath[currentWaypoint])
        {
            currentWaypoint++;
        }
    }

    private void RandomAction()
    {
        int r = Random.Range(0, 2);

        switch (r)
        {
            case 0:
                TakeIdle();
                break;
            case 1:
                TakeMove();
                break;
        }
    }

    public void ContinueMove()
    {
        canMove = true;
        RandomAction();
    }

    public void Hatch()
    {
        canMove = false;
        TakeIdle(1);
    }

    public void Mate()
    {
        canMove = false;
        PlayMateAnim();
    }

    public void TakeIdle(int index = -1)
    {
        ran = index == -1 ? Random.Range(1, 5) : index;
        PlayIdleAnim(ran);
        isRun = false;
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
            isRun = true;
            PlayRunAnim();
            targetPoint = path.vectorPath[^1];
        }
    }

    public void TakeMove()
    {
        seeker.StartPath(transform.position, HomeController.Current.astarPathController.RandomNode(), OnPathComplete);
    }

    public void PlayIdleAnim(int index)
    {
        PlayAnim("Idle_" + index);
    }

    public void PlayRunAnim()
    {
        PlayAnim("Run");
    }

    public void PlayEatAnim()
    {
        PlayAnim("Attack");
    }

    public void PlayMateAnim()
    {
        PlayAnim("Mate");
    }

    public void PlayAnim(string name)
    {
        foreach (var animator in animators) animator.CrossFade(name, 0.3f);
    }

    public void SetSpeedAnim(float speed)
    {
        foreach (var animator in animators) animator.speed = speed;
    }
}
