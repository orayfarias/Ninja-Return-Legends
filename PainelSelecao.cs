using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Firebase.Firestore;
using Firebase.Auth;
using System.Threading.Tasks;

public class PainelSelecao : MonoBehaviour
{
    [Header("Configurações")]
    public Transform content; // Content onde os personagens serão instanciados
    public GameObject prefabRosto; // Prefab para exibir os personagens
    public Button botaoConfirmar; // Botão para confirmar a seleção

    [Header("Referências")]
    public PainelNinja painelNinja; // Referência ao PainelNinja
    public SincronizadorFirestore sincronizadorFirestore; // Referência ao SincronizadorFirestore

    private Dictionary<Button, PersonagemData> botaoParaPersonagem = new Dictionary<Button, PersonagemData>();
    private List<PersonagemData> personagensDoUsuario = new List<PersonagemData>(); // Lista de personagens do usuário
    private List<PersonagemData> personagensSelecionados = new List<PersonagemData>(); // Lista de personagens selecionados

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        botaoConfirmar.onClick.AddListener(ConfirmarSelecao);

        CarregarPersonagensDoFirestore();

    }

    private async void CarregarPersonagensDoFirestore()
    {
        Debug.Log("Iniciando CarregarPersonagensDoFirestore...");

        if (auth.CurrentUser == null)
        {
            Debug.LogWarning("Usuário não autenticado!");
            return;
        }

        await sincronizadorFirestore.CarregarDadosIniciais();

        List<PersonagemData> personagens = sincronizadorFirestore.ObterCacheLocal().personagens;

        if (personagens == null || personagens.Count == 0)
        {
            Debug.LogError("Nenhum personagem carregado do Firestore!");
            return;
        }

        personagensDoUsuario = personagens;

        Debug.Log($"Personagens carregados do Firestore: {personagens.Count}");

        await CarregarSelecaoAnterior(); // Só carrega a seleção se os personagens já foram carregados
        Debug.Log("Seleção anterior carregada.");

        CarregarPersonagens(personagens);
        Debug.Log("Personagens instanciados na UI.");

        AtualizarUI();
        Debug.Log("UI atualizada com sucesso");

    }


    private async Task CarregarSelecaoAnterior()
    {
        Debug.Log("Iniciando CarregarSelecaoAnterior...");

        string userId = auth.CurrentUser.UserId;
        DocumentReference userRef = db.Collection("users").Document(userId);

        DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();

        if (!snapshot.Exists || !snapshot.ContainsField("personagensSelecionados"))
        {
            Debug.LogWarning("Nenhuma seleção anterior encontrada no Firestore.");
            return;
        }

        List<string> idsSelecionados = snapshot.GetValue<List<string>>("personagensSelecionados");
        personagensSelecionados.Clear();
        Debug.Log($"IDs dos personagens selecionados carregados: {string.Join(", ", idsSelecionados)}");

        List<PersonagemData> personagens = sincronizadorFirestore.ObterCacheLocal().personagens;

        if (personagens == null || personagens.Count == 0)
        {
            Debug.LogError("Erro: Lista de personagens no cache local está vazia.");
            return;
        }

        foreach (string id in idsSelecionados)
        {
            PersonagemData personagem = personagens.Find(p => p.personagemId == id);

            if (personagem != null)
            {
                personagensSelecionados.Add(personagem);
            }
            else
            {
                Debug.LogError($"Personagem com ID {id} não encontrado no cache local.");
            }
        }

        Debug.Log($"Personagens selecionados carregados: {personagensSelecionados.Count}");

        AtualizarUI();
    }

    public void CarregarPersonagens(List<PersonagemData> personagens)
    {
        Debug.Log("Iniciando CarregarPersonagens...");

        if (personagens == null || personagens.Count == 0)
        {
            Debug.LogError("Lista de personagens vazia ou nula!");
            return;
        }

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        foreach (PersonagemData personagem in personagens)
        {
            CriarEntradaPersonagem(personagem);
        }
        AtualizarUI();
    }
    private void CriarEntradaPersonagem(PersonagemData personagem)
    {
        Debug.Log($"Criando entrada para personagem: {personagem.nome} (ID: {personagem.personagemId})");

        GameObject entrada = Instantiate(prefabRosto, content);
        AtualizarDadosPersonagem(entrada, personagem);

        Button botaoSelecao = entrada.GetComponent<Button>();
        if (botaoSelecao != null)
        {
            Debug.Log($"Botão encontrado para o personagem: {personagem.nome}");
            botaoSelecao.onClick.AddListener(() => SelecionarPersonagem(personagem));

            botaoParaPersonagem[botaoSelecao] = personagem; // Armazena o PersonagemData associado ao botão
        }
        else
        {
            Debug.LogError($"Botão não encontrado no prefab para o personagem: {personagem.nome}");
        }
        AtualizarUI();
    }
    private void AtualizarDadosPersonagem(GameObject entrada, PersonagemData personagem)
    {
        if (entrada == null || personagem == null)
        {
            Debug.LogError("Entrada ou personagem nulo!");
            return;
        }

        Transform imagemRostoTransform = entrada.transform.Find("ImagemRosto");
        if (imagemRostoTransform == null)
        {
            return;
        }

        Image imagemRosto = imagemRostoTransform.GetComponent<Image>();
        if (imagemRosto == null)
        {
            return;
        }

        Sprite rosto = Resources.Load<Sprite>($"Dados/Rosto_{personagem.nome}");
        if (rosto != null)
        {
            imagemRosto.sprite = rosto;
        }


        Transform nomePersonagemTransform = entrada.transform.Find("NomePersonagem");
        if (nomePersonagemTransform != null)
        {
            TextMeshProUGUI nomePersonagem = nomePersonagemTransform.GetComponent<TextMeshProUGUI>();
            if (nomePersonagem != null)
            {
                nomePersonagem.text = personagem.nome;
            }
        }
        Transform imagemElementoTransform = entrada.transform.Find("ImagemElemento");
        if (imagemElementoTransform != null)
        {
            Image imagemElemento = imagemElementoTransform.GetComponent<Image>();
            if (imagemElemento != null && personagem.imagemElemento != null)
            {
                imagemElemento.sprite = personagem.imagemElemento;
            }
        }
        Transform nivelTransform = entrada.transform.Find("Nivel");
        if (nivelTransform != null)
        {
            TextMeshProUGUI nivel = nivelTransform.GetComponent<TextMeshProUGUI>();
            if (nivel != null)
            {
                nivel.text = $"Nv {personagem.nivel}";
            }
        }

        Transform hpTransform = entrada.transform.Find("HP");
        if (hpTransform != null)
        {
            TextMeshProUGUI hp = hpTransform.GetComponent<TextMeshProUGUI>();
            if (hp != null)
            {
                hp.text = $" {personagem.hpBase}";
            }
        }

        Transform ataqueTransform = entrada.transform.Find("Ataque");
        if (ataqueTransform != null)
        {
            TextMeshProUGUI ataque = ataqueTransform.GetComponent<TextMeshProUGUI>();
            if (ataque != null)
            {
                ataque.text = $" {personagem.ataqueBase}";
            }
        }
    }

    private void SelecionarPersonagem(PersonagemData personagem)
    {
        if (personagem == null)
        {
            Debug.LogError("[SelecionarPersonagem] Erro: O personagem passado para a função é NULL! Não foi iniciado corretamente.");
            return;
        }

        Debug.Log($"[SelecionarPersonagem] Personagem clicado: {personagem.nome} ({personagem.personagemId})");

        if (personagensSelecionados.Contains(personagem))
        {
            personagensSelecionados.Remove(personagem);
            Debug.Log($"[SelecionarPersonagem] {personagem.nome} removido da seleção.");
        }
        else
        {
            if (personagensSelecionados.Count >= 5)
            {
                Debug.LogWarning("[SelecionarPersonagem] Limite de 5 personagens selecionados atingido!");
                return;
            }

            personagensSelecionados.Add(personagem);
            Debug.Log($"[SelecionarPersonagem] {personagem.nome} adicionado à seleção.");
        }

        Debug.Log($"[SelecionarPersonagem] Total de personagens selecionados: {personagensSelecionados.Count}");

        AtualizarUI();
    }


    private async void ConfirmarSelecao()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogWarning("Usuário não autenticado!");
            return;
        }

        if (personagensSelecionados.Count == 0)
        {
            Debug.LogWarning("Nenhum personagem selecionado!");
            return;
        }

        string userId = auth.CurrentUser.UserId;

        DocumentReference userRef = db.Collection("users").Document(userId);

        List<string> idsSelecionados = new List<string>(); // Salva a lista de IDs dos personagens selecionados
        foreach (PersonagemData personagem in personagensSelecionados)
        {
            if (personagem != null)
            {
                idsSelecionados.Add(personagem.personagemId);
            }
        }

        try
        {
            await userRef.UpdateAsync("personagensSelecionados", idsSelecionados);
            Debug.Log("Personagens selecionados salvos no Firestore com sucesso!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Erro ao salvar personagens selecionados no Firestore: {ex.Message}");
        }


        if (painelNinja != null) // Notifica o PainelNinja para exibir os personagens selecionados
        {
            painelNinja.AtualizarPersonagens(personagensSelecionados);
            Debug.Log("Personagens selecionados enviados ao PainelNinja!");
        }
        else
        {
            Debug.LogError("PainelNinja não atribuído!");
        }
        AtualizarUI();
    }
    private void AtualizarUI()
    {
        Debug.Log("Atualizando UI com personagens selecionados...");

        foreach (Transform child in content)
        {
            Button botao = child.GetComponent<Button>();
            if (botao != null)
            {
                if (botaoParaPersonagem.TryGetValue(botao, out PersonagemData personagem)) // Obtém o Personagem associado ao Botão.
                {
                    Debug.Log($"Verificando personagem: {personagem.nome} (ID: {personagem.personagemId})");

                    Transform selecaoTransform = child.Find("ImagemSelecao"); // Nome do objeto da imagem de seleção
                    if (selecaoTransform != null)
                    {
                        Image imagemSelecao = selecaoTransform.GetComponent<Image>();
                        if (imagemSelecao != null)
                        {
                            bool estaSelecionado = personagensSelecionados.Contains(personagem);
                            Debug.Log($"Personagem {personagem.nome} está selecionado? {estaSelecionado}");
                            imagemSelecao.gameObject.SetActive(estaSelecionado);
                        }
                        else
                        {
                            Debug.LogError($"Componente Image não encontrado em ImagemSelecao para o personagem: {personagem.nome}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Objeto ImagemSelecao não encontrado para o personagem: {personagem.nome}");
                    }
                }
                else
                {
                    Debug.LogError($"PersonagemData não encontrado para o botão: {botao.name}");
                }
            }
            else
            {
                Debug.LogError($"Botão não encontrado no objeto: {child.name}");
            }
        }
    }

    public void AbrirComTodosPersonagens(List<PersonagemData> personagens)
    {
        if (personagens == null || personagens.Count == 0)
        {
            Debug.LogError("Lista de personagens vazia ou nula!");
            return;
        }

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (PersonagemData personagem in personagens)         // Itera sobre a lista de personagens e cria uma entrada para cada um
        {
            CriarEntradaPersonagem(personagem);
        }
        gameObject.SetActive(true);
    }
}