﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.Impl.CodeStyle;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.PsiPlugin.ResearchFormatter.Psi;
using JetBrains.Util;

namespace JetBrains.ReSharper.PsiPlugin.ResearchFormatter
{
  public abstract class IndentingStageResearchBase
  {
    private string StandardIndent = "  ";
    protected readonly FormatterResearchBase Formatter;

    public IndentingStageResearchBase(FormatterResearchBase formatter)
    {
      Formatter = formatter;
    }

    public void DoIndent(CodeFormattingContext context, IProgressIndicator progress, bool indent = false)
    {
      var indentRanges = BuildRanges(context);
      var nodePairs = context.SequentialEnumNodes().Where(p => context.CanModifyInsideNodeRange(p.First, p.Last)).ToList();
      var indents = nodePairs.
        Select(range => new FormatResult<string>(range, CalcIndent(new FormattingStageContext(range), indentRanges, indent))).
        Where(res => res.ResultValue != null);

      FormatterImplHelper.ForeachResult(
        indents,
        progress,
        res => MakeIndent(res.Range.Last, res.ResultValue));
    }

    private IList<IndentRange> BuildRanges(CodeFormattingContext context)
    {
      var parent = context.FirstNode.FindCommonParent(context.LastNode);
      return BuildRanges(parent);
    }

    private IList<IndentRange> BuildRanges(ITreeNode node)
    {
      var range = Match(node);
      if (range != null)
      {
        var ranges = BuildChildRanges(node);
        range.AddChildRanges(ranges);
        return new List<IndentRange> { range };
      }
      return BuildChildRanges(node);
    }

    private IndentRange Match(ITreeNode node)
    {
      IndentRange range = null;
      foreach (var indentingRangeType in Formatter.IndentingRules)
      {
        if (indentingRangeType.Match(node))
        {
          LinkedList<ITreeNode> nodes = new LinkedList<ITreeNode>();
          /*var token = node.GetFirstTokenIn();
          token = token.GetPrevToken();
          while((token != null) && (token.IsWhitespaceToken()))
          {
            token = token.GetPrevToken();
            nodes.AddFirst(token);
          }*/
          nodes.AddLast(node);
          range = new IndentRange(nodes.ToArray());
        }
      }
      return range;
    }

    private IList<IndentRange> BuildChildRanges(ITreeNode node)
    {
      var child = node.FirstChild;
      IList<IndentRange> ranges = new List<IndentRange>();
      while (child != null)
      {
        var childRanges = BuildChildRanges(child);
        var range = Match(child);
        if (range != null)
        {
          range.AddChildRanges(childRanges);
          ranges.Add(range);
        }
        else
        {
          ranges.AddRange(childRanges);
        }
        child = child.NextSibling;
      }
      return ranges;
    }

    private string CalcIndent(FormattingStageContext formattingStageContext, IList<IndentRange> indentRanges, bool indent)
    {
      ITreeNode rChild = formattingStageContext.RightChild;
      if ((!HasLineFeedsTo(formattingStageContext.LeftChild, rChild)))
      {
        return null;
      }
      var offset = formattingStageContext.RightChild.GetTreeStartOffset();
      IndentRange range = null;
      foreach (var indentRange in indentRanges)
      {
        if (indentRange.Contains(offset))
        {
          range = indentRange;
          break;
        }
      }
      string parentIndent = "";
      if (range == null)
      {
        parentIndent = GetIndent(formattingStageContext.Parent);
        return parentIndent;
      }
      /*while(range.Parent != null)
        {
          range = range.Parent;
        }*/
      if (indent)
      {
        parentIndent = GetIndent(range.Nodes[0].Parent);
      }
      string selfIndent = CalcSelfIndent(range, offset);
      return selfIndent;
    }

    private string CalcSelfIndent(IndentRange indentRange, TreeOffset offset)
    {
      IList<TreeOffset> newLineOffsets = new List<TreeOffset>();
      var token = indentRange.Nodes[0].GetFirstTokenIn();
      var prevToken = token.GetPrevToken();
      /*while ((prevToken != null) && (prevToken.GetTokenType() != NewLineType))
      {
        prevToken = prevToken.GetPrevToken();
      }*/
      if(prevToken != null)
      {
        if(prevToken.GetTokenType() == NewLineType)
        {
          //if(indentRange.Contains(prevToken.GetTreeStartOffset()))
          //{
            newLineOffsets.Add(prevToken.GetTreeEndOffset());
          //}
        }
      }
      while ((token != null) && (indentRange.Contains(token.GetTreeEndOffset())))
      {
        if (token.GetTokenType() == NewLineType)
        {
          newLineOffsets.Add(token.GetTreeEndOffset());
        }
        token = token.GetNextToken();
      }
      if (newLineOffsets.Count == 0)
      {
        return "";
      }
      bool hasOnlyParentnewLine = false;
      IndentRange range = null;
      foreach (var childrange in indentRange.ChildRanges)
      {
        if (childrange.Contains(offset))
        {
          range = childrange;
          break;
        }
      }
      foreach (var newLineOffset in newLineOffsets)
      {
        bool containsInChildrange = false;
        foreach (var childRange in indentRange.ChildRanges)
        {
          if (childRange.Contains(newLineOffset))
          {
            containsInChildrange = true;
            break;
          }
        }
        if (!containsInChildrange)
        {
          hasOnlyParentnewLine = true;
          break;
        }
      }
      string s = "";
      if (hasOnlyParentnewLine)
      {
        s = StandardIndent;
      }
      if (range != null)
      {
        return s + CalcSelfIndent(range, offset);
      }
      return s;
    }

    private string GetIndent(ITreeNode treeNode)
    {
      string s = "";
      var token = treeNode.GetPreviousToken();
      while ((token != null) && (token.GetTokenType() != NewLineType))
      {
        token = token.GetPrevToken();
      }
      if (token == null)
      {
        return s;
      }
      token = token.GetNextToken();
      while ((token != null) && (token.GetTokenType() == WhiteSpaceType))
      {
        s = s + token.GetText();
        token = token.GetNextToken();
      }
      return s;
    }

    public void MakeIndent(ITreeNode indentNode, string indent)
    {
      var lastSpace = AsWhitespaceNode(indentNode.PrevSibling);
      if (lastSpace != null && lastSpace.GetTokenType() != NewLineType)
      {
        var firstSpace = LeftWhitespaces(lastSpace).TakeWhile(ws => ws.GetTokenType() != NewLineType).LastOrDefault() ?? lastSpace;

        if (firstSpace != lastSpace || lastSpace.GetText() != indent)
          if (indent.IsEmpty())
            LowLevelModificationUtil.DeleteChildRange(firstSpace, lastSpace);
          else
            LowLevelModificationUtil.ReplaceChildRange(firstSpace, lastSpace, CreateSpace(indent));
      }
      else if (!indent.IsEmpty())
        LowLevelModificationUtil.AddChildBefore(indentNode, CreateSpace(indent));
    }

    protected abstract ITreeNode[] CreateSpace(string indent);

    public virtual TokenNodeType NewLineType { get; private set; }
    public virtual TokenNodeType WhiteSpaceType { get; private set; }
    public abstract ITreeNode AsWhitespaceNode(ITreeNode node);


    private IEnumerable<ITreeNode> LeftWhitespaces(ITreeNode node)
    {
      return node.LeftSiblings().Select(sib => AsWhitespaceNode(sib)).TakeWhile(sib => sib != null);
    }

    public bool HasLineFeedsTo(ITreeNode fromNode, ITreeNode toNode)
    {
      return GetLineFeedsTo(fromNode, toNode).Any();
    }

    private IEnumerable<ITreeNode> GetLineFeedsTo(ITreeNode fromNode, ITreeNode toNode)
    {
      return fromNode.GetWhitespacesTo(toNode).Where(wsNode => (wsNode.GetTokenType() == NewLineType) && (AsWhitespaceNode(wsNode) != null));
    }
  }
}
