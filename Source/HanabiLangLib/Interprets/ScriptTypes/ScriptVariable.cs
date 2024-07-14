using HanabiLang.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    public class ScriptVariable : ScriptType
    {
        public string Name { get; private set; }
        /// <summary>
        /// No Type Defined
        /// </summary>
        public HashSet<ScriptClass> DataTypes { get; private set; }
        public ScriptValue Value { get; set; }
        public ScriptFns Get { get; private set; }
        public ScriptFns Set { get; private set; }
        public bool IsConstant { get; private set; }
        public bool IsStatic { get; private set; }
        public AccessibilityLevel Level { get; private set; }

        public ScriptVariable(string name, HashSet<ScriptClass> dataTypes, ScriptValue value, bool isConstant, bool isStatic, AccessibilityLevel level)
        {
            this.Name = name;
            this.DataTypes = dataTypes;
            this.Value = value;
            this.IsConstant = isConstant;
            this.IsStatic = isStatic;
            this.Level = level;
        }

        public ScriptVariable(string name, HashSet<ScriptClass> dataTypes, ScriptFns Get, ScriptFns Set, bool isConstant, bool isStatic, AccessibilityLevel level)
        {
            this.Name = name;
            this.DataTypes = dataTypes;
            this.Get = Get;
            this.Set = Set;
            this.IsConstant = isConstant;
            this.IsStatic = isStatic;
            this.Level = level;
        }

        public ValueReference GetValueReference(ScriptObject _this, AccessibilityLevel accessLevel)
        {
            if (this.Value == null)
            {
                return new ValueReference(
                    () =>
                    {
                        if (this.Get == null)
                            throw new SystemException($"{this.Name} cannot be read");
                        var fnInfo = this.Get.FindCallableInfo();
                        if ((int)accessLevel < (int)fnInfo.Item1.Level)
                            throw new SystemException($"{this.Name} cannot be read");
                        return this.Get.Call(_this, fnInfo);
                    },
                    x =>
                    {
                        if (this.IsConstant || this.Set == null)
                            throw new SystemException($"{this.Name} cannot be written");

                        if (this.DataTypes != null)
                        {
                            if (!x.IsObject)
                                throw new SystemException($"{this.Name} cannot be written by value: {x}, due to wrong data type");
                            if (!this.DataTypes.Contains(x.TryObject?.ClassType))
                                throw new SystemException($"{this.Name} cannot be written by value: {x}, due to wrong data type");
                        }

                        var fnInfo = this.Set.FindCallableInfo(x);
                        if ((int)accessLevel < (int)fnInfo.Item1.Level)
                            throw new SystemException($"{this.Name} cannot be written");
                        this.Set.Call(_this, fnInfo);
                    }
                );
            }
            return new ValueReference(() => this.Value, x =>
            {
                if (this.IsConstant)
                    throw new SystemException($"const {this.Name} cannot be written");

                if (this.DataTypes != null)
                {
                    if (!x.IsObject)
                        throw new SystemException($"{this.Name} cannot be written by value: {x}, due to wrong data type");
                    if (!this.DataTypes.Contains(x.TryObject?.ClassType))
                        throw new SystemException($"{this.Name} cannot be written by value: {x}, due to wrong data type");
                }

                this.Value = x;
            });
        } 

        public override string ToString()
        {
            return $"<variable: {this.Name}>";
        }
    }
}
