using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public static class GUIUtility
    {
        private struct GUIState
        {
            public GameFont font;
            public FontStyle curStyle;
            public FontStyle curTextAreaReadOnlyStyle;
            public FontStyle curTextAreaStyle;
            public FontStyle curTextFieldStyle;
            public TextAnchor anchor;
            public Color color;
            public Color contentColor;
            public Color backgroundColor;
            public bool wordWrap;
        }

        private readonly static List<GUIState> queue = new List<GUIState>();

        public static void StashGUIState()
        {
            queue.Add(new GUIState()
            {
                font = Text.Font,
                curStyle = Text.CurFontStyle.fontStyle,
                curTextAreaReadOnlyStyle = Text.CurTextAreaReadOnlyStyle.fontStyle,
                curTextAreaStyle = Text.CurTextAreaStyle.fontStyle,
                curTextFieldStyle = Text.CurTextFieldStyle.fontStyle,
                anchor = Text.Anchor,
                color = GUI.color,
                wordWrap = Text.WordWrap,
                contentColor = GUI.contentColor,
                backgroundColor = GUI.backgroundColor
            });
        }

        public static void RestoreGUIState()
        {
            GUIState config = queue.Last();
            queue.RemoveLast();
            Text.Font = config.font;
            Text.CurFontStyle.fontStyle = config.curStyle;
            Text.CurTextAreaReadOnlyStyle.fontStyle = config.curTextAreaReadOnlyStyle;
            Text.CurTextAreaStyle.fontStyle = config.curTextAreaStyle;
            Text.CurTextFieldStyle.fontStyle = config.curTextFieldStyle;
            Text.WordWrap = config.wordWrap;
            Text.Anchor = config.anchor;
            GUI.color = config.color;
            GUI.contentColor = config.contentColor;
            GUI.backgroundColor = config.backgroundColor;
        }

        public static Exception ExecuteSafeGUIAction(Action function, Action fallbackAction = null, bool catchExceptions = false)
        {
            StashGUIState();
            Exception exception = null;
            try
            {
                function.Invoke();
            }
            catch (Exception er)
            {
                exception = er;
            }
            finally
            {
                RestoreGUIState();
            }
            if (exception != null && !catchExceptions)
            {
                if (fallbackAction != null)
                    exception = ExecuteSafeGUIAction(
                        fallbackAction,
                        catchExceptions: false
                        );
                if (exception != null)
                    throw exception;
            }
            return exception;
        }
    }
}
