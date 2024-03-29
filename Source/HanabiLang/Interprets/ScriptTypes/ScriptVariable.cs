﻿using HanabiLang.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptVariable : ScriptType
    {
        public string Name { get; private set; }
        public ScriptValue Value { get; set; }
        public ScriptFns Get { get; private set; }
        public ScriptFns Set { get; private set; }
        public bool IsConstant { get; private set; }
        public bool IsStatic { get; private set; }
        public AccessibilityLevel Level { get; private set; }

        public ScriptVariable(string name, ScriptValue value, bool isConstant, bool isStatic, AccessibilityLevel level)
        {
            this.Name = name;
            this.Value = value;
            this.IsConstant = isConstant;
            this.IsStatic = isStatic;
            this.Level = level;
        }

        public ScriptVariable(string name, ScriptFns Get, ScriptFns Set, bool isConstant, bool isStatic, AccessibilityLevel level)
        {
            this.Name = name;
            this.Get = Get;
            this.Set = Set;
            this.IsConstant = isConstant;
            this.IsStatic = isStatic;
            this.Level = level;
        }

        public override string ToString()
        {
            return $"<variable: {this.Name}>";
        }
    }
}
