using UnityEngine;

public class GerenciarPrata : MonoBehaviour
{
    [Header("Referências")]
    public PrataData[] pratas; // Lista de Pratas disponíveis

    [Header("Configurações")]
    public int prataPorItem = 2500; // Prata fornecida ao adicionar um item de Prata
    public int expPorPrata = 1000; // Exp fornecida ao usar a Prata

    private void Start()
    {
        AdicionarPrata("Sapo");
    }

    // Adiciona Prata à mochila
    public void AdicionarPrata(string nomePrata)
    {
        // Carrega o ComidaData da pasta Resources/Itens
        string caminho = "Itens/PrataData_" + nomePrata;
        PrataData prata = Resources.Load<PrataData>(caminho);
     
        {
             if (prata.nome != null)
            {
                Debug.Log($"Prata {nomePrata} adicionada à mochila.");
                return;
            }
        }

        Debug.LogError($"Prata {nomePrata} não encontrada.");
    }

    // Usa a Prata para ganhar Exp
    public void UsarPrataParaExp(string nomePrata)
    {
        Debug.Log($"Usando {nomePrata} para ganhar {expPorPrata} de Exp.");
        // Aqui você pode adicionar a lógica para upar o personagem
    }

    // Acumula a Prata na mochila
    public void AcumularPrata(string nomePrata)
    {
        Debug.Log($"Acumulando {prataPorItem} de Prata.");
        // Aqui você pode adicionar a lógica para acumular Prata
    }
}