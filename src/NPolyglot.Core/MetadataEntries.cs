using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPolyglot.Core
{
    public class MetadataEntries
    {
        public MetadataEntries()
        {
            Parser = null;
            Transform = null;
        }

        public bool HasParser => Parser != null;

        public bool HasTransform => Transform != null;

        public string Parser { get; private set; }

        public string Transform { get; private set; }

        public void SetParser(string parser)
        {
            Debug.Assert(parser != null);

            Parser = parser;
        }

        public void SetTransform(string transform)
        {
            Debug.Assert(transform != null);

            Transform = transform;
        }

        public void ClearParser() =>
            Parser = null;

        public void ClearTransform() =>
            Transform = null;
    }
}
