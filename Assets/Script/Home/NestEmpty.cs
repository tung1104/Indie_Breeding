using UnityEngine;

public class NestEmpty : MonoBehaviour
{
    public void OnClick()
    {
        HomeUIController.Current.Get<NestBuilderPanel>().SetNestEmpty(this);
        HomeUIController.Current.Show<NestBuilderPanel>();
    }
}
