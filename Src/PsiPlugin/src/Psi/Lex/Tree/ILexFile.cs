﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.ReSharper.Psi.Tree;

namespace JetBrains.ReSharper.PsiPlugin.Psi.Lex.Tree
{
  public partial interface ILexFile : IFile
  {
    string Namespace { get; }
  }
}