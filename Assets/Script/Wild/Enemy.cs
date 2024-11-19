using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public float speedMove;
    public float speedRotate;
    public EnemyInfo info;

    private Vector3 positionStart;
    private Vector3 positionTarget;
    private float timeIdle;

    private void Start()
    {
        positionStart = transform.position;
        RandomAction();
    }

    private void Update()
    {
        if (timeIdle > 0)
        {
            timeIdle -= Time.deltaTime;
        }
        else if (transform.position == positionTarget)
        {
            RandomAction();
        }
        else
        {
            MoveToTarget();
        }

        levelText.transform.rotation = Quaternion.LookRotation(Vector3.forward + Vector3.down);
    }

    public void SetInfo(EnemyInfo info)
    {
        this.info = info;
        levelText.text = "Lv " + info.level;
    }

    private void MoveToTarget()
    {
        var position = Vector3.MoveTowards(transform.position, positionTarget, Time.deltaTime * speedMove);
        var rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(positionTarget - transform.position), Time.deltaTime * speedRotate);
        transform.SetPositionAndRotation(position, rotation);
    }

    private void RandomAction()
    {
        if (Random.Range(0, 1) == 0)
        {
            var circleRandom = Random.insideUnitCircle;
            positionTarget = positionStart + new Vector3(circleRandom.x, 0, circleRandom.y);
            PlayAnimWalk();
        }
        else
        {
            timeIdle = Random.Range(1f, 2f);
            PlayAnimIdle();
        }
    }

    private void PlayAnimWalk()
    {

    }

    private void PlayAnimIdle()
    {

    }

    public void OnClick()
    {
        WildUIController.Current.Get<EnemyInfoPanel>().enemyInfo = info;
        WildUIController.Current.Get<EnemyInfoPanel>().positionScreen = Camera.main.WorldToScreenPoint(transform.position);
        WildUIController.Current.Show<EnemyInfoPanel>();
    }
}
