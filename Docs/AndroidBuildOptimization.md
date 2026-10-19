# Otimização da build Android

Projeto: Unity 2022.3.62f3. Diagnóstico em 17/10/2026.

## Resultado medido

| Arquivo | Antes | Depois |
| --- | ---: | ---: |
| APK completo | 131.548.536 bytes (131,55 MB) | 77.302.524 bytes (77,30 MB) |
| `data.unity3d` | 76.718.472 bytes | 22.474.140 bytes |

**Redução do APK: 54.246.012 bytes (41,24%).** Comparação com o APK local
anterior; a nova build de validação contém as mesmas 38 cenas habilitadas e
as duas arquiteturas, ARMv7 e ARM64. A assinatura desta build é de debug.

Build concluída com sucesso e integridade ZIP verificada. O APK original foi
preservado, conferido pelo SHA-256, e a configuração da chave de publicação
foi restaurada. Não foi realizado teste de jogabilidade/visual em aparelho.

Arquivos locais: `Builds/Android/SemcompJogo-validacao.apk`,
`Builds/Android/SemcompJogo-validacao.apk.report.json` e
`Builds/Android/comparison.json`. O tamanho é medido diretamente no arquivo,
pois `BuildReport.summary.totalSize` incluiu outros artefatos nesta execução.

## Ponto de partida

O APK local `SemcompJogo.apk` tinha **131.548.536 bytes** (131,55 MB / 125,45 MiB).
O relatório da build anterior registrava **817,9 MiB de texturas** antes da
compressão do pacote, 98,8% do tamanho dos assets. Esse número não representa
o tamanho do download nem a memória simultaneamente usada pelo jogo.

No ZIP do APK, `data.unity3d` ocupava 73,16 MiB; as bibliotecas Unity/IL2CPP
das duas arquiteturas ocupavam aproximadamente 43,2 MiB. ARMv7 e ARM64 foram
mantidos para preservar a compatibilidade existente.

## Alterações

- Remoção de `com.unity.visualscripting` e de 12 imports C# desnecessários.
  A análise de GUIDs não encontrou referências aos scripts desse pacote nos
  arquivos serializados de cenas, prefabs, assets e controllers.
- Overrides Android para sprites usados pelas cenas habilitadas/Resources:
  ETC2 RGBA com Crunch, qualidade 100. As imagens originais, resoluções,
  recortes, pivôs, animações e configurações das outras plataformas são mantidos.
- Texturas com Read/Write, overrides Android existentes ou dimensões importadas
  incompatíveis com blocos de 4 pixels são preservadas. Isso inclui a textura
  de pintura do Pixe e o cursor legível pelo código.
- Ferramenta de build release com LZ4HC e relatório JSON de tamanho por asset.
  LZ4HC não altera a qualidade dos assets. ETC2/Crunch é compressão com perdas,
  mesmo com qualidade 100: a aparência deve ser conferida em um aparelho.

Áudio, fontes, cenas, autenticação, serviços online, APIs gráficas e níveis de
stripping não foram alterados. Assets comuns sem referência, fora de Resources
e StreamingAssets, já são excluídos normalmente pela Unity; apagá-los do projeto
não reduziria esse APK.

## Gerar as próximas builds

1. Em Build Settings, selecione Android e faça Switch Platform se necessário.
2. Para novos sprites, use **Semcomp > Build > Otimizar texturas Android**.
   O comando considera as dependências das cenas habilitadas e de Resources;
   respeita overrides já definidos e pode ser executado novamente.
3. Configure a assinatura habitual em Player Settings e use
   **Semcomp > Build > APK release otimizado**.
4. O APK será salvo em `Builds/Android/Jeitinho Brasileiro.apk`, acompanhado de
   `Jeitinho Brasileiro.apk.report.json`. O campo `apkBytes` é o tamanho final;
   `assets[].packedBytes` representa os dados dos assets antes da compressão
   final do pacote e não deve ser somado para estimar o tamanho do APK.

Para uma validação local sem a senha da chave de publicação, execute a Unity
em batch com `-buildTarget Android -executeMethod AndroidBuildOptimization.ValidateBuild`.
Essa rotina aplica os overrides e gera `Builds/Android/Jeitinho Brasileiro-validacao.apk`
com assinatura de debug, restaurando a opção de assinatura original ao terminar.
Esse arquivo não é uma atualização assinada para distribuição.

Antes de distribuir, confira transparências e contornos dos sprites, textos
incorporados nas imagens, pintura no Pixe, áudio/ritmo dos minigames e login/ranking.

## Verificações realizadas

- 255 texturas otimizadas e 84 preservadas pela rotina.
- Comparação dos 255 `.meta` com a versão anterior: nenhuma mudança fora de
  `platformSettings`; GUIDs, recortes, pivôs e demais dados dos sprites intactos.
  Todos os overrides existentes de outras plataformas também foram preservados.
- Nenhum arquivo de imagem, áudio, fonte, cena ou animação alterado.
- Remoção exclusiva do pacote Visual Scripting, sem atualizações dos outros pacotes;
  nos 12 scripts existentes, a única alteração foi retirar o import desse pacote.
- O arquivo `data.unity3d` passou de 76.718.472 para 22.474.140 bytes. O tamanho
  final do APK também inclui bibliotecas nativas, áudio e outros arquivos.

O log registra avisos de scripts ausentes em `CanvasHUD` de `TelaIntervalo`
e em `Canvas` de `FloresNoMar`; ambos já constavam nos logs das builds anteriores.
Também há avisos de variáveis não usadas no `GoogleAuthManager` e de probes de
iluminação durante a execução sem interface gráfica. A validação em aparelho
continua necessária para confirmar aparência, controles e serviços online.

Referências: [formatos de textura na Unity 2022.3](https://docs.unity3d.com/2022.3/Documentation/Manual/class-TextureImporterOverride.html)
e [configurações de build Android](https://docs.unity3d.com/2022.3/Documentation/Manual/android-build-settings.html).
