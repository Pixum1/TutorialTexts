using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;

public class TutorialText : MonoBehaviour
{
    [Header("References")]
    public TMP_SpriteAsset keyboardSprites;
    public TMP_SpriteAsset controllerSprites;
    [HideInInspector] public TMP_SpriteAsset currentSpriteAsset;
    [SerializeField]
    private TMP_Text textField;

    [SerializeField]
    public List<string> keywords;
    [HideInInspector] public List<int> keywordIndex;

    private const string controller = "Controller";
    private const string keyboard = "Keyboard";
    [HideInInspector] public string inputMethod = controller;

    [HideInInspector] public string tutorialText;

    [SerializeField]
    private GameObject trigger;
    [SerializeField]
    private LayerMask playerLayer;
    private bool triggered = false;

    private void Awake()
    {
        if (keywords == null)
            keywords = new List<string>();
        if (keywordIndex == null)
            keywordIndex = new List<int>();
    }

    private void Start()
    {
        GetComponentInChildren<Canvas>().worldCamera = Camera.main;
        textField.color = new Color(textField.color.r, textField.color.g, textField.color.b, 0);
        textField.spriteAsset = currentSpriteAsset;

    }

    private void Update()
    {
        #region Get Input Method
        if (Input.GetAxisRaw("ControllerInput") != 0 && inputMethod != controller)
        {
            StopCoroutine(FadeText());
            inputMethod = "Controller";
            if (textField.color.a > 0)
                StartCoroutine(FadeText());
        }
        if (Input.GetAxisRaw("KeyboardInput") != 0 && inputMethod != keyboard)
        {
            StopCoroutine(FadeText());
            inputMethod = "Keyboard";
            if (textField.color.a > 0)
                StartCoroutine(FadeText());
        }

        if (inputMethod == controller && controllerSprites != null)
            currentSpriteAsset = controllerSprites;
        else if (inputMethod == keyboard && keyboardSprites != null)
            currentSpriteAsset = keyboardSprites;

        #endregion

        if (Physics2D.OverlapBox(trigger.transform.position, trigger.transform.localScale, 0, playerLayer) && !triggered)
        {
            UpdateText();
            StartCoroutine(TriggerText());
        }
        else if(Physics2D.OverlapBox(trigger.transform.position, trigger.transform.localScale, 0, playerLayer) == null && !triggered)
        {
            StartCoroutine(CloseText());
        }
    }

    public void UpdateText()
    {
        textField.spriteAsset = currentSpriteAsset;
        string tempText = tutorialText;
        for (int i = 0; i < keywords.Count; i++)
        {
            if (tutorialText.Contains(keywords[i]))
            {
                tempText = tempText.Replace(keywords[i], $"<sprite={keywordIndex[i]}>");
            }
        }
        textField.text = tempText;
    }

    private IEnumerator TriggerText()
    {
        triggered = true;
        while (textField.color.a < 1)
        {
            textField.color = new Color(textField.color.r, textField.color.g, textField.color.b, textField.color.a + Time.deltaTime * 2f);
            yield return null;
        }
        triggered = false;
    }
    private IEnumerator CloseText()
    {
        triggered = true;
        while (textField.color.a > 0)
        {
            textField.color = new Color(textField.color.r, textField.color.g, textField.color.b, textField.color.a - Time.deltaTime * 2f);
            yield return null;
        }
        triggered = false;
    }
    private IEnumerator FadeText()
    {
        while (textField.color.a > 0)
        {
            textField.color = new Color(textField.color.r, textField.color.g, textField.color.b, textField.color.a - Time.deltaTime * 4f);
            yield return null;
        }
        textField.spriteAsset = currentSpriteAsset;
        while (textField.color.a < 1)
        {
            textField.color = new Color(textField.color.r, textField.color.g, textField.color.b, textField.color.a + Time.deltaTime * 4f);
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(trigger.transform.position, trigger.transform.localScale);
    }
}

[CustomEditor(typeof(TutorialText))]
public class TutorialTextEditor : Editor
{
    int selected = 0;

    [SerializeField]
    private string keyword;
    [SerializeField]
    private int index;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(20f);
        GUILayout.Label("Displayed Tutorial Text");
        TutorialText text = (TutorialText)target;

        GUILayout.BeginVertical();

        text.tutorialText = EditorGUILayout.TextArea(text.tutorialText);

        GUILayout.Space(20f);
        GUILayout.Label("Create new Keywords");

        keyword = EditorGUILayout.TextField("Keyword: ",keyword);
        index = EditorGUILayout.IntField("Sprite Asset Index: ", index);
        if (GUILayout.Button("Add Keyword"))
        {
            if (keyword != null && !text.keywordIndex.Contains(index))
            {
                text.keywords.Add(keyword);
                text.keywordIndex.Add(index);
            }
        }

        GUILayout.EndVertical();

        GUILayout.Space(20f);
        GUILayout.Label("Preview Settings");

        string[] options = new string[]
        {
            "Controller", "Keyboard",
        };
        selected = EditorGUILayout.Popup("Preview Input Methods", selected, options);

        if (selected == 0)
        {
            text.inputMethod = "Controller";
            text.currentSpriteAsset = text.controllerSprites;
        }
        else if (selected == 1)
        {
            text.inputMethod = "Keyboard";
            text.currentSpriteAsset = text.keyboardSprites;
        }

        if (GUILayout.Button("Show Text"))
        {
            text.UpdateText();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
