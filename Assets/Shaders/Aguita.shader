Shader "Jermix/Aguita"
{
	Properties
	{

		//distorcion de geometria
		_Color ("Color del Agua ", Color) = (0, 0.5, 1, 0.5)//el color del Aguita
		_OlaVelocidad ("Velocidad de las olas", Float) = 1.0//la velocidad de las olas
		_OlaAltura ("Altura de las olas", Float) = 0.1//la altura de las olas
		_OlaFrecuencia ("Frecuencia de las olas", Float) = 1.0//la frecuencia de las olas

		//distorcion de textura
		_TexOlaVelocidad ("Velocidad de las olas(textura)", Float) = 1.0//la velocidad de las olas de textura
		_TexOlaAltura ("Altura de las olas(textura)", Float) = 0.1//la altura de las olas de textura
		_TexOlaFrecuencia ("Frecuencia de las olas(textura)", Float) = 1.0//la frecuencia de las olas de textura

		_MainTex("Textura Base", 2D) = "white" {}
	}
		SubShader
		{
			Tags 
			{ 
				"Queue" = "Transparent" 
				"RenderType" = "Transparent" 
			}
			LOD 200
					
			ZWrite Off

			Blend DstColor Zero				

			BlendOp Sub

		
			pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				//propiedades
				uniform float4 _Color;
				uniform float _OlaVelocidad;
				uniform float _OlaAltura;
				uniform float _OlaFrecuencia;
				uniform float _TexOlaVelocidad;
				uniform float _TexOlaAltura;
				uniform float _TexOlaFrecuencia;
				uniform sampler2D _MainTex;
				uniform float4 _MainTex_ST;


				//Estructura de la entrada del vertex shader 
				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				//Estructura de la salida del vertex shader
				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				//Vertex shader
				v2f vert(appdata v)
				{
					v2f o;
					
					//Aplicar ondulacion a los vertices 
					float ola =  sin((v.vertex.x + _Time.y * _OlaVelocidad) * _OlaFrecuencia) * _OlaAltura;
					
					v.vertex.y += ola;

					//Transformar la posicion al espacio de la camara
					o.pos = UnityObjectToClipPos(v.vertex);

					//Transformar las coordenadas de textura
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);

					return o;

				}
				//Fragment shader
				fixed4 frag(v2f i) : SV_Target
				{

					 // Aplicar efecto de olas a las coordenadas UV
					float2 wavedUV = i.uv;
					wavedUV.x += sin((i.uv.y + _Time.y * _TexOlaVelocidad) * _TexOlaFrecuencia) * _TexOlaAltura;
					wavedUV.y += cos((i.uv.x + _Time.y * _TexOlaVelocidad) * _TexOlaFrecuencia) * _TexOlaAltura;

					//Muestrear la textura
					//fixed4 texColor = tex2D(_MainTex, i.uv);

					// Muestrear la textura con las coordenadas modificadas
					fixed4 texColor = tex2D(_MainTex, wavedUV);

					//mezclat el color del agua con la textura
					fixed4 waterColor = _Color * texColor;

					//aplicar transparencia al agua
					waterColor.a = _Color.a;

					return waterColor;
				}
				ENDCG
			}
		}
		Fallback "Diffuse"
}