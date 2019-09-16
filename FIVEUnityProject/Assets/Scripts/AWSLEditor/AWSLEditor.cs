﻿using UnityEngine;
using System;
using FIVE.EventSystem;

public class AWSLEditor : MonoBehaviour
{
    public Texture2D EditorBackground;
    public Color EditorTextColor;

    private GUIStyle EditorStyle;
    private GUIStyle ButtonStyle;

    private LauncherEditorArgs code;

    private void Awake()
    {
        enabled = false;
        EventManager.Subscribe<DoLaunchEditor, EventHandler<LauncherEditorArgs>, LauncherEditorArgs>((sender, args) =>
        {
            code = args;
            enabled = true;
        });
    }

    private void Start()
    {
        EditorStyle = new GUIStyle
        {
            font = Font.CreateDynamicFontFromOSFont("Courier New", 16),
            fontSize = 16,
            padding = new RectOffset(20, 20, 20, 20),
            wordWrap = true,
        };
        EditorStyle.normal.textColor = EditorTextColor;
        EditorStyle.normal.background = EditorBackground;

        ButtonStyle = new GUIStyle
        {
            font = Font.CreateDynamicFontFromOSFont("Courier New", 20),
            fontSize = 20,
        };
        ButtonStyle.normal.background = EditorBackground;
        ButtonStyle.normal.textColor = Color.white;
        ButtonStyle.hover.background = EditorBackground;
        ButtonStyle.hover.textColor = Color.blue;
        ButtonStyle.alignment = TextAnchor.MiddleCenter;
    }

    private void OnGUI()
    {
        if (!enabled)
        {
            return;
        }

        Rect r = Screen.safeArea;
        float w = r.width, h = r.height;
        code.Code = GUI.TextArea(new Rect(20, 40, w - 40, h - 120), code.Code, int.MaxValue, EditorStyle);
        if (GUI.Button(new Rect(20, h - 80, w - 40, 60), "Done!", ButtonStyle))
        {
            enabled = false;
        }
    }
}