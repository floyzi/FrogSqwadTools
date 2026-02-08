using System;
using System.Collections.Generic;
using System.Text;

namespace FrogSqwadTools.FLZ_UI.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class UIReferenceAttribute(string objectName) : Attribute
    {
        internal string Name => objectName;
    }
}
