using UnityEngine;

public class GerenciarComida : MonoBehaviour
{
    [Header("Refer�ncias")]

    [Header("Configura��es")]
    public int expPorComida = 2500; // Exp fornecida ao usar a comida
    public int prataPorTroca = 1000; // Prata obtida ao trocar a comida

    private void Start()
    {
        // AdicionarComida("Sushi", 50); // Adiciona 2 sushis
    }

    // Adiciona uma comida � mochila
    public void AdicionarComida(string nomeComida, int quantidade = 1)
    {
        // Carrega o ComidaData da pasta Resources/Itens
        string caminho = "Itens/ComidaData_" + nomeComida;
        ComidaData comida = Resources.Load<ComidaData>(caminho);

        if (comida != null)
        {
            for (int i = 0; i < quantidade; i++)
            {
                Debug.Log($"Comida {nomeComida} adicionada � mochila.");
            }
        }
        else
        {
            Debug.LogError($"Comida {nomeComida} n�o encontrada no caminho: {caminho}");
        }
    }

    // Usa a comida para ganhar Exp
    public void UsarComidaParaExp(string nomeComida)
    {
        Debug.Log($"Usando {nomeComida} para ganhar {expPorComida} de Exp.");
        // Aqui voc� pode adicionar a l�gica para upar o personagem
    }

    // Troca a comida por Prata
    public void TrocarComidaPorPrata(string nomeComida)
    {
        Debug.Log($"Trocando {nomeComida} por {prataPorTroca} de Prata.");
        // Aqui voc� pode adicionar a l�gica para adicionar Prata � mochila
    }
}