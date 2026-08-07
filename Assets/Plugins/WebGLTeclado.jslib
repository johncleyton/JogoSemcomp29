mergeInto(LibraryManager.library, {
    SolicitarNickMobile: function(mensagem) {
        // Converte a mensagem do C# para o JavaScript
        var strMsg = UTF8ToString(mensagem);
        
        // window.prompt força o celular a abrir a caixa de texto nativa com o teclado!
        var nick = window.prompt(strMsg, "");
        
        // Se o jogador não cancelou, enviamos de volta para a Unity
        if (nick !== null && nick !== "") {
            SendMessage('GoogleAuthManager', 'ReceberNickDoNavegador', nick);
        }
    }
});