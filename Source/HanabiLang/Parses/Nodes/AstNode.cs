﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    abstract class AstNode
    {
        public int Line;
        public string NodeName
        {
            get
            {
                string removeText = "Node";
                string result = this.GetType().Name;
                if (result.EndsWith(removeText))
                    result = result.Remove(result.Length - removeText.Length, removeText.Length);
                return result;
            }
        }
    }
}