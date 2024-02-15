using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Flexalon.Editor
{
    internal class FlexalonSurvey : EditorWindow
    {
        private struct SurveyData
        {
            public string version;
            public string unityVersion;
            public string buildTarget;
            public int xr;
            public int experience;
            public string benefits;
            public string improvements;
            public string layouts;
        }

        private enum SurveyState
        {
            Ask,
            DontAsk,
            Completed
        }

        private enum XRType
        {
            None,
            XRI,
            Oculus
        }

        private static readonly string[] _options = new string[] { "Very Disappointed", "Somewhat Disappointed", "Not Disappointed" };
        private static readonly Vector2 _initialSize = new Vector2(580, 400);
        private static readonly Vector2 _expandedSize = new Vector2(580, 520);
        private static readonly string _stateKey = "FlexalonSurveyState";
        private static readonly string _dateKey = "FlexalonSurveyDate";
        private static readonly string _attemptKey = "FlexalonSurveyAttempt";
        private static readonly TimeSpan _askFrequency = new TimeSpan(3, 0, 0, 0);

        private GUIStyle _bodyStyle;
        private GUIStyle _boldStyle;
        private GUIStyle _toggleStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _dontAskButtonStyle;
        private GUIStyle _textAreaStyle;
        private SurveyData _surveyData;
        private Texture _surveyImg;

        public static bool Completed => EditorPrefs.GetInt(_stateKey, 0) == (int)SurveyState.Completed;

        public static void ResetState()
        {
            EditorPrefs.SetInt(_stateKey, 0);
        }

        public static bool ShouldAsk()
        {
            if (SessionState.GetBool(_attemptKey, false))
            {
                return false;
            }

            SessionState.SetBool(_attemptKey, true);

            if (EditorPrefs.GetInt(_stateKey, 0) != (int)SurveyState.Ask)
            {
                return false;
            }

            if (!EditorPrefs.HasKey(_dateKey))
            {
                EditorPrefs.SetString(_dateKey, DateTime.Now.ToBinary().ToString());
                return false;
            }

            var lastAsked = DateTime.FromBinary(Convert.ToInt64(EditorPrefs.GetString(_dateKey, "0")));
            if (DateTime.Now - lastAsked < _askFrequency)
            {
                return false;
            }

            return true;
        }

        public static void ShowSurvey()
        {
            var window = GetWindow<FlexalonSurvey>(true, "Flexalon Feedback", true);
            window.Show();
        }

        private void Init()
        {
            if (_surveyData.version != null) return;

            _bodyStyle = new GUIStyle(EditorStyles.label);
            _bodyStyle.wordWrap = true;
            _bodyStyle.fontSize = 14;
            _bodyStyle.margin.left = 10;
            _bodyStyle.margin.bottom = 10;
            _bodyStyle.stretchWidth = false;
            _bodyStyle.alignment = TextAnchor.MiddleCenter;

            _boldStyle = new GUIStyle(_bodyStyle);
            _boldStyle.fontStyle = FontStyle.Bold;

            _toggleStyle = new GUIStyle(EditorStyles.miniButton);
            _toggleStyle.margin = new RectOffset(10, 10, 10, 10);
            _toggleStyle.fixedHeight = 45;
            _toggleStyle.fixedWidth = 180;
            _toggleStyle.fontSize = 14;
            _toggleStyle.alignment = TextAnchor.MiddleCenter;

            _buttonStyle = new GUIStyle(EditorStyles.miniButton);
            _buttonStyle.margin = new RectOffset(10, 10, 10, 10);
            _buttonStyle.fixedHeight = 35;
            _buttonStyle.fixedWidth = 170;
            _buttonStyle.fontSize = 14;
            _buttonStyle.alignment = TextAnchor.MiddleCenter;

            _dontAskButtonStyle = new GUIStyle(EditorStyles.miniButton);
            _dontAskButtonStyle.normal.background = null;
            _dontAskButtonStyle.margin = new RectOffset(10, 10, 10, 10);
            _dontAskButtonStyle.fixedWidth = 110;

            _textAreaStyle = new GUIStyle(EditorStyles.textArea);
            _textAreaStyle.margin.left = 10;
            _textAreaStyle.margin.right = 10;

            this.titleContent = new GUIContent("Flexalon Feedback");

            this.minSize = this.maxSize = _expandedSize;
            WindowUtil.CenterOnEditor(this);

            _surveyData = new SurveyData
            {
                version = WindowUtil.GetVersion(),
                unityVersion = Application.unityVersion,
                buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString(),
#if FLEXALON_OCULUS
                xr = (int)XRType.Oculus,
#elif UNITY_XRI
                xr = (int)XRType.XRI,
#else
                xr = (int)XRType.None,
#endif
                experience = -1,
                benefits = "",
                improvements = "",
                layouts = string.Join(",", WindowUtil.GetInstalledLayouts())
            };

            var surveyImgPath = AssetDatabase.GUIDToAssetPath("0ea942e8eabc7e34c8cfd062416108ac");
            _surveyImg = AssetDatabase.LoadAssetAtPath<Texture>(surveyImgPath);

            EditorPrefs.SetString(_dateKey, DateTime.Now.ToBinary().ToString());
        }

        private int ToggleGroup(int selected, string[] options)
        {
            int newSelected = selected;
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < options.Length; i++)
            {
                var option = options[i];
                if (GUILayout.Toggle(selected == i, option, _toggleStyle))
                {
                    newSelected = i;
                }

                if (i < options.Length - 1)
                {
                    GUILayout.FlexibleSpace();
                }
            }

            EditorGUILayout.EndHorizontal();
            return newSelected;
        }

        private void BeginCenter()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
        }

        private void EndCenter()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void CenterLabel(string text, GUIStyle style)
        {
            BeginCenter();
            GUILayout.Label(text, style);
            EndCenter();
        }

        private void CenterImage(Texture image, params GUILayoutOption[] options)
        {
            BeginCenter();
            GUILayout.Label(image, options);
            EndCenter();
        }

        private void OnGUI()
        {
            Init();

            EditorGUILayout.BeginVertical();

                GUILayout.FlexibleSpace();

                BeginCenter();
                WindowUtil.DrawFlexalonIcon(128);
                EndCenter();

                GUILayout.FlexibleSpace();

                CenterLabel("Please help improve Flexalon by answering 3 quick questions.", _boldStyle);

                if (_surveyData.experience == -1)
                {
                    CenterImage(_surveyImg, GUILayout.Width(300), GUILayout.Height(200));
                }

                CenterLabel("How would you feel if you could no longer use Flexalon 3D Layouts?", _bodyStyle);

                _surveyData.experience = ToggleGroup(_surveyData.experience, _options);

                if (_surveyData.experience == -1)
                {
                    this.minSize = this.maxSize = _initialSize;

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Ask me later", _dontAskButtonStyle))
                        {
                            Close();
                        }
                        if (GUILayout.Button("Don't ask again", _dontAskButtonStyle))
                        {
                            EditorPrefs.SetInt(_stateKey, (int)SurveyState.DontAsk);
                            Close();
                        }
                    EditorGUILayout.EndHorizontal();
                }
                else if (_surveyData.experience == 0 || _surveyData.experience == 1)
                {
                    this.minSize = this.maxSize = _expandedSize;

                    GUILayout.FlexibleSpace();

                    GUILayout.Label("What is the main benefit you get from Flexalon?", _bodyStyle);
                    _surveyData.benefits = GUILayout.TextArea(_surveyData.benefits, _textAreaStyle, GUILayout.Height(100));

                    GUILayout.FlexibleSpace();

                    GUILayout.Label("How can Flexalon be improved for you?", _bodyStyle);
                    _surveyData.improvements = GUILayout.TextArea(_surveyData.improvements, _textAreaStyle, GUILayout.Height(100));

                    GUILayout.FlexibleSpace();

                    EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Send Feedback", _buttonStyle))
                        {
                            SendSurvey();
                            Close();
                        }
                        GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    SendSurvey();
                    Close();
                }

            EditorGUILayout.EndVertical();
        }

        private void SendSurvey()
        {
#if UNITY_WEB_REQUEST
            var request = new UnityWebRequest("https://www.flexalon.com/api/survey", UnityWebRequest.kHttpVerbPOST);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            var json = JsonUtility.ToJson(_surveyData);
            var jsonData = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonData);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SendWebRequest().completed += op => {
                if (request.responseCode == 200)
                {
                    Debug.Log("Flexalon feedback sent successfully.");
                    EditorPrefs.SetInt(_stateKey, (int)SurveyState.Completed);
                }
                else if (request.responseCode == 400)
                {
                    Debug.LogError("Failed to send Flexalon feedback: " + request.downloadHandler.text);
                }
                else
                {
                    Debug.LogError("Failed to send Flexalon feedback: " + request.error);

                }

                request.Dispose();
            };
#else
            EditorPrefs.SetInt(_stateKey, (int)SurveyState.Completed);
#endif
        }
    }
}