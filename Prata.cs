using UnityEngine;

[CreateAssetMenu(fileName = "NovaPrata", menuName = "Itens/Prata")]

public class PrataData : ScriptableObject
{

    [Header("identificação")]
    public string nome;
    public int prata = 2500;
    public Sprite fundoRostoLaranja;
    public Sprite imagemElemento;

    [Header ("mais")]
    public int exp = 1000;
    public int nivel = 1;
    public int ataqueBase = 30;
    public int hpBase = 30;


}
