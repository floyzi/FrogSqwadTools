using FrogSqwad.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace FrogSqwadTools
{
    internal static class FLZ_Extensions
    {
        internal static CustomButton SwapButton(this Button oldBtn)
        {
            if (oldBtn == null) return null;

            var go = oldBtn.gameObject;

            var trans = oldBtn.transition;
            var graph = oldBtn.targetGraphic;
            var inter = oldBtn.interactable;
            var cols = oldBtn.colors;
            var onclick = oldBtn.onClick;

            GameObject.DestroyImmediate(oldBtn);

            var joinBtn = go.AddComponent<CustomButton>();

            joinBtn.transition = trans;
            joinBtn.targetGraphic = graph;
            joinBtn.interactable = inter;
            joinBtn.colors = cols;
            joinBtn.onClick = onclick;

            return joinBtn;
        }
    }
}
