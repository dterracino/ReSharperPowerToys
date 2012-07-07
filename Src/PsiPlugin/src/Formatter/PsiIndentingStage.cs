using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Impl.CodeStyle;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.PsiPlugin.Tree;

namespace JetBrains.ReSharper.PsiPlugin.Formatter
{
  public class PsiIndentingStage
  {
    private readonly bool myInTypingAssist;
    private readonly PsiIndentCache myIndentCache;
    private readonly PsiIndentVisitor myIndentVisitor;
    [UsedImplicitly]
    private PsiCodeFormattingSettings myFormattingSettings;

    private PsiIndentingStage(PsiCodeFormattingSettings formattingSettings, bool inTypingAssist)
    {
      myFormattingSettings = formattingSettings;
      myInTypingAssist = inTypingAssist;
      myIndentCache = new PsiIndentCache();
      myIndentVisitor = CreateIndentVisitor(myIndentCache, inTypingAssist);
    }

    public static void DoIndent(PsiCodeFormattingSettings formattingSettings, CodeFormattingContext context, IProgressIndicator progress, bool inTypingAssist)
    {
      var stage = new PsiIndentingStage(formattingSettings, inTypingAssist);
      List<FormattingRange> nodePairs = context.SequentialEnumNodes().Where(p => context.CanModifyInsideNodeRange(p.First, p.Last)).ToList();
      IEnumerable<FormatResult<string>> indents = nodePairs.
        Select(range => new FormatResult<string>(range, stage.CalcIndent(new FormattingStageContext(range)))).
        Where(res => res.ResultValue != null);

      FormatterImplHelper.ForeachResult(
        indents,
        progress,
        res => res.Range.Last.MakeIndent(res.ResultValue));
    }

    private string CalcIndent(FormattingStageContext context)
    {
      CompositeElement parent = context.Parent;

      ITreeNode rChild = context.RightChild;
      if ((!context.LeftChild.HasLineFeedsTo(rChild)) && (!myInTypingAssist))
      {
        //if ((!context.LeftChild.HasLineFeedsTo(rChild)))
        return null;
      }

      var psiTreeNode = context.Parent as IPsiTreeNode;

      return psiTreeNode != null
        ? psiTreeNode.Accept(myIndentVisitor, context)
        : myIndentVisitor.VisitNode(parent, context);
    }

    [NotNull]
    private PsiIndentVisitor CreateIndentVisitor([NotNull] PsiIndentCache indentCache, bool inTypingAssist)
    {
      return new PsiIndentVisitor(indentCache, inTypingAssist);
    }
  }
}