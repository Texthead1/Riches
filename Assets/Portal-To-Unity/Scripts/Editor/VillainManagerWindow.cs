using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PortalToUnity
{
    public class VillainManagerWindow : EditorWindow
    {
        private List<Villain> villains;
        private Villain selectedVillain;
        private Vector2 leftScrollPos;
        private Vector2 rightScrollPos;
        private float leftPanelWidth = 0.5f;
        private bool refreshingVillains = false;

        [MenuItem("Portal-To-Unity/Villains Database")]
        public static void ShowWindow()
        {
            GetWindow<VillainManagerWindow>("Villains Database");
        }

        public void OnGUI()
        {
            if (!refreshingVillains)
                LoadVillains();

            EditorGUILayout.BeginHorizontal();
            GUIStyle boldStyle = new GUIStyle(GUI.skin.label);
            boldStyle.fontStyle = FontStyle.Bold;
            boldStyle.fontSize = 14;

            Event e;

            if (villains == null) return;

            if (villains.Count == 0)
            {
                boldStyle.wordWrap = true;
                leftScrollPos = EditorGUILayout.BeginScrollView(leftScrollPos);
                EditorGUILayout.LabelField("Please add Villains to the database by putting them in \"Assets/Resources/Portal-To-Unity/Villains/\"", boldStyle);
                e = Event.current;
                if (e.type == EventType.ContextClick)
                    ShowContextOption("Open File Location", "Assets/Resources/Portal-To-Unity/Villains/");
                selectedVillain = null;
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();
                return;
            }
            villains = villains.OrderBy(x => x.VillainID).ToList();
            leftScrollPos = EditorGUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(position.width * leftPanelWidth));

            foreach (Villain villain in villains)
            {
                bool button = GUILayout.Button(villain.Name);
                e = Event.current;

                if (!mouseOverWindow) continue;

                if (GUILayoutUtility.GetLastRect().Contains(e.mousePosition) && e.button == 1)
                    ShowContextOption("Show Villain in Explorer", "Assets/Resources/Portal-To-Unity/Villains/", villain);
                else if (button && e.button != 1)
                    selectedVillain = villain;
            }

            EditorGUILayout.EndScrollView();
            rightScrollPos = EditorGUILayout.BeginScrollView(rightScrollPos, GUILayout.Width(position.width * (1 - leftPanelWidth)));

            e = Event.current;
            if (e.type == EventType.ContextClick)
                ShowContextOption("Show Villain in Explorer", "Assets/Resources/Portal-To-Unity/Villains/", selectedVillain);
            
            if (selectedVillain != null)
            {
                EditorGUILayout.LabelField($"Villain Info ({selectedVillain.name})", boldStyle);
                EditorGUILayout.LabelField("Name", selectedVillain.Name);
                EditorGUILayout.LabelField("Villain ID", $"{selectedVillain.VillainID} ({(int)selectedVillain.VillainID})");
                EditorGUILayout.LabelField("Element", selectedVillain.Element.ToString());
                EditorGUILayout.Space();

                try
                {
                    VillainVariant variant = selectedVillain.Variant;

                    if (variant != null)
                    {
                        EditorGUILayout.LabelField($"Variant ({variant.name})", boldStyle);

                        //if (EditorGUILayout.Foldout(true, variant.name))
                        {
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField("Variant Name", variant.Name);
                            EditorGUILayout.LabelField("Name Override", variant.NameOverride);
                            string fullName = $"{variant.Name} {(variant.NameOverride != string.Empty && variant.NameOverride != null ? variant.NameOverride : selectedVillain.Name)}";
                            EditorGUILayout.LabelField("Full Name", fullName);
                            EditorGUILayout.EndVertical();
                        }

                        if (GUILayoutUtility.GetLastRect().Contains(e.mousePosition) && e.button == 1 && mouseOverWindow == this)
                            ShowContextOption("Show Variant in Explorer", AssetDatabase.GetAssetPath(variant));
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Variant", boldStyle);
                    }
                }
                catch (Exception) {}
            }
            else if (villains.Count > 0)
                EditorGUILayout.LabelField("Select a Villain", boldStyle);
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }

        private void OnEnable()
        {
            LoadVillains();
            InspectVillains();
        }

        private void OnDisable()
        {
            refreshingVillains = false;
        }

        private async void LoadVillains()
        {
            refreshingVillains = true;
            while (refreshingVillains)
            {
                villains = Resources.LoadAll<Villain>("Portal-To-Unity/Villains").ToList();
                await Task.Delay(750);
            }
            refreshingVillains = false;
        }

        private void ShowContextOption(string text, string path)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent(text), false, () => OpenFileLocation(path));
            menu.ShowAsContext();
        }

        private void ShowContextOption(string text, string path, Villain file)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent(text), false, () => OpenFileLocation(path, file));
            menu.ShowAsContext();
        }

        private void OpenFileLocation(string path) => EditorUtility.RevealInFinder(path);

        private void OpenFileLocation(string path, Villain file) => OpenFileLocation(path + file.name + ".asset");

        private void InspectVillains()
        {
            HashSet<VillainVariant> villainVariants = new HashSet<VillainVariant>(Resources.LoadAll<VillainVariant>("Portal-To-Unity/Villains/"));
            Debug.Log($"Loaded {villains.Count} Villains and {villainVariants.Count} variants");

            foreach (Villain villain in villains)
            {
                if (villain.Variant != null)
                {
                    if (villainVariants.Contains(villain.Variant))
                        villainVariants.Remove(villain.Variant);
                }
            }

            if (villainVariants.Count > 0)
            {
                Debug.LogError("Unreferenced Villain variants:");
                foreach (VillainVariant unreferencedVariant in villainVariants)
                    Debug.LogError(unreferencedVariant.name);
            }
        }
    }
}