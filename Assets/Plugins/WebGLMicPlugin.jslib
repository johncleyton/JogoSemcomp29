mergeInto(LibraryManager.library, {

    InitWebGLMic: function () {
        // Verifica se o navegador suporta microfone
        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            navigator.mediaDevices.getUserMedia({ audio: true })
                .then(function (stream) {
                    // Cria o contexto de áudio do HTML5
                    window.micAudioCtx = new (window.AudioContext || window.webkitAudioContext)();
                    window.micAnalyser = window.micAudioCtx.createAnalyser();
                    window.micAnalyser.fftSize = 256; // Tamanho rápido da amostra
                    
                    var source = window.micAudioCtx.createMediaStreamSource(stream);
                    source.connect(window.micAnalyser);
                    window.micDataArray = new Uint8Array(window.micAnalyser.frequencyBinCount);
                    
                    console.log("Microfone WebGL inicializado com sucesso!");
                })
                .catch(function (err) {
                    console.error("Erro ao acessar o microfone no navegador: " + err);
                });
        } else {
            console.error("getUserMedia não suportado neste navegador.");
        }
    },

    GetWebGLLoudness: function () {
        // Se ainda não carregou, retorna 0 de volume
        if (!window.micAnalyser || !window.micDataArray) return 0.0;

        // Pega os dados brutos da onda de áudio do microfone
        window.micAnalyser.getByteTimeDomainData(window.micDataArray);

        var total = 0;
        for (var i = 0; i < window.micDataArray.length; i++) {
            // Em Javascript, o silêncio é 128. Vamos normalizar para ficar entre 0 e 1 (igual no C#)
            var sample = (window.micDataArray[i] - 128) / 128.0;
            total += Math.abs(sample);
        }

        return total / window.micDataArray.length;
    }
});