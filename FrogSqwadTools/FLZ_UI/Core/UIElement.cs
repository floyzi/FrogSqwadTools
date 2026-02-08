using FrogSqwad.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace FrogSqwadTools.FLZ_UI.Core
{
    internal abstract class UIElement
    {
        protected Transform ElementInstance;
        internal UIElement(Transform core)
        {
            ElementInstance = core;
            var elements = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetCustomAttribute<UIReferenceAttribute>() != null);

            foreach (var obj in core.GetComponentsInChildren<Transform>(true))
            {
                var field = elements.FirstOrDefault(x => x.GetCustomAttribute<UIReferenceAttribute>().Name == obj.name);
                if (field == null) continue;

                if (field.PropertyType == typeof(CustomButton))
                    field.SetValue(this, obj.GetComponent<Button>().SwapButton());
                else
                    field.SetValue(this, obj.GetComponent(field.PropertyType));
            }
        }
    }
}
