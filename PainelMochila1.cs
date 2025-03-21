using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PainelMochilaUI : MonoBehaviour
{
    [Header("Referências")]
    public Transform content; // Content do ScrollView onde os itens serão exibidos
    public GameObject itemPrefab; // Prefab do item
    public GerenciadorMochila gerenciadorMochila; // Referência ao GerenciadorMochila
    public Button botaoVender; // Botão de vender (único)
    public TMP_InputField inputQuantidade; // Campo para inserir a quantidade de itens a vender
    public FeedbackPrata feedbackPrata; // Referência ao FeedbackPrata
    public UserDataTester userDataTester; // Referência ao UserDataTester

    private void Start()
    {
        CarregarItensNaMochila();

        // Configura o botão de vender
        if (botaoVender != null)
        {
            botaoVender.onClick.AddListener(VenderItemSelecionado);
        }
        else
        {
            Debug.LogError("Botão de vender não está configurado no PainelMochilaUI.");
        }
    }

    // Carrega os itens na mochila na UI
    public void CarregarItensNaMochila()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in gerenciadorMochila.itensNaMochila)
        {
            CriarItemNaMochila(item);
        }
    }

    // Cria um item na mochila na UI
    private void CriarItemNaMochila(ScriptableObject item)
    {
        if (itemPrefab != null && content != null)
        {
            GameObject itemObj = Instantiate(itemPrefab, content);
            itemObj.transform.SetParent(content, false);

            int quantidade = gerenciadorMochila.ObterQuantidadeItem(item);
            ConfigurarItemPrefab(itemObj, item, quantidade);

            // Configura o botão de seleção
            Button botaoItem = itemObj.GetComponent<Button>();
            if (botaoItem != null)
            {
                ButtonMochila buttonMochila = itemObj.GetComponent<ButtonMochila>();
                if (buttonMochila == null)
                {
                    buttonMochila = itemObj.AddComponent<ButtonMochila>();
                }
                buttonMochila.Configurar(item, gerenciadorMochila, itemObj);
                botaoItem.onClick.AddListener(buttonMochila.SelecionarItem);
            }
            else
            {
                Debug.LogError("Botão não encontrado no ItemPrefab.");
            }
        }
        else
        {
            Debug.LogError("ItemPrefab ou Content não está configurado no PainelMochilaUI.");
        }
    }

    // Vende o item selecionado
    private async void VenderItemSelecionado()
    {
        ScriptableObject itemSelecionado = gerenciadorMochila.ObterItemSelecionado();
        if (itemSelecionado != null)
        {
            if (int.TryParse(inputQuantidade.text, out int quantidade) && quantidade > 0)
            {
                gerenciadorMochila.VenderQuantidade(itemSelecionado, quantidade);

                // Calcula o valor das pratas recebidas
                int prata = gerenciadorMochila.CalcularPrata(itemSelecionado, quantidade);

                // Adiciona as pratas ao menu do usuário
                if (userDataTester != null)
                {
                    await userDataTester.AdicionarPrata(prata);
                }
                else
                {
                    Debug.LogError("UserDataTester não está configurado no PainelMochilaUI.");
                }

                // Exibe o feedback visual
                if (feedbackPrata != null)
                {
                    feedbackPrata.MostrarFeedback(prata);
                }
                else
                {
                    Debug.LogError("FeedbackPrata não está configurado no PainelMochilaUI.");
                }

                CarregarItensNaMochila(); // Recarrega os itens na mochila
            }
            else
            {
                Debug.LogError("Quantidade inválida. Insira um número maior que 0.");
            }
        }
        else
        {
            Debug.LogError("Nenhum item selecionado para vender.");
        }
    }


    // Configura os componentes de UI do ItemPrefab
    private void ConfigurarItemPrefab(GameObject itemObj, ScriptableObject item, int quantidade)
    {
        Transform imagemRostoTransform = itemObj.transform.Find("ImagemRosto");
        Transform nomeTransform = itemObj.transform.Find("NomeItem");
        Transform nivelTransform = itemObj.transform.Find("Nivel");
        Transform ataqueTransform = itemObj.transform.Find("Ataque");
        Transform hpTransform = itemObj.transform.Find("HP");

        if (imagemRostoTransform == null || nomeTransform == null || nivelTransform == null || ataqueTransform == null || hpTransform == null)
        {
            Debug.LogError("Um ou mais objetos de UI não foram encontrados no ItemPrefab.");
            return;
        }

        Image fundoRostoLaranja = imagemRostoTransform.GetComponent<Image>();
        TextMeshProUGUI nome = nomeTransform.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI nivel = nivelTransform.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI ataqueBase = ataqueTransform.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI hpBase = hpTransform.GetComponent<TextMeshProUGUI>();

        if (fundoRostoLaranja == null || nome == null || nivel == null || ataqueBase == null || hpBase == null)
        {
            Debug.LogError("Um ou mais componentes de UI não foram encontrados no ItemPrefab.");
            return;
        }

        ConfigurarItem(fundoRostoLaranja, nome, nivel, ataqueBase, hpBase, item, quantidade);
    }

    // Configura os dados do item na UI
    private void ConfigurarItem(Image imagemRosto, TextMeshProUGUI nomeItem, TextMeshProUGUI nivel, TextMeshProUGUI ataque, TextMeshProUGUI hp, ScriptableObject item, int quantidade)
    {
        string caminho = "";

        switch (item)
        {
            case PersonagemData personagem:
                caminho = "Dados/PersonagemData_" + personagem.nome;
                break;
            case ComidaData comida:
                caminho = "Itens/ComidaData_" + comida.nome;
                break;
            case PrataData prata:
                caminho = "Itens/PrataData_" + prata.nome;
                break;
            default:
                Debug.LogError("Tipo de item não reconhecido: " + item.GetType().Name);
                return;
        }

        ScriptableObject loadedItem = Resources.Load<ScriptableObject>(caminho);

        if (loadedItem != null)
        {
            switch (loadedItem)
            {
                case PersonagemData personagem:
                    ConfigurarPersonagem(imagemRosto, nomeItem, nivel, ataque, hp, personagem, quantidade);
                    break;
                case ComidaData comida:
                    ConfigurarComida(imagemRosto, nomeItem, nivel, ataque, hp, comida, quantidade);
                    break;
                case PrataData prata:
                    ConfigurarPrata(imagemRosto, nomeItem, nivel, ataque, hp, prata, quantidade);
                    break;
                default:
                    Debug.LogError("Tipo de ScriptableObject não reconhecido.");
                    break;
            }
        }
    }

    // Configura os dados de um personagem na UI
    private void ConfigurarPersonagem(Image imagemRosto, TextMeshProUGUI nomeItem, TextMeshProUGUI nivel, TextMeshProUGUI ataque, TextMeshProUGUI hp, PersonagemData personagem, int quantidade)
    {
        if (imagemRosto != null) imagemRosto.sprite = personagem.fundoRostoLaranja;
        if (nomeItem != null) nomeItem.text = $"{personagem.nome} ({quantidade})";
        if (nivel != null) nivel.text = $"Nv{personagem.nivel}";
        if (ataque != null) ataque.text = $"{personagem.ataqueBase}";
        if (hp != null) hp.text = $"{personagem.hpBase}";
    }

    // Configura os dados de uma comida na UI
    private void ConfigurarComida(Image imagemRosto, TextMeshProUGUI nomeItem, TextMeshProUGUI nivel, TextMeshProUGUI ataque, TextMeshProUGUI hp, ComidaData comida, int quantidade)
    {
        if (imagemRosto != null) imagemRosto.sprite = comida.fundoRostoLaranja;
        if (nomeItem != null) nomeItem.text = $"{comida.nome} ({quantidade})";
        if (nivel != null) nivel.text = "Nivel";
        if (ataque != null) ataque.text = $"{comida.ataqueBase}";
        if (hp != null) hp.text = $"{comida.hpBase}";
    }

    // Configura os dados de uma prata na UI
    private void ConfigurarPrata(Image imagemRosto, TextMeshProUGUI nomeItem, TextMeshProUGUI nivel, TextMeshProUGUI ataque, TextMeshProUGUI hp, PrataData prata, int quantidade)
    {
        if (imagemRosto != null) imagemRosto.sprite = prata.fundoRostoLaranja;
        if (nomeItem != null) nomeItem.text = $"{prata.nome} ({quantidade})";
        if (nivel != null) nivel.text = "Nivel";
        if (ataque != null) ataque.text = $"{prata.ataqueBase}";
        if (hp != null) hp.text = $"{prata.hpBase}";
    }
}