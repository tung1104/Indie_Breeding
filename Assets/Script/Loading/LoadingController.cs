using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    public Image logo;
    public Image progress;
    public Button startButton;
    public GameObject progressBar;

    private void Awake()
    {
        progress.fillAmount = 0;
        logo.rectTransform.anchoredPosition = new Vector2(0, 0);
        startButton.onClick.AddListener(OnClickStartButton);
        startButton.gameObject.SetActive(false);
        progressBar.SetActive(true);
    }

    private void Start()
    {
        DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() =>
        {
            logo.rectTransform.DOAnchorPos(new Vector2(0, -700), 1).SetEase(Ease.OutBack);
        });

        progress.DOFillAmount(1, 2).OnComplete(() =>
        {
            startButton.gameObject.SetActive(true);
            progressBar.SetActive(false);
        });
    }

    private void OnClickStartButton()
    {
        SceneManager.LoadScene("01-Home");
    }
}
