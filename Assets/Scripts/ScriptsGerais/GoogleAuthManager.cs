using UnityEngine;
using UnityEngine.Networking;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;

#if UNITY_STANDALONE || UNITY_EDITOR
using System.Net;
#endif

[System.Serializable]
public class GooglePayload 
{ 
    public string name; 
    public string email;
}

[System.Serializable]
public class TokenResponsePC { public string id_token; }

public class GoogleAuthManager : MonoBehaviour
{

    [Header("Referências de UI")]
    public GameObject botaoGeral;
    public GameObject painelLogin;
    public GameObject painelCriarNick;
    public GameObject painelMenuPrincipal;
    public TMP_InputField campoInputNick;


    /*private string logNaTela = "=== DEBUG INICIADO ===\n";
    private Vector2 scrollPosition;

    void OnEnable() { Application.logMessageReceived += CapturarLog; }
    void OnDisable() { Application.logMessageReceived -= CapturarLog; }

    private void CapturarLog(string logString, string stackTrace, LogType type)
    {
        logNaTela = $"[{DateTime.Now:HH:mm:ss}] {logString}\n" + logNaTela;
        if (logNaTela.Length > 5000) logNaTela = logNaTela.Substring(0, 5000);
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.textArea);
        style.fontSize = 24;
        style.normal.textColor = Color.green;

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height / 2));
        GUILayout.TextArea(logNaTela, style, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
    }*/

    #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] 
            private static extern void ShowGoogleLoginButton();
        [DllImport("__Internal")] 
            private static extern void HideGoogleLoginButton();
    #endif
    #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SolicitarNickMobile(string mensagem);
    #endif

    // Essa função será chamada quando o jogador tocar no campo de texto
    public void AbrirTecladoMobile()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        // Verifica se a página está rodando em um celular (Android/iOS)
        if (Application.isMobilePlatform)
        {
            SolicitarNickMobile("Digite seu nick (máx 20 letras):");
        }
        #endif
    }

    [UnityEngine.Scripting.Preserve]
    public void ReceberNickDoNavegador(string nick)
    {
        if (campoInputNick != null)
            campoInputNick.text = nick;
    }

    private void Awake()
    {
        Application.deepLinkActivated += OnDeepLinkActivated;
    }

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("UGS Inicializado com sucesso.");
            
            // Verifica se o jogador já estava logado na sessão anterior
            if (AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Sessão recuperada. Entrando direto no jogo.");
                MudarParaTela(painelMenuPrincipal);
            }
            else
            {
                // Inicia o jogo no Menu Principal padrão
                MudarParaTela(painelMenuPrincipal);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Erro UGS: " + e.Message);
        }

        #if UNITY_WEBGL && !UNITY_EDITOR
            // Esconde o botão HTML logo de cara, pois o jogo começa no Menu Principal
            HideGoogleLoginButton();
            if(botaoGeral != null) botaoGeral.SetActive(false);
        #endif

        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            Debug.Log("App aberto com URL absoluta: " + Application.absoluteURL);
            OnDeepLinkActivated(Application.absoluteURL);
        }
    }

    public void AbrirTelaDeLogin()
    {
        MudarParaTela(painelLogin);
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            ShowGoogleLoginButton();
        #endif
    }
    
    public void FecharTelaDeLogin()
    {
        MudarParaTela(painelMenuPrincipal);
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            HideGoogleLoginButton();
        #endif
    }

    // Função auxiliar para garantir que apenas uma tela fique ativa por vez
    private void MudarParaTela(GameObject telaAtiva)
    {
        if (painelMenuPrincipal != null) 
            painelMenuPrincipal.SetActive(painelMenuPrincipal == telaAtiva);
        if (painelLogin != null) 
            painelLogin.SetActive(painelLogin == telaAtiva);
        if (painelCriarNick != null) 
            painelCriarNick.SetActive(painelCriarNick == telaAtiva);
    }

    public void FazerLoginNativo()
    {
        #if UNITY_EDITOR || UNITY_STANDALONE
            _ = FazerLoginPC();
        #elif UNITY_ANDROID || UNITY_IOS
            FazerLoginMobileDeepLink();
        #else
            Debug.LogWarning("Plataforma não configurada para login.");
        #endif
    }

    public void OnGoogleLoginSuccess(string token)
    {
        _ = ProcessarLoginUnity(token);
    }

    private void FazerLoginMobileDeepLink()
    {
        Debug.Log("1. Botão clicado. Abrindo navegador...");
        string redirectUri = "https://johncleyton.github.io/"; 
        string authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientID_Web}&redirect_uri={redirectUri}&response_type=code&scope=openid profile email";
        Application.OpenURL(authUrl);
    }

    private async void OnDeepLinkActivated(string url)
    {
        Debug.Log("2. Deep Link Interceptado: " + url);
        
        if (url.Contains("meujogo://oauth"))
        {
            string code = GetQueryParam(url, "code");
            if (!string.IsNullOrEmpty(code))
            {
                Debug.Log("3. Código extraído com sucesso.");
                await TrocarCodigoPorTokenWeb(code, "https://johncleyton.github.io/");
            }
        }
    }

    private string GetQueryParam(string url, string paramName)
    {
        Regex regex = new Regex($@"[?&]{paramName}=([^&]+)");
        Match match = regex.Match(url);
        if (match.Success) return UnityWebRequest.UnEscapeURL(match.Groups[1].Value);
        return null;
    }

    #if UNITY_EDITOR || UNITY_STANDALONE
    private async Task FazerLoginPC()
    {
        Debug.Log("Abrindo navegador no PC para login...");
        Application.runInBackground = true; 
        
        string redirectUri = "http://127.0.0.1:52000/"; 
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(redirectUri);
        listener.Start();

        string authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientID_Web}&redirect_uri={redirectUri}&response_type=code&scope=openid profile email";
        Application.OpenURL(authUrl);

        HttpListenerContext context;
        string codigoAutorizacao = null;

        while (true)
        {
            context = await listener.GetContextAsync();
            if (context.Request.Url.AbsolutePath != "/favicon.ico")
            {
                codigoAutorizacao = context.Request.QueryString["code"];
                if (!string.IsNullOrEmpty(codigoAutorizacao)) break; 
            }
            context.Response.StatusCode = 404;
            context.Response.Close();
        }

        string html = @"
            <html>
                <head><script>setTimeout(function(){ window.close(); }, 1500);</script></head>
                <body style='text-align:center; font-family:sans-serif; margin-top:100px; background-color:#222; color:white;'>
                    <h1>Login Concluido!</h1><p>Voltando para o jogo...</p>
                </body>
            </html>";
            
        byte[] buffer = Encoding.UTF8.GetBytes(html);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.Close();
        listener.Stop();

        await TrocarCodigoPorTokenWeb(codigoAutorizacao, redirectUri);
    }
    #endif

    private async Task TrocarCodigoPorTokenWeb(string code, string redirectUri)
    {
        Debug.Log("4. Trocando código por token com o Google...");
        
        WWWForm form = new WWWForm();
        form.AddField("code", code);
        form.AddField("client_id", clientID_Web);
        form.AddField("client_secret", clientSecret_Web); 
        form.AddField("redirect_uri", redirectUri);
        form.AddField("grant_type", "authorization_code");

        using (UnityWebRequest www = UnityWebRequest.Post("https://oauth2.googleapis.com/token", form))
        {
            var op = www.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("5. Token recebido com sucesso!");
                TokenResponsePC json = JsonUtility.FromJson<TokenResponsePC>(www.downloadHandler.text);
                await ProcessarLoginUnity(json.id_token);
            }
            else
            {
                Debug.LogError("ERRO FATAL (Google): " + www.error);
            }
        }
    }

    private async Task ProcessarLoginUnity(string tokenGoogle)
    {
        Debug.Log("6. Autenticando no UGS...");
        try
        {
            await AuthenticationService.Instance.SignInWithGoogleAsync(tokenGoogle);
            Debug.Log("7. SUCESSO UGS! PlayerID: " + AuthenticationService.Instance.PlayerId);

            #if UNITY_WEBGL && !UNITY_EDITOR
                HideGoogleLoginButton();
            #endif
            
            string[] partes = tokenGoogle.Split('.');
            if (partes.Length > 1)
            {
                string b64 = partes[1].Replace('-', '+').Replace('_', '/');
                switch (b64.Length % 4) { case 2: b64 += "=="; break; case 3: b64 += "="; break; }
                string json = Encoding.UTF8.GetString(Convert.FromBase64String(b64));
                GooglePayload dadosDoUsuario = JsonUtility.FromJson<GooglePayload>(json);

                if (!string.IsNullOrEmpty(dadosDoUsuario.email))
                {
                    _ = SalvarEmailNaNuvem(dadosDoUsuario.email);
                }
            }

            bool ehPrimeiroLogin = await VerificarSeEhPrimeiroLogin();

            if (ehPrimeiroLogin)
            {
                Debug.Log("Usuário novato! Indo para tela de Nick.");
                MudarParaTela(painelCriarNick);
            }
            else
            {
                Debug.Log("Usuário veterano. Entrando no jogo.");
                MudarParaTela(painelMenuPrincipal);
            }
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError("ERRO UGS: " + ex.Message);
        }
    }

    public async void ConfirmarNickCustomizado()
    {
        string nickDigitado = campoInputNick.text;

        if (string.IsNullOrWhiteSpace(nickDigitado)) return;

        try
        {
            string nomeFormatado = Regex.Replace(nickDigitado.Replace(" ", "_"), "[^a-zA-Z0-9_\\-]", "");
            if (nomeFormatado.Length > 20) nomeFormatado = nomeFormatado.Substring(0, 20);
            
            await AuthenticationService.Instance.UpdatePlayerNameAsync(nomeFormatado);
            Debug.Log($"Nick salvo no servidor: {nomeFormatado}");
            
            var dados = new Dictionary<string, object> 
            { 
                { "ja_escolheu_nick", true },
                { "nick_jogador", nomeFormatado }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(dados);
            
            Debug.Log("Cadastro finalizado. Retornando ao menu principal.");
            MudarParaTela(painelMenuPrincipal);
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao registrar o nick: " + e.Message);
        }
    }

    private async Task SalvarEmailNaNuvem(string emailDoJogador)
    {
        try
        {
            var dadosParaSalvar = new Dictionary<string, object> { { "contato_email", emailDoJogador } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(dadosParaSalvar);
        }
        catch (Exception e) {}
    }

    private async Task<bool> VerificarSeEhPrimeiroLogin()
    {
        try
        {
            var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "ja_escolheu_nick" });
            return !playerData.ContainsKey("ja_escolheu_nick");
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public void DeslogarUsuario()
    {
        if(AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut(true);
            Debug.Log("Usuário deslogado com sucesso.");
            
            MudarParaTela(painelMenuPrincipal);
        }
    }
}