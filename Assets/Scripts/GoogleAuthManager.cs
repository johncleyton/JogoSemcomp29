using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Text.RegularExpressions;

[System.Serializable]
public class GooglePayload
{
    public string name;
}

public class GoogleAuthManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ShowGoogleLoginButton();

    [DllImport("__Internal")]
    private static extern void HideGoogleLoginButton();

    async void Start()
    {
        await UnityServices.InitializeAsync();
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            ShowGoogleLoginButton();
        #else
            Debug.LogWarning("O login do Google WebGL só funciona após o Build.");
        #endif
    }

    public async void OnGoogleLoginSuccess(string idToken)
    {
        Debug.Log("Token recebido do JavaScript!");
        
        try
        {
            await AuthenticationService.Instance.SignInWithGoogleAsync(idToken);
            Debug.Log("Login no UGS feito com sucesso! PlayerID: " + AuthenticationService.Instance.PlayerId);
            
            await AtualizarNomeDoJogador(idToken);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError("Erro ao logar no UGS: " + ex.Message);
        }
    }

    private async Task AtualizarNomeDoJogador(string token)
    {
        try
        {
            string[] partes = token.Split('.');
            if (partes.Length > 1)
            {
                string base64Payload = partes[1].Replace('-', '+').Replace('_', '/');
                switch (base64Payload.Length % 4)
                {
                    case 2: base64Payload += "=="; break;
                    case 3: base64Payload += "="; break;
                }

                string json = Encoding.UTF8.GetString(Convert.FromBase64String(base64Payload));
                GooglePayload dadosGoogle = JsonUtility.FromJson<GooglePayload>(json);
                string nomeOriginal = dadosGoogle.name;
                string nomeFormatado = Regex.Replace(nomeOriginal.Replace(" ", "_"), "[^a-zA-Z0-9_\\-]", "");
                
                if (nomeFormatado.Length > 50) 
                    nomeFormatado = nomeFormatado.Substring(0, 50); // Limite máximo da Unity

                await AuthenticationService.Instance.UpdatePlayerNameAsync(nomeFormatado);
                Debug.Log($"Nome atualizado com sucesso no banco de dados para: {nomeFormatado}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao atualizar nome: " + e.Message);
        }
    }
}