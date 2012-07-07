﻿using System.Collections.Generic;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Impl.Shared;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.PsiPlugin.Resolve;
using JetBrains.ReSharper.PsiPlugin.Tree;
using JetBrains.ReSharper.Psi.Web.Generation;

namespace JetBrains.ReSharper.PsiPlugin.GeneratedDocument
{
  internal class CSharpFromPsiGenerator
  {
    private readonly IPsiFile myFile;
    private readonly IDictionary<string, string> myClassesToNamespaces = new Dictionary<string, string>();

    private GenerationResults myGeneratedMethodBody;
    private GenerationResults myGeneratedFile;

    public CSharpFromPsiGenerator(IPsiFile file)
    {
      myFile = file;
      myClassesToNamespaces.Add("parserClassName", "parserPackage");
      myClassesToNamespaces.Add("psiStubsBaseClass", "psiStubsPackageName");
      myClassesToNamespaces.Add("visitorClassName", "psiInterfacePackageName");
    }

    public GenerationResults Generate()
    {
      myGeneratedMethodBody = new GenerationResults(CSharpLanguage.Instance, "", GeneratedRangeMapFactory.CreateGeneratedRangeMap(myFile));
      myGeneratedFile = new GenerationResults(CSharpLanguage.Instance, "", GeneratedRangeMapFactory.CreateGeneratedRangeMap(myFile));

      AddOptions(myFile);

      myGeneratedFile.Append(new GenerationResults(CSharpLanguage.Instance, "class A{\n void foo(){\n", GeneratedRangeMapFactory.CreateGeneratedRangeMap(myFile)));
      myGeneratedFile.Append(myGeneratedMethodBody);
      myGeneratedFile.Append(new GenerationResults(CSharpLanguage.Instance, "}\n }\n", GeneratedRangeMapFactory.CreateGeneratedRangeMap(myFile)));

      return myGeneratedFile;
    }

    private void AddOptions(ITreeNode treeNode)
    {
      if (treeNode is IOptionsDefinition || treeNode is IInterfacesDefinition || treeNode is IRuleDeclaration || treeNode is IPsiFile)
      {
        var child = treeNode.FirstChild;
        while (child != null)
        {
          AddOptions(child);
          child = child.NextSibling;
        }
      }
      var optionDefinition = treeNode as IOptionDefinition;
      if (optionDefinition != null)
      {
        AddOption(optionDefinition);
      }
    }

    private void AddOption(IOptionDefinition optionDefinition)
    {
      var value = optionDefinition.OptionStringValue;
      if (value != null)
      {
        var optionName = GetOptionNameWithoutQuotes(optionDefinition);

        string optionValueText = value.GetText();
        int startOffset = value.GetTreeStartOffset().Offset;
        if ((optionValueText.Length > 0) && ("\"".Equals(optionValueText.Substring(0, 1))))
        {
          optionValueText = optionValueText.Substring(1, optionValueText.Length - 1);
          startOffset++;
        }
        int endOffset = value.GetTreeEndOffset().Offset;
        if ((optionValueText.Length > 0) && ("\"".Equals(optionValueText.Substring(optionValueText.Length - 1, 1))))
        {
          optionValueText = optionValueText.Substring(0, optionValueText.Length - 1);
          endOffset--;
        }

        if (optionValueText.Length > 0)
        {
          if (OptionDeclaredElements.NamespacesOptions.Contains(optionName))
          {
            AddNamespaceOption(endOffset, optionValueText, startOffset);
          }
          else if (OptionDeclaredElements.ClassesOptions.Contains(optionName))
          {
            AddClassesOption(optionDefinition, endOffset, startOffset, optionValueText);
          }
          else if (OptionDeclaredElements.MethodsOptions.Contains(optionName))
          {
            AddMethodOption(endOffset, optionValueText, startOffset);
          }
        }
      }
    }

    private static string GetOptionNameWithoutQuotes(IOptionDefinition optionDefinition)
    {
      string optionName = optionDefinition.OptionName.GetText();
      if ((optionName.Length > 0) && ("\"".Equals(optionName.Substring(0, 1))))
      {
        optionName = optionName.Substring(1, optionName.Length - 1);
      }
      if ((optionName.Length > 0) && ("\"".Equals(optionName.Substring(optionName.Length - 1, 1))))
      {
        optionName = optionName.Substring(0, optionName.Length - 1);
      }
      return optionName;
    }

    private void AddClassesOption(IOptionDefinition optionDefinition, int endOffset, int startOffset, string optionValueText)
    {
      var classes = optionDefinition.GetPsiServices().CacheManager.GetDeclarationsCache(optionDefinition.GetPsiModule(), false, true).GetTypeElementsByCLRName(optionValueText);
      foreach (var typeElement in classes)
      {
        var cls = typeElement as IClass;

        if(cls != null){
            AddStaticClassOption(endOffset, startOffset, optionValueText);
            return;
        }
      }
      AddClassOption(endOffset, startOffset, optionValueText);
    }

    private void AddClassOption(int endOffset, int startOffset, string optionValueText)
    {
      var map = GeneratedRangeMapFactory.CreateGeneratedRangeMap(myFile);
      map.Add(new TreeTextRange<Generated>(new TreeOffset(), new TreeOffset(optionValueText.Length)),
        new TreeTextRange<Original>(new TreeOffset(startOffset), new TreeOffset(endOffset)));
      myGeneratedMethodBody.Append(new GenerationResults(CSharpLanguage.Instance, optionValueText + " a;\n", map));
    }

    private void AddStaticClassOption(int endOffset, int startOffset, string optionValueText)
    {
      var staticMap = GeneratedRangeMapFactory.CreateGeneratedRangeMap(myFile);
      staticMap.Add(new TreeTextRange<Generated>(new TreeOffset(14), new TreeOffset(optionValueText.Length + 14)),
        new TreeTextRange<Original>(new TreeOffset(startOffset), new TreeOffset(endOffset)));
      myGeneratedFile.Append(new GenerationResults(CSharpLanguage.Instance,"using __alias=" + optionValueText + "\n", staticMap));
    }

    private void AddMethodOption(int endOffset, string optionValueText, int startOffset)
    {
      var map = GeneratedRangeMapFactory.CreateGeneratedRangeMap(myFile);
      map.Add(new TreeTextRange<Generated>(new TreeOffset(8), new TreeOffset(optionValueText.Length + 8)),
        new TreeTextRange<Original>(new TreeOffset(startOffset), new TreeOffset(endOffset)));
      myGeneratedMethodBody.Append(new GenerationResults(CSharpLanguage.Instance, "var a = " + optionValueText + ";\n", map));
    }

    private void AddNamespaceOption(int endOffset, string optionValueText, int startOffset)
    {
      var map = GeneratedRangeMapFactory.CreateGeneratedRangeMap(myFile);
      map.Add(new TreeTextRange<Generated>(new TreeOffset(6), new TreeOffset(6 + optionValueText.Length)),
        new TreeTextRange<Original>(new TreeOffset(startOffset), new TreeOffset(endOffset)));
      myGeneratedFile.Append(new GenerationResults(CSharpLanguage.Instance, "using " + optionValueText + ";\n", map));
    }
  }
}