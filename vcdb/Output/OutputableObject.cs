﻿using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.Output
{
    internal class OutputableObject<T> : IOutputable
    {
        private DatabaseDetails value;

        public OutputableObject(DatabaseDetails value)
        {
            this.value = value;
        }

        public async Task WriteToOutput(IOutput output)
        {
            await output.WriteJsonToOutput(value);
        }
    }
}