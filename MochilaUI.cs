using UnityEngine;
using System.Collections.Generic;
using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using Firebase.Auth;


public class GerenciadorMochila : MonoBehaviour
{
    [Header("Dados da Mochila")]
    public List<ScriptableObject> itensNaMochila; // Lista de itens na mochila
    private Dictionary<string, int> quantidadeItens = new Dictionary<string, int>(); // Quantidade de cada item
    private ScriptableObject itemSelecionado; // Item atualmente selecionado
    private FirebaseFirestore firestore;
    private string userId; // ID do usuário autenticado
    private FirebaseAuth auth;

    private async void Start()
    {
        // Inicializa o Firebase
        firestore = FirebaseFirestore.DefaultInstance;

        // Obtém o ID do usuário (substitua pelo ID real do usuário)
        userId = "n71DMcR2u3TIxdmuoWtSWNcROAU2";
        auth = FirebaseAuth.DefaultInstance;

        await CarregarItensDoFirestore();

    }

    private async Task CarregarItensDoFirestore()
    {
        try
        {
            // Referência ao documento do usuário
            DocumentReference userRef = firestore.Collection("users").Document(userId);

            // Obtém o documento do usuário
            DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {


                // Obtém a lista de itens da mochila
                if (snapshot.ContainsField("mochila"))
                {
                    List<object> mochilaData = snapshot.GetValue<List<object>>("mochila");

                    foreach (var itemData in mochilaData)
                    {
                        Dictionary<string, object> itemDict = itemData as Dictionary<string, object>;
                        if (itemDict != null)
                        {
                            string nome = itemDict["nome"].ToString();
                            int quantidade = Convert.ToInt32(itemDict["quantidade"]);
                            string tipo = itemDict["tipo"].ToString();

                            // Carrega o ScriptableObject correspondente
                            ScriptableObject item = CarregarItemPorNomeETipo(nome, tipo);
                            if (item != null)
                            {
                                // Adiciona o item à mochila
                                itensNaMochila.Add(item);
                                quantidadeItens[nome] = quantidade;
                            }
                        }
                    }
                }

                Debug.Log("Itens da mochila carregados com sucesso!");
            }
            else
            {
                Debug.LogError("Documento do usuário não encontrado.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Erro ao carregar dados do Firestore: {ex.Message}");
        }
    }

    private ScriptableObject CarregarItemPorNomeETipo(string nome, string tipo)
    {
        // Implemente a lógica para carregar o ScriptableObject pelo nome e tipo
        // Exemplo:
        return Resources.Load<ScriptableObject>($"Itens/{tipo}/{nome}");
    }

    // Adiciona um item à mochila
    public async void AdicionarItem(ScriptableObject item, int quantidade = 1)
    {
        string nomeItem = ObterNomeItem(item);
        string tipoItem = ObterTipoItem(item);

        if (!itensNaMochila.Contains(item))
        {
            itensNaMochila.Add(item);
        }

        if (quantidadeItens.ContainsKey(nomeItem))
        {
            quantidadeItens[nomeItem] += quantidade;
        }
        else
        {
            quantidadeItens[nomeItem] = quantidade;
        }

        // Salva no Firestore
        await SalvarMochilaNoFirestore();

        Debug.Log($"Item adicionado: {nomeItem}. Quantidade: {quantidadeItens[nomeItem]}");
    }

    private async Task SalvarMochilaNoFirestore()
    {
        try
        {
            // Cria a lista de itens para salvar no Firestore
            List<Dictionary<string, object>> mochilaData = new List<Dictionary<string, object>>();

            foreach (var item in itensNaMochila)
            {
                string nomeItem = ObterNomeItem(item);
                int quantidade = quantidadeItens[nomeItem];
                string tipoItem = ObterTipoItem(item);

                mochilaData.Add(new Dictionary<string, object>
            {
                { "nome", nomeItem },
                { "quantidade", quantidade },
                { "tipo", tipoItem }
            });
            }

            // Referência ao documento do usuário
            DocumentReference userRef = firestore.Collection("users").Document(userId);

            // Salva a lista de itens no Firestore
            await userRef.SetAsync(new Dictionary<string, object>
        {
            { "mochila", mochilaData }
        }, SetOptions.MergeAll);

            Debug.Log("Mochila salva no Firestore com sucesso!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao salvar mochila no Firestore: {ex.Message}");
        }
    }

    // Remove uma quantidade específica de um item da mochila
    public async void VenderQuantidade(ScriptableObject item, int quantidade)
    {
        string nomeItem = ObterNomeItem(item);

        if (itensNaMochila.Contains(item))
        {
            if (quantidadeItens.ContainsKey(nomeItem))
            {
                if (quantidade <= quantidadeItens[nomeItem])
                {
                    quantidadeItens[nomeItem] -= quantidade;

                    if (quantidadeItens[nomeItem] <= 0)
                    {
                        quantidadeItens.Remove(nomeItem);
                        itensNaMochila.Remove(item);
                    }

                    // Atualiza a mochila no Firestore
                    await SalvarMochilaNoFirestore();

                    Debug.Log($"Vendido: {quantidade} unidade(s) de {nomeItem}. Restante: {quantidadeItens[nomeItem]}");
                }
                else
                {
                    Debug.LogError($"Quantidade insuficiente para vender: {nomeItem}");
                }
            }
            else
            {
                Debug.LogError($"Item não encontrado no dicionário de quantidades: {nomeItem}");
            }
        }
        else
        {
            Debug.LogError($"Item não encontrado na mochila: {nomeItem}");
        }
    }

    // Obtém a quantidade de um item na mochila
    public int ObterQuantidadeItem(ScriptableObject item)
    {
        string nomeItem = ObterNomeItem(item);
        return quantidadeItens.ContainsKey(nomeItem) ? quantidadeItens[nomeItem] : 1;
    }

    // Método auxiliar para obter o nome do item
    public string ObterNomeItem(ScriptableObject item)
    {
        switch (item)
        {
            case PersonagemData personagem:
                return personagem.nome;
            case ComidaData comida:
                return comida.nome;
            case PrataData prata:
                return prata.nome;
            default:
                Debug.LogError("Tipo de ScriptableObject não reconhecido.");
                return "";
        }
    }

    // Método para selecionar/deselecionar um item
    public void SelecionarItem(ScriptableObject item, bool selecionado)
    {
        if (selecionado)
        {
            itemSelecionado = item;
        }
        else
        {
            itemSelecionado = null;
        }
    }

    private string ObterTipoItem(ScriptableObject item)
    {
        if (item is PersonagemData) return "Personagem";
        if (item is ComidaData) return "Comida";
        if (item is PrataData) return "Prata";
        return "Desconhecido";
    }

    public void AdicionarPrata(int quantidade)
    {
        // Aqui você pode acessar o script que gerencia o menu do usuário
        // Exemplo: MenuUsuario.Instance.AdicionarPratas(quantidade);
        Debug.Log($"Pratas adicionadas: {quantidade}");
    }

    // Retorna o item atualmente selecionado
    public ScriptableObject ObterItemSelecionado()
    {
        return itemSelecionado;
    }
    public int CalcularPrata(ScriptableObject item, int quantidade)
    {
        if (item is PrataData prata)
        {
            return prata.prata * quantidade;
        }
        else if (item is ComidaData comida)
        {
            return comida.prata * quantidade;
        }
        else if (item is PersonagemData personagem)
        {
            return personagem.prata * quantidade;
        }
        else
        {
            Debug.LogError("Tipo de item não reconhecido.");
            return 0;
        }
    }
}