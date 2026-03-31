using UnityEngine;
using UnityEngine.UI;

public class Cheese : MonoBehaviour
{
   [SerializeField] private int cheeseCount;
   
   [SerializeField] private Text cheeseCountText;

   public void ClickCheese()
   {
      cheeseCount++;
      cheeseCountText.text = cheeseCount.ToString();
   }
}
