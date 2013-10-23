// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class ComplexTypeMapping : EFElement
    {
        internal static readonly string ElementName = "ComplexTypeMapping";
        internal static readonly string AttributeTypeName = "TypeName";
        internal static readonly string AttributeIsPartial = "IsPartial";

        private SingleItemBinding<ComplexType> _typeName;
        private DefaultableValue<bool> _isPartial;

        private readonly List<ScalarProperty> _scalarProperties = new List<ScalarProperty>();
        private readonly List<ComplexProperty> _complexProperties = new List<ComplexProperty>();
        private readonly List<Condition> _conditions = new List<Condition>();

        internal ComplexTypeMapping(EFContainer parent, XElement element)
            : base(parent, element)
        {
        }

        internal SingleItemBinding<ComplexType> TypeName
        {
            get
            {
                if (_typeName == null)
                {
                    _typeName = new SingleItemBinding<ComplexType>(
                        this, AttributeTypeName, EFNormalizableItemDefaults.DefaultNameNormalizerForMSL);
                }
                return _typeName;
            }
        }

        internal DefaultableValue<bool> IsPartial
        {
            get
            {
                if (_isPartial == null)
                {
                    _isPartial = new IsPartialDefaultableValue(this);
                }
                return _isPartial;
            }
        }

        private class IsPartialDefaultableValue : DefaultableValue<bool>
        {
            internal IsPartialDefaultableValue(EFElement parent)
                : base(parent, AttributeIsPartial)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeIsPartial; }
            }

            public override bool DefaultValue
            {
                get { return false; }
            }
        }

        internal IList<ScalarProperty> ScalarProperties()
        {
            return _scalarProperties.AsReadOnly();
        }

        internal IList<ComplexProperty> ComplexProperties()
        {
            return _complexProperties.AsReadOnly();
        }

        internal IList<Condition> Conditions()
        {
            return _conditions.AsReadOnly();
        }

        // we unfortunately get a warning from the compiler when we use the "base" keyword in "iterator" types generated by using the
        // "yield return" keyword.  By adding this method, I was able to get around this.  Unfortunately, I wasn't able to figure out
        // a way to implement this once and have derived classes share the implementation (since the "base" keyword is resolved at 
        // compile-time and not at runtime.
        private IEnumerable<EFObject> BaseChildren
        {
            get { return base.Children; }
        }

        internal override IEnumerable<EFObject> Children
        {
            get
            {
                foreach (var efobj in BaseChildren)
                {
                    yield return efobj;
                }

                foreach (var child in ScalarProperties())
                {
                    yield return child;
                }

                foreach (var child2 in ComplexProperties())
                {
                    yield return child2;
                }

                foreach (var child4 in Conditions())
                {
                    yield return child4;
                }

                yield return TypeName;
                yield return IsPartial;
            }
        }

        protected override void OnChildDeleted(EFContainer efContainer)
        {
            var sp = efContainer as ScalarProperty;
            if (sp != null)
            {
                _scalarProperties.Remove(sp);
                return;
            }

            var cp = efContainer as ComplexProperty;
            if (cp != null)
            {
                _complexProperties.Remove(cp);
                return;
            }

            var cond = efContainer as Condition;
            if (cond != null)
            {
                _conditions.Remove(cond);
                return;
            }

            base.OnChildDeleted(efContainer);
        }

#if DEBUG
        internal override ICollection<string> MyAttributeNames()
        {
            var s = base.MyAttributeNames();
            s.Add(AttributeTypeName);
            s.Add(AttributeIsPartial);
            return s;
        }
#endif

#if DEBUG
        internal override ICollection<string> MyChildElementNames()
        {
            var s = base.MyChildElementNames();
            s.Add(ScalarProperty.ElementName);
            s.Add(ComplexProperty.ElementName);
            s.Add(Condition.ElementName);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_typeName);
            _typeName = null;

            ClearEFObject(_isPartial);
            _isPartial = null;

            ClearEFObjectCollection(_scalarProperties);
            ClearEFObjectCollection(_complexProperties);
            ClearEFObjectCollection(_conditions);

            base.PreParse();
        }

        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            if (elem.Name.LocalName == ScalarProperty.ElementName)
            {
                var prop = new ScalarProperty(this, elem);
                _scalarProperties.Add(prop);
                prop.Parse(unprocessedElements);
            }
            else if (elem.Name.LocalName == ComplexProperty.ElementName)
            {
                var complexProperty = new ComplexProperty(this, elem);
                _complexProperties.Add(complexProperty);
                complexProperty.Parse(unprocessedElements);
            }
            else if (elem.Name.LocalName == Condition.ElementName)
            {
                var cond = new Condition(this, elem);
                _conditions.Add(cond);
                cond.Parse(unprocessedElements);
            }
            else
            {
                return base.ParseSingleElement(unprocessedElements, elem);
            }
            return true;
        }

        protected override void DoResolve(EFArtifactSet artifactSet)
        {
            TypeName.Rebind();
            // TypeName attribute is optional so its status might be undefined
            if (TypeName.Status == BindingStatus.Known
                || TypeName.Status == BindingStatus.Undefined)
            {
                State = EFElementState.Resolved;
            }
        }

        internal override void GetXLinqInsertPosition(EFElement child, out XNode insertAt, out bool insertBefore)
        {
            if (child is ScalarProperty)
            {
                insertAt = FirstChildXElementOrNull();
                insertBefore = true;
            }
            else
            {
                base.GetXLinqInsertPosition(child, out insertAt, out insertBefore);
            }
        }
    }
}