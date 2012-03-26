﻿using System.Collections.Generic;
using JetBrains.ProjectModel;

namespace JetBrains.ReSharper.PsiPlugin.Grammar
{
  [ProjectFileTypeDefinition(Name)]
  public class PsiProjectFileType : KnownProjectFileType
  {
    public new const string Name = "PSI";
    public static readonly PsiProjectFileType Instance = new PsiProjectFileType();

    private PsiProjectFileType() : base(Name, "Psi", new[] {PsiExtension})
    {
    }

    protected PsiProjectFileType(string name) : base(name)
    {
    }

    protected PsiProjectFileType(string name, string presentableName) : base(name, presentableName)
    {
    }

    protected PsiProjectFileType(string name, string presentableName, IEnumerable<string> extensions) : base(name, presentableName, extensions)
    {
    }

    private const string PsiExtension = ".psi";
  }
}
