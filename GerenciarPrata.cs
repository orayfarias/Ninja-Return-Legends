using UnityEngine;

public class GerenciarPrata : MonoBehaviour
{
    [Header("Refer�ncias")]
    public PrataData[] pratas; // Lista de Pratas dispon�veis

    [Header("Configura��es")]
    public int prataPorItem = 2500; // Prata fornecida ao adicionar um item de Prata
    public int expPorPrata = 1000; // Exp fornecida ao usar a Prata

    private void Start()
    {
        AdicionarPrata("Sapo");
    }

    // Adiciona Prata � mochila
    public void AdicionarPrata(string nomePrata)
    {
        // Carrega o ComidaData da pasta Resources/Itens
        string caminho = "Itens/PrataData_" + nomePrata;
        PrataData prata = Resources.Load<PrataData>(caminho);
     
        {
             if (prata.nome != null)
            {
                Debug.Log($"Prata {nomePrata} adicionada � mochila.");
                return;
            }
        }

        Debug.LogError($"Prata {nomePrata} n�o encontrada.");
    }

    // Usa a Prata para ganhar Exp
    public void UsarPrataParaExp(string nomePrata)
    {
        Debug.Log($"Usando {nomePrata} para ganhar {expPorPrata} de Exp.");
        // Aqui voc� pode adicionar a l�gica para upar o personagem
    }

    // Acumula a Prata na mochila
    public void AcumularPrata(string nomePrata)
    {
        Debug.Log($"Acumulando {prataPorItem} de Prata.");
        // Aqui voc� pode adicionar a l�gica para acumular Prata
    }
}