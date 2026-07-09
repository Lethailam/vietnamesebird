using UnityEngine;

public class ShopGridManager : MonoBehaviour
{
    public void OnItemClicked(int itemIndex)
    {
        Debug.Log("Clicked item: " + itemIndex);
    }
}