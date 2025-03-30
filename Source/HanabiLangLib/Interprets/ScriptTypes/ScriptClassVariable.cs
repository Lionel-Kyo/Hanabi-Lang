//using HanabiLang.Interprets.ScriptTypes;
//using HanabiLang.Interprets;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using HanabiLang;
//using HanabiLang.Parses.Nodes;

//namespace HanabiLangLib.Interprets.ScriptTypes
//{
//    internal class ScriptClassVariable
//    {
//        public string Name { get; private set; }
//        public HashSet<ScriptClass> DataTypes { get; private set; }
//        public bool IsInterpreted { get; private set; }

//        // Interpreted
//        public ScriptFns GetFn { get; private set; }
//        public ScriptFns SetFn { get; private set; }
//        //

//        // Not Interpreted
//        public AstNode ValueNode { get; private set; }
//        public AstNode DataTypeNode { get; private set; }
//        public FnDefineNode GetFnNode { get; private set; }
//        public FnDefineNode SetFnNode { get; private set; }
//        //

//        public bool IsConstant { get; private set; }
//        public bool IsStatic { get; private set; }
//        public AccessibilityLevel Level { get; private set; }

//        public ScriptClassVariable(string name, HashSet<ScriptClass> dataTypes, ScriptFns get, ScriptFns set, bool isConstant, bool isStatic, AccessibilityLevel level)
//        {
//            this.Name = name;
//            this.DataTypes = dataTypes;
//            this.IsInterpreted = true;
//            this.GetFn = get;
//            this.SetFn = set;
//            this.IsConstant = isConstant;
//            this.IsStatic = isStatic;
//            this.Level = level;
//        }

//        public ScriptClassVariable(string name, HashSet<ScriptClass> dataTypes, AstNode value, AstNode dataType, FnDefineNode getFn, FnDefineNode setFn, bool isConstant, bool isStatic, AccessibilityLevel level)
//        {
//            this.Name = name;
//            this.DataTypes = dataTypes;
//            this.IsInterpreted = true;
//            this.ValueNode = value;
//            this.DataTypeNode = dataType;
//            this.GetFnNode = getFn;
//            this.SetFnNode = setFn;
//            this.IsConstant = isConstant;
//            this.IsStatic = isStatic;
//            this.Level = level;
//        }
//    }
//}
