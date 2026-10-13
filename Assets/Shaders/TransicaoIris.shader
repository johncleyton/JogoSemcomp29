Shader "UI/TransicaoIris"
{
    Properties
    {
        [PerRendererData] _MainTex ("Captura da tela", 2D) = "white" {}
        _Raio ("Raio", Float) = 0
        _Aspecto ("Largura / altura", Float) = 1.777778
        _Suavidade ("Suavidade", Float) = 0.003
        _MostrarDentro ("Mostrar dentro do circulo", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Overlay"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _Raio;
            float _Aspecto;
            float _Suavidade;
            float _MostrarDentro;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 posicao = i.uv - float2(0.5, 0.5);
                posicao.x *= _Aspecto;
                float distancia = length(posicao);

                float dentro = 1.0 - smoothstep(
                    _Raio - _Suavidade, _Raio + _Suavidade, distancia);
                float mascara = lerp(1.0 - dentro, dentro, _MostrarDentro);

                fixed4 cor = tex2D(_MainTex, i.uv);
                // Ignora o alpha da captura: a cobertura inicial deve ser opaca.
                cor.a = mascara;
                return cor;
            }
            ENDCG
        }
    }
    Fallback Off
}
