using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
    public class MB3_ShadersThatShareTiling
    {
        public struct ShaderThatSharesTiling
        {
            public string shadername;
            public bool allPropsShareTiling;
            public string tilingTexturePropName;
        }

        private static MB3_ShadersThatShareTiling _singleton;

        Dictionary<string, ShaderThatSharesTiling> shadersThatShareTiling;

        public static MB3_ShadersThatShareTiling GetShadersThatShareTiling()
        {
            if (_singleton == null)
            {
                Init();
            }

            return _singleton;
        }

        public static void GetScaleAndOffsetForTextureProp(Material m, string texturePropName, out Vector2 offset, out Vector2 scale)
        {
            MB3_ShadersThatShareTiling ss = GetShadersThatShareTiling();
            ShaderThatSharesTiling sst;
            if (ss.shadersThatShareTiling.TryGetValue(m.shader.name, out sst))
            {
                if (sst.allPropsShareTiling && m.HasProperty(sst.tilingTexturePropName))
                {
                    scale = m.GetTextureScale(sst.tilingTexturePropName);
                    offset = m.GetTextureOffset(sst.tilingTexturePropName);
                    return;
                }
            }

            scale = m.GetTextureScale(texturePropName);
            offset = m.GetTextureOffset(texturePropName);
            return;
        }

        private static void Init()
        {
            _singleton = new MB3_ShadersThatShareTiling();
            Dictionary<string, ShaderThatSharesTiling> ss = _singleton.shadersThatShareTiling = new Dictionary<string, ShaderThatSharesTiling>();

            ShaderThatSharesTiling standard;
            standard.shadername = "Standard";
            standard.allPropsShareTiling = true;
            standard.tilingTexturePropName = "_MainTex";

            ShaderThatSharesTiling standardSpecSetup;
            standardSpecSetup.shadername = "Standard (Specular setup)";
            standardSpecSetup.allPropsShareTiling = true;
            standardSpecSetup.tilingTexturePropName = "_MainTex";

            ShaderThatSharesTiling urpLit;
            urpLit.shadername = "Universal Render Pipeline/Lit";
            urpLit.allPropsShareTiling = true;
            urpLit.tilingTexturePropName = "_BaseMap";

            ShaderThatSharesTiling urpSimpleLit;
            urpSimpleLit.shadername = "Universal Render Pipeline/Simple Lit";
            urpSimpleLit.allPropsShareTiling = true;
            urpSimpleLit.tilingTexturePropName = "_BaseMap";

            ShaderThatSharesTiling urpComplexLit;
            urpComplexLit.shadername = "Universal Render Pipeline/Complex Lit";
            urpComplexLit.allPropsShareTiling = true;
            urpComplexLit.tilingTexturePropName = "_BaseMap";

            ShaderThatSharesTiling urpBakedLit;
            urpBakedLit.shadername = "Universal Render Pipeline/Baked Lit";
            urpBakedLit.allPropsShareTiling = true;
            urpBakedLit.tilingTexturePropName = "_BaseMap";

            ss.Add(standard.shadername, standard);
            ss.Add(standardSpecSetup.shadername, standardSpecSetup);
            ss.Add(urpLit.shadername, urpLit);
            ss.Add(urpSimpleLit.shadername, urpLit);
            ss.Add(urpComplexLit.shadername, urpLit);
            ss.Add(urpBakedLit.shadername, urpLit);
        }
    }
}
