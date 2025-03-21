using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Firebase.Firestore;
using System.Threading.Tasks;

public class PainelNinja : MonoBehaviour
{
    [Header("Configurações")]
    public Transform gridRostos; // Grid onde os rostos serão exibidos 
    public GameObject fundoRostoPrefab; // Prefab da carta do personagem

    [Header("Referências")]
    public PainelSelecao painelSelecao; // Referência ao PainelSelecao
    public SincronizadorFirestore sincronizadorFirestore; // Referência ao SincronizadorFirestore

    private List<PersonagemData> personagensDoUsuario = new List<PersonagemData>(); // Lista de personagens do usuário
    private const int MAX_PERSONAGENS = 5; // Limite de personagens no PainelNinja

    private void Start()
    {
        CarregarDadosEAtualizarCena();
    }

    private async void CarregarDadosEAtualizarCena()
    {
        await sincronizadorFirestore.CarregarDadosIniciais();

        personagensDoUsuario = sincronizadorFirestore.ObterCacheLocal().personagens; // Atualiza a lista de personagens do usuário

        CarregarRostosDosPersonagens();

        if (painelSelecao != null) // Notifica o PainelSelecao para carregar os personagens atualizados
        {
            painelSelecao.CarregarPersonagens(personagensDoUsuario);
        }
    }

    public async void AdicionarPersonagemAoUsuario(string userId, PersonagemData personagem)
    {
        await sincronizadorFirestore.SalvarPersonagemNaNuvem(userId, personagem);

        personagensDoUsuario.Add(personagem);    // Adiciona à lista de personagens do usuário

        CarregarRostosDosPersonagens();     // Recarrega os rostos dos personagens

        if (painelSelecao != null)      // Notifica o PainelSelecao para carregar os personagens atualizados

        {
            painelSelecao.CarregarPersonagens(personagensDoUsuario);
        }
    }

    public void AtualizarPersonagens(List<PersonagemData> novosPersonagens)     // Método para atualizar a lista de personagens e recarregar a exibição
    {
        personagensDoUsuario = novosPersonagens;        // Atualiza a lista de personagens do usuário

        CarregarRostosDosPersonagens();        // Recarrega os rostos dos personagens

    }

    public void CarregarRostosDosPersonagens()
    {
        foreach (Transform child in gridRostos)        // Limpa o grid antes de carregar novos personagens

        {
            Destroy(child.gameObject);
        }

        int personagensParaMostrar = Mathf.Min(personagensDoUsuario.Count, MAX_PERSONAGENS);        // Garante que apenas os 5 primeiros personagens sejam exibidos

        for (int i = 0; i < personagensParaMostrar; i++)
        {
            CriarCartaPersonagem(personagensDoUsuario[i]);
        }
    }

    private void CriarCartaPersonagem(PersonagemData personagem)
    {
        if (fundoRostoPrefab == null)        // Verifica se o prefab e o grid estão atribuídos
        {
            Debug.LogError("O prefab fundoRostoPrefab não está atribuído!");
            return;
        }

        if (gridRostos == null)
        {
            Debug.LogError("O gridRostos não está atribuído!");
            return;
        }

        GameObject carta = Instantiate(fundoRostoPrefab, gridRostos);

        Image imagemRosto = carta.GetComponentInChildren<Image>();
        if (imagemRosto != null)
        {
            Sprite rosto = CarregarRostoPeloNome(personagem.nome);
            if (rosto != null)
            {
                imagemRosto.sprite = rosto;
            }
            else
            {
                Debug.LogError($"Imagem do rosto não encontrada para o personagem: {personagem.nome}");
            }
        }
        else
        {
            Debug.LogError("Componente Image não encontrado no prefab da carta!");
        }
    }

    private Sprite CarregarRostoPeloNome(string nomePersonagem)
    {
        string nomeImagem = $"Rosto_{nomePersonagem}";
        Sprite rosto = Resources.Load<Sprite>($"Dados/{nomeImagem}");
        if (rosto == null)
        {
            Debug.LogError($"Imagem do rosto não encontrada: {nomeImagem}");
        }
        return rosto;
    }
}