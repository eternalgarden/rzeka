#if UNITY_EDITOR

/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using UnityEditor;
using UnityEditor.Callbacks;

namespace Rzeka.Unirx
{
    /* 🌊 ---- ---- */

    [InitializeOnLoad]
    public class ScenePlaybackDetector
    {
        private static bool _isPlaying = false;

        private static bool AboutToStartScene
        {
            get
            {
                return EditorPrefs.GetBool("AboutToStartScene");
            }
            set
            {
                EditorPrefs.SetBool("AboutToStartScene", value);
            }
        }

        public static bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                }
            }
        }

        // This callback is notified after scripts have been reloaded.
        [DidReloadScripts]
        public static void OnDidReloadScripts()
        {
            // Filter DidReloadScripts callbacks to the moment where playmodeState transitions into isPlaying.
            if (AboutToStartScene)
            {
                IsPlaying = true;
            }
        }

        // InitializeOnLoad ensures that this constructor is called when the Unity Editor is started.
        static ScenePlaybackDetector()
        {
#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged += e =>
#else
            EditorApplication.playmodeStateChanged += () =>
#endif
            {
                // Before scene start:          isPlayingOrWillChangePlaymode = false;  isPlaying = false
                // Pressed Playback button:     isPlayingOrWillChangePlaymode = true;   isPlaying = false
                // Playing:                     isPlayingOrWillChangePlaymode = false;  isPlaying = true
                // Pressed stop button:         isPlayingOrWillChangePlaymode = true;   isPlaying = true
                if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                {
                    AboutToStartScene = true;
                }
                else
                {
                    AboutToStartScene = false;
                }

                // Detect when playback is stopped.
                if (!EditorApplication.isPlaying)
                {
                    IsPlaying = false;
                }
            };
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 26 May 2022 🌊 */

#endif