using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Element", menuName = "Periodic Table/Element")]
public class Element : ScriptableObject
{
  public string number = "-1";
  public string code = "Code";
  public string name = "Name";
  public string quantity = "0";
  public Families family = Families.NobleGas;
  public Color color;

  public static readonly Dictionary<Families, Color> familiesColors =
   new Dictionary<Families, Color> {
        {Families.NonMetal, new Color(62/255f, 100/255f, 24/255f, 1)},
        {Families.NobleGas, new Color(58/255f, 33/255f, 81/255f, 1)},
        {Families.Metalloid, new Color(1/255f, 81/255f, 70/255f, 1)},
        {Families.PostTransitionMetals, new Color(0/255f, 54/255f, 102/255f, 1)},
        {Families.TransitionMetals, new Color(113/255f, 16/255f, 25/255f, 1)},
        {Families.AlkalineEarthMetals, new Color(132/255f, 96/255f, 17/255f, 1)},
        {Families.AlkaliMetals, new Color(108/255f, 59/255f, 1/255f, 1)},
   };

  private void OnValidate()
  {
    color = familiesColors[family];
  }


}
