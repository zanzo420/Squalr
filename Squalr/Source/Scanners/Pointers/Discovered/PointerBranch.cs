﻿namespace Squalr.Source.Scanners.Pointers.Discovered
{
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;

    internal class PointerBranch
    {
        public PointerBranch(Int32 offset)
        {
            this.Offset = offset;
            this.Branches = new List<PointerBranch>();
        }

        public Int32 Offset { get; set; }

        public IList<PointerBranch> Branches { get; set; }

        public void AddOffsets(IEnumerable<Int32> offsets)
        {
            if (offsets.IsNullOrEmpty())
            {
                return;
            }

            foreach (Int32 offset in offsets)
            {
                this.Branches.Add(new PointerBranch(offset));
            }
        }
    }
}
