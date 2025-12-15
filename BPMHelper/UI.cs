using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BPMHelper
{
    // Lots of code here is based on/copied from Loloppe's Ratings.UI code
    internal class UI
    {
        public GameObject menu;
        private readonly BPMHelper bpmHelper;

        public UI(BPMHelper bpmHelper)
        {
            this.bpmHelper = bpmHelper;
            ExtensionButtons.AddButton(PersistentUI.Instance.Sprites.Background, "Open BPM Helper", () => menu.SetActive(!menu.activeSelf));
        }

        public void AddMenu(MapEditorUI mapEditorUI) {
            CanvasGroup parent = mapEditorUI.MainUIGroup[5];
            menu = new GameObject("BPM Helper Menu");
            menu.transform.parent = parent.transform;
            AttachTransform(menu, 100, 125, 1, 1, 0, 0, 1, 1);
            Image image = menu.AddComponent<Image>();
            image.sprite = PersistentUI.Instance.Sprites.Background;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.24f, 0.24f, 0.24f);

            AddButton(menu.transform, "Add Initial BPM", "Add Initial BPM", new Vector2(0, -25), bpmHelper.AddInitialBPM);
            AddButton(menu.transform, "Add Middle BPM", "Add Middle BPM", new Vector2(0, -50), bpmHelper.AddMiddleBPM);
            AddButton(menu.transform, "Add Final BPM", "Add Final BPM", new Vector2(0, -75), () => bpmHelper.AddFinalBPM());
            AddTextInput(menu.transform, "Number of Beats", "Number of Beats", new Vector2(0, -100), "1", bpmHelper.UpdateNumberOfBeats);

            menu.SetActive(false);
        }

        private void AddButton(Transform parent, string title, string text, Vector2 pos, UnityAction onClick)
        {
            var button = Object.Instantiate(PersistentUI.Instance.ButtonPrefab, parent);
            MoveTransform(button.transform, 60, 25, 0.5f, 1, pos.x, pos.y);

            button.name = title;
            button.Button.onClick.AddListener(onClick);

            button.SetText(text);
            button.Text.enableAutoSizing = false;
            button.Text.fontSize = 12;
        }

        private void AddTextInput(Transform parent, string title, string text, Vector2 pos, string value, UnityAction<string> onChange, bool interactable = true, string tooltip = "")
        {
            var entryLabel = new GameObject(title + " Label", typeof(TextMeshProUGUI));
            var rectTransform = ((RectTransform)entryLabel.transform);
            rectTransform.SetParent(parent);

            MoveTransform(rectTransform, 30, 16, 0.5f, 1, pos.x - 27.5f, pos.y);
            var textComponent = entryLabel.GetComponent<TextMeshProUGUI>();

            textComponent.name = title;
            textComponent.font = PersistentUI.Instance.ButtonPrefab.Text.font;
            textComponent.alignment = TextAlignmentOptions.Right;
            textComponent.fontSize = 12;
            textComponent.text = text;

            var textInput = Object.Instantiate(PersistentUI.Instance.TextInputPrefab, parent);
            MoveTransform(textInput.transform, 55, 20, 0.5f, 1, pos.x + 27.5f, pos.y);
            textInput.GetComponent<Image>().pixelsPerUnitMultiplier = 3;
            textInput.InputField.text = value;
            textInput.InputField.onFocusSelectAll = false;
            textInput.InputField.textComponent.alignment = TextAlignmentOptions.Left;
            textInput.InputField.textComponent.fontSize = 10;

            textInput.InputField.onValueChanged.AddListener(onChange);
            if (!interactable)
                textInput.InputField.interactable = false;

            if (!string.IsNullOrWhiteSpace(tooltip))
            {
                var tt = textInput.InputField.gameObject.AddComponent<Tooltip>();
                tt.TooltipOverride = tooltip;
            }
        }

        private RectTransform AttachTransform(GameObject obj, float sizeX, float sizeY, float anchorX, float anchorY, float anchorPosX, float anchorPosY, float pivotX = 0.5f, float pivotY = 0.5f)
        {
            RectTransform rectTransform = obj.AddComponent<RectTransform>();
            rectTransform.localScale = new Vector3(1, 1, 1);
            rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
            rectTransform.pivot = new Vector2(pivotX, pivotY);
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(anchorX, anchorY);
            rectTransform.anchoredPosition = new Vector3(anchorPosX, anchorPosY, 0);

            return rectTransform;
        }

        private void MoveTransform(Transform transform, float sizeX, float sizeY, float anchorX, float anchorY, float anchorPosX, float anchorPosY, float pivotX = 0.5f, float pivotY = 0.5f)
        {
            if (!(transform is RectTransform rectTransform)) return;

            rectTransform.localScale = new Vector3(1, 1, 1);
            rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
            rectTransform.pivot = new Vector2(pivotX, pivotY);
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(anchorX, anchorY);
            rectTransform.anchoredPosition = new Vector3(anchorPosX, anchorPosY, 0);
        }
    }
}
