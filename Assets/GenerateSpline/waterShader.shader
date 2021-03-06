// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/waterShader" 
{

Properties 
{
	_waterColor ("waterColor", Color) = (0, 0, 0, 1)
	_waterTx ("waterTexture", 2D) = "" {}
	_waterSpeed ("waterSpeed XY:Uppder	ZW:Lower" , Vector) = (1,1,0,0)
	
	_skyColor ("skyColor", Color) = (0, 0, 0, 1)
	_skyTx ("skyTexture", 2D) = "" {}
	_skySpeed ("SkySpeed XY:	ShoreSpeed ZW:" , Vector) = (1,1,0,0)
	
	_shoreColor ("skyColor", Color) = (0, 0, 0, 1)
	_shoreTx ("shoreTexture", 2D) = "" {}	
	
	_depth_data ("_data" , Vector) = (0,0,50,100)
//	_BumpMap ("Normalmap", 2D) = "bump" {}	
}
	
SubShader 
{
//	GrabPass {							
////			Name "BASE"
////			Tags { "LightMode" = "Always" }
//	}
	
	pass 
	{
		Tags
		{ 
		"Queue"="geometry" 
		"IgnoreProjector"="True" 
		"RenderType"="Transparent" 
		}
//		Tags { "Queue"="Transparent+1" "RenderType"="Opaque" }
	//	LOD 200
		
		Blend SrcAlpha OneMinusSrcAlpha 
//		ZTest Less
		ZWrite Off
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#include "UnityCG.cginc"
		
		uniform sampler2D _CameraDepthTexture;
		uniform half4 _color;
		uniform half4 _depth_data;
		
		uniform half4 _waterColor;
		uniform sampler2D _waterTx;
		uniform half4 _waterTx_ST;
		
		uniform half4 _skyColor;
		uniform sampler2D _skyTx;
		uniform half4 _skyTx_ST;
		uniform half4 _waterSpeed;
		uniform half4 _skySpeed;
		uniform half4 _shoreColor;
		uniform sampler2D _shoreTx;
		uniform half4 _shoreTx_ST;
		
		uniform sampler2D _BumpMap;
		sampler2D _GrabTexture;
		float4 _GrabTexture_TexelSize;
		
		struct vin
		{
			float4 vertex : POSITION;
			half2 texcoord : TEXCOORD0;
			
		};
		
		struct fin
		{
			float4 vertex : SV_POSITION;
			float3 wpos : TEXCOORD0;
			half4 cpos : TEXCOORD1;
			half2 texcoord1 : TEXCOORD2;
			half2 texcoord2 : TEXCOORD3;
			half2 texcoord3 : TEXCOORD4;
			half4 uvgrab    : TEXCOORD5;
			 
		};
	
		fin vert(vin v)
		{
			fin o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
			o.cpos = ComputeScreenPos(o.vertex);
			o.texcoord1 = v.texcoord.xy * _waterTx_ST.xy + _waterTx_ST.zw; 
			o.texcoord2 = v.texcoord.xy * _skyTx_ST.xy + _skyTx_ST.zw; 
			o.texcoord3 = v.texcoord.xy * _shoreTx_ST.xy + _shoreTx_ST.zw; 
			
			o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y) + o.vertex.w) * 0.5;
			o.uvgrab.zw = o.vertex.zw;
			return o;
		}
	
	//((uint8)(255 - (((255 - A) * (255 - B)) >> 8)))
		half screen1(half A, half B)
		{
			half c = (1 - (((1 - A) * (1 - B))));		
			return c;
		}
		half over1(half A, half B)
		{
			half c ;
			if (B < 0.5)
			{
				c = (2 * A * B / 1);
			}
			else
			{
				c = (1 - 2 * (1 - A) * (1 - B) / 1);
			}
			return c;
		} 
		
		half3 screen3(half3 A, half3 B)
		{
			half3 c;
			c.r = screen1(A.r, B.r);
			c.g = screen1(A.g, B.g);
			c.b = screen1(A.b, B.b);		
			return c;
		}
		
		half3 over3(half3 A, half3 B)
		{
			half3 c;
			c.r = over1(A.r, B.r);
			c.g = over1(A.g, B.g);
			c.b = over1(A.b, B.b);		
			return c;
		}
		
		half4 over4(half4 A, half4 B)
		{
			half4 c;
			c.r = over1(A.r, B.r);
			c.g = over1(A.g, B.g);
			c.b = over1(A.b, B.b);
			c.a = over1(A.a, B.a);
			return c;
		}
		
		fixed4 frag(fin f) : COLOR
		{
		
			f.cpos /= f.cpos.w;
			half3 v = normalize(_WorldSpaceCameraPos);// - f.wpos);
			half3 l = -float3(-.8, -.4, 0);
			half3 h = normalize((l + v) / 2);
			half lf_zbuff = Linear01Depth(tex2D(_CameraDepthTexture, f.cpos.xy).r);
			half lf_depth = Linear01Depth(f.cpos.z);
			half zdiff = abs(lf_depth - lf_zbuff);
			half foam_fct = zdiff * _depth_data.z;
			
			half4 c1 = tex2D(_waterTx, half2(f.texcoord1.x + (_Time.x * _waterSpeed.x), f.texcoord1.y + (_Time.x * _waterSpeed.y))) * _waterColor;  
			half4 c12= tex2D(_waterTx, half2(f.texcoord1.x + (_Time.x * _waterSpeed.z), f.texcoord1.y + (_Time.x * _waterSpeed.w))) * _waterColor;  
			half4 c3 = tex2D(_shoreTx, half2(f.texcoord3.x + (_Time.x * _skySpeed.z), f.texcoord3.y + (_Time.x * _skySpeed.w)));
			
			
			c1.a *=  _waterColor.a;
			c12.a *=  _waterColor.a;
			c1.rgb = c1.rgb + c12.rgb;
			c3.a *=  _shoreColor.a;
			
			
//			half4 c2 = tex2D(_skyTx, half2(((v.x)*2) + f.texcoord2.x + (_Time.x * _skySpeed.x),((v.x)*2) + f.texcoord2.y + (_Time.x * _skySpeed.y))) * _skyColor;
			half4 c2 = tex2D(_skyTx, half2(f.cpos.x+(_Time.x * _skySpeed.x) ,f.cpos.y+(_Time.x * _skySpeed.y))) * _skyColor;
			c2.a *=  _skyColor.a;
			
			//c3 = c3.a * (1-(zdiff * _depth_data.w));
			half d = (1-clamp((zdiff * _depth_data.w)* _depth_data.z,0 , 1));
			c3 = c3.a * (d*2) * _shoreColor;
			
			half4 c;// = zdiff * _depth_data.w;
			c = ((c1) + (c2))+ (c3);//screen3(c1.rgb, c2.rgb);
//			c = ((c1.rgb*c1.a) + (c2.rgb*c2.a))+ (c3.rgb*(c3.a));//screen3(c1.rgb, c2.rgb);
			c.a = c.a * (1-d);// + (c2.a); 
			//c = over4(c1,c2);
			
			//refraction--------------------------------------
//			half2 bump = UnpackNormal(tex2D( _BumpMap, half2(f.texcoord3.x + (_Time.x * -_waterSpeed.x), f.texcoord3.y + (_Time.x * -_waterSpeed.y)))).rg; // we could optimize this by just reading the x & y without reconstructing the Z
//			float2 offset = bump * 128 * _GrabTexture_TexelSize.xy;
//			f.uvgrab.xy = offset * f.uvgrab.z + f.uvgrab.xy;
			
//			half4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(f.uvgrab));
//			half4 tint = tex2D( _MainTex, i.uvmain );
//			return col + c;// * tint;
			//refraction--------------------------------------
			half4 dep = d;
			dep.a = 1;
			
			return c;
		}
		ENDCG
	}
}
}