using UnityEngine;

[CreateAssetMenu(fileName = "Comida", menuName = "Itens/Comida")]
public class ComidaData : ScriptableObject

{
    [Header ("identificação")]
    public string nome;
    public int exp;
    public int prata;
    public Sprite fundoRostoLaranja;
    public int nivel = 1;
    public Sprite imagemElemento;

    [Header("atributos")]
    public int ataqueBase;
    public int hpBase;
}