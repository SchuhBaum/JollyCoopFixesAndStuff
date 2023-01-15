using System.Collections.Generic;
using OptionalUI;
using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    public class MainModOptions : OptionInterface
    {
        private static Vector2 marginX = new();
        private static Vector2 pos = new();
        private static readonly float spacing = 20f;

        private static readonly List<float> boxEndPositions = new();

        private static readonly int numberOfCheckboxes = 3;
        private static readonly float checkBoxSize = 24f;
        private static readonly List<OpCheckBox> checkBoxes = new();
        private static readonly List<OpLabel> checkBoxesTextLabels = new();

        private static readonly float fontHeight = 20f;
        private static readonly List<OpLabel> textLabels = new();

        private static float CheckBoxWithSpacing => checkBoxSize + 0.25f * spacing;

        public MainModOptions() : base(MainMod.instance)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[1];
            Tabs[0] = new OpTab("Options");
            InitializeMarginAndPos();

            // Title
            AddNewLine();
            AddTextLabel("JollyCoop FixesAndStuff Mod", bigText: true);
            DrawTextLabels(ref Tabs[0]);

            // Subtitle
            AddNewLine(0.5f);
            AddTextLabel("Version " + MainMod.instance?.Info.Metadata.Version, FLabelAlignment.Left);
            AddTextLabel("by " + MainMod.instance?.author, FLabelAlignment.Right);
            DrawTextLabels(ref Tabs[0]);

            // Content //
            AddNewLine(2f);
            AddBox();
            AddCheckBox("isSharedRelationshipsEnabled", "Shared Relationships", "This option is meant to prevent tamed lizards from eating your friends. The relationships can still individually change after initialization.", defaultBool: true);
            AddCheckBox("isSlugcatCollisionEnabled", "Slugcat Collision", "When enabled, slugcats (and creatures that are being carried by slugcats) collide with each other.", defaultBool: false);
            AddCheckBox("hasPlayerPointers", "Player Pointer", "When enabled, shows an arrow with the player name above the slugcat. The position is based on the first camera. This is automatically disabled when using SplitScreenMod.", defaultBool: true);

            DrawCheckBoxes(ref Tabs[0]);
            DrawBox(ref Tabs[0]);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }

        public override void ConfigOnChange()
        {
            base.ConfigOnChange();
            MainMod.hasPlayerPointers = bool.Parse(config["hasPlayerPointers"]) && !MainMod.isSplitScreenModEnabled; // disable automatically when SplitScreenMod is used
            MainMod.isSharedRelationshipsEnabled = bool.Parse(config["isSharedRelationshipsEnabled"]);
            MainMod.isSlugcatCollisionEnabled = bool.Parse(config["isSlugcatCollisionEnabled"]);
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void InitializeMarginAndPos()
        {
            marginX = new Vector2(50f, 550f);
            pos = new Vector2(50f, 600f);
        }

        private static void AddNewLine(float spacingModifier = 1f)
        {
            pos.x = marginX.x; // left margin
            pos.y -= spacingModifier * spacing;
        }

        private static void AddBox()
        {
            marginX += new Vector2(spacing, -spacing);
            boxEndPositions.Add(pos.y);
            AddNewLine();
        }

        private static void DrawBox(ref OpTab tab)
        {
            marginX += new Vector2(-spacing, spacing);
            AddNewLine();

            float boxWidth = marginX.y - marginX.x;
            int lastIndex = boxEndPositions.Count - 1;
            tab.AddItems(new OpRect(pos, new Vector2(boxWidth, boxEndPositions[lastIndex] - pos.y)));
            boxEndPositions.RemoveAt(lastIndex);
        }

        private void AddCheckBox(string key, string text, string description, bool? defaultBool = null)
        {
            OpCheckBox opCheckBox = new(new Vector2(), key, defaultBool: defaultBool ?? false)
            {
                description = description
            };

            checkBoxes.Add(opCheckBox);
            checkBoxesTextLabels.Add(new OpLabel(new Vector2(), new Vector2(), text, FLabelAlignment.Left));
        }

        private void DrawCheckBoxes(ref OpTab tab) // changes pos.y but not pos.x
        {
            if (checkBoxes.Count != checkBoxesTextLabels.Count)
            {
                return;
            }

            float width = marginX.y - marginX.x;
            float elementWidth = (width - (numberOfCheckboxes - 1) * 0.5f * spacing) / numberOfCheckboxes;
            pos.y -= checkBoxSize;
            float _posX = pos.x;

            for (int index = 0; index < checkBoxes.Count; index++)
            {
                OpCheckBox checkBox = checkBoxes[index];
                checkBox.pos = new Vector2(_posX, pos.y);
                tab.AddItems(checkBox);
                _posX += CheckBoxWithSpacing;

                OpLabel checkBoxLabel = checkBoxesTextLabels[index];
                checkBoxLabel.pos = new Vector2(_posX, pos.y + 2f);
                checkBoxLabel.size = new Vector2(elementWidth - CheckBoxWithSpacing, fontHeight);
                tab.AddItems(checkBoxLabel);

                if (index < checkBoxes.Count - 1)
                {
                    if ((index + 1) % numberOfCheckboxes == 0)
                    {
                        AddNewLine();
                        pos.y -= checkBoxSize;
                        _posX = pos.x;
                    }
                    else
                    {
                        _posX += elementWidth - CheckBoxWithSpacing + 0.5f * spacing;
                    }
                }
            }

            checkBoxes.Clear();
            checkBoxesTextLabels.Clear();
        }

        private static void AddTextLabel(string text, FLabelAlignment alignment = FLabelAlignment.Center, bool bigText = false)
        {
            float textHeight = (bigText ? 2f : 1f) * fontHeight;
            if (textLabels.Count == 0)
            {
                pos.y -= textHeight;
            }

            OpLabel textLabel = new(new Vector2(), new Vector2(20f, textHeight), text, alignment, bigText) // minimal size.x = 20f
            {
                autoWrap = true
            };
            textLabels.Add(textLabel);
        }

        private static void DrawTextLabels(ref OpTab tab)
        {
            if (textLabels.Count == 0)
            {
                return;
            }

            float width = (marginX.y - marginX.x) / textLabels.Count;
            foreach (OpLabel textLabel in textLabels)
            {
                textLabel.pos = pos;
                textLabel.size += new Vector2(width - 20f, 0.0f);
                tab.AddItems(textLabel);
                pos.x += width;
            }

            pos.x = marginX.x;
            textLabels.Clear();
        }
    }
}