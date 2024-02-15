using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Flexalon.Editor
{
    [InitializeOnLoad]
    internal class FlexalonMenu : EditorWindow
    {
        private static readonly string _website = "https://www.flexalon.com?utm_source=fxmenu";
        public static readonly string StoreLink = "https://assetstore.unity.com/packages/tools/utilities/flexalon-3d-layouts-230509?aid=1101lqSYn";
        private static readonly string _review = "https://assetstore.unity.com/packages/tools/utilities/flexalon-3d-layouts-230509#reviews";
        private static readonly string _discord = "https://discord.gg/VM9cWJ9rjH";
        private static readonly string _docs = "https://www.flexalon.com/docs?utm_source=fxmenu";
        private static readonly string _templates = "https://www.flexalon.com/templates?utm_source=fxmenu";
        private static readonly string _examples = "https://github.com/afarchy/flexalon-examples";
        // private static readonly string _proxima = "https://www.unityproxima.com?utm_source=pxmenu";
        private static readonly string _copilot = "https://www.flexalon.com/ai?utm_source=pxmenu";

        private static readonly string _showOnStartKey = "FlexalonMenu_ShowOnStart";
        private static readonly string _versionKey = "FlexalonMenu_Version";

        private GUIStyle _trialStyle;
        private GUIStyle _errorStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _versionStyle;
        private GUIStyle _boldStyle;
        private GUIStyle _semiboldStyle;
        private GUIStyle _proximaButtonStyle;
        private GUIStyle _moreLayoutsStyle;

        private static ShowOnStart _showOnStart;
        private static readonly string[] _showOnStartOptions = {
            "Always", "On Update", "Never"
        };

        private Vector2 _scrollPosition;

        private List<string> _changelog = new List<string>();

        private bool _haveAllLayouts = false;

        private enum ShowOnStart
        {
            Always,
            OnUpdate,
            Never
        }

        static FlexalonMenu()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            EditorApplication.update -= OnEditorUpdate;
            Initialize();
        }

        internal static void Initialize()
        {
            var shownKey = "FlexalonMenuShown";
            bool alreadyShown = SessionState.GetBool(shownKey, false);
            SessionState.SetBool(shownKey, true);

            var version = WindowUtil.GetVersion();
            var lastVersion = EditorPrefs.GetString(_versionKey, "0.0.0");
            var newVersion = version.CompareTo(lastVersion) > 0;
            if (newVersion)
            {
                EditorPrefs.SetString(_versionKey, version);
                alreadyShown = false;
            }

            _showOnStart = (ShowOnStart)EditorPrefs.GetInt(_showOnStartKey, 0);
            bool showPref = _showOnStart == ShowOnStart.Always ||
                (_showOnStart == ShowOnStart.OnUpdate && newVersion);
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !alreadyShown && showPref)
            {
                StartScreen();
            }
            else
            {
                FlexalonTrial.UpdateRemainingDays();
            }

            if (!FlexalonTrial.IsTrial && !EditorApplication.isPlayingOrWillChangePlaymode && FlexalonSurvey.ShouldAsk())
            {
                FlexalonSurvey.ShowSurvey();
            }
        }

        [MenuItem("Tools/Flexalon/Start Screen")]
        public static void StartScreen()
        {
            FlexalonTrial.UpdateRemainingDays();
            FlexalonMenu window = GetWindow<FlexalonMenu>(true, "Flexalon Start Screen", true);
            window.minSize = new Vector2(800, 600);
            window.maxSize = window.minSize;
            window.Show();
        }

        [MenuItem("Tools/Flexalon/Website")]
        public static void OpenStore()
        {
            Application.OpenURL(_website);
        }

        [MenuItem("Tools/Flexalon/Write a Review")]
        public static void OpenReview()
        {
            Application.OpenURL(_review);
        }

        [MenuItem("Tools/Flexalon/Support (Discord)")]
        public static void OpenSupport()
        {
            Application.OpenURL(_discord);
        }

        private void InitStyles()
        {
            if (_bodyStyle != null) return;

            _bodyStyle = new GUIStyle(EditorStyles.label);
            _bodyStyle.wordWrap = true;
            _bodyStyle.fontSize = 14;
            _bodyStyle.margin.left = 10;
            _bodyStyle.margin.top = 10;
            _bodyStyle.stretchWidth = false;
            _bodyStyle.richText = true;

            _trialStyle = new GUIStyle(_bodyStyle);
            _trialStyle.fontStyle = FontStyle.Bold;
            _trialStyle.margin.top = 10;
            _trialStyle.normal.textColor = Color.yellow;

            _boldStyle = new GUIStyle(_bodyStyle);
            _boldStyle.fontStyle = FontStyle.Bold;
            _boldStyle.fontSize = 16;

            _semiboldStyle = new GUIStyle(_bodyStyle);
            _semiboldStyle.fontStyle = FontStyle.Bold;

            _errorStyle = new GUIStyle(_trialStyle);
            _errorStyle.normal.textColor = new Color(1, 0.2f, 0);

            _buttonStyle = new GUIStyle(_bodyStyle);
            _buttonStyle.fontSize = 14;
            _buttonStyle.margin.bottom = 5;
            _buttonStyle.padding.top = 5;
            _buttonStyle.padding.left = 10;
            _buttonStyle.padding.right = 10;
            _buttonStyle.padding.bottom = 5;
            _buttonStyle.hover.background = Texture2D.grayTexture;
            _buttonStyle.hover.textColor = Color.white;
            _buttonStyle.active.background = Texture2D.grayTexture;
            _buttonStyle.active.textColor = Color.white;
            _buttonStyle.focused.background = Texture2D.grayTexture;
            _buttonStyle.focused.textColor = Color.white;
            _buttonStyle.normal.background = Texture2D.grayTexture;
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.wordWrap = false;
            _buttonStyle.stretchWidth = false;

            _versionStyle = new GUIStyle(EditorStyles.label);
            _versionStyle.padding.right = 10;

            _proximaButtonStyle = new GUIStyle(_buttonStyle);
            _proximaButtonStyle.normal.background = Texture2D.blackTexture;
            _proximaButtonStyle.hover.background = Texture2D.blackTexture;
            _proximaButtonStyle.focused.background = Texture2D.blackTexture;
            _proximaButtonStyle.active.background = Texture2D.blackTexture;
            _proximaButtonStyle.padding.left = 0;
            _proximaButtonStyle.padding.right = 0;
            _proximaButtonStyle.padding.bottom = 0;
            _proximaButtonStyle.padding.top = 0;
            _proximaButtonStyle.margin.bottom = 10;

            _moreLayoutsStyle = new GUIStyle(_buttonStyle);
            _moreLayoutsStyle.normal.background = new Texture2D(1, 1);
            _moreLayoutsStyle.normal.background.SetPixel(0, 0, new Color(0.18f, 0.47f, 0.63f));
            _moreLayoutsStyle.normal.background.Apply();
            _moreLayoutsStyle.hover.background = _moreLayoutsStyle.normal.background;
            _moreLayoutsStyle.focused.background = _moreLayoutsStyle.normal.background;
            _moreLayoutsStyle.active.background = _moreLayoutsStyle.normal.background;
            _moreLayoutsStyle.normal.textColor = Color.white;
            _moreLayoutsStyle.fontStyle = FontStyle.Bold;

            WindowUtil.CenterOnEditor(this);

            ReadChangeLog();

            _haveAllLayouts = WindowUtil.AllLayoutsInstalled();
        }

        private void LinkButton(string label, string url, GUIStyle style = null, int width = 170)
        {
            if (style == null) style = _buttonStyle;
            var labelContent = new GUIContent(label);
            var position = GUILayoutUtility.GetRect(width, 35, style);
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            if (GUI.Button(position, labelContent, style))
            {
                Application.OpenURL(url);
            }
        }

        private bool Button(string label, GUIStyle style = null, int width = 170)
        {
            if (style == null) style = _buttonStyle;
            var labelContent = new GUIContent(label);
            var position = GUILayoutUtility.GetRect(width, 35, style);
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            return GUI.Button(position, labelContent, style);
        }

        private void Bullet(string text)
        {
            var ws = 1 + text.IndexOf('-');
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < ws; i++)
            {
                GUILayout.Space(10);
            }
            GUILayout.Label("•", _bodyStyle);

            GUILayout.Label(text.Substring(ws + 1), _bodyStyle);

            EditorGUILayout.EndHorizontal();
        }

        private void ReadChangeLog()
        {
            _changelog.Clear();
            var changelogPath = AssetDatabase.GUIDToAssetPath("b711ce346029a6f43969ef8de5691942");
            var changelogAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(changelogPath);
            _changelog = changelogAsset.text.Split('\n')
                .Select(x => Regex.Replace(x.TrimEnd(), @"\*\*(.*?)\*\*", "<b>$1</b>"))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var start = _changelog.FindIndex(l => l.StartsWith("## "));
            var end = _changelog.FindIndex(start + 1, l => l.StartsWith("---"));
            _changelog = _changelog.GetRange(start, end - start);
        }

        private void WhatsNew()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Label("What's New in Flexalon", _boldStyle);
            EditorGUILayout.Space();

            for (int i = 0; i < _changelog.Count; i++)
            {
                var line = _changelog[i];
                if (line.StartsWith("###"))
                {
                    EditorGUILayout.Space();
                    GUILayout.Label(line.Substring(4), _semiboldStyle);
                    EditorGUILayout.Space();
                }
                else if (line.StartsWith("##"))
                {
                    EditorGUILayout.Space();
                    GUILayout.Label(line.Substring(3), _boldStyle, GUILayout.ExpandWidth(true));
                    EditorGUILayout.Space();
                }
                else
                {
                    Bullet(line);
                    EditorGUILayout.Space();
                }
            }

            EditorGUILayout.Space();
        }

        private void OnGUI()
        {
            InitStyles();

            GUILayout.BeginHorizontal("In BigTitle", GUILayout.ExpandWidth(true));
            {
                WindowUtil.DrawFlexalonIcon(128);
                GUILayout.FlexibleSpace();
                GUILayout.Label("Version: " + WindowUtil.GetVersion(), _versionStyle, GUILayout.ExpandHeight(true));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Label("Resources", _boldStyle);
                    LinkButton("Discord Invite", _discord);
                    LinkButton("Documentation", _docs);
                    if (_haveAllLayouts)
                    {
                        LinkButton("Templates", _templates);
                        LinkButton("More Examples", _examples);
                    }
                    else
                    {
                        LinkButton("Get More Layouts", _website, _moreLayoutsStyle);
                    }

                    LinkButton(FlexalonTrial.IsTrial ? "Reviews" : "Write a Review", _review);

                    if (!FlexalonTrial.IsTrial && !FlexalonSurvey.Completed)
                    {
                        if (Button("Feedback"))
                        {
                            FlexalonSurvey.ShowSurvey();
                        }
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.Label("More Tools", _boldStyle);
                    if (WindowUtil.DrawCopilotButton(165, _proximaButtonStyle))
                    {
                        Application.OpenURL(_copilot);
                    }

                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                }
                GUILayout.EndVertical();

                EditorGUILayout.Separator();

                GUILayout.BeginVertical();
                {
                    _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

                    GUILayout.Label("Thank you for using Flexalon!", _boldStyle);

                    EditorGUILayout.Space();

                    GUILayout.Label("You're invited to join the Discord community for support and feedback. Let us know how to make Flexalon better for you!", _bodyStyle);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        if (FlexalonTrial.IsTrial)
                        {
                            if (FlexalonTrial.RemainingDays > 0)
                            {
                                var day = FlexalonTrial.RemainingDays == 1 ? "day" : "days";
                                GUILayout.Label("You have " + FlexalonTrial.RemainingDays + " " + day + " left on your trial.", _trialStyle);
                            }
                            else
                            {
                                GUILayout.Label("Your trial has expired. Flexalon components will no longer update.", _errorStyle);
                            }

                            GUILayout.Label("You can upgrade the trial to a full license at any time without affecting your project.", _bodyStyle);

                            EditorGUILayout.Space();
                            LinkButton("Purchase Flexalon", StoreLink);
                        }
                        else
                        {
                            GUILayout.Label("If you enjoy Flexalon, please consider writing a review. It helps a ton!", _bodyStyle);
                        }

                        EditorGUILayout.Space();
                    }
                    GUILayout.EndVertical();

                    WhatsNew();

                    EditorGUILayout.EndScrollView();
                }
                GUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("In BigTitle", GUILayout.ExpandHeight(true));
            {
                GUILayout.Label("Tools/Flexalon/Start Screen");
                GUILayout.FlexibleSpace();
                GUILayout.Label("Show On Start: ");
                var newShowOnStart = (ShowOnStart)EditorGUILayout.Popup((int)_showOnStart, _showOnStartOptions);
                if (_showOnStart != newShowOnStart)
                {
                    _showOnStart = newShowOnStart;
                    EditorPrefs.SetInt(_showOnStartKey, (int)_showOnStart);
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}