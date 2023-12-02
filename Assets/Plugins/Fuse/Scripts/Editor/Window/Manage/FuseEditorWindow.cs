using System;
using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Primary editor window for Fuse.
    /// </summary>
    public class FuseEditorWindow : EditorWindow
    {
        private const float SideBarSize = 48;
        private static readonly Color SelectedColor = new Color(1, 1, 1);
        private static readonly Color DeselectedColor = new Color(0.75f, 0.75f, 0.75f);

        private IMenu _activeMenu;
        private Menu _selectedMenu = Menu.Configuration;

        public enum Menu
        {
            Configuration,
            State,
            Content,
            Scene,
            Environment,
            Build,
            Documentation
        }

        public static void Display(Menu menu)
        {
            var texture = AssetUtility.FetchByPath<Texture2D>(PathUtility.MenuHeaderIconPath);
            var window = GetWindow<FuseEditorWindow>();
            window.titleContent = new GUIContent(texture, "FUSE - Application Framework");
            window.minSize = new Vector2(480, 480);
            window.SetMenu(menu);
            window.Show();
        }

        [MenuItem("FUSE/Manage", priority = 100)]
        private static void DisplayToConfigure()
        {
            Display(Menu.Configuration);
        }


        private void OnGUI()
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), new Color(0.2f, 0.2f, 0.2f));
            DrawMenu(new Rect(SideBarSize + 10, 0, position.width - SideBarSize - 25, position.height));
            DrawSideBar(new Rect(0, 0, SideBarSize, position.height));
        }

        #region Drawing - Menu

        public void SetMenu(Menu menu)
        {
            _selectedMenu = menu;

            if (_activeMenu != null)
            {
                _activeMenu.Cleanup();
                _activeMenu = null;
            }
        }

        private void DrawMenu(Rect window)
        {
            if (_activeMenu == null)
            {
                switch (_selectedMenu)
                {
                    case Menu.Configuration:
                        _activeMenu = new ConfigureMenu();
                        break;
                    case Menu.State:
                        _activeMenu = new StateMenu();
                        break;
                    case Menu.Content:
                        _activeMenu = new ContentMenu();
                        break;
                    case Menu.Scene:
                        _activeMenu = new SceneMenu();
                        break;
                    case Menu.Environment:
                        _activeMenu = new EnvironmentMenu();
                        break;
                    case Menu.Build:
                        _activeMenu = new BuildMenu();
                        break;
                    case Menu.Documentation:
                        _activeMenu = new DocumentationMenu();
                        break;
                    default:
                        throw new NotImplementedException();
                }

                _activeMenu.Setup();
                _activeMenu.Refresh += () =>
                {
                    GUI.changed = true;
                    Repaint();
                };
            }

            EditorGUI.DrawRect(new Rect(window.x, window.y, window.width, window.height),
                new Color(0.2f, 0.2f, 0.2f));
            _activeMenu?.Draw(window);
        }

        #endregion

        #region Side Bar

        private void DrawSideBar(Rect window)
        {
            EditorGUI.DrawRect(new Rect(window.x, window.y, window.width, window.height),
                new Color(0.18f, 0.18f, 0.18f));
            EditorGUI.DrawRect(new Rect(window.x + window.width, window.y, 1f, window.height),
                new Color(0.15f, 0.15f, 0.15f));

            var index = 0;
            foreach (var menu in Enum.GetValues(typeof(Menu)))
            {
                DrawMenuIcon(window.x, window.y + window.width * index, window.width, window.width,
                    menu.ToString());
                index++;
            }
        }

        private void DrawMenuIcon(float x, float y, float width, float height, string menuName)
        {
            var iconMenu = GetMenu(menuName);
            GUI.backgroundColor = _selectedMenu == iconMenu ? SelectedColor : DeselectedColor;
            var content = new GUIContent(GetMenuIcon(iconMenu), GetMenuTooltip(iconMenu));
            if (GUI.Button(new Rect(x, y, width, height), content))
                OnSideBar(menuName);
        }

        private void OnSideBar(string menuName)
        {
            Menu selectedMenu = (Menu) Enum.Parse(typeof(Menu), menuName, true);
            _selectedMenu = selectedMenu;
            _activeMenu.Cleanup();
            _activeMenu = null;
        }

        private Texture2D GetMenuIcon(Menu menu)
        {
            return AssetUtility.FetchByPath<Texture2D>(string.Format(PathUtility.IconPath, menu.ToString()));
        }

        private string GetMenuTooltip(Menu menu)
        {
            switch (menu)
            {
                case Menu.Configuration:
                    return "Home - Manage your application's configuration";
                case Menu.State:
                    return "State - Manage the application's state";
                case Menu.Content:
                    return "Content - Manage your application's bundles";
                case Menu.Scene:
                    return "Scene - Manage your application's scenes";
                case Menu.Environment:
                    return "Environment - Manage your environmental configuration (e.g. Develop, Stage, Production)";
                case Menu.Build:
                    return "Build - Choose a platform and environment to export a build or assets";
                case Menu.Documentation:
                    return "Documentation - Access supporting documentation";
                default:
                    throw new ArgumentOutOfRangeException(nameof(menu), menu, null);
            }
        }

        private Menu GetMenu(string menuName)
        {
            return (Menu) Enum.Parse(typeof(Menu), menuName, true);
        }

        #endregion
    }
}