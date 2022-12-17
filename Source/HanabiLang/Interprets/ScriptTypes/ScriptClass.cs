using HanabiLang.Parses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Parses.Nodes;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptClass : ScriptType
    {
        public string Name { get; private set; }
        public List<AstNode> Body { get; private set; }
        public List<string> Constructor { get; private set; }
        public ScriptScope Scope { get; private set; }
        public ScriptObject.CreateBuildInObject CreateBuildInObject { get; private set; }
        public bool IsStatic { get; private set; }
        public bool IsBuildIn => CreateBuildInObject != null;

        public ScriptClass(string name, List<AstNode> body, List<string> constructor,
            ScriptScope scope, bool isStatic, ScriptObject.CreateBuildInObject createBuildInObject = null)
        {
            this.Name = name;
            this.Body = body;
            this.Constructor = constructor;
            this.Scope = scope;
            this.IsStatic = isStatic;
            this.CreateBuildInObject = createBuildInObject; 
        }

        public ScriptValue Call(ScriptScope currentScope, FnCallNode callNode)
        {
            if (this.IsStatic)
                throw new SystemException("Static class cannot create an object");
            var interpreter = new Interpreter(currentScope);
            var parentScope = interpreter.currentScope;

            ScriptObject buildInObject = null;
            if (this.IsBuildIn)
                buildInObject = CreateBuildInObject();
            else
                buildInObject = new ScriptObject(this);

            var classScope = buildInObject.Scope;
            classScope.Parent = parentScope;

            // Data Class
            /*var index = 0;
            foreach (var parameter in this.Constructor)
            {
                classScope.Variables[parameter] = new InterpretedVariable(parameter,
                                                  interpreter.InterpretExpression(
                                                          callNode.Args[index]).Ref, false);
                index++;
            }*/

            interpreter.currentScope = classScope;

            if (!this.IsBuildIn)
            { 
                foreach (var bodyNode in this.Body)
                {
                    interpreter.InterpretChild(bodyNode);
                }
            }

            // A Function with same name as class is constructor
            ScriptFn currentConstructor;
            
            if (interpreter.currentScope.Functions.TryGetValue(callNode.Name, out currentConstructor))
            {
                currentConstructor.Call(interpreter.currentScope, callNode);
            }

            interpreter.currentScope = parentScope;

            return new ScriptValue(buildInObject);
        }

        public override string ToJsonString(int basicIndent = 2, int currentIndent = 0)
        {
            return new string(' ', currentIndent) + $"\"Class\": \"{Name}\"";
        }

        public override string ToString()
        {
            return $"Class: {Name}";
        }
    }
}
