using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace PortalToUnity
{
    public class SkylanderManagerWindow : EditorWindow
    {
        private List<Skylander> skylanders;
        private Skylander selectedSkylander;
        private Vector2 leftScrollPos;
        private Vector2 rightScrollPos;
        private float leftPanelWidth = 0.5f;
        private Dictionary<SkylanderVariant, bool> foldoutStates = new Dictionary<SkylanderVariant, bool>();
        private Dictionary<string, bool> groupFoldoutStates = new Dictionary<string, bool>();
        string[] groupLabelNames = new string[7] { "Spyro's Adventure", "Giants", "SWAP Force", "Trap Team", "SuperChargers", "Imaginators", "Unknown" };
        private bool refreshingSkylanders = false;

        [MenuItem("Portal-To-Unity/Skylanders Database")]
        public static void ShowWindow()
        {
            GetWindow<SkylanderManagerWindow>("Skylanders Database");
        }

        public void OnGUI()
        {
            if (!refreshingSkylanders)
                LoadSkylanders();

            EditorGUILayout.BeginHorizontal();

            GUIStyle boldStyle = new GUIStyle(GUI.skin.label);
            boldStyle.fontStyle = FontStyle.Bold;
            boldStyle.fontSize = 14;

            Event e;

            if (skylanders == null) return;
    
            if (skylanders.Count == 0)
            {
                boldStyle.wordWrap = true;
                leftScrollPos = EditorGUILayout.BeginScrollView(leftScrollPos);
                EditorGUILayout.LabelField("Please add Skylanders to the database by putting them in \"Assets/Resources/Portal-To-Unity/Figures/\"", boldStyle);
                e = Event.current;
                if (e.type == EventType.ContextClick)
                    ShowContextOption("Open File Location", "Assets/Resources/Portal-To-Unity/Figures/");
                selectedSkylander = null;
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();
                return;
            }
            skylanders = skylanders.OrderBy(x => x.CharacterID).ToList();
            leftScrollPos = EditorGUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(position.width * leftPanelWidth));
            IEnumerable<Skylander>[] groups = new IEnumerable<Skylander>[6];

            List<Skylander> remaining = new List<Skylander>(skylanders);
            groups[0] = remaining.Where(x => x.CharacterID.IsSSA() || x.CharacterID.IsTFB_Item() && (int)x.CharacterID < ToyCodeExtensions.TFB_BattlePieces_Low || (int)x.CharacterID >= ToyCodeExtensions.TFB_Expansions_Low && (int)x.CharacterID < 305 || x.CharacterID == ToyCode.Mini_Terrabite || x.CharacterID == ToyCode.Mini_GillRunt || x.CharacterID == ToyCode.Mini_TriggerSnappy || x.CharacterID == ToyCode.Mini_WhisperElf).ToList();
            remaining.RemoveAll(x => groups[0].Contains(x));
            groups[1] = remaining.Where(x => x.CharacterID.IsGiants() || x.CharacterID.IsTFB_BattlePiece() || x.CharacterID.IsMini() && (int)x.CharacterID > 539).ToList();
            remaining.RemoveAll(x => groups[1].Contains(x));
            groups[2] = remaining.Where(x => x.CharacterID.IsSwapForce() || x.CharacterID.IsSwapPart() || (int)x.CharacterID >= ToyCodeExtensions.VV_Items_Low && (int)x.CharacterID < ToyCodeExtensions.Vehicles_Low || x.CharacterID.IsVV_Expansion()).ToList();
            remaining.RemoveAll(x => groups[2].Contains(x));
            groups[3] = remaining.Where(x => x.CharacterID.IsTrapTeam() || x.CharacterID.IsMini() || (int)x.CharacterID != 235 && (int)x.CharacterID >= ToyCodeExtensions.Traps_Low && (int)x.CharacterID < 310 || x.CharacterID.IsTrapTeam_Debug()).ToList();
            remaining.RemoveAll(x => groups[3].Contains(x));
            groups[4] = remaining.Where(x => x.CharacterID.IsVehicle() || (int)x.CharacterID >= ToyCodeExtensions.SuperChargers_Low && (int)x.CharacterID <= ToyCodeExtensions.TemplateVehicle_High).ToList();
            remaining.RemoveAll(x => groups[4].Contains(x));
            groups[5] = remaining;

            for (int i = 0; i < groups.Length; i++)
            {
                IEnumerable<Skylander> group = groups[i];

                if (!groupFoldoutStates.ContainsKey(i.ToString()))
                    groupFoldoutStates[i.ToString()] = EditorPrefs.GetBool($"GroupFoldoutState_{i}", true);

                groupFoldoutStates[i.ToString()] = EditorGUILayout.Foldout(groupFoldoutStates[i.ToString()], groupLabelNames[i], true);
                EditorPrefs.SetBool($"GroupFoldoutState_{i}", groupFoldoutStates[i.ToString()]);

                if (groupFoldoutStates[i.ToString()])
                {
                    foreach (Skylander skylander in group)
                    {
                        bool button = GUILayout.Button(skylander.Prefix == string.Empty ? skylander.Name : $"{skylander.Prefix} {skylander.Name}");
                        e = Event.current;

                        if (!mouseOverWindow) continue;

                        if (GUILayoutUtility.GetLastRect().Contains(e.mousePosition) && e.button == 1)
                            ShowContextOption("Show Skylander in Explorer", "Assets/Resources/Portal-To-Unity/Figures/", skylander);
                        else if (button && e.button != 1)
                            selectedSkylander = skylander;
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            rightScrollPos = EditorGUILayout.BeginScrollView(rightScrollPos, GUILayout.Width(position.width * (1 - leftPanelWidth)));

            e = Event.current;
            if (e.type == EventType.ContextClick)
                ShowContextOption("Show Skylander in Explorer", "Assets/Resources/Portal-To-Unity/Figures/", selectedSkylander);

            if (selectedSkylander != null)
            {
                EditorGUILayout.LabelField($"Skylander Info ({selectedSkylander.name})", boldStyle);
                EditorGUILayout.LabelField("Prefix", selectedSkylander.Prefix);
                EditorGUILayout.LabelField("Name", selectedSkylander.Name);
                EditorGUILayout.LabelField("Character ID", $"{selectedSkylander.CharacterID} ({(int)selectedSkylander.CharacterID})");
                EditorGUILayout.LabelField("Type", selectedSkylander.Type.ToString());
                EditorGUILayout.LabelField("Element", selectedSkylander.Element.ToString());
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Variants", boldStyle);

                try
                {                
                    foreach (SkylanderVariant variant in selectedSkylander.Variants)
                    {
                        if (variant == null) continue;
                        
                        if (!foldoutStates.ContainsKey(variant))
                            foldoutStates[variant] = EditorPrefs.GetBool($"FoldoutState_{variant.GetInstanceID()}", true);

                        // Algorithmically find foldout name in future
                        foldoutStates[variant] = EditorGUILayout.Foldout(foldoutStates[variant], variant.name, true);
                        EditorPrefs.SetBool($"FoldoutState_{variant.GetInstanceID()}", foldoutStates[variant]);
                        if (foldoutStates[variant])
                        {
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField("Variant Name", variant.Name);
                            if (variant.NameOverride != string.Empty && variant.NameOverride != null)
                                EditorGUILayout.LabelField("Name Override", variant.NameOverride);
                            EditorGUILayout.LabelField("Tag", variant.Tag);
                            EditorGUILayout.LabelField("Year Code", variant.VariantID.YearCode.ToString());
                            EditorGUILayout.LabelField("Is Repose", variant.VariantID.IsRepose.ToString());
                            EditorGUILayout.LabelField("Is Alt Deco", variant.VariantID.IsAltDeco.ToString());
                            EditorGUILayout.LabelField("Is LightCore", variant.VariantID.IsLightCore.ToString());
                            EditorGUILayout.LabelField("Is SuperCharger", variant.VariantID.IsSuperCharger.ToString());
                            EditorGUILayout.LabelField("DecoID", $"{variant.VariantID.DecoID} ({(DecoID)variant.VariantID.DecoID})");
                            EditorGUILayout.LabelField("Encoded VariantID", variant.VariantID.Encode().ToString());
                            EditorGUILayout.EndVertical();
                        }

                        if (GUILayoutUtility.GetLastRect().Contains(e.mousePosition) && e.button == 1 && mouseOverWindow == this)
                            ShowContextOption("Show Variant in Explorer", AssetDatabase.GetAssetPath(variant));
                    }
                }
                catch (Exception) {}
            }
            else if (skylanders.Count > 0)
                EditorGUILayout.LabelField("Select a Skylander", boldStyle);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }

        private void OnEnable()
        {
            LoadSkylanders();
            InspectSkylanders();
        }

        private void OnDisable()
        {
            refreshingSkylanders = false;           
        }

        private async void LoadSkylanders()
        {
            refreshingSkylanders = true;
            while (refreshingSkylanders)
            {
                skylanders = Resources.LoadAll<Skylander>("Portal-To-Unity/Figures/").ToList();
                await Task.Delay(750);
            }
            refreshingSkylanders = false;
        }

        private void ShowContextOption(string text, string path)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent(text), false, () => OpenFileLocation(path));
            menu.ShowAsContext();
        }

        // Skylander ScriptableObjects have an assumable file path, unlike their variants
        private void ShowContextOption(string text, string path, Skylander file)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent(text), false, () => OpenFileLocation(path, file));
            menu.ShowAsContext();
        }
        
        private void OpenFileLocation(string path) => EditorUtility.RevealInFinder(path);

        private void OpenFileLocation(string path, Skylander file) => OpenFileLocation(path + file.name + ".asset");

        private void InspectSkylanders()
        {
            HashSet<SkylanderVariant> skylanderVariants = new HashSet<SkylanderVariant>(Resources.LoadAll<SkylanderVariant>("Portal-To-Unity/Figures/"));
            Debug.Log($"Loaded {skylanders.Count} Skylanders and {skylanderVariants.Count} variants (approx. {skylanders.Count + skylanderVariants.Count} unique figures)");

            foreach (Skylander skylander in skylanders)
            {
                foreach (SkylanderVariant variant in skylander.Variants)
                {
                    if (skylanderVariants.Contains(variant))
                        skylanderVariants.Remove(variant);
                }
            }

            if (skylanderVariants.Count > 0)
            {
                Debug.LogError("Unreferenced Skylander variants:");
                foreach (SkylanderVariant unreferencedVariant in skylanderVariants)
                    Debug.LogError(unreferencedVariant.name);
            }
        }
    }
}