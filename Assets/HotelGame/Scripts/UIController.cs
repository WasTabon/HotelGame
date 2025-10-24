using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _moneyText;

    private void Update()
    {
        _moneyText.text = $"{WalletController.Instance.Money}<sprite name=\"brilliant\">";
    }
}
