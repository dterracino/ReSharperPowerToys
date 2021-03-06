/*
 * Copyright 2007-2014 JetBrains
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using JetBrains.ReSharper.Feature.Services.Tree;
using JetBrains.ReSharper.Psi;

namespace JetBrains.ReSharper.PowerToys.ExploreTypeInterface
{
  /// <summary>
  /// Subclasses default presenter to avoid member qualification
  /// </summary>
  internal class TypeInterfacePresenter : TreeModelBrowserPresenter
  {
    protected override bool IsNaturalParent(object parentValue, object childValue)
    {
      // Never qualify members which are not types
      if (childValue is ITypeMember && !(childValue is ITypeElement))
        return true;
      return base.IsNaturalParent(parentValue, childValue);
    }
  }
}