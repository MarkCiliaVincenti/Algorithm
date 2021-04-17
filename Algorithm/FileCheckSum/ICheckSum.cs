﻿using System.Collections.Generic;

namespace Eocron.Algorithms.FileCheckSum
{
    public interface ICheckSum<T>
    {
        int CalculateCapacity(long streamLength);

        int CalculatePartSize(IReadOnlyList<T> hashes);

        T InitialHash();

        T NextHash(T hash, byte[] readBytes, int offset, int count);
    }
}
